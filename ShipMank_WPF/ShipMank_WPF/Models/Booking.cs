using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Booking
    {
        public int BookingID { get; private set; }
        public int UserID { get; private set; }
        public int KapalID { get; private set; }
        public DateTime DateBooking { get; private set; }
        public DateTime DateBerangkat { get; set; }
        public int JumlahPenumpang { get; set; }
        public string Tujuan { get; set; }
        public BookingStatus Status { get; private set; }

        public User User { get; private set; }
        public Kapal Kapal { get; private set; }
        public Payment Payment { get; private set; }

        public Booking(int bookingID, int userID, int kapalID, DateTime dateBerangkat,
                       int jumlahPenumpang, string tujuan, User user, Kapal kapal)
        {
            BookingID = bookingID;
            UserID = userID;
            KapalID = kapalID;
            DateBooking = DateTime.Now;
            DateBerangkat = dateBerangkat;
            JumlahPenumpang = jumlahPenumpang;
            Tujuan = tujuan;
            Status = BookingStatus.Pending;
            User = user;
            Kapal = kapal;
        }

        public bool BuatPesanan(int userID, int kapalID, DateTime dateBerangkat,
                               int jumlahPenumpang, string tujuan)
        {
            if (jumlahPenumpang <= 0 || dateBerangkat <= DateTime.Now || string.IsNullOrEmpty(tujuan))
                return false;

            UserID = userID;
            KapalID = kapalID;
            DateBerangkat = dateBerangkat;
            JumlahPenumpang = jumlahPenumpang;
            Tujuan = tujuan;

            return true;
        }

        public bool BatalkanPesanan()
        {
            if (Status == BookingStatus.Pending || Status == BookingStatus.Confirmed)
            {
                Status = BookingStatus.Cancelled;
                return true;
            }
            return false;
        }

        public bool KonfirmasiPesanan()
        {
            if (Status == BookingStatus.Pending)
            {
                Status = BookingStatus.Confirmed;
                return true;
            }
            return false;
        }

        public string GetDetailBooking()
        {
            return $"Booking ID: {BookingID}\n" +
                   $"User: {User?.Name ?? "Unknown"}\n" +
                   $"Kapal: {Kapal?.NamaKapal ?? "Unknown"}\n" +
                   $"Tanggal Booking: {DateBooking:dd/MM/yyyy HH:mm}\n" +
                   $"Tanggal Berangkat: {DateBerangkat:dd/MM/yyyy}\n" +
                   $"Jumlah Penumpang: {JumlahPenumpang}\n" +
                   $"Tujuan: {Tujuan}\n" +
                   $"Status: {Status}";
        }

        public double HitungTotalHarga() => Kapal != null ? Kapal.HargaPerjalanan * JumlahPenumpang : 0;

        public void SetPayment(Payment payment) => Payment = payment;

        public override string ToString() => $"Booking #{BookingID} - {Status} ({DateBerangkat:dd/MM/yyyy})";
    }
}
