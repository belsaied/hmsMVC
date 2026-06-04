using HMS.BLL.Shared.Dtos.MedicalRecordsDto;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IVitalSignService
    {
        Task<VitalSignResultDto> AddVitalSignAsync(int medicalRecordId, CreateVitalSignDto dto);
        Task<IEnumerable<VitalSignResultDto>> GetPatientVitalHistoryAsync(int patientId);
        Task<VitalSignResultDto?> GetLatestVitalsAsync(int patientId);
    }
}
