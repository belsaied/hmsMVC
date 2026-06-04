using HMS.BLL.Shared.Dtos.PatientModule.AllergyDtos;
using HMS.BLL.Shared.Dtos.PatientModule.EmergencyContactsDtos;
using HMS.BLL.Shared.Dtos.PatientModule.Medical_History_Dtos;

namespace HMS.BLL.Shared.Dtos.PatientModule.PatientDtos
{
    public record PatientWithDetailsResultDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public DateTime DateOfBirth { get; init; }
        public int Age { get; init; }
        public string Gender { get; init; } = string.Empty;
        public string? BloodType { get; init; }
        public string NationalId { get; init; } = string.Empty;
        public string MedicalRecordNumber { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public AddressDto Address { get; init; } = null!;
        public string Status { get; init; } = string.Empty;
        public DateTime RegistrationDate { get; init; }
        public string? PictureUrl { get; init; }
        public IEnumerable<AllergyResultDto> Allergies { get; init; } = [];
        public IEnumerable<MedicalHistoryResultDto> MedicalHistories { get; init; } = [];
        public IEnumerable<EmergencyContactResultDto> EmergencyContacts { get; init; } = [];
    }
}
