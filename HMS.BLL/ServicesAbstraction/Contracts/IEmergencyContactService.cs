using HMS.BLL.Shared.Dtos.PatientModule.EmergencyContactsDtos;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IEmergencyContactService
    {
        Task<EmergencyContactResultDto> AddEmergencyContactAsync(int patientId, CreateEmergencyContactDto contactDto);
        Task<IEnumerable<EmergencyContactResultDto>> GetEmergencyContactsAsync(int patientId);
        Task<EmergencyContactResultDto> UpdateEmergencyContactAsync(int patientId, int contactId, UpdateEmergencyContactDto contactDto);
        Task<bool> DeleteEmergencyContactAsync(int patientId, int contactId);
    }
}
