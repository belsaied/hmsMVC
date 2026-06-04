using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;
using HMS.BLL.Shared.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IDoctorService
    {
        Task<DoctorResultDto> RegisterDoctorAsync(CreateDoctorDto dto);
        Task<DoctorResultDto> GetDoctorByIdAsync(int id);
        Task<DoctorResultDto> UpdateDoctorAsync(int id, UpdateDoctorDto dto);
        Task<bool> DeactivateDoctorAsync(int id);
        Task<PaginatedResult<DoctorResultDto>> GetAllDoctorsAsync(DoctorSpecificationParameters parameters);
        Task<DoctorWithDetailsResultDto> GetDoctorWithDetailsAsync(int id);
        Task<QualificationResultDto> AddQualificationAsync(int doctorId, CreateQualificationDto dto);
        Task<bool> RemoveQualificationAsync(int doctorId, int qualId);
        Task<ScheduleResultDto> SetScheduleAsync(int doctorId, CreateScheduleDto dto);
        Task<IEnumerable<ScheduleResultDto>> GetScheduleAsync(int doctorId);
        Task<IEnumerable<DoctorResultDto>> GetDoctorByDepartmentAsync(int departmentId);
        Task<IEnumerable<DoctorResultDto>> GetAvailableDoctorAsync(DateTime date);

    }
}
