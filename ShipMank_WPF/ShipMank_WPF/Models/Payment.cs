using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Payment
    {
        public int PaymentID { get; private set; }
        public int BookingID { get; private set; }
        public PaymentMethod Metode { get; private set; }
        public double Jumlah { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime DatePayment { get; private set; }

        public Booking Booking { get; private set; }

        public Payment(int paymentID, int bookingID, PaymentMethod metode, double jumlah, Booking booking)
        {
            PaymentID = paymentID;
            BookingID = bookingID;
            Metode = metode;
            Jumlah = jumlah;
            Status = PaymentStatus.Pending;
            DatePayment = DateTime.Now;
            Booking = booking;

            if (booking != null)
                booking.SetPayment(this);
        }

        public bool ProsesPembayaran(PaymentMethod metode, double jumlah)
        {
            if (Status != PaymentStatus.Pending)
                return false;

            var random = new Random();
            bool success = random.Next(1, 11) <= 8;

            if (success)
            {
                Status = PaymentStatus.Success;
                Booking?.KonfirmasiPesanan();
                return true;
            }
            else
            {
                Status = PaymentStatus.Failed;
                return false;
            }
        }

        public bool ValidasiPembayaran() => Status == PaymentStatus.Success && Jumlah > 0;

        public bool Refund()
        {
            if (Status == PaymentStatus.Success)
            {
                Status = PaymentStatus.Refunded;
                return true;
            }
            return false;
        }

        public string CetakStrukPembayaran()
        {
            return "=====================================\n" +
                   "         STRUK PEMBAYARAN\n" +
                   "=====================================\n" +
                   $"Payment ID    : {PaymentID}\n" +
                   $"Booking ID    : {BookingID}\n" +
                   $"Tanggal       : {DatePayment:dd/MM/yyyy HH:mm}\n" +
                   $"Metode        : {Metode}\n" +
                   $"Jumlah        : Rp {Jumlah:N0}\n" +
                   $"Status        : {Status}\n" +
                   "=====================================\n" +
                   "    Terima kasih atas pembayaran Anda\n" +
                   "=====================================";
        }

        public override string ToString() => $"Payment #{PaymentID} - {Status} (Rp {Jumlah:N0})";
    }
}
