using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public int KapalID { get; set; }
        public Kapal Kapal { get; set; }
        public DateTime DateBooking { get; set; }
        public DateTime DateBerangkat { get; set; }
        public BookingStatus Status { get; set; }
        public Payment Payment { get; set; }
        public Review Review { get; set; }
        public Booking(int userID, int kapalID, DateTime dateBerangkat)
        {
            UserID = userID;
            KapalID = kapalID;
            DateBooking = DateTime.Now;
            DateBerangkat = dateBerangkat;
            Status = BookingStatus.Unpaid;
        }

        public bool BuatPesanan(int userID, int kapalID, DateTime dateBerangkat)
        {
            if (dateBerangkat <= DateTime.Now)
                return false;

            UserID = userID;
            KapalID = kapalID;
            DateBerangkat = dateBerangkat;
            Status = BookingStatus.Unpaid;

            return true;
        }

        public bool BatalkanPesanan()
        {
            if (Status != BookingStatus.Completed && Status != BookingStatus.Cancelled)
            {
                Status = BookingStatus.Cancelled;

                if (Payment != null && Payment.Status != PaymentStatus.Cancelled)
                {
                    Payment.Status = PaymentStatus.Cancelled;
                }
                return true;
            }
            return false;
        }

        public bool KonfirmasiPesanan()
        {
            if (Status == BookingStatus.Unpaid)
            {
                Status = BookingStatus.Upcoming;
                return true;
            }
            return false;
        }

        public string GetDetailBooking()
        {
            return $"Booking ID: {BookingID}\n" +
                   $"User: {User?.Name ?? "Unknown"}\n" +
                   $"Kapal: {Kapal?.NamaKapal ?? "Unknown"}\n" +
                   $"Tgl Booking: {DateBooking:dd/MM/yyyy HH:mm}\n" +
                   $"Tgl Berangkat: {DateBerangkat:dd/MM/yyyy}\n" +
                   $"Status: {Status}";
        }

        public decimal HitungTotalHarga() => Kapal != null ? Kapal.HargaPerjalanan : 0;
        public static bool IsDateBooked(int kapalId, DateTime date)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    // Query: Hitung booking yang tidak di-cancel
                    string sql = "SELECT COUNT(*) FROM Booking WHERE kapalID = @id AND dateBerangkat = @tgl AND status != 'Cancelled'";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", kapalId);
                        cmd.Parameters.Add(new NpgsqlParameter("@tgl", NpgsqlDbType.Date) { Value = date });
                        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                return true; // Return true (booked/unavailable) jika error demi keamanan
            }
        }
    }
}
