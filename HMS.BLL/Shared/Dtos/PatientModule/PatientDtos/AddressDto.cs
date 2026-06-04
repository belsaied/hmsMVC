namespace HMS.BLL.Shared.Dtos.PatientModule.PatientDtos
{
    public record AddressDto
    {
        public string Street { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
    }
}
