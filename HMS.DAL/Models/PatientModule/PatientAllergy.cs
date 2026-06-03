using HMS.DAL.Models.Enums.PatientEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.PatientModule
{
    public class PatientAllergy :BaseEntity<int>
    {
        public int PatientId { get; set; }
        public AllergyType  Type { get; set; }
        public Severity Severity { get; set; }
        public string Description { get; set; } = default!;
        public DateTime RecordDate { get; set; } = DateTime.Now;
        public string RecordedBy { get; set; } = default!;

        #region Navigation Property
        public Patient Patient { get; set; } = null!;
        #endregion
    }
}
