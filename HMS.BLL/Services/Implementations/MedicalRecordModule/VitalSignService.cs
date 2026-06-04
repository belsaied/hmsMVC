using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.DAL.Models.PatientModule;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.MedicalRecordModule;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;
using HMS.BLL.ServicesAbstraction.Contracts;

namespace HMS.BLL.Services.Implementations.MedicalRecordModule
{
    public class VitalSignService (IUnitOfWork _unitOfWork, IMapper _mapper) : IVitalSignService
    {
        public async Task<VitalSignResultDto> AddVitalSignAsync(int medicalRecordId, CreateVitalSignDto dto)
        {
            // Validate record exists
            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            var record = await recordRepo.GetByIdAsync(medicalRecordId);
            if (record is null) throw new MedicalRecordNotFoundException(medicalRecordId);

            var vital = _mapper.Map<VitalSign>(dto);
            vital.MedicalRecordId = medicalRecordId;
            vital.PatientId = record.PatientId;
            vital.RecordedAt = DateTime.UtcNow;

            var vitalRepo = _unitOfWork.GetRepository<VitalSign, int>();
            await vitalRepo.AddAsync(vital);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VitalSignResultDto>(vital);
        }

        public async Task<IEnumerable<VitalSignResultDto>> GetPatientVitalHistoryAsync(int patientId)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);

            var vitalRepo = _unitOfWork.GetRepository<VitalSign, int>();
            var vitals = await vitalRepo.GetAllAsync(new PatientVitalHistorySpecification(patientId));
            return _mapper.Map<IEnumerable<VitalSignResultDto>>(vitals);
        }

        public async Task<VitalSignResultDto?> GetLatestVitalsAsync(int patientId)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);

            var vitalRepo = _unitOfWork.GetRepository<VitalSign, int>();
            var vitals = await vitalRepo.GetAllAsync(new PatientVitalHistorySpecification(patientId));
            var latest = vitals.FirstOrDefault(); // already ordered desc by RecordedAt
            return latest is null ? null : _mapper.Map<VitalSignResultDto>(latest);
        }
    }
}
