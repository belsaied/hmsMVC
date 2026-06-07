using HMS.DAL.Models.Enums.BillingEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.BillingModule
{
    public class CreateInvoiceViewModel
    {
        [Required]
        [Display(Name = "Patient ID")]
        public int PatientId { get; set; }

        [Display(Name = "Appointment ID (optional)")]
        public int? AppointmentId { get; set; }

        [Display(Name = "Due Date")]
        public DateOnly? DueDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<LineItemEntryViewModel> LineItems { get; set; } = [];
    }

    public class LineItemEntryViewModel
    {
        [Required, MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public LineItemType LineItemType { get; set; }

        public string? ReferenceId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        public bool IsAutoAdded { get; set; }
    }
}
