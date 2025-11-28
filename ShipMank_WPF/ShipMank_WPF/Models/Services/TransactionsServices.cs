using System;
using System.Threading.Tasks;
using Npgsql;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Models.Services
{
    public class TransactionServices
    {
        public static async Task<(bool Success, string Message, string VaNumber)> ProcessBookingTransaction(
            int userId, int kapalId, DateTime date, decimal amount,
            string bank, string paymentType,
            MidtransServices midtransService)
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
                        // 1. INSERT BOOKING
                        string bookingSql = @"INSERT INTO Booking (userID, kapalID, dateBooking, dateBerangkat, status)
                                              VALUES (@UserID, @KapalID, NOW(), @DateBerangkat, 'Unpaid') 
                                              RETURNING bookingID;";

                        int newBookingID;
                        using (var cmd = new NpgsqlCommand(bookingSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("UserID", userId);
                            cmd.Parameters.AddWithValue("KapalID", kapalId);
                            cmd.Parameters.AddWithValue("DateBerangkat", date);
                            newBookingID = (int)await cmd.ExecuteScalarAsync();
                        }

                        // 2. REQUEST MIDTRANS
                        string uniqueOrderID = $"BKG-{newBookingID}";
                        vaNumber = await midtransService.CreateVaAsync(bank, (long)amount, uniqueOrderID, paymentType);

                        // 3. INSERT PAYMENT
                        string paymentSql = @"INSERT INTO Payment (bookingID, paymentMethod, jumlah, paymentStatus, datePayment, va_number)
                                              VALUES (@BookingID, 'VirtualAccount', @Jumlah, 'Unpaid', NOW(), @VA);";

                        using (var cmd = new NpgsqlCommand(paymentSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("BookingID", newBookingID);
                            cmd.Parameters.AddWithValue("Jumlah", amount);
                            cmd.Parameters.AddWithValue("VA", vaNumber);
                            await cmd.ExecuteNonQueryAsync();
                        }

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
    }
}