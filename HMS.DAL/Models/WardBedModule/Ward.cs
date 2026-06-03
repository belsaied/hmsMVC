using HMS.DAL.Models.Enums.WardBedEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.WardBedModule
{
    public class Ward :BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;          
        public WardType WardType { get; set; }                    
        public int? Floor { get; set; }                           
        public string? PhoneExtension { get; set; }               
        public string? Description { get; set; }                  
        public bool IsActive { get; set; } = true;

        // Computed in service — not stored
        // TotalBeds    => sum of all Beds in all Rooms
        // OccupiedBeds => count of Beds with Status = Occupied

        #region Navigation Property
        public ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
        #endregion
    }
}
