using System;
using System.Threading.Tasks;
using Npgsql;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Models.Services
{
    public interface ITransactionProcessor
    {
        Task<(bool Success, string Message, string VaNumber)> ProcessBooking(
            int userId, int kapalId, DateTime date, decimal amount,
            string bank, string paymentType);
    }

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
                        // 1. Insert Booking
                        int newBookingID = await InsertBookingAsync(conn, trans, userId, kapalId, date);

                        // 2. Request Midtrans
                        string uniqueOrderID = $"BKG-{newBookingID}";
                        vaNumber = await _midtransService.CreateVaAsync(bank, (long)amount, uniqueOrderID, paymentType);

                        // 3. Insert Payment 
                        await InsertPaymentAsync(conn, trans, newBookingID, amount, vaNumber, bank);

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

        private async Task<int> InsertBookingAsync(NpgsqlConnection conn, NpgsqlTransaction trans, int userId, int kapalId, DateTime date)
        {
            string sql = @"INSERT INTO Booking (userID, kapalID, dateBooking, dateBerangkat, status)
                           VALUES (@UserID, @KapalID, NOW(), @DateBerangkat, 'Unpaid') 
                           RETURNING bookingID;";

            using (var cmd = new NpgsqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("UserID", userId);
                cmd.Parameters.AddWithValue("KapalID", kapalId);
                cmd.Parameters.AddWithValue("DateBerangkat", date);
                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        private async Task InsertPaymentAsync(NpgsqlConnection conn, NpgsqlTransaction trans, int bookingId, decimal amount, string vaNumber, string bankName)
        {
            string sql = @"INSERT INTO Payment (bookingID, paymentMethod, jumlah, paymentStatus, datePayment, va_number, bankName)
                           VALUES (@BookingID, 'VirtualAccount', @Jumlah, 'Unpaid', NOW(), @VA, @BankName);";

            using (var cmd = new NpgsqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("BookingID", bookingId);
                cmd.Parameters.AddWithValue("Jumlah", amount);
                cmd.Parameters.AddWithValue("VA", vaNumber);
                cmd.Parameters.AddWithValue("BankName", bankName);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}