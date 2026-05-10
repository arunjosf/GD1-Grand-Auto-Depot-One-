using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Domain.Interfaces;
using GD1.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using FluentValidation;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class SubmitApplicationCommand : IRequest<BaseResponse<long>>
    {
        public SubmitApplicationRequest Request { get; set; } = null!;
        public long ApplicantId { get; set; }
    }

    public class SubmitApplicationCommandValidator : AbstractValidator<SubmitApplicationCommand>
    {
        public SubmitApplicationCommandValidator()
        {
            RuleFor(x => x.ApplicantId)
                .GreaterThan(0).WithMessage("Invalid Applicant ID.");

            RuleFor(x => x.Request.BusinessName)
                .NotEmpty().WithMessage("Business Name is required.");

            RuleFor(x => x.Request.ContactEmail)
                .NotEmpty().WithMessage("Contact Email is required.")
                .EmailAddress().WithMessage("Please enter a valid business email address.");

            RuleFor(x => x.Request.FrontImageUrl)
                .NotEmpty().WithMessage("Front view image is required.")
                .Must(BeAValidUrl).WithMessage("The front image must be a valid URL.");

            RuleFor(x => x.Request.OtherImageUrls)
                .ForEach(url => url.Must(BeAValidUrl).WithMessage("One or more property images have an invalid URL."));

            RuleFor(x => x.Request.LotUnits)
                .NotEmpty().WithMessage("At least one lot unit must be defined.");
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }

    public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotUnit> _unitRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.PropertyImage> _imageRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotUnitImage> _unitImageRepo;
        private readonly IGeocodingService _geocoding;

        public SubmitApplicationCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<GD1.Domain.Entities.LotUnit> unitRepo,
            IGenericRepository<GD1.Domain.Entities.PropertyImage> imageRepo,
            IGenericRepository<GD1.Domain.Entities.LotUnitImage> unitImageRepo,
            IGeocodingService geocoding)
        {
            _appRepo = appRepo;
            _unitRepo = unitRepo;
            _imageRepo = imageRepo;
            _unitImageRepo = unitImageRepo;
            _geocoding = geocoding;
        }

        public async Task<BaseResponse<long>> Handle(SubmitApplicationCommand cmd, CancellationToken cancellationToken)
        {
            var req = cmd.Request;

            if (req.LotUnits is null || req.LotUnits.Count == 0)
                throw new InvalidOperationException(
                    "At least one lot unit is required.");

            var previousApps = await _appRepo.GetAllAsync();
            var prevRejected = previousApps.FirstOrDefault(a => 
                a.IsDeleted && 
                (a.BusinessName.ToLower() == req.BusinessName.ToLower() || 
                 a.AddressLine.ToLower() == req.AddressLine.ToLower()));

            var adminNotes = prevRejected != null 
                ? $"WARNING: Previously rejected application found. Previous App ID: {prevRejected.Id}." 
                : null;

            var application = new GD1.Domain.Entities.FranchiseApplication
            {
                ApplicantId = cmd.ApplicantId,
                ApplicationType = req.ApplicationType,
                BusinessName = req.BusinessName.Trim(),
                OwnerName = req.OwnerName.Trim(),
                ContactEmail = req.ContactEmail.ToLower().Trim(),
                PhoneNumber = req.PhoneNumber.Trim(),
                AddressLine = req.AddressLine.Trim(),
                City = req.City.Trim(),
                State = req.State.Trim(),
                Country = req.Country,
                PostalCode = req.PostalCode.Trim(),
                PreferredInspectionDate = req.PreferredInspectionDate,
                BusinessRegistrationUrl = req.BusinessRegistrationUrl,
                LicenseDocumentUrl = req.LicenseDocumentUrl,
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                PropertyProofUrl = req.PropertyProofUrl,
                ExtraFacilities = req.ExtraFacilities != null ? string.Join(",", req.ExtraFacilities) : null,
                ApplicationFee = 2000,
                FeeStatus = "Pending",
                Status = "Pending",
                AdminNotes = adminNotes
            };

            // Automatic Geocoding
            var fullAddress = $"{application.AddressLine}, {application.City}, {application.State}, {application.PostalCode}, {application.Country}";
            var coords = await _geocoding.GetCoordinatesAsync(fullAddress);
            
            if (coords == null)
            {
                throw new InvalidOperationException($"Could not determine location for address: {fullAddress}. Please ensure the address is correct.");
            }

            application.Latitude = coords.Value.Lat;
            application.Longitude = coords.Value.Lon;

            await _appRepo.AddAsync(application);

            await _imageRepo.AddAsync(new GD1.Domain.Entities.PropertyImage
            {
                ApplicationId = application.Id,
                UploadedBy = "Owner",
                Label = "Front View",
                ImageUrl = req.FrontImageUrl,
                IsMain = true,
                Remark = null
            });

            if (req.OtherImageUrls != null)
            {
                foreach (var imgUrl in req.OtherImageUrls)
                {
                    await _imageRepo.AddAsync(new GD1.Domain.Entities.PropertyImage
                    {
                        ApplicationId = application.Id,
                        UploadedBy = "Owner",
                        Label = "Overall Property View",
                        ImageUrl = imgUrl,
                        IsMain = false,
                        Remark = null
                    });
                }
            }

            for (int i = 0; i < req.LotUnits.Count; i++)
            {
                var unitReq = req.LotUnits[i];
                var unitLabel = string.IsNullOrWhiteSpace(unitReq.Label) 
                    ? $"Unit {i + 1}" 
                    : unitReq.Label.Trim();

                var unit = new GD1.Domain.Entities.LotUnit
                {
                    FranchiseApplicationId = application.Id,
                    Label = unitLabel,
                    Description = unitReq.Description,
                    Tier = unitReq.Tier,
                    Capacity = unitReq.Capacity,
                    HasCCTV = unitReq.HasCCTV,
                    HasSecurity = unitReq.HasSecurity,
                    HasWorkshop = unitReq.HasWorkshop,
                    HasWashingArea = unitReq.HasWashingArea,
                    HasFireSafety = unitReq.HasFireSafety,
                    ExtraFacilities = unitReq.ExtraFacilities != null ? string.Join(",", unitReq.ExtraFacilities) : null,
                    Status = "Pending"
                };

                await _unitRepo.AddAsync(unit);

                for (int j = 0; j < unitReq.Images.Count; j++)
                {
                    var imgUrl = unitReq.Images[j];
                    await _unitImageRepo.AddAsync(new GD1.Domain.Entities.LotUnitImage
                    {
                        LotUnitId = unit.Id,
                        UploadedBy = "Owner",
                        IsMain = j == 0,
                        ImageUrl = imgUrl,
                        Remark = null
                    });
                }
            }

            return BaseResponse<long>.Ok(application.Id,
                "Application submitted. Fee of Rs 2000 is pending payment.");
        }
    }
}
