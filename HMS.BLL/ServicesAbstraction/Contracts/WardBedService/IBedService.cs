using HMS.DAL.Models.Enums.WardBedEnums;
using HMS.BLL.Shared.Dtos.WardBedModule.BedDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts.WardBedService
{
    public interface IBedService
    {
        Task<BedResultDto> AddBedToRoomAsync(int roomId, CreateBedDto dto);
        Task<IEnumerable<BedResultDto>> GetBedsInRoomAsync(int roomId);
        Task<BedResultDto> UpdateBedStatusAsync(int bedId, UpdateBedStatusDto dto);
        Task<IEnumerable<BedAvailabilityResultDto>> GetAvailableBedsAsync(string? wardType = null, string? bedType = null);

    }
}
