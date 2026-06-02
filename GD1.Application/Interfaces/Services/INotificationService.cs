using System.Collections.Generic;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(long userId, string title, string body, string? actionType = null, long? referenceId = null, string? actionUrl = null);
        Task SendToManyAsync(IEnumerable<long> userIds, string title, string body, string? actionType = null, long? referenceId = null, string? actionUrl = null);
    }
}
