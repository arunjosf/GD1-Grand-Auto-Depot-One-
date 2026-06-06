using GD1.Application.Common.Interfaces;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Payment.Commands
{
    public class CreatePaymentOrderCommand : IRequest<CreatePaymentOrderResult>
    {
        public long BookingId { get; set; }
        // The arrival method selected on PickupOptionsPage
        public bool IsPickupRequested { get; set; } 
        // For pickup details
        public string? PickupAddress { get; set; }
        public string? PickupPincode { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
        public string? RequestedPickupTime { get; set; }
    }

    public class CreatePaymentOrderResult
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public decimal TotalAmountToPay { get; set; }
        public string Currency { get; set; } = "INR";
    }

    public class CreatePaymentOrderCommandHandler : IRequestHandler<CreatePaymentOrderCommand, CreatePaymentOrderResult>
    {
        private readonly IGenericRepository<Domain.Entities.Booking> _bookingRepo;
        private readonly IGenericRepository<Domain.Entities.Payment> _paymentRepo;
        private readonly IPaymentService _paymentService;

        public CreatePaymentOrderCommandHandler(
            IGenericRepository<Domain.Entities.Booking> bookingRepo,
            IGenericRepository<Domain.Entities.Payment> paymentRepo,
            IPaymentService paymentService)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _paymentService = paymentService;
        }

        public async Task<CreatePaymentOrderResult> Handle(CreatePaymentOrderCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null) throw new Exception("Booking not found");

            // 1. Calculate 3 days storage cost
            decimal pricePerDay = booking.PricePerDay;
            decimal threeDaysCost = pricePerDay * 3;

            // 2. Add Pickup charge if applicable
            decimal pickupCharge = request.IsPickupRequested ? 200m : 0m;
            decimal totalUpfront = threeDaysCost + pickupCharge;

            // 3. Calculate 15% Admin cut strictly from the 3 days storage (no cut on pickup)
            decimal adminCut = threeDaysCost * 0.15m;
            decimal ownerCut = (threeDaysCost - adminCut) + pickupCharge;

            // Optional: get Lot Owner's Razorpay Account ID if we linked them
            // For now, assume null if not stored.
            string lotOwnerAccountId = null; 

            // Create Order in Razorpay
            string receiptId = $"receipt_bk_{booking.Id}";
            var (orderId, amount) = await _paymentService.CreateOrderAsync(receiptId, totalUpfront, adminCut, lotOwnerAccountId);

            // Save to Database
            var payment = new Domain.Entities.Payment
            {
                BookingId = booking.Id,
                RazorpayOrderId = orderId,
                TotalAmount = totalUpfront,
                AdminCutAmount = adminCut,
                PropertyOwnerAmount = ownerCut,
                PickupChargeAmount = pickupCharge,
                Status = "created"
            };

            await _paymentRepo.AddAsync(payment);

            // Update Booking pickup details if passed
            if (request.IsPickupRequested)
            {
                booking.IsPickupRequested = true;
                booking.PickupAddress = request.PickupAddress;
                booking.PickupPincode = request.PickupPincode;
                booking.PickupLatitude = request.PickupLatitude;
                booking.PickupLongitude = request.PickupLongitude;
                // RequestedPickupTime could be mapped to an entity property if we added it, skipping for brevity or add it later
            }

            booking.Status = Domain.Entities.Enums.BookingStatus.AwaitingPayment;
            await _bookingRepo.UpdateAsync(booking);

            return new CreatePaymentOrderResult
            {
                RazorpayOrderId = orderId,
                TotalAmountToPay = totalUpfront
            };
        }
    }
}
