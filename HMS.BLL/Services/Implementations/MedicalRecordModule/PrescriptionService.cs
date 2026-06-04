using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.MedicalRecordEnums;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.DAL.Models.PatientModule;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.MedicalRecordModule;
using HMS.BLL.Services.Specifications.PatientModule;
using HMS.BLL.Shared.Common;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;

namespace HMS.BLL.Services.Implementations.MedicalRecordModule
{
    public class PrescriptionService (IUnitOfWork _unitOfWork, IMapper _mapper, ICacheService _cacheService
        , IPrescriptionPdfGenerator _pdfGenerator) : IPrescriptionService
    {
        public async Task<PrescriptionResultDto> AddPrescriptionAsync(int medicalRecordId, CreatePrescriptionDto dto)
        {
            // 1. Validate medical record exists
            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            var record = await recordRepo.GetByIdAsync(medicalRecordId);
            if (record is null) throw new MedicalRecordNotFoundException(medicalRecordId);

            // 2. ExpiresAt must be in the future
            if (dto.ExpiresAt <= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ValidationException(new[] { "Prescription expiry date must be in the future." });

            // 3. Allergy cross-check
            var allergyRepo = _unitOfWork.GetRepository<PatientAllergy, int>();
            var allergies = await allergyRepo.GetAllAsync(new PatientAllergySpecification(record.PatientId));

            var matchingAllergy = allergies.FirstOrDefault(a =>
                a.Description.Contains(dto.MedicationName, StringComparison.OrdinalIgnoreCase) ||
                dto.MedicationName.Contains(a.Description, StringComparison.OrdinalIgnoreCase));

            if (matchingAllergy is not null)
                throw new BusinessRuleException(
                    $"Cannot prescribe '{dto.MedicationName}' — patient has a known {matchingAllergy.Severity} " +
                    $"{matchingAllergy.Type} allergy: {matchingAllergy.Description}.");

            // 4. Map and save
            var prescription = _mapper.Map<Prescription>(dto);
            prescription.MedicalRecordId = medicalRecordId;
            prescription.PatientId = record.PatientId;
            prescription.DoctorId = record.DoctorId;
            prescription.PrescribedAt = DateTime.UtcNow;
            prescription.Status = PrescriptionStatus.Active;

            var prescRepo = _unitOfWork.GetRepository<Prescription, int>();
            await prescRepo.AddAsync(prescription);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveAsync(CacheKeys.PatientPrescriptions(record.PatientId));
            await _cacheService.RemoveAsync(CacheKeys.PatientActivePrescriptions(record.PatientId));
            await _cacheService.RemoveAsync(CacheKeys.MedicalRecord(medicalRecordId));
            return _mapper.Map<PrescriptionResultDto>(prescription);
        }

        public async Task<IEnumerable<PrescriptionResultDto>> GetPatientPrescriptionsAsync(int patientId)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);

            var prescRepo = _unitOfWork.GetRepository<Prescription, int>();
            var prescriptions = await prescRepo.GetAllAsync(new PatientPrescriptionsSpecification(patientId));
            return _mapper.Map<IEnumerable<PrescriptionResultDto>>(prescriptions);
        }

        public async Task<IEnumerable<PrescriptionResultDto>> GetActivePrescriptionsAsync(int patientId)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);

            var prescRepo = _unitOfWork.GetRepository<Prescription, int>();
            var prescriptions = await prescRepo.GetAllAsync(new ActivePrescriptionsSpecification(patientId));
            return _mapper.Map<IEnumerable<PrescriptionResultDto>>(prescriptions);
        }

        public async Task<bool> CancelPrescriptionAsync(int medicalRecordId, int prescriptionId)
        {
            var prescRepo = _unitOfWork.GetRepository<Prescription, int>();
            var prescription = await prescRepo.GetByIdAsync(prescriptionId);

            if (prescription is null) throw new NotFoundException("Prescription", prescriptionId);

            if (prescription.MedicalRecordId != medicalRecordId)
                throw new BusinessRuleException(
                    $"Prescription {prescriptionId} does not belong to medical record {medicalRecordId}.");

            if (prescription.Status != PrescriptionStatus.Active)
                throw new BusinessRuleException(
                    $"Only Active prescriptions can be cancelled. Current status: {prescription.Status}.");

            prescription.Status = PrescriptionStatus.Cancelled;
            prescRepo.Update(prescription);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<byte[]> GeneratePrescriptionPdfAsync(int prescriptionId , int patientId)
        {
            // 1. Load prescription
            var prescRepo = _unitOfWork.GetRepository<Prescription, int>();
            var prescription = await prescRepo.GetByIdAsync(
                new PrescriptionWithDetailsSpecification(prescriptionId))
                ?? throw new PrescriptionNotFoundException(prescriptionId);

            // Ownership check
            if (prescription.PatientId != patientId)
                throw new ForbiddenException(
                    "This prescription does not belong to the specified patient.");

            // 2. Load patient
            var patient = await _unitOfWork.GetRepository<Patient, int>()
                .GetByIdAsync(prescription.PatientId)
                ?? throw new PatientNotFoundException(prescription.PatientId);

            // 3. Load doctor
            var doctor = await _unitOfWork.GetRepository<Doctor, int>()
                .GetByIdAsync(prescription.DoctorId)
                ?? throw new DoctorNotFoundException(prescription.DoctorId);

            var patientName = $"{patient.FirstName} {patient.LastName}";
            var doctorName = $"{doctor.FirstName} {doctor.LastName}";

            return _pdfGenerator.Generate(prescription, patientName, doctorName, doctor.Specialization);
        }
    }
}
