using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.WardDtos
{
    public record WardResultDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string WardType { get; init; } = string.Empty;
        public int? Floor { get; init; }
        public string? PhoneExtension { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; }
    }
}
