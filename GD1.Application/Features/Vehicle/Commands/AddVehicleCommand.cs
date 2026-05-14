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
            RuleFor(x => x.Request.RegistrationNo).NotEmpty();
            RuleFor(x => x.Request.OwnerIdProofUrl).NotEmpty().WithMessage("ID Proof is required for AI verification.");
            RuleFor(x => x.Request.VehicleRcUrl).NotEmpty().WithMessage("Vehicle RC is required for AI verification.");
            RuleFor(x => x.Request.Images).NotEmpty();
        }
    }

    public class AddVehicleCommandHandler : IRequestHandler<AddVehicleCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleImage> _imageRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;
        private readonly IOcrService _ocrService;

        public AddVehicleCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleImage> imageRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            IOcrService ocrService)
        {
            _vehicleRepo = vehicleRepo;
            _imageRepo = imageRepo;
            _userRepo = userRepo;
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

            // 2. Mandatory Image Check (Front, Rear, Left, Right)
            var labels = req.Images.Select(i => i.Label).ToList();
            var required = new[] { "Front", "Rear", "LeftSide", "RightSide" };
            var missing = required.Where(r => !labels.Contains(r)).ToList();

            if (missing.Any())
                throw new InvalidOperationException($"Missing required images: {string.Join(", ", missing)}");

            // 3. AI Security Verification
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

            // 4. Create Vehicle Entity
            var vehicle = new GD1.Domain.Entities.Vehicle
            {
                OwnerId = cmd.OwnerId,
                Brand = req.Brand.Trim(),
                Model = req.Model.Trim(),
                Year = req.Year,
                RegistrationNo = req.RegistrationNo.ToUpper().Trim(),
                Color = req.Color,
                FuelType = req.FuelType,
                VehicleType = req.VehicleType,
                
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                VehicleRcUrl = req.VehicleRcUrl,
                VerificationStatus = isAiVerified ? "Verified" : "Mismatch",
                
                HealthScore = 100,
                Images = new List<GD1.Domain.Entities.VehicleImage>()
            };

            foreach (var img in req.Images)
            {
                vehicle.Images.Add(new GD1.Domain.Entities.VehicleImage
                {
                    Label = img.Label,
                    ImageUrl = img.ImageUrl,
                    UploadedBy = "Owner",
                    Remark = img.Remark
                });
            }

            await _vehicleRepo.AddAsync(vehicle);

            // 5. Build Response
            var message = isAiVerified 
                ? "Vehicle added and identity verified by AI successfully." 
                : "Vehicle added, but AI detected a name mismatch on your documents. A Lot Manager will review it manually.";

            return BaseResponse<long>.Ok(vehicle.Id, message);
        }
    }
}
