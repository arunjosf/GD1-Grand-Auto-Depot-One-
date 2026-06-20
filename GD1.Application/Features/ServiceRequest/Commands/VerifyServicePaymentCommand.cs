using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceRequest.Commands
{
    public class VerifyServicePaymentCommand : IRequest<bool>
    {
        public long ServiceRequestId { get; set; }
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }

    public class VerifyServicePaymentCommandHandler : IRequestHandler<VerifyServicePaymentCommand, bool>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;

        public VerifyServicePaymentCommandHandler(IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo)
        {
            _requestRepo = requestRepo;
        }

        public async Task<bool> Handle(VerifyServicePaymentCommand request, CancellationToken cancellationToken)
        {
            var serviceRequest = await _requestRepo.GetByIdAsync(request.ServiceRequestId);
            if (serviceRequest == null) return false;

            // In a real app, verify the signature using RazorpayClient. 
            // Here we just mark it as paid for the demo.
            serviceRequest.IsPaid = true;
            serviceRequest.RazorpayPaymentId = request.RazorpayPaymentId;
            serviceRequest.RazorpayOrderId = request.RazorpayOrderId;
            serviceRequest.RazorpaySignature = request.RazorpaySignature;
            serviceRequest.Status = "Completed";

            await _requestRepo.UpdateAsync(serviceRequest);

            return true;
        }
    }
}
