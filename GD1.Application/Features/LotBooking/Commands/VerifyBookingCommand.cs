using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GD1.Application.Interfaces.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class VerifyBookingCommand : IRequest<BaseResponse<string>>
    {
        public long BookingId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public long AdminId { get; set; }
    }

    public class VerifyBookingCommandHandler : IRequestHandler<VerifyBookingCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<Agreement> _agreementRepo;
        private readonly INotificationService _notificationService;

        public VerifyBookingCommandHandler(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<Agreement> agreementRepo,
            INotificationService notificationService)
        {
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
            _vehicleRepo = vehicleRepo;
            _agreementRepo = agreementRepo;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<string>> Handle(VerifyBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");

            if (booking.Status != BookingStatus.PendingVerification)
                return BaseResponse<string>.Fail("Booking is not in a verification pending state.");

            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
            if (property == null || property.LotOwnerId != request.AdminId)
                return BaseResponse<string>.Fail("Unauthorized. Only the property owner can verify this booking.");

            var user = await _userRepo.GetByIdAsync(booking.OwnerId);
            if (user == null) return BaseResponse<string>.Fail("User not found.");

            var vehicle = await _vehicleRepo.GetByIdAsync(booking.VehicleId);
            if (vehicle == null) return BaseResponse<string>.Fail("Vehicle not found.");

            if (request.IsApproved)
            {
                booking.Status = BookingStatus.VerifiedPendingPayment;
                await _bookingRepo.UpdateAsync(booking);

                // Generate the Digital Agreement NOW
                var agreementHtml = GD1.Application.Features.LotBooking.Templates.AgreementTemplate.Generate(
                    customerName: user.FullName,
                    customerEmail: user.Email,
                    vehicleBrand: vehicle.Brand,
                    vehicleModel: vehicle.Model,
                    vehicleYear: vehicle.Year.ToString(),
                    registrationNo: vehicle.RegistrationNo,
                    vehicleType: vehicle.Category,
                    propertyName: property.Name,
                    propertyAddress: property.AddressLine,
                    propertyCity: property.City,
                    propertyState: property.State,
                    startDate: booking.StartDate.ToString("dd MMM yyyy"),
                    endDate: booking.EndDate.ToString("dd MMM yyyy"),
                    pricePerDay: property.PricePerDay,
                    agreementDate: DateTime.UtcNow.ToString("dd MMM yyyy")
                );

                var agreement = new Agreement
                {
                    UserId = booking.OwnerId,
                    Type = AgreementType.LotBooking,
                    ReferenceId = booking.Id,
                    Content = agreementHtml,
                    Status = AgreementStatus.Pending
                };

                await _agreementRepo.AddAsync(agreement);

                try
                {
                    var connectionString = Environment.GetEnvironmentVariable("Azure__ServiceBusConnectionString")
                        ?? "Your_Local_Connection_String_Here_For_Testing";  

                    await using var client = new ServiceBusClient(connectionString);
                    var sender = client.CreateSender("pdf-queue");

                    var payload = new { AgreementId = agreement.Id };
                    var message = new ServiceBusMessage(JsonSerializer.Serialize(payload));

                    await sender.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Queue Error: {ex.Message}");
                }

                try
                {
                    await _notificationService.SendAsync(
                        userId: booking.OwnerId,
                        title: "Booking Approved",
                        body: $"Your booking at {property.Name} has been verified by the owner. Please confirm the booking to proceed to payment.",
                        actionType: "ConfirmBooking",
                        referenceId: agreement.Id,
                        actionUrl: $"/agreement/{booking.Id}"); 
                }
                catch { }

                return BaseResponse<string>.Ok("Booking verified and approved. The user has been notified to complete the payment.");
            }
            else
            {
                booking.Status = BookingStatus.AdminRejected;
                booking.RejectionReason = request.RejectionReason;
                await _bookingRepo.UpdateAsync(booking);

                try
                {
                    await _notificationService.SendAsync(
                        userId: booking.OwnerId,
                        title: "Booking Request Rejected",
                        body: $"Your booking at {property.Name} was not approved. Reason: {request.RejectionReason ?? "No reason provided."}",
                        actionType: "ViewBooking",
                        referenceId: booking.Id,
                        actionUrl: "/user/bookings");
                }
                catch { }

                return BaseResponse<string>.Ok("Booking request rejected successfully.");
            }
        }
    }
}
