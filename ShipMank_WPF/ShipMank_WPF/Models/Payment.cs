using ShipMank_WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Payment : TransactionBase
    {
        public int BookingID { get; private set; }
        public Booking Booking { get; set; }
        public PaymentMethod Metode { get; private set; }
        public decimal Jumlah { get; private set; }
        public PaymentStatus Status { get; set; }

        public Payment(int bookingID, decimal jumlah)
        {
            BookingID = bookingID;
            Jumlah = jumlah;
            Metode = PaymentMethod.VirtualAccount;
            Status = PaymentStatus.Unpaid;
            // DateCreated diurus oleh base class
        }

        // POLYMORPHISM: Implementasi proses transaksi untuk Payment (Bayar)
        public override bool ProcessTransaction()
        {
            return ProsesPembayaran();
        }

        public bool ProsesPembayaran()
        {
            if (Status != PaymentStatus.Unpaid) return false;

            var random = new Random();
            bool success = random.Next(1, 11) <= 8;

            if (success)
            {
                Status = PaymentStatus.Completed;
                // OOP Interaction
                Booking?.KonfirmasiPesanan();
                return true;
            }
            else
            {
                return false;
            }
        }

        // Method khusus untuk mendukung encapsulation Booking
        public void CancelPayment()
        {
            if (Status != PaymentStatus.Cancelled)
            {
                Status = PaymentStatus.Cancelled;
            }
        }

        // POLYMORPHISM: Override tampilan string
        public override string ToString() => $"Payment #{ID} - {Status} (Rp {Jumlah:N0})";

        public override string GetDetail()
        {
            return base.GetDetail() +
                   $"\nType: Payment" +
                   $"\nAmount: {Jumlah:N0}" +
                   $"\nStatus: {Status}";
        }
    }
}
