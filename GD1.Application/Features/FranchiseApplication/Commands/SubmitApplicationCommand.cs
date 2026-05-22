using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class SubmitApplicationCommand : IRequest<BaseResponse<long>>
    {
        public long ApplicantId { get; set; }
        public GD1.Domain.Entities.Enums.ApplicationType ApplicationType { get; set; } = GD1.Domain.Entities.Enums.ApplicationType.Franchise;
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public decimal PricePerDay { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime PreferredInspectionDate { get; set; }

        public string? BusinessRegistrationUrl { get; set; }
        public string? LicenseDocumentUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? PropertyProofUrl { get; set; }

        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasFireSafety { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }

        public List<string> PropertyImages { get; set; } = [];
        public List<FranchiseSlotRequest> Slots { get; set; } = [];
    }

    public class FranchiseSlotRequest
    {
        public string SlotNumber { get; set; } = string.Empty;
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<FranchiseSlot> _slotRepo;
        private readonly IGenericRepository<PropertyImage> _imageRepo;

        public SubmitApplicationCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<FranchiseSlot> slotRepo,
            IGenericRepository<PropertyImage> imageRepo)
        {
            _appRepo = appRepo;
            _slotRepo = slotRepo;
            _imageRepo = imageRepo;
        }

        public async Task<BaseResponse<long>> Handle(SubmitApplicationCommand cmd, CancellationToken ct)
        {
            if (cmd.Slots.Count == 0)
                return BaseResponse<long>.Fail("At least one garage (slot) must be added.");

            foreach (var slot in cmd.Slots)
            {
                if (string.IsNullOrEmpty(slot.ImageUrl))
                    return BaseResponse<long>.Fail($"Image is required for garage {slot.SlotNumber}.");
                if (slot.SquareFeet <= 0 || slot.HeightFeet <= 0)
                    return BaseResponse<long>.Fail($"Valid dimensions are required for garage {slot.SlotNumber}.");
            }

            var application = new GD1.Domain.Entities.FranchiseApplication
            {
                ApplicantId = cmd.ApplicantId,
                ApplicationType = cmd.ApplicationType,
                BusinessName = cmd.BusinessName,
                OwnerName = cmd.OwnerName,
                ContactEmail = cmd.ContactEmail,
                PhoneNumber = cmd.PhoneNumber,
                AddressLine = cmd.AddressLine,
                City = cmd.City,
                State = cmd.State,
                PostalCode = cmd.PostalCode,
                Latitude = cmd.Latitude,
                Longitude = cmd.Longitude,
                PreferredInspectionDate = cmd.PreferredInspectionDate,
                BusinessRegistrationUrl = cmd.BusinessRegistrationUrl,
                LicenseDocumentUrl = cmd.LicenseDocumentUrl,
                OwnerIdProofUrl = cmd.OwnerIdProofUrl,
                PropertyProofUrl = cmd.PropertyProofUrl,
                PricePerDay = cmd.PricePerDay,
                Status = FranchiseStatus.Pending,
                HasCCTV = cmd.HasCCTV,
                HasSecurity = cmd.HasSecurity,
                HasFireSafety = cmd.HasFireSafety,
                HasWorkshop = cmd.HasWorkshop,
                HasWashingArea = cmd.HasWashingArea,
                CreatedAt = DateTime.UtcNow
            };

            await _appRepo.AddAsync(application);

            foreach (var imgUrl in cmd.PropertyImages)
            {
                await _imageRepo.AddAsync(new PropertyImage
                {
                    ApplicationId = application.Id,
                    ImageUrl = imgUrl,
                    Label = "Property Main",
                    UploadedBy = "Owner",
                    IsMain = cmd.PropertyImages.First() == imgUrl
                });
            }

            foreach (var s in cmd.Slots)
            {
                await _slotRepo.AddAsync(new FranchiseSlot
                {
                    ApplicationId = application.Id,
                    SlotNumber = s.SlotNumber,
                    SquareFeet = s.SquareFeet,
                    HeightFeet = s.HeightFeet,
                    ImageUrl = s.ImageUrl
                });
            }

            return BaseResponse<long>.Ok(application.Id, "Application submitted successfully.");
        }
    }
}
