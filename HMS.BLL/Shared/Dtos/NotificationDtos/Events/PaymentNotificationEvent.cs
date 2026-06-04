using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Events
{
    public class PaymentNotificationEvent
    {
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty!;
        public int PatientId { get; set; }
        public string PatientEmail { get; set; } = string.Empty!;
        public string PatientName { get; set; } = string.Empty!;
        public decimal AmountPaid { get; set; }
        public DateTimeOffset PaidAt { get; set; }
    }
}
