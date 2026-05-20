using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Complaints.Commands
{
    public class EmailPropertyOwnerCommand : IRequest<BaseResponse<string>>
    {
        public long ComplaintId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class EmailPropertyOwnerCommandHandler : IRequestHandler<EmailPropertyOwnerCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Complaint> _complaintRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;

        public EmailPropertyOwnerCommandHandler(
            IGenericRepository<Complaint> complaintRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo)
        {
            _complaintRepo = complaintRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<string>> Handle(EmailPropertyOwnerCommand cmd, CancellationToken ct)
        {
            var complaint = await _complaintRepo.GetByIdAsync(cmd.ComplaintId);
            if (complaint is null)
                throw new KeyNotFoundException("Complaint not found.");

            var property = await _propertyRepo.GetByIdAsync(complaint.PropertyId);
            if (property is null)
                throw new KeyNotFoundException("Property not found.");

            var owner = await _userRepo.GetByIdAsync(property.LotOwnerId);
            if (owner is null)
                throw new KeyNotFoundException("Property owner not found.");

            // Fake Email sending for now. In a real system, you'd inject an IEmailService here.
            Console.WriteLine($"[EMAIL SENT to {owner.Email}] Regarding Complaint {complaint.Id}: {cmd.Message}");

            complaint.AdminResponse = cmd.Message;
            await _complaintRepo.UpdateAsync(complaint);

            return BaseResponse<string>.Ok(string.Empty, "Email sent to property owner successfully.");
        }
    }
}
