using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Repositories;
using GD1.Application.Interfaces;
using GD1.Application.Common.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Commands
{
    public class CancelMyServiceCenterApplicationCommand : IRequest<BaseResponse<string>>
    {
        public long ApplicationId { get; set; }
        public long ApplicantId { get; set; }
    }

    public class CancelMyServiceCenterApplicationCommandHandler : IRequestHandler<CancelMyServiceCenterApplicationCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> _appRepo;
        private readonly IPaymentService _paymentService;

        public CancelMyServiceCenterApplicationCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> appRepo,
            IPaymentService paymentService)
        {
            _appRepo = appRepo;
            _paymentService = paymentService;
        }

        public async Task<BaseResponse<string>> Handle(CancelMyServiceCenterApplicationCommand cmd, CancellationToken ct)
        {
            var app = await _appRepo.GetByIdAsync(cmd.ApplicationId);
            if (app == null) return BaseResponse<string>.Fail("Application not found.");

            if (app.ApplicantId != cmd.ApplicantId)
                return BaseResponse<string>.Fail("You are not authorized to cancel this application.");

            if (app.Status == "Approved")
                return BaseResponse<string>.Fail("Approved applications cannot be cancelled. Please contact support.");

            app.Status = "Cancelled";
            app.AdminNotes = "Cancelled by User.";
            app.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(app.FeeTransactionId) && app.FeeStatus != "Refunded")
            {
                try
                {
                    await _paymentService.RefundPaymentAsync(app.FeeTransactionId, app.ApplicationFee);
                    app.FeeStatus = "Refunded";
                }
                catch
                {
                    // If refund fails, log it or handle it. We still mark as Cancelled.
                }
            }
            
            await _appRepo.UpdateAsync(app);

            return BaseResponse<string>.Ok(string.Empty, "Application cancelled successfully.");
        }
    }
}
