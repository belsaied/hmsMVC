using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.BillingModule;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.DAL.Models.PatientModule;
using Microsoft.Extensions.Logging;
using HMS.BLL.ServicesAbstraction.Contracts.BillingService;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.BillingModule;
using HMS.BLL.Services.Specifications.PatientModule;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.BillingModule.Requests;
using HMS.BLL.Shared.Dtos.BillingModule.Results;
using HMS.BLL.Shared.Dtos.NotificationDtos.Events;
using HMS.BLL.Shared.Parameters;

namespace Services.Implementations.BillingModule
{
    public sealed class InvoiceService (IUnitOfWork _unitOfWork
        , IMapper _mapper , IInvoicePdfGenerator _pdfGenerator,ILogger<InvoiceService> _logger ,INotificationService _notificationService) : IInvoiceService
    {
        

        public async Task<InvoiceResultDto> CreateInvoiceAsync(CreateInvoiceRequest request)
        {
            // Validate patient exists and is Active
            var patientRepo = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepo.GetByIdAsync(request.PatientId)
                          ?? throw new PatientNotFoundException(request.PatientId);

            if (patient.Status != HMS.DAL.Models.Enums.PatientEnums.PatientStatus.Active)
                throw new BusinessRuleException("Invoices can only be created for Active patients.");

            var invoiceRepo = _unitOfWork.GetRepository<Invoice, Guid>();

            if (request.AppointmentId.HasValue)
            {
                var existingSpec = new InvoicesByPatientSpecification(request.PatientId);
                var allPatientInvoices = await invoiceRepo.GetAllAsync(existingSpec);
                var duplicate = allPatientInvoices.Any(i =>
                    i.AppointmentId == request.AppointmentId &&
                    i.Status != InvoiceStatus.Cancelled);

                if (duplicate)
                    throw new BusinessRuleException(
                        $"An active invoice already exists for appointment {request.AppointmentId}.");
            }

            var invoice = new Invoice
            {
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId,
                Notes = request.Notes,
                DueDate = request.DueDate
            };

            invoice.InvoiceNumber = await GenerateInvoiceNumberAsync(invoiceRepo);

            // Add initial line items if supplied
            if (request.LineItems is { Count: > 0 })
            {
                foreach (var itemReq in request.LineItems)
                {
                    var li = MapToLineItem(itemReq);
                    li.InvoiceId = invoice.Id;
                    invoice.LineItems.Add(li);
                }
            }

            invoice.RecalculateFinancials();

            await invoiceRepo.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return await BuildDetailDtoAsync(invoice, patient.FirstName + " " + patient.LastName);
        }

        public async Task<InvoiceResultDto> GetInvoiceByIdAsync(Guid id)
        {
            var spec = new InvoiceWithDetailsSpecification(id);
            var invoice = await _unitOfWork.GetRepository<Invoice, Guid>().GetByIdAsync(spec)
                          ?? throw new InvoiceNotFoundException(id);

            var patient = await _unitOfWork.GetRepository<Patient, int>().GetByIdAsync(invoice.PatientId);
            return await BuildDetailDtoAsync(invoice, patient?.FirstName + " " + patient?.LastName ?? string.Empty);
        }

        public async Task<IEnumerable<InvoiceSummaryResultDto>> GetInvoicesByPatientAsync(int patientId)
        {
            var patient = await _unitOfWork.GetRepository<Patient, int>().GetByIdAsync(patientId)
                          ?? throw new PatientNotFoundException(patientId);

            var spec = new InvoicesByPatientSpecification(patientId);
            var invoices = await _unitOfWork.GetRepository<Invoice, Guid>().GetAllAsync(spec);
            var patientName = $"{patient.FirstName} {patient.LastName}";

            return invoices.Select(i =>
            {
                var dto = _mapper.Map<InvoiceSummaryResultDto>(i);
                return dto with { PatientName = patientName };
            });
        }

        public async Task<PaginatedResult<InvoiceSummaryResultDto>> GetAllInvoicesAsync(InvoiceFilterParameters filters)
        {
            var repo = _unitOfWork.GetRepository<Invoice, Guid>();
            var invoices = await repo.GetAllAsync(new InvoiceListSpecification(filters));
            var count = await repo.CountAsync(new InvoiceCountSpecification(filters));

            // Batch patient name resolution to avoid N+1
            var patientIds = invoices.Select(i => i.PatientId).Distinct().ToList();
            var patients = await _unitOfWork.GetRepository<Patient, int>()
                                            .GetAllAsync(new PatientsByIdsSpecification(patientIds));
            var nameMap = patients.ToDictionary(p => p.Id, p => $"{p.FirstName} {p.LastName}");

            var items = invoices.Select(i =>
            {
                var dto = _mapper.Map<InvoiceSummaryResultDto>(i);
                return dto with { PatientName = nameMap.GetValueOrDefault(i.PatientId, string.Empty) };
            });

            return new PaginatedResult<InvoiceSummaryResultDto>(filters.PageIndex, filters.PageSize, count, items);
        }


        public async Task<InvoiceResultDto> AddLineItemAsync(Guid invoiceId, AddLineItemRequest request)
        {
            var invoice = await LoadInvoiceWithDetailsAsync(invoiceId);

            if (invoice.Status != InvoiceStatus.Draft)
                throw new InvalidInvoiceStatusTransitionException(invoice.Status.ToString(),
                    "AddLineItem — only Draft invoices accept new line items");

            var li = MapToLineItem(request);
            li.InvoiceId = invoiceId;
            invoice.LineItems.Add(li);
            invoice.RecalculateFinancials();

            _unitOfWork.GetRepository<Invoice, Guid>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            var patient = await _unitOfWork.GetRepository<Patient, int>().GetByIdAsync(invoice.PatientId);
            return await BuildDetailDtoAsync(invoice, $"{patient?.FirstName} {patient?.LastName}");
        }

        public async Task RemoveLineItemAsync(Guid invoiceId, Guid lineItemId)
        {
            var invoice = await LoadInvoiceWithDetailsAsync(invoiceId);

            if (invoice.Status != InvoiceStatus.Draft)
                throw new InvalidInvoiceStatusTransitionException(invoice.Status.ToString(),
                    "RemoveLineItem — only Draft invoices can be modified");

            var item = invoice.LineItems.FirstOrDefault(li => li.Id == lineItemId)
                       ?? throw new NotFoundException($"Line item '{lineItemId}' not found on invoice '{invoiceId}'.");

            invoice.LineItems.Remove(item);
            invoice.RecalculateFinancials();

            _unitOfWork.GetRepository<Invoice, Guid>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<InvoiceResultDto> IssueInvoiceAsync(Guid invoiceId, IssueInvoiceRequest request)
        {
            var invoice = await LoadInvoiceWithDetailsAsync(invoiceId);

            if (invoice.Status != InvoiceStatus.Draft)
                throw new InvalidInvoiceStatusTransitionException(invoice.Status.ToString(),
                    InvoiceStatus.Issued.ToString());

            invoice.DiscountAmount = request.DiscountAmount;
            invoice.DiscountPercent = request.DiscountPercent;
            invoice.TaxPercent = request.TaxPercent;
            if (request.Notes is not null)
                invoice.Notes = request.Notes;

            invoice.RecalculateFinancials();
            invoice.Status = InvoiceStatus.Issued;
            invoice.IssuedAt = DateTimeOffset.UtcNow;

            invoice.DueDate ??= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

            _unitOfWork.GetRepository<Invoice, Guid>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            var patient = await _unitOfWork.GetRepository<Patient, int>()
                .GetByIdAsync(invoice.PatientId)
                ?? throw new PatientNotFoundException(invoice.PatientId);
            try
            {
                byte[]? pdfBytes = null;
                try { pdfBytes = _pdfGenerator.Generate(invoice, patient?.FirstName + " " + patient?.LastName, patient?.Email ?? ""); }
                catch {  }

                await _notificationService.SendInvoiceIssuedAsync(new InvoiceNotificationEvent
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    PatientId = invoice.PatientId,
                    PatientEmail = patient?.Email ?? string.Empty,
                    PatientName = $"{patient?.FirstName} {patient?.LastName}",
                    TotalAmount = invoice.TotalAmount,
                    DueDate = invoice.DueDate,
                    PdfBytes = pdfBytes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notification] Failed to send InvoiceIssued for {Id}", invoice.Id);
            }
            return await BuildDetailDtoAsync(invoice, $"{patient?.FirstName} {patient?.LastName}");
        }

        public async Task CancelInvoiceAsync(Guid invoiceId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleException("A cancellation reason is required.");

            var invoice = await _unitOfWork.GetRepository<Invoice, Guid>().GetByIdAsync(invoiceId)
                          ?? throw new InvoiceNotFoundException(invoiceId);

            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new InvalidInvoiceStatusTransitionException(invoice.Status.ToString(),
                    InvoiceStatus.Cancelled.ToString());

            if (invoice.Status == InvoiceStatus.Paid)
                throw new InvoiceAlreadyPaidException(invoiceId);

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.Notes = string.IsNullOrWhiteSpace(invoice.Notes)
                ? $"Cancelled: {reason}"
                : $"{invoice.Notes} | Cancelled: {reason}";

            _unitOfWork.GetRepository<Invoice, Guid>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId)
        {
            var invoice = await LoadInvoiceWithDetailsAsync(invoiceId);
            var patient = await _unitOfWork.GetRepository<Patient, int>().GetByIdAsync(invoice.PatientId);
            var patientName = patient is not null ? $"{patient.FirstName} {patient.LastName}" : "Unknown";

            return _pdfGenerator.Generate(invoice, patientName, patient?.Email ?? string.Empty);
        }

        private async Task<Invoice> LoadInvoiceWithDetailsAsync(Guid invoiceId)
        {
            var spec = new InvoiceWithDetailsSpecification(invoiceId);
            return await _unitOfWork.GetRepository<Invoice, Guid>().GetByIdAsync(spec)
                   ?? throw new InvoiceNotFoundException(invoiceId);
        }

        private static InvoiceLineItem MapToLineItem(AddLineItemRequest r)
        {
            var li = new InvoiceLineItem
            {
                Description = r.Description,
                LineItemType = r.LineItemType,
                ReferenceId = r.ReferenceId,
                Quantity = r.Quantity,
                UnitPrice = r.UnitPrice
            };
            li.RecalculateTotal(); 
            return li;
        }

        private Task<InvoiceResultDto> BuildDetailDtoAsync(Invoice invoice, string patientName)
        {
            var dto = _mapper.Map<InvoiceResultDto>(invoice);
            return Task.FromResult(dto with { PatientName = patientName });
        }

        private static async Task<string> GenerateInvoiceNumberAsync(IGenericRepository<Invoice, Guid> repo)
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"INV-{today}-";

            var count = await repo.CountAsync(new InvoicesByDatePrefixSpecification(today));
            return $"{prefix}{(count + 1):D6}";
        }
    }
}
