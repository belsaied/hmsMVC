using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.AppointmentModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IAppointmentService
    {
        Task<AppointmentResultDto> BookAppointmentAsync(CreateAppointmentDto dto);
        Task<AppointmentResultDto> GetAppointmentByIdAsync(int id);
        Task<AppointmentResultDto> UpdateAppointmentStatusAsync(int id, UpdateAppointmentStatusDto dto);
        Task<bool> CancelAppointmentAsync(int id, string reason);
        Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date);
        Task<PaginatedResult<AppointmentResultDto>> GetAllAppointmentsAsync(AppointmentSpecificationParameters parameters);
        Task<IEnumerable<AppointmentResultDto>> GetPatientAppointmentsAsync(int patientId);
        Task<IEnumerable<AppointmentResultDto>> GetDoctorAppointmentsAsync(int doctorId, DateOnly date);
        Task<AppointmentResultDto> ConfirmAppointmentAsync(int id);
        Task<AppointmentResultDto> CompleteAppointmentAsync(int id, string? notes);
    }
}
