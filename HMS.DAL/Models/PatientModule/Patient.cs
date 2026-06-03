using HMS.DAL.Models.Enums.PatientEnums;

namespace HMS.DAL.Models.PatientModule
{
    public class Patient :BaseEntity<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; } 
        public BloodType? BloodType { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public Address Address { get; set; } = null!;
        public PatientStatus Status { get; set; } = PatientStatus.Active;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        // Computed property
        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year -
            (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

        #region Navigation Property
        public ICollection<PatientAllergy> PatientAllergies { get; set; } = new HashSet<PatientAllergy>();
        public ICollection<PatientMedicalHistory> PatientMedicalHistories { get; set; } = new HashSet<PatientMedicalHistory>();
        public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new HashSet<EmergencyContact>();
        #endregion
    }
}
