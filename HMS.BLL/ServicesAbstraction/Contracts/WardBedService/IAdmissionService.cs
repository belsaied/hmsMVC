using HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts.WardBedService
{
    public interface IAdmissionService
    {
        Task<AdmissionResultDto> AdmitPatientAsync(CreateAdmissionDto dto);
        Task<AdmissionResultDto> GetAdmissionByIdAsync(int admissionId);
        Task<IEnumerable<AdmissionResultDto>> GetPatientAdmissionHistoryAsync(int patientId);
        Task<IEnumerable<AdmissionResultDto>> GetActiveAdmissionsAsync();
        Task<AdmissionResultDto> DischargePatientAsync(int admissionId, DischargeDto dto);
        Task<AdmissionResultDto> TransferPatientAsync(int admissionId, TransferBedDto dto);

    }
}
