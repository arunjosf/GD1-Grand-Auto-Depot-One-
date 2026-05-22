using System;

namespace GD1.Application.Features.ServiceCenter.DTOs
{
    public class ServiceCenterDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? SupportedBrand { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
