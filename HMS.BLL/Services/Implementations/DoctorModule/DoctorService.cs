using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.DoctorEnums;
using Services.Abstraction.Contracts;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.DoctorModule;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Common;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Implementations.DoctorModule
{
    public class DoctorService(IUnitOfWork _unitOfWork 
        , IMapper _mapper , ICacheService _cacheService , IEmailService _emailService) : IDoctorService
    {
        public async Task<DoctorResultDto> RegisterDoctorAsync(CreateDoctorDto dto)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var deptRepo = _unitOfWork.GetRepository<Department, int>();

            // Step 1: Validate department exists
            var department = await deptRepo.GetByIdAsync(dto.DepartmentId);
            if (department is null)
                throw new DepartmentNotFoundException(dto.DepartmentId);

            // Step 2: Check LicenseNumber uniqueness with a targeted DB query
            // — no full table load, uses the existing unique index
            var licenseExists = await doctorRepo.CountAsync(
                new DoctorByLicenseSpecification(dto.LicenseNumber)) > 0;
            if (licenseExists)
                throw new DuplicateLicenseNumberException(dto.LicenseNumber);

            // Step 3: Check Email uniqueness with a targeted DB query
            var emailExists = await doctorRepo.CountAsync(
                new DoctorByEmailSpecification(dto.Email)) > 0;
            if (emailExists)
                throw new DuplicateDoctorEmailException(dto.Email);

            var nationalIdExists = await doctorRepo.CountAsync(
                new DoctorByNationalIdSpecification(dto.NationalId)) > 0;
            if (nationalIdExists)
                throw new ConflictException($"A doctor with NationalId '{dto.NationalId}' already exists.");
            // Step 4: Map and save
            var doctor = _mapper.Map<Doctor>(dto);
            doctor.JoinDate = DateTime.UtcNow;
            doctor.Status = DoctorStatus.Active;

            await doctorRepo.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            // Cache invalidation
            await _cacheService.RemoveAsync(CacheKeys.DoctorsByDepartment(dto.DepartmentId));

            // Reload with Department for mapping
            var saved = await doctorRepo.GetByIdAsync(
                new DoctorWithDetailsSpecification(doctor.Id));
            // Notify the doctor with their ID
            try
            {
                await _emailService.SendDoctorWelcomeEmailAsync(
                    doctor.Email,
                    $"{doctor.FirstName} {doctor.LastName}",
                    saved!.Id);
            }
            catch
            {
                
            }

            return _mapper.Map<DoctorResultDto>(saved);
            
        }
        public async Task<DoctorResultDto> GetDoctorByIdAsync(int id)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(new DoctorWithDetailsSpecification(id));

            if (doctor is null)
                throw new DoctorNotFoundException(id);

            return _mapper.Map<DoctorResultDto>(doctor);

        }
        public async Task<DoctorResultDto> UpdateDoctorAsync(int id, UpdateDoctorDto dto)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(new DoctorWithDetailsSpecification(id));
            
            if(doctor is null)
                throw new DoctorNotFoundException(id);

            if (!string.IsNullOrEmpty(dto.FirstName)) doctor.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) doctor.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Phone)) doctor.Phone = dto.Phone;
            if (!string.IsNullOrEmpty(dto.Email)) doctor.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Specialization)) doctor.Specialization = dto.Specialization;
            if (!string.IsNullOrEmpty(dto.Bio)) doctor.Bio = dto.Bio;
            if (!string.IsNullOrEmpty(dto.PictureUrl)) doctor.PictureUrl = dto.PictureUrl;
            if (dto.DepartmentId.HasValue) doctor.DepartmentId = dto.DepartmentId.Value;
            if (dto.YearsOfExperience.HasValue) doctor.YearsOfExperience = dto.YearsOfExperience.Value;
            if (dto.ConsultationFee.HasValue) doctor.ConsultationFee = dto.ConsultationFee.Value;
            if (dto.Status.HasValue) doctor.Status = dto.Status.Value;

            doctorRepo.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            // Cache Invalidation
            await _cacheService.RemoveAsync(CacheKeys.Doctor(id));
            await _cacheService.RemoveAsync(CacheKeys.DoctorDetails(id));
            await _cacheService.RemoveAsync(CacheKeys.DoctorsByDepartment(doctor.DepartmentId));
            var updated = await doctorRepo.GetByIdAsync(new DoctorWithDetailsSpecification(id));
            return _mapper.Map<DoctorResultDto>(updated!);

        }
        public async Task<bool> DeactivateDoctorAsync(int id)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(id);
            if (doctor is null)
                throw new DoctorNotFoundException(id);
            if (doctor.Status == DoctorStatus.Inactive)
                throw new BusinessRuleException("Doctor is already inactive.");

            doctor.Status =DoctorStatus.Inactive;
            doctorRepo.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            // Cache Invalidation
            await _cacheService.RemoveAsync(CacheKeys.Doctor(id));
            await _cacheService.RemoveAsync(CacheKeys.DoctorDetails(id));
            await _cacheService.RemoveAsync(CacheKeys.DoctorsByDepartment(doctor.DepartmentId));
            return true; 
        }
        public async Task<PaginatedResult<DoctorResultDto>> GetAllDoctorsAsync(DoctorSpecificationParameters parameters)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();

            var doctors = await doctorRepo.GetAllAsync(new DoctorListSpecification(parameters));
            var totalCount = await doctorRepo.CountAsync(new DoctorCountSpecification(parameters));
            var doctorResult = _mapper.Map<IEnumerable<DoctorResultDto>>(doctors);

            return new PaginatedResult<DoctorResultDto>(
                parameters.PageIndex,
                parameters.PageSize,
                totalCount,
                doctorResult );

        }
        public async Task<DoctorWithDetailsResultDto> GetDoctorWithDetailsAsync(int id)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(new DoctorWithDetailsSpecification(id));

            if (doctor is null)
                throw new DoctorNotFoundException(id);

            return _mapper.Map<DoctorWithDetailsResultDto>(doctor);

        }
        public async Task<QualificationResultDto> AddQualificationAsync(int doctorId, CreateQualificationDto dto)
        {

            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor = await doctorRepo.GetByIdAsync(doctorId);

            if (doctor is null)
                throw new DoctorNotFoundException(doctorId);

            var qualification = _mapper.Map<DoctorQualification>(dto);
            qualification.DoctorId = doctorId;

            var qualRepo = _unitOfWork.GetRepository<DoctorQualification, int>();
            await qualRepo.AddAsync(qualification);
            await _unitOfWork.SaveChangesAsync();
            // cahce
            await _cacheService.RemoveAsync(CacheKeys.DoctorDetails(doctorId));
            return _mapper.Map<QualificationResultDto>(qualification);

        }
        public async Task<bool> RemoveQualificationAsync(int doctorId, int qualId)
        {

            var qualRepo = _unitOfWork.GetRepository<DoctorQualification, int>();
            var qualification = await qualRepo.GetByIdAsync(qualId);

            if(qualification is null)
                throw new QualificationNotFoundException(qualId);

            if (qualification.DoctorId != doctorId)
                throw new BusinessRuleException($"Qualification with ID {qualId} does not belong to doctor {doctorId}");


            qualRepo.Delete(qualification);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveAsync(CacheKeys.DoctorDetails(doctorId));
            return true;

        }
        public async Task<ScheduleResultDto> SetScheduleAsync(int doctorId, CreateScheduleDto dto)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctor =await doctorRepo.GetByIdAsync(doctorId);

            if (doctor is null)
                throw new DoctorNotFoundException(doctorId);

            // Business Rule 1: EndTime > StartTime
            if (dto.EndTime <= dto.StartTime)
                throw new InvalidScheduleTimeException();

            var scheduleRepo = _unitOfWork.GetRepository<DoctorSchedule, int>();
            var existing = await scheduleRepo.GetAllAsync(new DoctorScheduleSpecification (doctorId));


            //مفيش جدولين لنفس اليوم

            if (existing.Any(s => s.DayOfWeek == dto.DayOfWeek))
                throw new OverlappingScheduleException(dto.DayOfWeek.ToString());

            var schedule = _mapper.Map<DoctorSchedule>(dto);
            schedule.DoctorId = doctorId;


            await scheduleRepo.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveAsync(CacheKeys.DoctorSchedule(doctorId));
            await _cacheService.RemoveAsync(CacheKeys.DoctorDetails(doctorId));
            return _mapper.Map<ScheduleResultDto>(schedule);

        }
        public async Task<IEnumerable<ScheduleResultDto>> GetScheduleAsync(int doctorId)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            if (await doctorRepo.GetByIdAsync(doctorId) is null)
                throw new DoctorNotFoundException(doctorId);


            var scheduleRepo = _unitOfWork.GetRepository<DoctorSchedule, int>();
            var schedules = await scheduleRepo.GetAllAsync(new DoctorScheduleSpecification(doctorId));

            return _mapper.Map<IEnumerable<ScheduleResultDto>>(schedules);

        }
        public async Task<IEnumerable<DoctorResultDto>> GetDoctorByDepartmentAsync(int departmentId)
        {
            var deptRepo = _unitOfWork.GetRepository<Department, int>();
            if(await deptRepo.GetByIdAsync(departmentId)is null)
                throw new DepartmentNotFoundException(departmentId);

            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctors = await doctorRepo.GetAllAsync(new DoctorByDepartmentSpecification(departmentId));

            return _mapper.Map<IEnumerable<DoctorResultDto>>(doctors);

        }
        public async Task<IEnumerable<DoctorResultDto>> GetAvailableDoctorAsync(DateTime date)
        {
            var doctorRepo = _unitOfWork.GetRepository<Doctor, int>();
            var doctors = await doctorRepo.GetAllAsync(new DoctorAvailableOnDateSpecification(date));

            return _mapper.Map<IEnumerable<DoctorResultDto>>(doctors);

        }

        
    }
}
