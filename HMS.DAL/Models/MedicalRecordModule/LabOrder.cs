using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.MedicalRecordEnums;
using HMS.DAL.Models.PatientModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.MedicalRecordModule
{
    public class LabOrder :BaseEntity<int>
    {
        //FK
        public int MedicalRecordId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        //Test Info
        public string TestName { get; set; } = string.Empty;
        public string? TestCode { get; set; }
        //Priorty & Status
        public LabOrderPriority Priority { get; set; } = LabOrderPriority.Routine;
        public LabOrderStatus Status { get; set; } = LabOrderStatus.Pending;
        //Audit
        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        #region Navigation Property

        public MedicalRecord MedicalRecord { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public LabResult? Result { get; set; }

        #endregion


    }
}
