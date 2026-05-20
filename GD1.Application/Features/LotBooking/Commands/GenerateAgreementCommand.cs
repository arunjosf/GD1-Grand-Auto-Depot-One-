using GD1.Application.Common;
using GD1.Application.Features.LotBooking.Templates;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class GenerateAgreementCommand : IRequest<BaseResponse<long>>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public long OwnerId { get; set; }
        public long BookingId { get; set; }
    }

    public class GenerateAgreementCommandHandler : IRequestHandler<GenerateAgreementCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<BookingAgreement> _agreementRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;

        public GenerateAgreementCommandHandler(
            IGenericRepository<BookingAgreement> agreementRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo)
        {
            _agreementRepo = agreementRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _vehicleRepo = vehicleRepo;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<long>> Handle(GenerateAgreementCommand cmd, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(cmd.BookingId);
            if (booking == null) return BaseResponse<long>.Fail("Booking not found.");
            if (booking.OwnerId != cmd.OwnerId) return BaseResponse<long>.Fail("Unauthorized.");

            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
            if (property == null) return BaseResponse<long>.Fail("Selected property not found.");

            var vehicle = await _vehicleRepo.GetByIdAsync(booking.VehicleId);
            if (vehicle == null) return BaseResponse<long>.Fail("Vehicle not found.");

            var user = await _userRepo.GetByIdAsync(cmd.OwnerId);
            if (user == null) return BaseResponse<long>.Fail("User not found.");

            // Remove any existing unbooked agreements for this booking to ensure only the latest generated plan is active
            var existingAgreements = await _agreementRepo.FindAsync(a => 
                a.BookingId == cmd.BookingId);
                
            foreach (var existing in existingAgreements)
            {
                await _agreementRepo.DeleteAsync(existing);
            }

            var agreementHtml = AgreementTemplate.Generate(
                customerName: user.FullName,
                customerEmail: user.Email,
                vehicleBrand: vehicle.Brand,
                vehicleModel: vehicle.Model,
                vehicleYear: vehicle.Year.ToString(),
                registrationNo: vehicle.RegistrationNo,
                vehicleType: vehicle.VehicleType,
                propertyName: property.Name,
                propertyAddress: property.AddressLine,
                propertyCity: property.City,
                propertyState: property.State,
                startDate: booking.StartDate.ToString("dd MMM yyyy"),
                endDate: booking.EndDate.ToString("dd MMM yyyy"),
                pricePerDay: property.PricePerDay,
                agreementDate: DateTime.UtcNow.ToString("dd MMM yyyy")
            );

            var agreement = new BookingAgreement
            {
                OwnerId = cmd.OwnerId,
                VehicleId = booking.VehicleId,
                PropertyId = booking.PropertyId,
                BookingId = booking.Id,
                Content = agreementHtml,
                Status = AgreementStatus.Pending,
                VehicleSnapshotJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    vehicle.Brand,
                    vehicle.Model,
                    vehicle.Year,
                    vehicle.RegistrationNo,
                    vehicle.VehicleType
                }),
                LotSnapshotJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    property.Name,
                    property.AddressLine,
                    property.City,
                    property.State,
                    property.PricePerDay
                })
            };

            await _agreementRepo.AddAsync(agreement);

            return BaseResponse<long>.Ok(agreement.Id, "Agreement generated successfully. Please review and accept.");
        }
    }
}
