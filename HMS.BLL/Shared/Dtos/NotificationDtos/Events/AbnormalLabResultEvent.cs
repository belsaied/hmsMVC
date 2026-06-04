using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.NotificationDtos.Events
{
    public class AbnormalLabResultEvent
    {
        public int LabOrderId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty!;
        public int OrderingDoctorId { get; set; }
        public string DoctorEmail { get; set; } = string.Empty!;
        public string DoctorName { get; set; } = string.Empty!;
        public string TestName { get; set; } = string.Empty!;
        public string ResultValue { get; set; } = string.Empty!;
        public string? NormalRange { get; set; }
    }
}
