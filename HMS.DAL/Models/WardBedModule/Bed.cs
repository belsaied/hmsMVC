using HMS.DAL.Models.Enums.WardBedEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.WardBedModule
{
    public class Bed :BaseEntity<int>
    {
        public int RoomId { get; set; }                      
        public string BedNumber { get; set; } = string.Empty; 
        public BedType BedType { get; set; }
        public BedStatus Status { get; set; } = BedStatus.Available; 
        public string? Notes { get; set; }                    

        
        #region Navigation Property
        public Room Room { get; set; } = null!;
        public  ICollection<Admission> Admissions { get; set; } = new List<Admission>();
        #endregion
    }
}
