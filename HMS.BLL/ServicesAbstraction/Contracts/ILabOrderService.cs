using HMS.BLL.Shared.Dtos.MedicalRecordsDto;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface ILabOrderService
    {
        Task<LabOrderResultDto> CreateLabOrderAsync(int medicalRecordId, CreateLabOrderDto dto);
        Task<IEnumerable<LabOrderResultDto>> GetPatientLabOrdersAsync(int patientId);
        Task<LabOrderResultDto> UpdateLabOrderStatusAsync(int orderId, UpdateLabOrderStatusDto dto);
        Task<LabResultResultDto> AddLabResultAsync(int orderId, CreateLabResultDto dto);
    }
}
