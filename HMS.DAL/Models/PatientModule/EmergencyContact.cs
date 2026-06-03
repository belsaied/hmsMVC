using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.PatientModule
{
    public class EmergencyContact :BaseEntity<int>
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;  
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsPrimaryContact { get; set; } = false;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
     
        #region Navigation Property
        public Patient Patient { get; set; } = null!;

        #endregion
    }
}
