using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Domain.Entities.Enums;

namespace GD1.Application.Features.Vehicle.Commands
{
    public class EditVehicleCommand : IRequest<BaseResponse<string>>
    {
        public long VehicleId { get; set; }
        public long UserId { get; set; }
        public UserRole UserRole { get; set; }
        
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? FuelType { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string? DocumentUrls { get; set; }
    }

    public class EditVehicleCommandHandler : IRequestHandler<EditVehicleCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;
        private readonly GD1.Application.Interfaces.IVehicleService _vehicleService;

        public EditVehicleCommandHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo,
            GD1.Application.Interfaces.IVehicleService vehicleService)
        {
            _vehicleRepo = vehicleRepo;
            _bookingRepo = bookingRepo;
            _vehicleService = vehicleService;
        }

        public async Task<BaseResponse<string>> Handle(EditVehicleCommand cmd, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(cmd.VehicleId);
            if (vehicle is null)
                throw new KeyNotFoundException("Vehicle not found.");

            var allBookings = await _bookingRepo.FindAsync(b => true, "Property");
            var activeBooking = allBookings.FirstOrDefault(b => b.VehicleId == cmd.VehicleId && b.Status == BookingStatus.InLot);
            
            if (activeBooking is not null)
                throw new InvalidOperationException("Cannot edit vehicle details after storage has started.");

            if (cmd.UserRole == UserRole.VehicleOwner && vehicle.OwnerId != cmd.UserId)
            {
                throw new UnauthorizedAccessException("You don't own this vehicle.");
            }
            else if (cmd.UserRole == UserRole.LotOwner)
            {
                var relatedBooking = allBookings.FirstOrDefault(b => b.VehicleId == cmd.VehicleId && b.Property?.LotOwnerId == cmd.UserId);
                if (relatedBooking is null)
                    throw new UnauthorizedAccessException("You are not the property owner for this vehicle's booking.");

                Console.WriteLine($"[NOTIFICATION] Vehicle {vehicle.RegistrationNo} details were updated by Property Owner.");
            }

            vehicle.Brand = cmd.Brand;
            vehicle.Model = cmd.Model;
            vehicle.Year = cmd.Year;
            vehicle.RegistrationNo = cmd.RegistrationNo;
            vehicle.Color = cmd.Color;
            vehicle.FuelType = cmd.FuelType;
            vehicle.VehicleType = cmd.VehicleType;
            
            var dims = await _vehicleService.GetDimensionsAsync(cmd.Brand, cmd.Model, cmd.VehicleType);
            vehicle.LengthFeet = dims.Length;
            vehicle.WidthFeet = dims.Width;
            vehicle.HeightFeet = dims.Height;
            vehicle.DocumentUrls = cmd.DocumentUrls;

            await _vehicleRepo.UpdateAsync(vehicle);

            return BaseResponse<string>.Ok(string.Empty, "Vehicle details updated successfully.");
        }
    }
}
