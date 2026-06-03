using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.DAL.Models.Enums.PatientEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Models.DoctorModule
{
    public class Doctor :BaseEntity<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty; //رقم الترخيص 
        public string Email { get; set; }=string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public int? YearsOfExperience { get; set; }
        public decimal ConsultationFee { get; set; } //رسوم الاستشارة
        public string? PictureUrl { get; set; }
        public string? Bio { get; set; }
        public DoctorStatus Status { get; set; } = DoctorStatus.Active;
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year -
            (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);


        #region Navigation property
        public Department Department { get; set; } = null!;
        public ICollection<DoctorQualification> Qualifications { get; set; } = new HashSet<DoctorQualification>();
        public ICollection<DoctorSchedule> AvailabilitySchedules { get; set; } = new HashSet<DoctorSchedule>();
        #endregion

    }
}
