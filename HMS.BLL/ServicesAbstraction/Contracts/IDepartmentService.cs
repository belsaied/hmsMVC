using HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IDepartmentService
    {
        Task<DepartmentResultDto> CreateDepartmentAsync(CreateDepartmentDto dto);
        Task<IEnumerable<DepartmentResultDto>> GetAllDepartmentAsync();
        Task<DepartmentResultDto> GetDepartmentByIdAsync(int id);
        Task<DepartmentResultDto> UpadateDepartmentAsync(int id, UpdateDepartmentDto dto);

    }
}
