using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.DAL.Models.WardBedModule;
using HMS.BLL.ServicesAbstraction.Contracts.WardBedService;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.WardBedModule;
using HMS.BLL.Shared.Dtos.WardBedModule.RoomsDtos;
using HMS.BLL.Shared.Dtos.WardBedModule.WardDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Implementations.WardBedModule
{
    public class WardService(IUnitOfWork _unitOfWork, IMapper _mapper) : IWardService
    {
        public async Task<WardResultDto> CreateWardAsync(CreateWardDto dto)
        {
            var repo = _unitOfWork.GetRepository<Ward, int>();

            // BR: Ward Name must be unique
            var all = await repo.GetAllAsync(asNoTracking: true);
            if (all.Any(w => w.Name.ToLower() == dto.Name.ToLower()))
                throw new DuplicateWardNameException(dto.Name);

            var ward = _mapper.Map<Ward>(dto);
            ward.IsActive = true;
            await repo.AddAsync(ward);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<WardResultDto>(ward);
        }

        public async Task<IEnumerable<WardOccupancySummaryDto>> GetAllWardsWithOccupancyAsync()
        {
            var repo = _unitOfWork.GetRepository<Ward, int>();
            var wards = await repo.GetAllAsync(new AllWardsWithRoomsSpecification());

            return wards.Select(w =>
            {
                var allBeds = w.Rooms.SelectMany(r => r.Beds).ToList();
                var total = allBeds.Count;
                var occupied = allBeds.Count(b => b.Status == BedStatus.Occupied);
                var available = allBeds.Count(b => b.Status == BedStatus.Available);
                var maintenance = allBeds.Count(b => b.Status == BedStatus.Maintenance);
                var reserved = allBeds.Count(b => b.Status == BedStatus.Reserved);

                return new WardOccupancySummaryDto
                {
                    WardId = w.Id,
                    WardName = w.Name,
                    WardType = w.WardType.ToString(),
                    Floor = w.Floor,
                    IsActive = w.IsActive,
                    TotalBeds = total,
                    OccupiedBeds = occupied,
                    AvailableBeds = available,
                    MaintenanceBeds = maintenance,
                    ReservedBeds = reserved
                };
            });
        }

        //FIX: Removed explicit interface implementation (IWardService.GetWardByIdAsync)
        // Changed to standard public method — cleaner and consistent with rest of the class
        public async Task<WardWithDetailsResultDto> GetWardByIdAsync(int wardId)
        {
            var repo = _unitOfWork.GetRepository<Ward, int>();
            var ward = await repo.GetByIdAsync(new WardWithRoomsAndBedsSpecification(wardId));
            if (ward is null) throw new WardNotFoundException(wardId);
            return _mapper.Map<WardWithDetailsResultDto>(ward);
        }

        public async Task<WardResultDto> UpdateWardAsync(int wardId, UpdateWardDto dto)
        {
            var repo = _unitOfWork.GetRepository<Ward, int>();
            var ward = await repo.GetByIdAsync(wardId);
            if (ward is null) throw new WardNotFoundException(wardId);

            if (!string.IsNullOrEmpty(dto.Name)) ward.Name = dto.Name;
            if (dto.WardType.HasValue) ward.WardType = dto.WardType.Value;
            if (dto.Floor.HasValue) ward.Floor = dto.Floor.Value;
            if (!string.IsNullOrEmpty(dto.PhoneExtension)) ward.PhoneExtension = dto.PhoneExtension;
            if (!string.IsNullOrEmpty(dto.Description)) ward.Description = dto.Description;
            if (dto.IsActive.HasValue) ward.IsActive = dto.IsActive.Value;

            repo.Update(ward);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<WardResultDto>(ward);
        }

        public async Task<RoomResultDto> AddRoomToWardAsync(int wardId, CreateRoomDto dto)
        {
            var wardRepo = _unitOfWork.GetRepository<Ward, int>();
            if (await wardRepo.GetByIdAsync(wardId) is null)
                throw new WardNotFoundException(wardId);

            var room = _mapper.Map<Room>(dto);
            room.WardId = wardId;
            room.IsActive = true;

            var roomRepo = _unitOfWork.GetRepository<Room, int>();
            await roomRepo.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();

            var rooms = await roomRepo.GetAllAsync(new RoomsByWardSpecification(wardId));
            var loaded = rooms.First(r => r.Id == room.Id);
            return _mapper.Map<RoomResultDto>(loaded);
        }

        public async Task<IEnumerable<RoomResultDto>> GetRoomsInWardAsync(int wardId)
        {
            var wardRepo = _unitOfWork.GetRepository<Ward, int>();
            if (await wardRepo.GetByIdAsync(wardId) is null)
                throw new WardNotFoundException(wardId);

            var roomRepo = _unitOfWork.GetRepository<Room, int>();
            var rooms = await roomRepo.GetAllAsync(new RoomsByWardSpecification(wardId));
            return _mapper.Map<IEnumerable<RoomResultDto>>(rooms);
        }
    }

}
