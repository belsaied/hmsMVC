using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.AppointmentModule;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.PatientEnums;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.DAL.Models.PatientModule;
using Services.Abstraction.Contracts;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.MedicalRecordModule;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Common;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;
using HMS.BLL.Shared.Parameters;

namespace Services.Implementations.MedicalRecordModule
{
    public class MedicalRecordService (IUnitOfWork _unitOfWork , IMapper _mapper,ICacheService _cacheService) : IMedicalRecordService
    {
        public async Task<MedicalRecordResultDto> CreateMedicalRecordAsync(CreateMedicalRecordDto dto)
        {
            // 1. Validate patient
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepo.GetByIdAsync(dto.PatientId);
            if (patient is null) throw new PatientNotFoundException(dto.PatientId);
            if (patient.Status == PatientStatus.Inactive || patient.Status == PatientStatus.Deceased)
                throw new BusinessRuleException("Cannot create a medical record for an Inactive or Deceased patient.");

            // 2. Validate doctor
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(dto.DoctorId);
            if (doctor is null) throw new DoctorNotFoundException(dto.DoctorId);

            // 3. Visit date cannot be in the future
            if (dto.VisitDate > DateTime.UtcNow)
                throw new ValidationException(new[] { "Visit date cannot be in the future." });

            // 4. Validate appointment ownership if provided
            if (dto.AppointmentId.HasValue)
            {
                var aptRepo = _unitOfWork.GetRepository<Appointment, int>();
                var appointment = await aptRepo.GetByIdAsync(dto.AppointmentId.Value);
                if (appointment is null)
                    throw new NotFoundException("Appointment", dto.AppointmentId.Value);
                if (appointment.PatientId != dto.PatientId || appointment.DoctorId != dto.DoctorId)
                    throw new BusinessRuleException(
                        "The provided AppointmentId does not belong to the same patient and doctor.");
            }

            // 5. Map and save
            var record = _mapper.Map<MedicalRecord>(dto);
            record.RecordedAt = DateTime.UtcNow;

            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            await recordRepo.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            // 6. Reload with full navigation
            var saved = await recordRepo.GetByIdAsync(new MedicalRecordWithDetailsSpecification(record.Id));
            return _mapper.Map<MedicalRecordResultDto>(saved!);
        }

        public async Task<MedicalRecordResultDto> GetMedicalRecordByIdAsync(int id)
        {
            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            var record = await recordRepo.GetByIdAsync(new MedicalRecordWithDetailsSpecification(id));
            if (record is null) throw new MedicalRecordNotFoundException(id);
            return _mapper.Map<MedicalRecordResultDto>(record);
        }

        public async Task<MedicalRecordResultDto> UpdateMedicalRecordAsync(
            int id, UpdateMedicalRecordDto dto, int requestingDoctorId)
        {
            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            var record = await recordRepo.GetByIdAsync(new MedicalRecordWithDetailsSpecification(id));
            if (record is null) throw new MedicalRecordNotFoundException(id);

            // Author ownership check
            if (record.DoctorId != requestingDoctorId)
                throw new BusinessRuleException("Only the authoring doctor can edit this medical record.");

            if (!string.IsNullOrEmpty(dto.ChiefComplaint)) record.ChiefComplaint = dto.ChiefComplaint;
            if (!string.IsNullOrEmpty(dto.Diagnosis)) record.Diagnosis = dto.Diagnosis;
            if (!string.IsNullOrEmpty(dto.IcdCode)) record.IcdCode = dto.IcdCode;
            if (!string.IsNullOrEmpty(dto.ClinicalNotes)) record.ClinicalNotes = dto.ClinicalNotes;
            if (!string.IsNullOrEmpty(dto.TreatmentPlan)) record.TreatmentPlan = dto.TreatmentPlan;
            if (dto.FollowUpDate.HasValue) record.FollowUpDate = dto.FollowUpDate.Value;
            if (dto.IsConfidential.HasValue) record.IsConfidential = dto.IsConfidential.Value;
            record.UpdatedAt = DateTime.UtcNow;

            recordRepo.Update(record);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveAsync(CacheKeys.MedicalRecord(id));
            var updated = await recordRepo.GetByIdAsync(new MedicalRecordWithDetailsSpecification(id));
            return _mapper.Map<MedicalRecordResultDto>(updated!);
        }

        public async Task<PaginatedResult<MedicalRecordResultDto>> GetPatientMedicalRecordsAsync(int patientId, MedicalRecordSpecificationParameters p)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);

            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            var records = await recordRepo.GetAllAsync(new PatientMedicalRecordsSpecification(patientId,p));
            var count = await recordRepo.CountAsync(new PatientMedicalRecordsCountSpecification(patientId));

            return new PaginatedResult<MedicalRecordResultDto>(
                 p.PageIndex, p.PageSize, count,
                _mapper.Map<IEnumerable<MedicalRecordResultDto>>(records));
        }

        public async Task<PaginatedResult<MedicalRecordResultDto>> GetDoctorMedicalRecordsAsync(int doctorId, MedicalRecordSpecificationParameters p)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            if (await doctorRepo.GetByIdAsync(doctorId) is null)
                throw new DoctorNotFoundException(doctorId);

            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            var records = await recordRepo.GetAllAsync(new DoctorMedicalRecordsSpecification(doctorId, p));
            var count = await recordRepo.CountAsync(new DoctorMedicalRecordsCountSpecification(doctorId));

            return new PaginatedResult<MedicalRecordResultDto>(
                p.PageIndex, p.PageSize, count,
                _mapper.Map<IEnumerable<MedicalRecordResultDto>>(records));
        }

    }
}
