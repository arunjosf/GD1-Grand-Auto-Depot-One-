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
            RuleFor(x => x.Request.Brand).NotEmpty();
            RuleFor(x => x.Request.Model).NotEmpty();
            RuleFor(x => x.Request.Category).NotEmpty();
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
        private readonly IVehicleService _vehicleService;
        
        public AddVehicleCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleCatalogItem> catalogRepo,
            IOcrService ocrService,
            IVehicleService vehicleService)
        {
            _vehicleRepo = vehicleRepo;
            _userRepo = userRepo;
            _catalogRepo = catalogRepo;
            _ocrService = ocrService;
            _vehicleService = vehicleService;
        }

        public async Task<BaseResponse<long>> Handle(AddVehicleCommand cmd, CancellationToken cancellationToken)
        {
            var req = cmd.Request;

            // 1. Check if Vehicle already exists (Duplicate Check)
            var existingVehicle = (await _vehicleRepo.GetAllAsync())
                .FirstOrDefault(v => v.RegistrationNo == req.RegistrationNo.ToUpper().Trim());
            
            if (existingVehicle != null)
                return BaseResponse<long>.Fail("A vehicle with this registration number already exists in our system.");

            // 2. Fetch the vehicle dimensions from Catalog or estimate them
            var dimensions = await _vehicleService.GetDimensionsAsync(req.Brand, req.Model, req.Category);


            // 3. Validate Year - simple range check (no NHTSA API)
            if (req.Year > 2026)
                return BaseResponse<long>.Fail($"Vehicle model year cannot be greater than 2026.");

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
                Brand = req.Brand,
                Model = req.Model,
                Year = req.Year,
                RegistrationNo = req.RegistrationNo.ToUpper().Trim(),
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                VehicleRcUrl = req.VehicleRcUrl,
                Color = req.Color,
                FuelType = req.FuelType,
                IsHybrid = req.IsHybrid,
                Category = req.Category,
                LengthFeet = dimensions.Length,
                WidthFeet = dimensions.Width,
                HeightFeet = dimensions.Height,
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
