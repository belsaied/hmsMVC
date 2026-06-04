using HMS.BLL.Shared.Dtos.MedicalRecordsDto;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IPrescriptionService
    {
        Task<PrescriptionResultDto> AddPrescriptionAsync(int medicalRecordId, CreatePrescriptionDto dto);
        Task<IEnumerable<PrescriptionResultDto>> GetPatientPrescriptionsAsync(int patientId);
        Task<IEnumerable<PrescriptionResultDto>> GetActivePrescriptionsAsync(int patientId);
        Task<bool> CancelPrescriptionAsync(int medicalRecordId, int prescriptionId);
    }
}
