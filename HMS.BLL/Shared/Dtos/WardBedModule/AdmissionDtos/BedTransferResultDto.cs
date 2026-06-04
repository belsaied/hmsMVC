using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos
{
    public record BedTransferResultDto
    {
        public int Id { get; init; }
        public int FromBedId { get; init; }
        public string FromBedNumber { get; init; } = string.Empty;
        public int ToBedId { get; init; }
        public string ToBedNumber { get; init; } = string.Empty;
        public DateTime TransferredAt { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string TransferredBy { get; init; } = string.Empty;
    }
}
