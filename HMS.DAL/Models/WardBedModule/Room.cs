using HMS.DAL.Models.Enums.WardBedEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.WardBedModule
{
    public class Room :BaseEntity<int>
    {
        public int WardId { get; set; }                    
        public string RoomNumber { get; set; } = string.Empty;  
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }                  
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }                 

        #region Navigation Property
        public Ward Ward { get; set; } = null!;
        public ICollection<Bed> Beds { get; set; } = new HashSet<Bed>();
        #endregion
    }
}
