using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.MedicalRecordModule
{
    public class LabResult :BaseEntity<int>
    {
        public int LabOrderId { get; set; }   //FK With LabOrder (one to one)
        public string ResultText { get; set; } = string.Empty;
        public bool IsAbnormal { get; set; } = false;
        public string? NormalRange { get; set; }
        public DateTime PerformedAt { get; set; }
        public DateTime ReportedAt { get; set; }
        public string PerformedBy { get; set; } = string.Empty;

        #region Navigation Property
        public LabOrder LabOrder { get; set; } = null!;

        #endregion
    }
}
