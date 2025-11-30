using System;
using System.Threading.Tasks;
using Npgsql;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Models.Services
{
    // Interface
    public interface ITransactionProcessor
    {
        Task<(bool Success, string Message, string VaNumber)> ProcessBooking(
            int userId, int kapalId, DateTime date, decimal amount,
            string bank, string paymentType);
    }

    // Class Implementation
    public class MidtransTransactionProcessor : ITransactionProcessor
    {
        private readonly MidtransServices _midtransService;

        public MidtransTransactionProcessor(MidtransServices midtransService)
        {
            _midtransService = midtransService;
        }

        public async Task<(bool Success, string Message, string VaNumber)> ProcessBooking(
            int userId, int kapalId, DateTime date, decimal amount,
            string bank, string paymentType)
        {
            string connString = DBHelper.GetConnectionString();
            string vaNumber = "";

            using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        DateTime localTime = DateTime.Now;

                        // 1. Insert Booking (Menggunakan localTime)
                        int newBookingID = await InsertBookingAsync(conn, trans, userId, kapalId, date, localTime);

                        // 2. Request Midtrans
                        string uniqueOrderID = $"BKG-{newBookingID}";

                        // Memanggil method logic API dari file MidtransServices.cs
                        vaNumber = await _midtransService.CreateVaAsync(bank, (long)amount, uniqueOrderID, paymentType);

                        // 3. Insert Payment (Menggunakan localTime agar jam sesuai komputer user)
                        await InsertPaymentAsync(conn, trans, newBookingID, amount, vaNumber, bank, localTime);

                        trans.Commit();
                        return (true, "Success", vaNumber);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return (false, ex.Message, null);
                    }
                }
            }
        }

        private async Task<int> InsertBookingAsync(NpgsqlConnection conn, NpgsqlTransaction trans, int userId, int kapalId, DateTime date, DateTime currentLocalTime)
        {
            string sql = @"INSERT INTO Booking (userID, kapalID, dateBooking, dateBerangkat, status)
                           VALUES (@UserID, @KapalID, @DateBooking, @DateBerangkat, 'Unpaid') 
                           RETURNING bookingID;";

            using (var cmd = new NpgsqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("UserID", userId);
                cmd.Parameters.AddWithValue("KapalID", kapalId);
                cmd.Parameters.AddWithValue("DateBooking", currentLocalTime);
                cmd.Parameters.AddWithValue("DateBerangkat", date);
                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        private async Task InsertPaymentAsync(NpgsqlConnection conn, NpgsqlTransaction trans, int bookingId, decimal amount, string vaNumber, string bankName, DateTime currentLocalTime)
        {
            string sql = @"INSERT INTO Payment (bookingID, paymentMethod, jumlah, paymentStatus, datePayment, va_number, bankName)
                           VALUES (@BookingID, 'VirtualAccount', @Jumlah, 'Unpaid', @DatePayment, @VA, @BankName);";

            using (var cmd = new NpgsqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("BookingID", bookingId);
                cmd.Parameters.AddWithValue("Jumlah", amount);
                cmd.Parameters.AddWithValue("DatePayment", currentLocalTime);
                cmd.Parameters.AddWithValue("VA", vaNumber);
                cmd.Parameters.AddWithValue("BankName", bankName);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}