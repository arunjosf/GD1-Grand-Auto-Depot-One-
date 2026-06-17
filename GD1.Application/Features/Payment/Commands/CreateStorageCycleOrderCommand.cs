using GD1.Application.Common.Interfaces;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Payment.Commands
{
    /// <summary>
    /// Creates a Razorpay order for a recurring storage cycle payment or an overdue booking payment.
    /// </summary>
    public class CreateStorageCycleOrderCommand : IRequest<CreateStorageCycleOrderResult>
    {
        public long BookingId { get; set; }
        public decimal AmountToPay { get; set; }
    }

    public class CreateStorageCycleOrderResult
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public decimal TotalAmountToPay { get; set; }
        public string Currency { get; set; } = "INR";
    }

    public class CreateStorageCycleOrderCommandHandler : IRequestHandler<CreateStorageCycleOrderCommand, CreateStorageCycleOrderResult>
    {
        private readonly IGenericRepository<Domain.Entities.Booking> _bookingRepo;
        private readonly IPaymentService _paymentService;

        public CreateStorageCycleOrderCommandHandler(
            IGenericRepository<Domain.Entities.Booking> bookingRepo,
            IPaymentService paymentService)
        {
            _bookingRepo = bookingRepo;
            _paymentService = paymentService;
        }

        public async Task<CreateStorageCycleOrderResult> Handle(CreateStorageCycleOrderCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null) throw new Exception("Booking not found");

            decimal adminCut = request.AmountToPay * 0.15m;
            string receiptId = $"receipt_cycle_{booking.Id}_{DateTime.UtcNow.Ticks}";

            var (orderId, amount) = await _paymentService.CreateOrderAsync(receiptId, request.AmountToPay, adminCut, null);

            return new CreateStorageCycleOrderResult
            {
                RazorpayOrderId = orderId,
                TotalAmountToPay = request.AmountToPay
            };
        }
    }
}
