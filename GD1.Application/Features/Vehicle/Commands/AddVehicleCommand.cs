using GD1.Application.Common;
using GD1.Application.Features.Vehicle.DTOs;
using GD1.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Commands
{
    public class AddVehicleCommand
    {
        public AddVehicleRequest Request { get; set; } = null!;
        public long OwnerId { get; set; }
    }

    public class AddVehicleCommandHandler
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleImage> _imageRepo;

        public AddVehicleCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleImage> imageRepo)
        {
            _vehicleRepo = vehicleRepo;
            _imageRepo = imageRepo;
        }

        public async Task<BaseResponse<long>> HandleAsync(AddVehicleCommand cmd)
        {
            var req = cmd.Request;
            var labels = req.Images.Select(i => i.Label).ToList();
            var required = new[] { "Front", "Rear", "LeftSide", "RightSide" };
            var missing = required.Where(r => !labels.Contains(r)).ToList();

            if (missing.Any())
                throw new InvalidOperationException(
                    $"Missing required images: {string.Join(", ", missing)}");

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
                DocumentUrls = req.DocumentUrls,
                HealthScore = 100
            };

            await _vehicleRepo.AddAsync(vehicle);

            foreach (var img in req.Images)
            {
                await _imageRepo.AddAsync(new GD1.Domain.Entities.VehicleImage
                {
                    VehicleId = vehicle.Id,
                    EventId = null,
                    Label = img.Label,
                    ImageUrl = img.ImageUrl,
                    UploadedBy = "Owner",
                    Remark = img.Remark
                });
            }

            return BaseResponse<long>.Ok(vehicle.Id, "Vehicle added successfully.");
        }
    }
}
