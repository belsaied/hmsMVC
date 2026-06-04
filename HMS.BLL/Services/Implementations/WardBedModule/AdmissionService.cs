using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.DAL.Models.Enums.PatientEnums;
using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.DAL.Models.PatientModule;
using HMS.DAL.Models.WardBedModule;
using Services.Abstraction.Contracts;
using Services.Abstraction.Contracts.WardBedService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.WardBedModule;
using HMS.BLL.Shared.Common;
using HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos;

namespace HMS.BLL.Services.Implementations.WardBedModule
{
    public class AdmissionService( IUnitOfWork _unitOfWork
        ,  IMapper _mapper,
        IBedNotifier _notifier,
        ICacheService _cacheService) : IAdmissionService
    {
        public async Task<AdmissionResultDto> AdmitPatientAsync(CreateAdmissionDto dto)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepo.GetByIdAsync(dto.PatientId);
            if (patient is null) throw new PatientNotFoundException(dto.PatientId);

            if (patient.Status != PatientStatus.Active)
                throw new BusinessRuleException(
                    $"Cannot admit patient with status '{patient.Status}'.");

            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(dto.AdmittingDoctorId);
            if (doctor is null) throw new DoctorNotFoundException(dto.AdmittingDoctorId);

            if (doctor.Status != DoctorStatus.Active)
                throw new BusinessRuleException(
                    $"Cannot assign doctor with status '{doctor.Status}'.");

            var bedRepo = _unitOfWork.GetRepository<Bed, int>();
            var bed = await bedRepo.GetByIdAsync(dto.BedId);
            if (bed is null) throw new BedNotFoundException(dto.BedId);
            if (bed.Status != BedStatus.Available)
                throw new BusinessRuleException(
                    $"Bed {dto.BedId} is not available. Current status: {bed.Status}.");

            var admissionRepo = _unitOfWork.GetRepository<Admission, int>();
            var existing = await admissionRepo.GetAllAsync(
                new ActiveAdmissionForPatientSpecification(dto.PatientId));
            if (existing.Any())
                throw new BusinessRuleException(
                    "Patient already has an active admission.");

            var admission = _mapper.Map<Admission>(dto);
            admission.AdmissionDate = dto.AdmissionDate == default
                ? DateTime.UtcNow
                : dto.AdmissionDate;
            admission.Status = AdmissionStatus.Active;

            await admissionRepo.AddAsync(admission);
            bed.Status = BedStatus.Occupied;
            bedRepo.Update(bed);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveAsync(CacheKeys.WardOccupancy);
            await _cacheService.RemoveAsync(CacheKeys.RoomBeds(bed.RoomId));

            await _notifier.NotifyDashboardAsync("BedOccupied", new
            {
                bedId = dto.BedId,
                bedNumber = bed.BedNumber,
                roomId = bed.RoomId,
                patientId = dto.PatientId,
                admissionId = admission.Id
            });

            var saved = await admissionRepo.GetByIdAsync(
                new AdmissionWithDetailsSpecification(admission.Id));
            return _mapper.Map<AdmissionResultDto>(saved!);
        }

        public async Task<AdmissionResultDto> GetAdmissionByIdAsync(int admissionId)
        {
            var repo = _unitOfWork.GetRepository<Admission, int>();
            var admission = await repo.GetByIdAsync(
                new AdmissionWithDetailsSpecification(admissionId));
            if (admission is null) throw new AdmissionNotFoundException(admissionId);
            return _mapper.Map<AdmissionResultDto>(admission);
        }

        public async Task<IEnumerable<AdmissionResultDto>> GetPatientAdmissionHistoryAsync(
            int patientId)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);

            var repo = _unitOfWork.GetRepository<Admission, int>();
            var admissions = await repo.GetAllAsync(
                new PatientAdmissionHistorySpecification(patientId));
            return _mapper.Map<IEnumerable<AdmissionResultDto>>(admissions);
        }

        public async Task<IEnumerable<AdmissionResultDto>> GetActiveAdmissionsAsync()
        {
            var repo = _unitOfWork.GetRepository<Admission, int>();
            var admissions = await repo.GetAllAsync(new ActiveAdmissionsSpecification());
            return _mapper.Map<IEnumerable<AdmissionResultDto>>(admissions);
        }

        public async Task<AdmissionResultDto> DischargePatientAsync(
            int admissionId, DischargeDto dto)
        {
            var admissionRepo = _unitOfWork.GetRepository<Admission, int>();
            var admission = await admissionRepo.GetByIdAsync(
                new AdmissionWithDetailsSpecification(admissionId));
            if (admission is null) throw new AdmissionNotFoundException(admissionId);

            if (admission.Status != AdmissionStatus.Active)
                throw new BusinessRuleException(
                    $"Cannot discharge. Admission status is: {admission.Status}.");

            admission.Status = AdmissionStatus.Discharged;
            admission.ActualDischargeDate = DateTime.UtcNow;
            admission.DischargeSummary = dto.DischargeSummary;
            admissionRepo.Update(admission);

            var bedRepo = _unitOfWork.GetRepository<Bed, int>();
            var bed = await bedRepo.GetByIdAsync(admission.BedId);
            if (bed is not null)
            {
                bed.Status = BedStatus.Available;
                bedRepo.Update(bed);
            }

            await _unitOfWork.SaveChangesAsync();

            // FIX: Same as AdmitPatientAsync — only invalidate CacheKeys-format keys.
            await _cacheService.RemoveAsync(CacheKeys.WardOccupancy);
            if (bed is not null)
                await _cacheService.RemoveAsync(CacheKeys.RoomBeds(bed.RoomId));

            await _notifier.NotifyDashboardAsync("BedReleased", new
            {
                bedId = admission.BedId,
                bedNumber = bed?.BedNumber,
                roomId = bed?.RoomId,
                newStatus = BedStatus.Available.ToString()
            });

            var updated = await admissionRepo.GetByIdAsync(
                new AdmissionWithDetailsSpecification(admissionId));
            return _mapper.Map<AdmissionResultDto>(updated!);
        }

        public async Task<AdmissionResultDto> TransferPatientAsync(
            int admissionId, TransferBedDto dto)
        {
            var admissionRepo = _unitOfWork.GetRepository<Admission, int>();
            var admission = await admissionRepo.GetByIdAsync(
                new AdmissionWithDetailsSpecification(admissionId));
            if (admission is null) throw new AdmissionNotFoundException(admissionId);

            if (admission.Status != AdmissionStatus.Active)
                throw new BusinessRuleException("Can only transfer an Active admission.");

            if (dto.ToBedId == admission.BedId)
                throw new BusinessRuleException(
                    "Target bed must be different from the patient's current bed.");

            var bedRepo = _unitOfWork.GetRepository<Bed, int>();
            var newBed = await bedRepo.GetByIdAsync(dto.ToBedId);
            if (newBed is null) throw new BedNotFoundException(dto.ToBedId);
            if (newBed.Status != BedStatus.Available)
                throw new BusinessRuleException(
                    $"Target bed {dto.ToBedId} is not available. Status: {newBed.Status}.");

            int fromBedId = admission.BedId;

            var transfer = new BedTransfer
            {
                AdmissionId = admissionId,
                FromBedId = fromBedId,
                ToBedId = dto.ToBedId,
                TransferredAt = DateTime.UtcNow,
                Reason = dto.Reason,
                TransferredBy = dto.TransferredBy
            };

            var transferRepo = _unitOfWork.GetRepository<BedTransfer, int>();
            await transferRepo.AddAsync(transfer);

            admission.BedId = dto.ToBedId;
            admissionRepo.Update(admission);

            var oldBed = await bedRepo.GetByIdAsync(fromBedId);
            if (oldBed is not null)
            {
                oldBed.Status = BedStatus.Available;
                bedRepo.Update(oldBed);
            }
            newBed.Status = BedStatus.Occupied;
            bedRepo.Update(newBed);

            await _unitOfWork.SaveChangesAsync();

            await _notifier.NotifyDashboardAsync("BedTransferred", new
            {
                fromBedId,
                toBedId = dto.ToBedId,
                admissionId,
                reason = dto.Reason
            });

            var updated = await admissionRepo.GetByIdAsync(
                new AdmissionWithDetailsSpecification(admissionId));
            return _mapper.Map<AdmissionResultDto>(updated!);
        }
    }
}
