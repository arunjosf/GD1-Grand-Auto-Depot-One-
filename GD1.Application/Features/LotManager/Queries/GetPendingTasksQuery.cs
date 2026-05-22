using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetPendingTasksQuery : IRequest<BaseResponse<IEnumerable<PendingTaskDto>>>
    {
        public long ManagerId { get; set; }
    }

    public class PendingTaskDto
    {
        public long Id { get; set; }
        public long VehicleId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public MaintenanceTaskType Type { get; set; }
        public DateTime RequestedAt { get; set; }
    }

    public class GetPendingTasksQueryHandler : IRequestHandler<GetPendingTasksQuery, BaseResponse<IEnumerable<PendingTaskDto>>>
    {
        private readonly IGenericRepository<MaintenanceTask> _taskRepo;

        public GetPendingTasksQueryHandler(IGenericRepository<MaintenanceTask> taskRepo)
        {
            _taskRepo = taskRepo;
        }

        public async Task<BaseResponse<IEnumerable<PendingTaskDto>>> Handle(GetPendingTasksQuery request, CancellationToken cancellationToken)
        {
            var pendingTasks = await _taskRepo.FindAsync(t => t.Manager.ManagerId == request.ManagerId && t.Status == MaintenanceTaskStatus.Pending, "Vehicle", "Manager");

            var dtos = pendingTasks.Select(t => new PendingTaskDto
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                Brand = t.Vehicle?.Brand ?? "Unknown",
                Model = t.Vehicle?.Model ?? "Unknown",
                RegistrationNo = t.Vehicle?.RegistrationNo ?? "Unknown",
                Type = t.Type,
                RequestedAt = t.RequestedAt
            }).OrderBy(t => t.RequestedAt).ToList();

            return BaseResponse<IEnumerable<PendingTaskDto>>.Ok(dtos);
        }
    }
}
