using GD1.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Features.Complaints.DTOs;

namespace GD1.Application.Features.Complaints.Queries
{
    public class GetComplaintsQuery : IRequest<BaseResponse<IEnumerable<ComplaintDto>>>
    {
        public long? LotId { get; set; }   
        public long? ComplainantId { get; set; }   
        public string? Status { get; set; }
    }

  

    public class GetComplaintsQueryHandler
        : IRequestHandler<GetComplaintsQuery, BaseResponse<IEnumerable<ComplaintDto>>>
    {
        private readonly GD1.Domain.Interfaces.IGenericRepository<GD1.Domain.Entities.Complaint> _repo;

        public GetComplaintsQueryHandler(GD1.Domain.Interfaces.IGenericRepository<GD1.Domain.Entities.Complaint> repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<ComplaintDto>>> Handle(
            GetComplaintsQuery query, CancellationToken ct)
        {
            var complaints = await _repo.GetAllAsync();

            if (query.LotId.HasValue)
                complaints = complaints.Where(c => c.LotId == query.LotId);
            
            if (query.ComplainantId.HasValue)
                complaints = complaints.Where(c => c.ComplainantId == query.ComplainantId);
            
            if (!string.IsNullOrEmpty(query.Status))
                complaints = complaints.Where(c => c.Status == query.Status);

            var list = complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                Subject = c.Subject,
                Description = c.Description,
                Status = c.Status,
                AdminResponse = c.AdminResponse,
                ComplainantName = c.Complainant?.FullName ?? "Unknown",
                LotName = c.Lot?.Name ?? "Unknown",
                CreatedAt = c.CreatedAt
            }).ToList();

            return BaseResponse<IEnumerable<ComplaintDto>>.Ok(list);
        }
    }
}
