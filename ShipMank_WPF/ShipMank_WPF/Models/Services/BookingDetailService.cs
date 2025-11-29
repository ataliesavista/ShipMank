using Npgsql;
using System;
using System.Threading.Tasks;
using System.Windows;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Models.Services
{
    public class BookingDetailInfo
    {
        public string OrderID { get; set; }
        public string CustName { get; set; }
        public string CustEmail { get; set; }
        public string CustPhone { get; set; }
        public string ShipName { get; set; }
        public string ShipType { get; set; }
        public string Route { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public decimal TotalPaid { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string VaNumber { get; set; }
        public string PaymentBank { get; set; }
    }

    public static class BookingDetailService
    {
        public static BookingDetailInfo GetBookingDetails(int bookingID)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"
                        SELECT b.bookingID, b.dateBooking, b.dateBerangkat, b.status,
                               k.namaKapal, s.typeName, l.city, l.province,
                               u.name AS custName, u.email AS custEmail, u.noTelp AS custPhone,
                               p.paymentMethod, p.datePayment, p.va_number, p.bankName,
                               COALESCE(p.jumlah, k.hargaPerjalanan) AS totalPaid
                        FROM Booking b
                        JOIN Users u ON b.userID = u.userID
                        JOIN Kapal k ON b.kapalID = k.kapalID
                        JOIN ShipType s ON k.shipType = s.typeID
                        LEFT JOIN Lokasi l ON k.lokasi = l.portID
                        LEFT JOIN Payment p ON b.bookingID = p.bookingID
                        WHERE b.bookingID = @bid";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("bid", bookingID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string bank = reader["bankName"]?.ToString();
                                string method = reader["paymentMethod"]?.ToString(); // Isinya "VirtualAccount"

                                // LOGIKA PENGGABUNGAN NAMA BANK & METHOD
                                string displayMethod = "-";
                                if (!string.IsNullOrEmpty(bank))
                                {
                                    if (bank.ToLower() == "mandiri")
                                        displayMethod = "Mandiri Bill Payment";
                                    else
                                        displayMethod = $"{bank.ToUpper()} Virtual Account";
                                }
                                else if (!string.IsNullOrEmpty(method))
                                {
                                    displayMethod = method; // Fallback jika bank null (transaksi lama)
                                }

                                return new BookingDetailInfo
                                {
                                    OrderID = $"BKG-{reader["bookingID"].ToString().PadLeft(5, '0')}",
                                    CustName = reader["custName"].ToString(),
                                    CustEmail = reader["custEmail"].ToString(),
                                    CustPhone = reader["custPhone"].ToString(),
                                    ShipName = reader["namaKapal"].ToString(),
                                    ShipType = reader["typeName"].ToString(),
                                    Route = $"{reader["city"]}, {reader["province"]}",
                                    DepartureDate = Convert.ToDateTime(reader["dateBerangkat"]),
                                    BookingDate = Convert.ToDateTime(reader["dateBooking"]),
                                    Status = reader["status"].ToString(),
                                    TotalPaid = Convert.ToDecimal(reader["totalPaid"]),
                                    PaymentMethod = displayMethod, // SUDAH DIGABUNG DI SINI
                                    PaymentDate = reader["datePayment"] as DateTime?,
                                    VaNumber = reader["va_number"]?.ToString(),
                                    PaymentBank = bank // Tetap disimpan untuk logika lain jika perlu
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading details: " + ex.Message); }
            return null;
        }

        public static bool CancelBooking(int bookingID)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            new NpgsqlCommand("UPDATE Booking SET status = 'Cancelled' WHERE bookingID = " + bookingID, conn, trans).ExecuteNonQuery();
                            new NpgsqlCommand("UPDATE Payment SET paymentStatus = 'Cancelled' WHERE bookingID = " + bookingID, conn, trans).ExecuteNonQuery();
                            trans.Commit();
                            return true;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to cancel: " + ex.Message);
                return false;
            }
        }
    }
}