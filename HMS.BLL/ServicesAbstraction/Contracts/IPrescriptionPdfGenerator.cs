using HMS.DAL.Models.MedicalRecordModule;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IPrescriptionPdfGenerator
    {
        byte[] Generate(Prescription prescription, string patientName, string doctorName, string doctorSpecialization);

    }
}
