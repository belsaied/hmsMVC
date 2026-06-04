using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.AppointmentModule;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.AppointmentEnums;
using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.DAL.Models.Enums.PatientEnums;
using HMS.DAL.Models.PatientModule;
using Microsoft.Extensions.Logging;
using Services.Abstraction.Contracts;
using Services.Abstraction.Contracts.NotificationService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.AppointmentModule;
using HMS.BLL.Shared.Dtos.NotificationDtos.Events;
using HMS.BLL.Shared.Parameters;
using ServicesAbstraction.Contracts;
using HMS.BLL.Services.Specifications.DoctorModule;
using HMS.BLL.Services.Specifications.AppointmentModule;

namespace HMS.BLL.Services.Implementations.AppointmentModule
{
    public class AppointmentService(
        IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IAppointmentNotifier _notifier,
        INotificationService _notificationService,
        ILogger<AppointmentService> _logger) : IAppointmentService
    {
        public async Task<AppointmentResultDto> BookAppointmentAsync(CreateAppointmentDto dto)
        {
            // 1. Validate patient exists and is Active
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepo.GetByIdAsync(dto.PatientId);
            if (patient is null) throw new PatientNotFoundException(dto.PatientId);
            if (patient.Status != PatientStatus.Active)
                throw new BusinessRuleException("Only Active patients can book appointments.");

            // 2. Validate doctor exists and is Active
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(new DoctorWithDetailsSpecification(dto.DoctorId));
            if (doctor is null) throw new DoctorNotFoundException(dto.DoctorId);
            if (doctor.Status != DoctorStatus.Active)
                throw new BusinessRuleException("Only Active doctors can receive bookings.");

            // 3. Date must be today or in the future
            if (dto.AppointmentDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ValidationException(new[] { "Appointment date must be today or in the future." });

            // 4. StartTime must fall within a DoctorSchedule slot for that DayOfWeek
            var customDay = dto.AppointmentDate.DayOfWeek;

            var matchingSchedule = doctor.AvailabilitySchedules
                .FirstOrDefault(s => s.DayOfWeek == customDay &&
                                     s.IsAvailable &&
                                     dto.StartTime >= s.StartTime &&
                                     dto.StartTime < s.EndTime);

            if (matchingSchedule is null)
                throw new BusinessRuleException(
                    "The requested time is outside the doctor's available schedule.");

            // 5. Compute EndTime from slot duration
            var endTime = dto.StartTime.AddMinutes(matchingSchedule.SlotDurationMinutes);

            // 6. Check for slot conflicts
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var conflicts = await aptRepo.GetAllAsync(
                new ConflictCheckSpecification(dto.DoctorId, dto.AppointmentDate, dto.StartTime, endTime));
            if (conflicts.Any())
                throw new BusinessRuleException(
                    "The doctor already has a conflicting appointment in this time slot.");

            // 7. Generate unique ConfirmationNumber
            var confirmationNumber = await GenerateConfirmationNumberAsync(dto.AppointmentDate, aptRepo);

            // 8. Create and save
            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = endTime,
                Status = AppointmentStatus.Scheduled,
                Type = dto.Type,
                ReasonForVisit = dto.ReasonForVisit,
                ConfirmationNumber = confirmationNumber,
                BookedAt = DateTime.UtcNow
            };

            await aptRepo.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            // 9. Notify doctor group — doctor's dashboard sees new booking instantly
            await _notifier.NotifyDoctorAsync(dto.DoctorId, "NewAppointmentBooked", new
            {
                appointmentId = appointment.Id,
                confirmationNumber = appointment.ConfirmationNumber,
                patientName = $"{patient.FirstName} {patient.LastName}",
                date = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                startTime = appointment.StartTime.ToString("HH:mm"),
                endTime = appointment.EndTime.ToString("HH:mm"),
                type = appointment.Type.ToString()
            });

            // 10. Reload with navigation props for mapping
            var saved = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(appointment.Id));
            return _mapper.Map<AppointmentResultDto>(saved!);
        }

        public async Task<AppointmentResultDto> GetAppointmentByIdAsync(int id)
        {
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apt = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            if (apt is null) throw new AppointmentNotFoundException(id);
            return _mapper.Map<AppointmentResultDto>(apt);
        }

        public async Task<AppointmentResultDto> ConfirmAppointmentAsync(int id)
        {
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apt = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            if (apt is null) throw new AppointmentNotFoundException(id);
            if (apt.Status != AppointmentStatus.Scheduled)
                throw new BusinessRuleException("Only Scheduled appointments can be confirmed.");

            apt.Status = AppointmentStatus.Confirmed;
            apt.UpdatedAt = DateTime.UtcNow;
            aptRepo.Update(apt);
            await _unitOfWork.SaveChangesAsync();

            // Notify patient — they are notified their booking is confirmed
            await _notifier.NotifyPatientAsync(apt.PatientId, "AppointmentConfirmed", new
            {
                appointmentId = apt.Id,
                confirmationNumber = apt.ConfirmationNumber,
                doctorName = $"{apt.Doctor.FirstName} {apt.Doctor.LastName}",
                date = apt.AppointmentDate.ToString("yyyy-MM-dd"),
                startTime = apt.StartTime.ToString("HH:mm")
            });

            //Add Notification Service here
            try
            {
                await _notificationService.SendAppointmentConfirmedAsync(new AppointmentNotificationEvent
                {
                    PatientId = apt.PatientId,
                    PatientEmail = apt.Patient.Email,
                    PatientPhone = apt.Patient.Phone,
                    PatientName = $"{apt.Patient.FirstName} {apt.Patient.LastName}",
                    DoctorId = apt.DoctorId,
                    DoctorEmail = apt.Doctor.Email,
                    DoctorName = $"{apt.Doctor.FirstName} {apt.Doctor.LastName}",
                    AppointmentDate = apt.AppointmentDate,
                    StartTime = apt.StartTime,
                    ConfirmationNumber = apt.ConfirmationNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notification] Failed to send AppointmentConfirmed for {Id}", apt.Id);
            }

            var updated = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            return _mapper.Map<AppointmentResultDto>(updated!);
        }

        public async Task<AppointmentResultDto> CompleteAppointmentAsync(int id, string? notes)
        {
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apt = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            if (apt is null) throw new AppointmentNotFoundException(id);
            if (apt.Status != AppointmentStatus.Confirmed)
                throw new BusinessRuleException("Only Confirmed appointments can be completed.");

            apt.Status = AppointmentStatus.Completed;
            apt.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(notes)) apt.Notes = notes;
            aptRepo.Update(apt);
            await _unitOfWork.SaveChangesAsync();

            // Notify patient — they know their visit is marked done
            await _notifier.NotifyPatientAsync(apt.PatientId, "AppointmentCompleted", new
            {
                appointmentId = apt.Id,
                confirmationNumber = apt.ConfirmationNumber,
                notes = apt.Notes
            });

            var updated = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            return _mapper.Map<AppointmentResultDto>(updated!);
        }

        public async Task<bool> CancelAppointmentAsync(int id, string reason)
        {
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apt = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            if (apt is null) throw new AppointmentNotFoundException(id);

            if (apt.Status == AppointmentStatus.Cancelled)
                throw new BusinessRuleException("Appointment is already cancelled.");

            var appointmentDateTime = apt.AppointmentDate.ToDateTime(apt.StartTime);
            if (DateTime.UtcNow > appointmentDateTime.AddHours(-2))
                throw new BusinessRuleException(
                    "Cannot cancel an appointment less than 2 hours before start time.");

            apt.Status = AppointmentStatus.Cancelled;
            apt.CancellationReason = reason;
            apt.UpdatedAt = DateTime.UtcNow;
            aptRepo.Update(apt);
            await _unitOfWork.SaveChangesAsync();

            // Notify BOTH doctor and patient
            var payload = new
            {
                appointmentId = apt.Id,
                confirmationNumber = apt.ConfirmationNumber,
                cancellationReason = reason,
                date = apt.AppointmentDate.ToString("yyyy-MM-dd"),
                startTime = apt.StartTime.ToString("HH:mm")
            };

            try
            {
                await _notificationService.SendAppointmentCancelledAsync(new AppointmentNotificationEvent
                {
                    PatientId = apt.PatientId,
                    PatientEmail = apt.Patient.Email,
                    PatientPhone = apt.Patient.Phone,
                    PatientName = $"{apt.Patient.FirstName} {apt.Patient.LastName}",
                    DoctorId = apt.DoctorId,
                    DoctorEmail = apt.Doctor.Email,
                    DoctorName = $"{apt.Doctor.FirstName} {apt.Doctor.LastName}",
                    AppointmentDate = apt.AppointmentDate,
                    StartTime = apt.StartTime,
                    ConfirmationNumber = apt.ConfirmationNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notification] Failed to send AppointmentCancelled for {Id}", apt.Id);
            }


            await _notifier.NotifyDoctorAsync(apt.DoctorId, "AppointmentCancelled", payload);
            await _notifier.NotifyPatientAsync(apt.PatientId, "AppointmentCancelled", payload);

            return true;
        }

        public async Task<AppointmentResultDto> UpdateAppointmentStatusAsync(int id, UpdateAppointmentStatusDto dto)
        {
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apt = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            if (apt is null) throw new AppointmentNotFoundException(id);

            ValidateStateTransition(apt.Status, dto.NewStatus);

            apt.Status = dto.NewStatus;
            apt.UpdatedAt = DateTime.UtcNow;

            if (dto.NewStatus == AppointmentStatus.Cancelled)
            {
                if (string.IsNullOrEmpty(dto.CancellationReason))
                    throw new ValidationException(new[] { "CancellationReason is required when cancelling." });
                apt.CancellationReason = dto.CancellationReason;
            }

            if (!string.IsNullOrEmpty(dto.Notes)) apt.Notes = dto.Notes;

            aptRepo.Update(apt);
            await _unitOfWork.SaveChangesAsync();

            // ── SignalR Notifications ────────────────────────────────────────────
            switch (dto.NewStatus)
            {
                case AppointmentStatus.Confirmed:
                    await _notifier.NotifyPatientAsync(apt.PatientId, "AppointmentConfirmed", new
                    {
                        appointmentId = apt.Id,
                        confirmationNumber = apt.ConfirmationNumber,
                        doctorName = $"{apt.Doctor.FirstName} {apt.Doctor.LastName}",
                        date = apt.AppointmentDate.ToString("yyyy-MM-dd"),
                        startTime = apt.StartTime.ToString("HH:mm")
                    });
                    break;

                case AppointmentStatus.Completed:
                    await _notifier.NotifyPatientAsync(apt.PatientId, "AppointmentCompleted", new
                    {
                        appointmentId = apt.Id,
                        confirmationNumber = apt.ConfirmationNumber,
                        notes = apt.Notes
                    });
                    break;

                case AppointmentStatus.Cancelled:
                    var cancelPayload = new
                    {
                        appointmentId = apt.Id,
                        confirmationNumber = apt.ConfirmationNumber,
                        cancellationReason = apt.CancellationReason,
                        date = apt.AppointmentDate.ToString("yyyy-MM-dd"),
                        startTime = apt.StartTime.ToString("HH:mm")
                    };
                    await _notifier.NotifyDoctorAsync(apt.DoctorId, "AppointmentCancelled", cancelPayload);
                    await _notifier.NotifyPatientAsync(apt.PatientId, "AppointmentCancelled", cancelPayload);
                    break;

                case AppointmentStatus.NoShow:
                    await _notifier.NotifyPatientAsync(apt.PatientId, "AppointmentNoShow", new
                    {
                        appointmentId = apt.Id,
                        confirmationNumber = apt.ConfirmationNumber,
                        date = apt.AppointmentDate.ToString("yyyy-MM-dd"),
                        startTime = apt.StartTime.ToString("HH:mm")
                    });
                    break;
            }
            // ────────────────────────────────────────────────────────────────────

            var updated = await aptRepo.GetByIdAsync(new AppointmentWithDetailsSpecification(id));
            return _mapper.Map<AppointmentResultDto>(updated!);
        }

        public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(new DoctorWithDetailsSpecification(doctorId));
            if (doctor is null) throw new DoctorNotFoundException(doctorId);

            var customDay = date.DayOfWeek;

            var schedule = doctor.AvailabilitySchedules
                .FirstOrDefault(s => s.DayOfWeek == customDay && s.IsAvailable);

            if (schedule is null) return [];

            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var booked = await aptRepo.GetAllAsync(new DoctorDailyAppointmentsSpecification(doctorId, date));
            var bookedActive = booked
                .Where(a => a.Status == AppointmentStatus.Scheduled ||
                            a.Status == AppointmentStatus.Confirmed)
                .ToList();

            var slots = new List<AvailableSlotDto>();
            var slotStart = schedule.StartTime;

            while (slotStart.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
            {
                var slotEnd = slotStart.AddMinutes(schedule.SlotDurationMinutes);
                var isTaken = bookedActive.Any(a => a.StartTime < slotEnd && a.EndTime > slotStart);
                slots.Add(new AvailableSlotDto
                {
                    DoctorId = doctorId,
                    Date = date,
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsAvailable = !isTaken
                });
                slotStart = slotEnd;
            }
            return slots;
        }

        public async Task<PaginatedResult<AppointmentResultDto>> GetAllAppointmentsAsync(
            AppointmentSpecificationParameters parameters)
        {
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apts = await aptRepo.GetAllAsync(new AppointmentListSpecification(parameters));
            var count = await aptRepo.CountAsync(new AppointmentCountSpecification(parameters));
            return new PaginatedResult<AppointmentResultDto>(
                parameters.PageIndex, parameters.PageSize, count,
                _mapper.Map<IEnumerable<AppointmentResultDto>>(apts));
        }

        public async Task<IEnumerable<AppointmentResultDto>> GetPatientAppointmentsAsync(int patientId)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apts = await aptRepo.GetAllAsync(new PatientAppointmentsSpecification(patientId));
            return _mapper.Map<IEnumerable<AppointmentResultDto>>(apts);
        }

        public async Task<IEnumerable<AppointmentResultDto>> GetDoctorAppointmentsAsync(
            int doctorId, DateOnly date)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            if (await doctorRepo.GetByIdAsync(doctorId) is null)
                throw new DoctorNotFoundException(doctorId);
            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var apts = await aptRepo.GetAllAsync(new DoctorDailyAppointmentsSpecification(doctorId, date));
            return _mapper.Map<IEnumerable<AppointmentResultDto>>(apts);
        }
        public async Task<IEnumerable<DoctorPatientSummaryDto>> GetDoctorPatientsAsync(int doctorId)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            if (await doctorRepo.GetByIdAsync(doctorId) is null)
                throw new DoctorNotFoundException(doctorId);

            var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
            var appointments = await aptRepo.GetAllAsync(new DoctorAllPatientsSpecification(doctorId));

            var grouped = appointments
                .GroupBy(a => a.PatientId)
                .Select(g =>
                {
                    var patient = g.First().Patient;
                    return new DoctorPatientSummaryDto
                    {
                        PatientId = patient.Id,
                        PatientName = $"{patient.FirstName} {patient.LastName}",
                        Email = patient.Email,
                        Phone = patient.Phone,
                        Gender = patient.Gender.ToString(),
                        Age = patient.Age,
                        MedicalRecordNumber = patient.MedicalRecordNumber,
                        Appointments = g.Select(a => new PatientAppointmentSummaryDto
                        {
                            AppointmentId = a.Id,
                            ConfirmationNumber = a.ConfirmationNumber,
                            AppointmentDate = a.AppointmentDate,
                            StartTime = a.StartTime,
                            EndTime = a.EndTime,
                            Status = a.Status.ToString(),
                            Type = a.Type.ToString(),
                            ReasonForVisit = a.ReasonForVisit,
                            Notes = a.Notes,
                            CancellationReason = a.CancellationReason,
                            BookedAt = a.BookedAt
                        }).OrderByDescending(a => a.AppointmentDate).ToList()
                    };
                });

            return grouped;
        }
        // ── Private Helpers ──────────────────────────────────────────────────

        private static void ValidateStateTransition(AppointmentStatus current, AppointmentStatus next)
        {
            var valid = (current, next) switch
            {
                (AppointmentStatus.Scheduled, AppointmentStatus.Confirmed) => true,
                (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.Completed) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.NoShow) => true,
                _ => false
            };
            if (!valid)
                throw new BusinessRuleException($"Cannot transition from {current} to {next}.");
        }

        private async Task<string> GenerateConfirmationNumberAsync(
            DateOnly date,
            IGenericRepository<Appointment, int> aptRepo)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rng = Random.Shared; 
            string number;
            do
            {
                var suffix = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[rng.Next(s.Length)]).ToArray());
                number = $"APT-{date:yyyyMMdd}-{suffix}";
            }
            while (await aptRepo.CountAsync(
                new ConfirmationNumberExistsSpecification(number)) > 0);
            return number;
        }
    }
}