using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class LotManager : BaseEntity
    {
        public long LotId { get; set; }
        public long ManagerId { get; set; }
        public long AddedBy { get; set; }
        public bool IsActive { get; set; } = true;

        public StorageLot LotOwner { get; set; } = null!;
        public User Manager { get; set; } = null!;
    }
}
