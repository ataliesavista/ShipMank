using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public enum KapalStatus
    {
        Available,
        Unavailable
    }

    public enum BookingStatus
    {
        Unpaid,
        Completed,
        Upcoming,
        Cancelled
    }

    public enum PaymentStatus
    {
        Unpaid,
        Completed,
        Upcoming,
        Cancelled
    }

    public enum PaymentMethod
    {
        VirtualAccount
    }
}
