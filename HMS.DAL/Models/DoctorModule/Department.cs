using HMS.DAL.Models.DoctorModule;

namespace HMS.DAL.Models.DoctorModule
{
    public class Department :BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? HeadDoctorId { get; set; }
        public string? PhoneExtension { get; set; }

        #region Navigation Prop
        public Doctor? HeadDoctor { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new HashSet<Doctor>();
        #endregion
    }
}
