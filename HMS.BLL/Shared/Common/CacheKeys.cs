namespace HMS.BLL.Shared.Common
{
    public static class CacheKeys
    {
        // ── Patient Module ─────────────────────────────────────────────
        public static string Patient(int id)
            => $"patients:id:{id}";

        public static string PatientDetails(int id)
            => $"patients:details:{id}";

        public static string PatientAllergies(int id)
            => $"patients:{id}:allergies";

        public static string PatientHistories(int id)
            => $"patients:{id}:histories";

        public static string PatientPrescriptions(int id)
            => $"patients:{id}:prescriptions";

        public static string PatientActivePrescriptions(int id)
            => $"patients:{id}:prescriptions:active";

        public static string PatientVitals(int id)
            => $"patients:{id}:vitals";

        public static string PatientLabOrders(int id)
            => $"patients:{id}:laborders";

        // ── Doctor Module ──────────────────────────────────────────────
        public static string Doctor(int id)
            => $"doctors:id:{id}";

        public static string DoctorDetails(int id)
            => $"doctors:details:{id}";

        public static string DoctorSchedule(int id)
            => $"doctors:{id}:schedules";

        public static string DoctorsByDepartment(int deptId)
            => $"doctors:dept:{deptId}";

        // ── Department Module ──────────────────────────────────────────
        public const string AllDepartments = "departments:all";
        public static string Department(int id) => $"departments:id:{id}";

        // ── Appointment Module ─────────────────────────────────────────
        public static string AvailableSlots(int doctorId, DateOnly date)
            => $"slots:doctor:{doctorId}:date:{date:yyyyMMdd}";

        public static string PatientAppointments(int patientId)
            => $"appointments:patient:{patientId}";

        public static string DoctorDailyAppointments(int doctorId, DateOnly date)
            => $"appointments:doctor:{doctorId}:date:{date:yyyyMMdd}";

        // ── Ward & Bed Module ──────────────────────────────────────────
        public const string WardOccupancy = "wards:occupancy";

        public static string Ward(int id) => $"wards:id:{id}";
        public static string WardRooms(int id) => $"wards:{id}:rooms";
        public static string RoomBeds(int id) => $"rooms:{id}:beds";

        public static string AvailableBeds(string? wardType, string? bedType)
            => $"beds:available:{wardType ?? "all"}:{bedType ?? "all"}";

        // ── Medical Records Module ─────────────────────────────────────
        public static string MedicalRecord(int id)
            => $"medicalrecords:id:{id}";

        public static string PatientMedicalRecords(int patientId, int pageIndex, int pageSize)
            => $"medicalrecords:patient:{patientId}:page:{pageIndex}:size:{pageSize}";

        public static string DoctorMedicalRecords(int doctorId, int pageIndex, int pageSize)
            => $"medicalrecords:doctor:{doctorId}:page:{pageIndex}:size:{pageSize}";

        // ── Billing Module ─────────────────────────────────────────────
        public static string Invoice(Guid id)
            => $"invoices:id:{id}";

        public static string PatientInvoices(int patientId)
            => $"invoices:patient:{patientId}";

        public const string OutstandingInvoicesReport = "reports:outstanding";
    }
}
