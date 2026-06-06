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
        AwaitingAgreement = 5,
        AgreementDeclined = 6,
        AwaitingPickupAssignment = 7,
        PickupAssigned = 8,
        ManagerArrived = 9,
        PickupVerified = 10,
        InTransit = 11,
        AwaitingPayment = 12,
        PendingVerification = 13,
        AdminRejected = 14,
        VerifiedPendingPayment = 15,
        InLot = 2,
        Completed = 3,
        Cancelled = 4
    }
}
