using GD1.Application.Common;
using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Interfaces;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using MediatR;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Commands
{
    public class AddVehicleCommand : IRequest<BaseResponse<long>>
    {
        public AddVehicleRequest Request { get; set; } = null!;
        public long OwnerId { get; set; }
    }

    public class AddVehicleCommandValidator : AbstractValidator<AddVehicleCommand>
    {
        public AddVehicleCommandValidator()
        {
            RuleFor(x => x.OwnerId).GreaterThan(0);
            RuleFor(x => x.Request.VehicleId).GreaterThan(0);
            RuleFor(x => x.Request.RegistrationNo).NotEmpty();
            RuleFor(x => x.Request.Color).NotEmpty();
            RuleFor(x => x.Request.FuelType).NotEmpty();
            RuleFor(x => x.Request.OwnerIdProofUrl).NotEmpty().WithMessage("ID Proof is required for AI verification.");
            RuleFor(x => x.Request.VehicleRcUrl).NotEmpty().WithMessage("Vehicle RC is required for AI verification.");
            RuleFor(x => x.Request.Images).NotEmpty();
        }
    }

            public class AddVehicleCommandHandler : IRequestHandler<AddVehicleCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleCatalogItem> _catalogRepo;
        private readonly IOcrService _ocrService;
        
        public AddVehicleCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleCatalogItem> catalogRepo,
            IOcrService ocrService)
        {
            _vehicleRepo = vehicleRepo;
            _userRepo = userRepo;
            _catalogRepo = catalogRepo;
            _ocrService = ocrService;
        }

        public async Task<BaseResponse<long>> Handle(AddVehicleCommand cmd, CancellationToken cancellationToken)
        {
            var req = cmd.Request;

            // 1. Check if Vehicle already exists (Duplicate Check)
            var existingVehicle = (await _vehicleRepo.GetAllAsync())
                .FirstOrDefault(v => v.RegistrationNo == req.RegistrationNo.ToUpper().Trim());
            
            if (existingVehicle != null)
                return BaseResponse<long>.Fail("A vehicle with this registration number already exists in our system.");

            // 2. Fetch the vehicle from Catalog
            var catalogVehicle = await _catalogRepo.GetByIdAsync(req.VehicleId);
            if (catalogVehicle == null)
            {
                return BaseResponse<long>.Fail("Selected vehicle model could not be found in the catalog.");
            }

            // 3. Validate Year
            if (!string.IsNullOrEmpty(catalogVehicle.ValidYearsCsv))
            {
                bool isYearValid = false;
                var yearsStr = catalogVehicle.ValidYearsCsv.Trim();

                if (yearsStr.Contains("-"))
                {
                    // Range format e.g. "2015-2020"
                    var years = yearsStr.Split('-');
                    if (years.Length == 2 && int.TryParse(years[0], out int startYear) && int.TryParse(years[1], out int endYear))
                    {
                        if (req.Year >= startYear && req.Year <= endYear)
                        {
                            isYearValid = true;
                        }
                    }
                }
                else if (yearsStr.Contains(","))
                {
                    // List format e.g. "2018, 2019, 2021"
                    var yearList = yearsStr.Split(',').Select(y => y.Trim());
                    if (yearList.Contains(req.Year.ToString()))
                    {
                        isYearValid = true;
                    }
                }
                else
                {
                    // Exact match format e.g. "2024"
                    if (yearsStr == req.Year.ToString())
                    {
                        isYearValid = true;
                    }
                }

                if (!isYearValid)
                {
                    return BaseResponse<long>.Fail($"The {catalogVehicle.Brand} {catalogVehicle.Model} is only valid for year(s): {catalogVehicle.ValidYearsCsv}. You entered {req.Year}.");
                }
            }

            // 4. AI Security Verification
            var user = await _userRepo.GetByIdAsync(cmd.OwnerId);
            if (user == null) throw new UnauthorizedAccessException("User not found.");

            string idText = "", rcText = "";
            bool isAiVerified = false;

            try {
                idText = await _ocrService.ExtractText(req.OwnerIdProofUrl);
                rcText = await _ocrService.ExtractText(req.VehicleRcUrl);
                isAiVerified = _ocrService.NamesMatch(idText, rcText, user.FullName);
            } catch {
                isAiVerified = false;
            }

            // 5. Create Vehicle Entity
            var vehicle = new GD1.Domain.Entities.Vehicle
            {
                OwnerId = cmd.OwnerId,
                Brand = catalogVehicle.Brand,
                Model = catalogVehicle.Model,
                Year = req.Year,
                RegistrationNo = req.RegistrationNo.ToUpper().Trim(),
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                VehicleRcUrl = req.VehicleRcUrl,
                Color = req.Color,
                FuelType = req.FuelType,
                IsHybrid = req.IsHybrid,
                Category = catalogVehicle.Category,
                LengthFeet = catalogVehicle.LengthFeet,
                WidthFeet = catalogVehicle.WidthFeet,
                HeightFeet = catalogVehicle.HeightFeet,
                VerificationStatus = isAiVerified ? "Verified" : "Mismatch",
                HealthScore = 100,
                Images = new List<GD1.Domain.Entities.VehicleImage>()
            };

            if (req.Images != null)
            {
                foreach (var img in req.Images)
                {
                    vehicle.Images.Add(new GD1.Domain.Entities.VehicleImage
                    {
                        Label = img.Label ?? "Exterior",
                        ImageUrl = img.ImageUrl,
                        UploadedBy = "Owner"
                    });
                }
            }

            await _vehicleRepo.AddAsync(vehicle);

            return BaseResponse<long>.Ok(vehicle.Id, isAiVerified ? "Vehicle added and AI verified." : "Vehicle added. AI verification failed;");
        }
    }
}
