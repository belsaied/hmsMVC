using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.DoctorModule;
using Microsoft.Extensions.Caching.Memory;
using Services.Abstraction.Contracts;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.DoctorModule;
using HMS.BLL.Shared.Common;
using HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos;

namespace HMS.BLL.Services.Implementations.DoctorModule
{
    public class DepartmentService(IUnitOfWork _unitOfWork 
        , IMapper _mapper , IMemoryCache _cache) : IDepartmentService
    {
        private static readonly MemoryCacheEntryOptions _cacheOptions =
    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        public async Task<DepartmentResultDto> CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            var deptRepo = _unitOfWork.GetRepository<Department, int>();
            
            var department = _mapper.Map<Department>(dto);
            await deptRepo.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(CacheKeys.AllDepartments);
            return _mapper.Map<DepartmentResultDto>(department);

        }

        public async Task<IEnumerable<DepartmentResultDto>> GetAllDepartmentAsync()
        {
            if (_cache.TryGetValue(CacheKeys.AllDepartments, out IEnumerable<DepartmentResultDto>? cached) && cached is not null)
                return cached;
            var deptRepo = _unitOfWork.GetRepository<Department, int>();
            var departments = await deptRepo.GetAllAsync(new AllDepartmentsSpecification());
            var result =  _mapper.Map<IEnumerable<DepartmentResultDto>>(departments);
            _cache.Set(CacheKeys.AllDepartments, result, _cacheOptions);
            return result;
        }

        public async Task<DepartmentResultDto> GetDepartmentByIdAsync(int id)
        {
            var cacheKey = CacheKeys.Department(id);
            if (_cache.TryGetValue(cacheKey, out DepartmentResultDto? cached) && cached is not null)
                return cached;
            var deptRepo = _unitOfWork.GetRepository<Department, int>();
            var departments = await deptRepo.GetByIdAsync(new DepartmentWithHeadSpecification(id));

            if (departments is null)
                throw new DepartmentNotFoundException(id);

            var result = _mapper.Map<DepartmentResultDto>(departments);
            _cache.Set(cacheKey, result, _cacheOptions);
            return result;
        }

        public async Task<DepartmentResultDto> UpadateDepartmentAsync(int id, UpdateDepartmentDto dto)
        {

            var deptRepo = _unitOfWork.GetRepository<Department, int>();
            var department = await deptRepo.GetByIdAsync(id);

            if (department is null)
                throw new DepartmentNotFoundException(id);


            if (!string.IsNullOrEmpty(dto.Name)) department.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) department.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.PhoneExtension)) department.PhoneExtension = dto.PhoneExtension;
            if (dto.HeadDoctorId.HasValue) department.HeadDoctorId = dto.HeadDoctorId.Value;

            deptRepo.Update(department);
            await _unitOfWork.SaveChangesAsync();
            // Evict both the single-item and the list
            _cache.Remove(CacheKeys.Department(id));
            _cache.Remove(CacheKeys.AllDepartments);
            var updated = await deptRepo.GetByIdAsync(new DepartmentWithHeadSpecification(id));
            return _mapper.Map<DepartmentResultDto>(updated!);


        }
    }
}
