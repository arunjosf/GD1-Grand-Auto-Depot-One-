using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class SubmitApplicationCommand
    {
        public SubmitApplicationRequest Request { get; set; } = null!;
        public long ApplicantId { get; set; }
    }

    public class SubmitApplicationCommandHandler
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotUnit> _unitRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.PropertyImage> _imageRepo;

        public SubmitApplicationCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<GD1.Domain.Entities.LotUnit> unitRepo,
            IGenericRepository<GD1.Domain.Entities.PropertyImage> imageRepo)
        {
            _appRepo = appRepo;
            _unitRepo = unitRepo;
            _imageRepo = imageRepo;
        }

        public async Task<BaseResponse<long>> HandleAsync(SubmitApplicationCommand cmd)
        {
            var req = cmd.Request;

            if (req.LotUnits is null || req.LotUnits.Count == 0)
                throw new InvalidOperationException(
                    "At least one lot unit is required.");

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
                BusinessRegistrationUrl = req.BusinessRegistrationUrl,
                LicenseDocumentUrl = req.LicenseDocumentUrl,
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                PropertyProofUrl = req.PropertyProofUrl,
                ApplicationFee = 2000,
                FeeStatus = "Pending",
                Status = "Pending"
            };

            await _appRepo.AddAsync(application);

            foreach (var img in req.OverallImages)
            {
                await _imageRepo.AddAsync(new GD1.Domain.Entities.PropertyImage
                {
                    ApplicationId = application.Id,
                    LotUnitId = null,
                    UploadedBy = "Owner",
                    Label = img.Label,
                    ImageUrl = img.ImageUrl,
                    Remark = img.Remark
                });
            }

            foreach (var unitReq in req.LotUnits)
            {
                var unit = new GD1.Domain.Entities.LotUnit
                {
                    FranchiseApplicationId = application.Id,
                    Label = unitReq.Label.Trim(),
                    Description = unitReq.Description,
                    Tier = unitReq.Tier,
                    Capacity = unitReq.Capacity,
                    HasCCTV = unitReq.HasCCTV,
                    HasSecurity = unitReq.HasSecurity,
                    HasWorkshop = unitReq.HasWorkshop,
                    HasWashingArea = unitReq.HasWashingArea,
                    Status = "Pending"
                };

                await _unitRepo.AddAsync(unit);

                foreach (var img in unitReq.Images)
                {
                    await _imageRepo.AddAsync(new GD1.Domain.Entities.PropertyImage
                    {
                        ApplicationId = application.Id,
                        LotUnitId = unit.Id,
                        UploadedBy = "Owner",
                        Label = img.Label,
                        ImageUrl = img.ImageUrl,
                        Remark = img.Remark
                    });
                }
            }

            return BaseResponse<long>.Ok(application.Id,
                "Application submitted. Fee of Rs 2000 is pending payment.");
        }
    }
}
