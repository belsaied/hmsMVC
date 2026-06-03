using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.MedicalRecordEnums;
using HMS.DAL.Models.PatientModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.MedicalRecordModule
{
    public class Prescription :BaseEntity<int>
    {
        // FK
        public int MedicalRecordId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        
        //Medication Info
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        public bool IsControlledSubstance { get; set; }=false;

        //Status & Dates
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;
        public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;
        public DateOnly ExpiresAt { get; set; } //Must be in future


        #region Navigation Property
        public MedicalRecord MedicalRecord { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        #endregion


    }
}
