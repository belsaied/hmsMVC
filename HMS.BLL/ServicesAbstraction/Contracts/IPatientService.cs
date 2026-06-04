using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.PatientModule.PatientDtos;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IPatientService
    {
        Task<PatientResultDto> RegisterPatientAsync(CreatePatientDto createPatientDto);
        Task<PatientResultDto> GetPatientByIdAsync(int id);
        Task<PatientResultDto> UpdatePatientAsync(int id, UpdatePatientDto updatePatientDto);
        Task<bool> DeactivatePatientAsync(int id);

        // Two new endpoints
        Task<PatientWithDetailsResultDto> GetPatientWithDetailsAsync(int id);
        Task<PaginatedResult<PatientResultDto>> GetAllPatientsAsync(PatientSpecificationParameters parameters);
    }
}
