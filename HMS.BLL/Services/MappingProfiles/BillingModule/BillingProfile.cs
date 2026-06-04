using AutoMapper;
using HMS.DAL.Models.BillingModule;
using HMS.BLL.Shared.Dtos.BillingModule.Results;

namespace HMS.BLL.Services.MappingProfiles.BillingModule
{
    public class BillingProfile : Profile
    {
        public BillingProfile()
        {
            // ── Invoice ───────────────────────────────────────────────────────
            CreateMap<Invoice, InvoiceResultDto>()
                .ForMember(d => d.PatientName, opt => opt.Ignore())   // resolved in service
                .ForMember(d => d.OutstandingBalance,
                    opt => opt.MapFrom(s => s.TotalAmount - s.PaidAmount))
                .ForMember(d => d.LineItems,
                    opt => opt.MapFrom(s => s.LineItems))
                .ForMember(d => d.Payments,
                    opt => opt.MapFrom(s => s.Payments));

            CreateMap<Invoice, InvoiceSummaryResultDto>()
                .ForMember(d => d.PatientName, opt => opt.Ignore())
                .ForMember(d => d.OutstandingBalance,
                    opt => opt.MapFrom(s => s.TotalAmount - s.PaidAmount));

            // ── Line Item ─────────────────────────────────────────────────────
            CreateMap<InvoiceLineItem, LineItemResultDto>()
                .ForMember(d => d.Total,
                    opt => opt.MapFrom(s => s.Quantity * s.UnitPrice));

            // ── Payment ───────────────────────────────────────────────────────
            CreateMap<Payment, PaymentResultDto>();

            // ── Insurance Claim ───────────────────────────────────────────────
            CreateMap<InsuranceClaim, ClaimResultDto>()
                .ForMember(d => d.PatientCopayment,
                    opt => opt.MapFrom(s =>
                        s.Invoice != null ? s.Invoice.TotalAmount - s.ApprovedAmount : 0m));
        }
    }
}
