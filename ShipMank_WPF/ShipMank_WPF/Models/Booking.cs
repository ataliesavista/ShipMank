using Npgsql;
using NpgsqlTypes;
using ShipMank_WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Booking : TransactionBase
    {
        public int UserID { get; private set; }
        public User User { get; set; }
        public int KapalID { get; private set; }
        public Kapal Kapal { get; set; }
        public DateTime DateBerangkat { get; private set; }
        public BookingStatus Status { get; private set; }
        public Payment Payment { get; set; }
        public Review Review { get; set; }

        public Booking(int userID, int kapalID, DateTime dateBerangkat)
        {
            UserID = userID;
            KapalID = kapalID;
            DateBerangkat = dateBerangkat;
            Status = BookingStatus.Unpaid;
        }

        // POLYMORPHISM: Implementasi proses transaksi untuk Booking (Konfirmasi)
        public override bool ProcessTransaction()
        {
            return KonfirmasiPesanan();
        }

        public bool BuatPesanan(int userID, int kapalID, DateTime dateBerangkat)
        {
            if (dateBerangkat <= DateTime.Now) return false;
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
                if (Payment != null)
                {
                    Payment.CancelPayment();
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

        // POLYMORPHISM: Override GetDetail
        public override string GetDetail()
        {
            return base.GetDetail() +
                   $"\nType: Booking" +
                   $"\nUser: {User?.Name ?? "Unknown"}" +
                   $"\nKapal: {Kapal?.NamaKapal ?? "Unknown"}" +
                   $"\nStatus: {Status}";
        }

        public decimal HitungTotalHarga() => Kapal != null ? Kapal.HargaPerjalanan : 0;

        public static bool IsDateBooked(int kapalId, DateTime date)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Booking WHERE kapalID = @id AND dateBerangkat = @tgl AND status != 'Cancelled'";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", kapalId);
                        cmd.Parameters.Add(new NpgsqlParameter("@tgl", NpgsqlDbType.Date) { Value = date });
                        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch { return true; }
        }
    }
}
