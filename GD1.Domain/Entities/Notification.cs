using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public long UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public string? ActionUrl { get; set; }
        public string? ActionType { get; set; }
        public long? ReferenceId { get; set; }

        public User User { get; set; } = null!;
    }
}
