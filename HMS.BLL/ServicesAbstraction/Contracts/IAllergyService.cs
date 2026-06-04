using HMS.BLL.Shared.Dtos.PatientModule.AllergyDtos;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IAllergyService
    {
        Task<AllergyResultDto> AddAllergyAsync(int patientId, CreateAllergyDto allergyDto);
        Task<IEnumerable<AllergyResultDto>> GetPatientAllergiesAsync(int patientId);
        Task<bool> RemoveAllergyAsync(int patientId, int allergyId);
    }
}
