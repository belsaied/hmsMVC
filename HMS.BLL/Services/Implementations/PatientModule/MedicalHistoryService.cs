using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.Enums.PatientEnums;
using HMS.DAL.Models.PatientModule;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.PatientModule;
using HMS.BLL.Shared.Dtos.PatientModule.Medical_History_Dtos;

namespace HMS.BLL.Services.Implementations.PatientModule
{
    public class MedicalHistoryService (IUnitOfWork _unitOfWork , IMapper _mapper) : IMedicalHistoryService
    {
        public async Task<MedicalHistoryResultDto> AddMedicalHistoryAsync(int patientId, CreateMedicalHistoryDto historyDto)
        {
            // STEP 1: Verify patient exists
            var patientRepository = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepository.GetByIdAsync(patientId);

            //Throw Exception instead of returning null
            if (patient is null)
                throw new NotFoundException(nameof(Patient), patientId);

            if (patient.Status != PatientStatus.Active)
                throw new BusinessRuleException("Cannot add medical history to an inactive patient.");

            if (historyDto.DiagnosisDate > DateTime.UtcNow)
                throw new BusinessRuleException("Diagnosis date cannot be in the future.");


            // STEP 2: Map DTO to Entity
            var medicalHistory = _mapper.Map<PatientMedicalHistory>(historyDto);
            medicalHistory.PatientId = patientId;
            medicalHistory.RecordedDate = DateTime.UtcNow;
            medicalHistory.IsActive = true;

            // STEP 3: Get medical history repository and add
            var historyRepository = _unitOfWork.GetRepository<PatientMedicalHistory, int>();
            await historyRepository.AddAsync(medicalHistory);

            // STEP 4: Save changes
            await _unitOfWork.SaveChangesAsync();

            // STEP 5: Return result DTO
            return _mapper.Map<MedicalHistoryResultDto>(medicalHistory);
        }

        public async Task<IEnumerable<MedicalHistoryResultDto>> GetPatientMedicalHistoryAsync(int patientId)
        {
            var patientRepository = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepository.GetByIdAsync(patientId);

            if (patient is null)
                throw new NotFoundException(nameof(Patient), patientId);

            var historyRepository = _unitOfWork.GetRepository<PatientMedicalHistory, int>();
            var spec = new PatientMedicalHistorySpecification(patientId);
            var patientHistories = await historyRepository.GetAllAsync(spec);

            return _mapper.Map<IEnumerable<MedicalHistoryResultDto>>(patientHistories);
        }
        public async Task<MedicalHistoryResultDto> UpdateMedicalHistoryAsync(int patientId, int historyId, UpdateMedicalHistoryDto historyDto)
        {
            // STEP 1: Get medical history repository
            var historyRepository = _unitOfWork.GetRepository<PatientMedicalHistory, int>();
            var medicalHistory = await historyRepository.GetByIdAsync(historyId);

            // STEP 2: Verify medical history exists and belongs to patient
            if (medicalHistory is null)
                throw new NotFoundException(nameof(PatientMedicalHistory), historyId);

            //Verify medical history belongs to patient

            if (medicalHistory.PatientId != patientId)
                throw new BusinessRuleException($"Medical history with ID {historyId} does not belong to patient {patientId}.");

            // Validate resolution date if provided (Business Rule)

            if (historyDto.ResolutionDate.HasValue)
            {
                if (historyDto.ResolutionDate.Value < medicalHistory.DiagnosisDate)
                    throw new BusinessRuleException("Resolution date cannot be before diagnosis date.");

                if (historyDto.ResolutionDate.Value > DateTime.UtcNow)
                    throw new BusinessRuleException("Resolution date cannot be in the future.");
            }

            // STEP 3: Update fields if provided
            if (!string.IsNullOrEmpty(historyDto.Treatment))
                medicalHistory.Treatment = historyDto.Treatment;

            if (historyDto.ResolutionDate.HasValue)
                medicalHistory.ResolutionDate = historyDto.ResolutionDate.Value;

            if (historyDto.IsActive.HasValue)
                medicalHistory.IsActive = historyDto.IsActive.Value;

            if (!string.IsNullOrEmpty(historyDto.Notes))
                medicalHistory.Notes = historyDto.Notes;

            // STEP 4: Mark as modified
            historyRepository.Update(medicalHistory);

            // STEP 5: Save changes
            await _unitOfWork.SaveChangesAsync();

            // STEP 6: Return updated DTO
            return _mapper.Map<MedicalHistoryResultDto>(medicalHistory);

        }
    }
}
