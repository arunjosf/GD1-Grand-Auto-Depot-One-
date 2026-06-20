using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetPartnerGarageDetailQuery : IRequest<BaseResponse<PartnerGarageDetailDto>>
    {
        public long Id { get; set; }
    }

    public class PartnerGarageDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalBookings { get; set; }
        public int TotalSlots { get; set; }

        public string? BusinessRegistrationUrl { get; set; }
        public string? LicenseDocumentUrl { get; set; }
        public string? PropertyProofUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }

        public AgentInspectionDto? AgentInspection { get; set; }
        public List<string> OwnerUploadedImages { get; set; } = new();
    }

    public class AgentInspectionDto
    {
        public string AgentName { get; set; } = string.Empty;
        public string AgentContact { get; set; } = string.Empty;
        public string? OverallDescription { get; set; }
        public List<string> AgentImages { get; set; } = new();
    }

    public class GetPartnerGarageDetailQueryHandler : IRequestHandler<GetPartnerGarageDetailQuery, BaseResponse<PartnerGarageDetailDto>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _applicationRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;

        public GetPartnerGarageDetailQueryHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> applicationRepo,
            IGenericRepository<Booking> bookingRepo)
        {
            _propertyRepo = propertyRepo;
            _applicationRepo = applicationRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<PartnerGarageDetailDto>> Handle(GetPartnerGarageDetailQuery request, CancellationToken cancellationToken)
        {
            var property = (await _propertyRepo.FindAsync(x => x.Id == request.Id, "LotOwner", "ActivePropertyImages", "Slots")).FirstOrDefault();
            if (property == null) return BaseResponse<PartnerGarageDetailDto>.Fail("Garage not found.");

            // Find the linked application using LotOwnerId
            var application = (await _applicationRepo.FindAsync(x => x.ApplicantId == property.LotOwnerId && x.Status == GD1.Domain.Entities.Enums.FranchiseStatus.Approved, 
                "Assignments", "Assignments.Agent", "Assignments.Agent.User", "Assignments.Report", "Assignments.Report.SiteImages"))
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            var bookings = await _bookingRepo.FindAsync(x => x.PropertyId == request.Id);

            var dto = new PartnerGarageDetailDto
            {
                Id = property.Id,
                Name = property.Name,
                AddressLine = property.AddressLine,
                City = property.City,
                State = property.State,
                PhoneNumber = property.LotOwner?.PhoneNumber ?? "N/A",
                ImageUrl = property.ActivePropertyImages?.OrderByDescending(x => x.Id).FirstOrDefault()?.ImageUrl,
                TotalBookings = bookings.Count(),
                TotalSlots = property.Slots?.Count ?? 0,
                OwnerUploadedImages = property.ActivePropertyImages?.Select(x => x.ImageUrl).ToList() ?? new List<string>()
            };

            if (application != null)
            {
                dto.BusinessRegistrationUrl = application.BusinessRegistrationUrl;
                dto.LicenseDocumentUrl = application.LicenseDocumentUrl;
                dto.PropertyProofUrl = application.PropertyProofUrl;
                dto.OwnerIdProofUrl = application.OwnerIdProofUrl;

                var completedAssignment = application.Assignments?.FirstOrDefault(a => a.Status == "Completed");
                if (completedAssignment != null)
                {
                    dto.AgentInspection = new AgentInspectionDto
                    {
                        AgentName = completedAssignment.Agent?.User?.FullName ?? "Unknown",
                        AgentContact = completedAssignment.Agent?.User?.PhoneNumber ?? "N/A",
                        OverallDescription = completedAssignment.Report?.OverallDescription,
                        AgentImages = completedAssignment.Report?.SiteImages?.Select(x => x.ImageUrl).ToList() ?? new List<string>()
                    };
                }
            }

            return BaseResponse<PartnerGarageDetailDto>.Ok(dto);
        }
    }
}
