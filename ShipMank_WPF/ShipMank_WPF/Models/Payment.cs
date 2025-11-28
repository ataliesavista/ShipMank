using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int BookingID { get; set; }
        public Booking Booking { get; set; }
        public PaymentMethod Metode { get; set; }
        public decimal Jumlah { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime DatePayment { get; set; }

        public Payment(int bookingID, decimal jumlah)
        {
            BookingID = bookingID;
            Jumlah = jumlah;
            Metode = PaymentMethod.VirtualAccount;
            Status = PaymentStatus.Unpaid;        
            DatePayment = DateTime.Now;
        }

        public bool ProsesPembayaran()
        {
            if (Status != PaymentStatus.Unpaid) return false;

            var random = new Random();
            bool success = random.Next(1, 11) <= 8;

            if (success)
            {
                Status = PaymentStatus.Completed;
                Booking?.KonfirmasiPesanan();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString() => $"Payment #{PaymentID} - {Status} (Rp {Jumlah:N0})";
    }
}
