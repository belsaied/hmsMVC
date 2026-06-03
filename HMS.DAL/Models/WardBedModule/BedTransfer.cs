using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.WardBedModule
{
    public class BedTransfer :BaseEntity<int>
    {
        public int AdmissionId { get; set; }   // FK -> Admissions (Cascade)
        public int FromBedId { get; set; }     // FK -> Beds (Restrict)
        public int ToBedId { get; set; }       // FK -> Beds (Restrict)
        public DateTime TransferredAt { get; set; }     // UTC timestamp
        public string Reason { get; set; } = string.Empty;    // Required, max 500
        public string TransferredBy { get; set; } = string.Empty; // Staff name
       
        #region Navigation Property
        public Admission Admission { get; set; } = null!;
        public Bed FromBed { get; set; } = null!;
        public Bed ToBed { get; set; } = null!; 
        #endregion
    }
}
