using HMS.DAL.Models.Enums.WardBedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.RoomsDtos
{
    public record CreateRoomDto
    {       
        [Required, MaxLength(20)]
        public string RoomNumber { get; init; } = string.Empty;

        [Required]
        public RoomType RoomType { get; init; }

        [Required, Range(1, 20)]
        public int Capacity { get; init; }
  
    }
}
