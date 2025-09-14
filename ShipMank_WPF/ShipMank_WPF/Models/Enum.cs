using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public enum ShipTypeEnum
    {
        Speedboat,
        Yacht,
        Cruise,
        Canoe,
        Dll
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Refunded
    }

    public enum PaymentMethod
    {
        BankTransfer,
        EWallet,
        Card
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed,
        Refunded
    }

    public enum KapalStatus
    {
        Available,
        Unavailable
    }
}
