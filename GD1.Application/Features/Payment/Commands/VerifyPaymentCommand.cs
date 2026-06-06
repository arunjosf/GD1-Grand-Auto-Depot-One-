using GD1.Application.Common.Interfaces;
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
        private readonly IPaymentService _paymentService;

        public VerifyPaymentCommandHandler(
            IGenericRepository<Domain.Entities.Booking> bookingRepo,
            IGenericRepository<Domain.Entities.Payment> paymentRepo,
            IPaymentService paymentService)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _paymentService = paymentService;
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
            }

            return true;
        }
    }
}
