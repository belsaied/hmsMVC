using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Events
{
    public class InvoiceNotificationEvent
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty!;
        public int PatientId { get; set; }
        public string PatientEmail { get; set; } = string.Empty!;
        public string PatientName { get; set; } = string.Empty!;
        public decimal TotalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateOnly? DueDate { get; set; }
        public byte[]? PdfBytes { get; set; }
    }
}
