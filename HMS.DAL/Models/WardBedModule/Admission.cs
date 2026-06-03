using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.DAL.Models.PatientModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.WardBedModule
{
    public class Admission :BaseEntity<int>
    {
        public int PatientId { get; set; }                        
        public int BedId { get; set; }                            
        public int AdmittingDoctorId { get; set; }                

        public DateTime AdmissionDate { get; set; }               
        public DateOnly? ExpectedDischargeDate { get; set; }      
        public DateTime? ActualDischargeDate { get; set; }        
        public string AdmissionReason { get; set; } = string.Empty;  
        public string? DischargeSummary { get; set; }
        public AdmissionStatus Status { get; set; } = AdmissionStatus.Active;              
        public string AdmittedBy { get; set; } = string.Empty;


        #region Navigation Property
        public Patient Patient { get; set; } = null!;
        public Bed Bed { get; set; } = null!;
        public Doctor AdmittingDoctor { get; set; } = null!;
        public ICollection<BedTransfer> Transfers { get; set; } = new List<BedTransfer>();
        #endregion
    }
}
