namespace HMS.BLL.Services.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object id)
            : base($"{entityName} with Id: {id} is not found") { }

        public NotFoundException(string message)
            : base(message) { }
    }

    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message)
            : base(message) { }
    }
    public class ConflictException : Exception
    {
        public ConflictException(string message) 
            :base(message)  { }
    }
    public class ValidationException : Exception
    {
        public IEnumerable<string> Errors { get; set; } = [];

        public ValidationException(IEnumerable<string> errors) : base("Validation Failed")
        {
            Errors = errors;
        }
    }
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
    public sealed class AccountLockedException : Exception
    {
        public AccountLockedException(DateTime lockoutEnd)
    : base($"Account locked until {lockoutEnd:u}") { }
    }
    public sealed class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
    public sealed class EmailNotVerifiedException : Exception
    {
        public EmailNotVerifiedException()
            : base("Please verify your email address before logging in.") { }
    }
    #region Patient Module
    public sealed class PatientNotFoundException : NotFoundException
    {
        public PatientNotFoundException(int id)
            : base("Patient", id) { }
    }
    #endregion

    #region Doctor Module
    public sealed class DoctorNotFoundException : NotFoundException
    {
        public DoctorNotFoundException(int id)
            :base("Doctor",id) { }
    }

    public sealed class DepartmentNotFoundException : NotFoundException
    {
        public DepartmentNotFoundException(int id)
            :base("Department",id) { }
    }
    public sealed class QualificationNotFoundException : NotFoundException
    {
        public QualificationNotFoundException(int id)
            : base("Qualification", id) { }
    }

    public sealed class DuplicateLicenseNumberException : ConflictException
    {
        public DuplicateLicenseNumberException(string licenseNumber)
            : base($"A doctor with LicenseNumber '{licenseNumber}' already exists") { }
    }

    public sealed class DuplicateDoctorEmailException : ConflictException
    {
        public DuplicateDoctorEmailException(string email)
            : base($"A doctor with Email '{email}' already exists") { }
    }

    public sealed class InvalidScheduleTimeException : ValidationException
    {
        public InvalidScheduleTimeException()
            : base(new[] { "EndTime must be strictly greater than StartTime" }) { }
    }

    public sealed class OverlappingScheduleException : ValidationException
    {
        public OverlappingScheduleException(string day)
            : base(new[] { $"Doctor already has a schedule for {day}. Remove it first before setting a new one" }) { }
    }
    #endregion

    #region Appointment Module
    public sealed class AppointmentNotFoundException : NotFoundException
    {
        public AppointmentNotFoundException(int id)
            : base("Appointment", id) { }
    }
    #endregion

    #region Medical Record Module

    public sealed class MedicalRecordNotFoundException : NotFoundException
    {
        public MedicalRecordNotFoundException(int id)
            : base("MedicalRecord", id) { }
    }

    public sealed class LabOrderNotFoundException : NotFoundException
    {
        public LabOrderNotFoundException(int id)
            : base("LabOrder", id) { }
    }

    public sealed class PrescriptionNotFoundException : NotFoundException
    {
        public PrescriptionNotFoundException(int id)
            : base("Prescription", id) { }
    }

    #endregion

    #region WardBedModule
    public class WardNotFoundException(int id)
        : NotFoundException($"Ward with ID {id} was not found.");

    public class RoomNotFoundException(int id)
        : NotFoundException($"Room with ID {id} was not found.");

    public class BedNotFoundException(int id)
        : NotFoundException($"Bed with ID {id} was not found.");

    public class AdmissionNotFoundException(int id)
        : NotFoundException($"Admission with ID {id} was not found.");

    public class DuplicateWardNameException(string name)
        : ConflictException($"A ward with the name '{name}' already exists.");
    #endregion

    #region BillingModule
    // ── Inherits NotFoundException (→ HTTP 404) ───────────────────────

    public sealed class InvoiceNotFoundException : NotFoundException
    {
        public InvoiceNotFoundException(Guid id)
            : base($"Invoice with Id '{id}' was not found.")
        {
        }
    }

    public sealed class PaymentNotFoundException : NotFoundException
    {
        public PaymentNotFoundException(Guid id)
            : base($"Payment with Id '{id}' was not found.")
        {
        }
    }

    public sealed class InsuranceClaimNotFoundException : NotFoundException
    {
        public InsuranceClaimNotFoundException(Guid invoiceId)
            : base($"No insurance claim found for invoice '{invoiceId}'.")
        {
        }
    }

    // ── Inherits BusinessRuleException (→ HTTP 422) ───────────────────

    public sealed class InvalidInvoiceStatusTransitionException : BusinessRuleException
    {
        public InvalidInvoiceStatusTransitionException(string from, string to)
            : base($"Cannot transition invoice status from '{from}' to '{to}'.")
        {
        }
    }

    public sealed class InvoiceAlreadyPaidException : BusinessRuleException
    {
        public InvoiceAlreadyPaidException(Guid invoiceId)
            : base($"Invoice '{invoiceId}' is already fully paid. Cancellation is not allowed — initiate a refund instead.")
        {
        }
    }

    public sealed class PaymentExceedsBalanceException : BusinessRuleException
    {
        public PaymentExceedsBalanceException(decimal requested, decimal outstanding)
            : base($"Payment amount {requested:N2} exceeds the outstanding balance of {outstanding:N2}.")
        {
        }
    }

    public sealed class DraftInvoicePaymentException : BusinessRuleException
    {
        public DraftInvoicePaymentException(Guid invoiceId)
            : base($"Cannot record a payment against Draft invoice '{invoiceId}'. Issue the invoice first.")
        {
        }
    }

    public sealed class ClaimNotRejectedException : BusinessRuleException
    {
        public ClaimNotRejectedException(Guid claimId)
            : base($"Claim '{claimId}' cannot be resubmitted because its current status is not 'Rejected'.")
        {
        }
    }

    // ── Inherits ConflictException (→ HTTP 409) ───────────────────────

    public sealed class DuplicateInsuranceClaimException : ConflictException
    {
        public DuplicateInsuranceClaimException(Guid invoiceId)
            : base($"An active insurance claim already exists for invoice '{invoiceId}'.")
        {
        }
    }
    #endregion

    #region Notification Module
    public sealed class NotificationNotFoundException : NotFoundException
    {
        public NotificationNotFoundException(Guid id)
            : base($"Notification with Id '{id}' was not found.") { }
    }

    public sealed class NotificationPreferenceNotFoundException : NotFoundException
    {
        public NotificationPreferenceNotFoundException(string userId, string type, string channel)
            : base($"Notification preference for user '{userId}', type '{type}', channel '{channel}' was not found.") { }
    }

    public sealed class TemplateNotFoundException : NotFoundException
    {
        public TemplateNotFoundException(string type, string channel)
            : base($"No active notification template found for type '{type}' on channel '{channel}'.") { }
    }

    public sealed class NotificationChannelUnavailableException : BusinessRuleException
    {
        public NotificationChannelUnavailableException(string channel)
            : base($"Recipient has no valid contact information to receive notifications via '{channel}'.") { }
    }
    #endregion

}
