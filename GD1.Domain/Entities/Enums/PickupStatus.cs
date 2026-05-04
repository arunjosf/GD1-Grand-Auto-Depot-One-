using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities.Enums
{
    public enum PickupStatus
    {
        Requested,
        Assigned,
        ManagerScheduled,
        Approved,
        OtpSent,
        Verified,
        VehiclePicked
    }
}
