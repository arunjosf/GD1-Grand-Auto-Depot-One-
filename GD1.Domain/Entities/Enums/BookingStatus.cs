using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities.Enums
{
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        InLot = 2,
        Completed = 3,
        Cancelled = 4
    }
}
