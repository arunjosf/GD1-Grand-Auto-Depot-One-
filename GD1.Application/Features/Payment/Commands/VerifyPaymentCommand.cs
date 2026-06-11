using GD1.Application.Common.Interfaces;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Payment.Commands
{
    public class VerifyPaymentCommand : IRequest<bool>
    {
        public long BookingId { get; set; }
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }

    public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, bool>
    {
        private readonly IGenericRepository<Domain.Entities.Booking> _bookingRepo;
        private readonly IGenericRepository<Domain.Entities.Payment> _paymentRepo;
        private readonly IGenericRepository<Domain.Entities.VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<Domain.Entities.PickupRequest> _pickupRepo;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;

        public VerifyPaymentCommandHandler(
            IGenericRepository<Domain.Entities.Booking> bookingRepo,
            IGenericRepository<Domain.Entities.Payment> paymentRepo,
            IGenericRepository<Domain.Entities.VehicleStorageProperty> propertyRepo,
            IGenericRepository<Domain.Entities.PickupRequest> pickupRepo,
            IPaymentService paymentService,
            INotificationService notificationService)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _propertyRepo = propertyRepo;
            _pickupRepo = pickupRepo;
            _paymentService = paymentService;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
        {
            // Verify signature using Razorpay standard mechanism
            bool isValid = _paymentService.VerifySignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

            if (!isValid)
            {
                return false;
            }

            // Find the payment
            // Using a simple workaround since IGenericRepository usually has GetAll or Find
            var allPayments = await _paymentRepo.GetAllAsync();
            var payment = System.Linq.Enumerable.FirstOrDefault(allPayments, p => p.RazorpayOrderId == request.RazorpayOrderId);
            
            if (payment == null) return false;

            payment.RazorpayPaymentId = request.RazorpayPaymentId;
            payment.RazorpaySignature = request.RazorpaySignature;
            payment.Status = "paid";

            await _paymentRepo.UpdateAsync(payment);

            // Update Booking status
            var booking = await _bookingRepo.GetByIdAsync(payment.BookingId);
            if (booking != null)
            {
                // If pickup was requested, it goes to AwaitingPickupAssignment. If self-drop, it goes to Confirmed.
                booking.Status = booking.IsPickupRequested 
                    ? Domain.Entities.Enums.BookingStatus.AwaitingPickupAssignment 
                    : Domain.Entities.Enums.BookingStatus.Confirmed;
                
                await _bookingRepo.UpdateAsync(booking);

                if (booking.IsPickupRequested)
                {
                    // Verify if a pickup request already exists
                    var existingPickups = await _pickupRepo.GetAllAsync();
                    if (!System.Linq.Enumerable.Any(existingPickups, p => p.BookingId == booking.Id))
                    {
                        var newPickup = new Domain.Entities.PickupRequest
                        {
                            BookingId = booking.Id,
                            RequestedPickupTime = booking.RequestedPickupTime, // Use the time saved during CreatePaymentOrder
                            Status = Domain.Entities.Enums.PickupStatus.Requested,
                            IsApprovedByLotOwner = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _pickupRepo.AddAsync(newPickup);
                    }
                }

                // Notify User via SignalR/DB
                try
                {
                    await _notificationService.SendAsync(
                        userId: booking.OwnerId,
                        title: "Booking Confirmed",
                        body: $"Your booking is confirmed! We have successfully received your payment of ₹{payment.TotalAmount}.",
                        actionType: "ViewBookingDetails",
                        referenceId: booking.Id,
                        actionUrl: "/my-bookings");
                }
                catch { /* Ignore notification failure */ }

                // Notify Lot Owner
                var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                if (property != null)
                {
                    try
                    {
                        await _notificationService.SendAsync(
                            userId: property.LotOwnerId,
                            title: "Booking Confirmed & Payment Received",
                            body: $"Payment of ₹{payment.TotalAmount} received for booking #{booking.Id}.",
                            actionType: "ViewBookingDetails",
                            referenceId: booking.Id,
                            actionUrl: $"/lot-owner/bookings/{booking.Id}");
                    }
                    catch { /* Ignore notification failure */ }
                }
            }

            return true;
        }
    }
}
