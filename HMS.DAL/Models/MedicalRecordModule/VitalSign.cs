using HMS.DAL.Models.PatientModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.MedicalRecordModule
{
    public class VitalSign :BaseEntity<int>
    {
        //FK
        public int MedicalRecordId { get; set; }
        public int PatientId { get; set; }

        //Vitals
        public decimal? Temperature { get; set; }
        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public decimal? Weight { get; set; } 
        public decimal? Height { get; set; }

        //Audit 
        public DateTime RecordedAt { get; set; } =DateTime.UtcNow;
        public string RecordedBy { get; set; } = string.Empty;

        #region Navigation Property

        public MedicalRecord MedicalRecord { get; set; } = null!;
        public Patient Patient { get; set; } = null!;

        #endregion

    }
}
