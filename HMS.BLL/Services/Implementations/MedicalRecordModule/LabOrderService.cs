using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.MedicalRecordEnums;
using HMS.DAL.Models.MedicalRecordModule;
using HMS.DAL.Models.PatientModule;
using Microsoft.Extensions.Logging;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.MedicalRecordModule;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;
using HMS.BLL.Shared.Dtos.NotificationDtos.Events;

namespace Services.Implementations.MedicalRecordModule
{
    public class LabOrderService (IUnitOfWork _unitOfWork, IMapper _mapper,ILogger<LabOrderService> _logger ,INotificationService _notificationService) : ILabOrderService
    {
        public async Task<LabOrderResultDto> CreateLabOrderAsync(int medicalRecordId, CreateLabOrderDto dto)
        {
            // 1. Validate record
            var recordRepo = _unitOfWork.GetRepository<MedicalRecord, int>();
            var record = await recordRepo.GetByIdAsync(medicalRecordId);
            if (record is null) throw new MedicalRecordNotFoundException(medicalRecordId);

            // 2. Stat orders require non-empty Notes
            if (dto.Priority == LabOrderPriority.Stat && string.IsNullOrWhiteSpace(dto.Notes))
                throw new ValidationException(new[] { "Stat priority lab orders require Notes explaining clinical urgency." });

            // 3. Map and save
            var labOrder = _mapper.Map<LabOrder>(dto);
            labOrder.MedicalRecordId = medicalRecordId;
            labOrder.PatientId = record.PatientId;
            labOrder.DoctorId = record.DoctorId;
            labOrder.OrderedAt = DateTime.UtcNow;
            labOrder.Status = LabOrderStatus.Pending;

            var orderRepo = _unitOfWork.GetRepository<LabOrder, int>();
            await orderRepo.AddAsync(labOrder);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<LabOrderResultDto>(labOrder);
        }

        public async Task<IEnumerable<LabOrderResultDto>> GetPatientLabOrdersAsync(int patientId)
        {
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            if (await patientRepo.GetByIdAsync(patientId) is null)
                throw new PatientNotFoundException(patientId);

            var orderRepo = _unitOfWork.GetRepository<LabOrder, int>();
            var orders = await orderRepo.GetAllAsync(new PatientLabOrdersSpecification(patientId));
            return _mapper.Map<IEnumerable<LabOrderResultDto>>(orders);
        }

        public async Task<LabOrderResultDto> UpdateLabOrderStatusAsync(int orderId, UpdateLabOrderStatusDto dto)
        {
            var orderRepo = _unitOfWork.GetRepository<LabOrder, int>();
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order is null) throw new NotFoundException("LabOrder", orderId);

            // Guard against invalid transitions — result attachment handles Completed separately
            if (order.Status == LabOrderStatus.Completed || order.Status == LabOrderStatus.Cancelled)
                throw new BusinessRuleException(
                    $"Cannot change status of a {order.Status} lab order.");

            order.Status = dto.NewStatus;
            orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<LabOrderResultDto>(order);
        }

        public async Task<LabResultResultDto> AddLabResultAsync(int orderId, CreateLabResultDto dto)
        {
            var orderRepo = _unitOfWork.GetRepository<LabOrder, int>();
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order is null) throw new NotFoundException("LabOrder", orderId);

            // Lab results can only be added to Pending or InProgress orders
            if (order.Status != LabOrderStatus.Pending && order.Status != LabOrderStatus.InProgress)
                throw new BusinessRuleException(
                    $"A lab result can only be added to a Pending or InProgress order. Current status: {order.Status}.");

            var result = _mapper.Map<LabResult>(dto);
            result.LabOrderId = orderId;

            var resultRepo = _unitOfWork.GetRepository<LabResult, int>();
            await resultRepo.AddAsync(result);

            // Auto-transition order to Completed
            order.Status = LabOrderStatus.Completed;
            orderRepo.Update(order);

            await _unitOfWork.SaveChangesAsync();
            
            if (dto.IsAbnormal)
            {
                try
                {
                    // Fetch doctor info for the notification
                    var doctor = await _unitOfWork.GetRepository<Doctor, int>().GetByIdAsync(order.DoctorId);
                    var patient = await _unitOfWork.GetRepository<Patient, int>().GetByIdAsync(order.PatientId);

                    await _notificationService.SendAbnormalLabResultAsync(new AbnormalLabResultEvent
                    {
                        LabOrderId = order.Id,
                        PatientId = order.PatientId,
                        PatientName = patient is not null ? $"{patient.FirstName} {patient.LastName}" : "Unknown",
                        OrderingDoctorId = order.DoctorId,
                        DoctorEmail = doctor?.Email ?? string.Empty,
                        DoctorName = doctor is not null ? $"{doctor.FirstName} {doctor.LastName}" : "Unknown",
                        TestName = order.TestName,
                        ResultValue = dto.ResultText,
                        NormalRange = dto.NormalRange
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Notification] Failed to send AbnormalLabResult for order {Id}", order.Id);
                }
            }
            return _mapper.Map<LabResultResultDto>(result);
        }
    }
}
