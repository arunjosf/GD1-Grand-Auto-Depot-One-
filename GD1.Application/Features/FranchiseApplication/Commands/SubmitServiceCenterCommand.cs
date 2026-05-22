using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class SubmitServiceCenterCommand : IRequest<BaseResponse<long>>
    {
        public long ApplicantId { get; set; }
        public string? BusinessName { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string? PostalCode { get; set; }
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? OemCertificateUrl { get; set; }
        public string? SupportedBrand { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public System.Collections.Generic.List<string> Images { get; set; } = new System.Collections.Generic.List<string>();
    }

    public class SubmitServiceCenterCommandHandler : IRequestHandler<SubmitServiceCenterCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> _serviceCenterRepo;
        private readonly GD1.Application.Interfaces.IGeocodingService _geocodingService;

        public SubmitServiceCenterCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> serviceCenterRepo,
            GD1.Application.Interfaces.IGeocodingService geocodingService)
        {
            _serviceCenterRepo = serviceCenterRepo;
            _geocodingService = geocodingService;
        }

        public async Task<BaseResponse<long>> Handle(SubmitServiceCenterCommand cmd, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(cmd.OemCertificateUrl))
                return BaseResponse<long>.Fail("OEM Certificate is required for Service Centers.");
                
            if (cmd.Images == null || cmd.Images.Count == 0)
                return BaseResponse<long>.Fail("At least one facility image is required.");
                
            // Geocode if not provided
            if (!cmd.Latitude.HasValue || !cmd.Longitude.HasValue || (cmd.Latitude.Value == 0 && cmd.Longitude.Value == 0))
            {
                string fullAddress = string.Join(", ", new[]
                {
                    cmd.AddressLine,
                    cmd.City,
                    cmd.State,
                    cmd.PostalCode,
                    cmd.Country
                }.Where(s => !string.IsNullOrWhiteSpace(s)));

                var coords = await _geocodingService.GetCoordinatesAsync(fullAddress);
                if (coords.HasValue)
                {
                    cmd.Latitude = coords.Value.Lat;
                    cmd.Longitude = coords.Value.Lon;
                }
            }

            var application = new GD1.Domain.Entities.ServiceCenterPartneringApplication
            {
                ApplicantId = cmd.ApplicantId,
                Name = !string.IsNullOrWhiteSpace(cmd.BusinessName) ? cmd.BusinessName : (cmd.SupportedBrand ?? "Authorized Service Center"),
                OwnerName = cmd.OwnerName,
                Email = cmd.ContactEmail,
                PhoneNumber = cmd.PhoneNumber,
                AddressLine = cmd.AddressLine,
                City = cmd.City,
                District = cmd.District,
                State = cmd.State,
                Country = cmd.Country,
                PostalCode = cmd.PostalCode,
                Latitude = cmd.Latitude,
                Longitude = cmd.Longitude,
                
                OemCertificateUrl = cmd.OemCertificateUrl,
                SupportedBrand = cmd.SupportedBrand,
                OwnerIdProofUrl = cmd.OwnerIdProofUrl,
                Images = cmd.Images.Select(url => new GD1.Domain.Entities.ServiceCenterImage
                {
                    ImageUrl = url,
                    CreatedAt = DateTime.UtcNow
                }).ToList(),

                Status = "PendingReview",
                CreatedAt = DateTime.UtcNow
            };

            await _serviceCenterRepo.AddAsync(application);

            return BaseResponse<long>.Ok(application.Id, "Service Center application submitted successfully.");
        }
    }
}
