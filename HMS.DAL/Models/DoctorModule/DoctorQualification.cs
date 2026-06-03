namespace HMS.DAL.Models.DoctorModule
{
    public class DoctorQualification :BaseEntity<int>
    {
        public int DoctorId { get; set; }
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public int YearAwarded { get; set; }
        public string Country { get; set; } = string.Empty;

        #region Navigation Property
        public Doctor Doctor { get; set; } = null!;
        #endregion
    }
}
