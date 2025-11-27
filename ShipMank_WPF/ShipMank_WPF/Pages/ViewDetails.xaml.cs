using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Npgsql;
using ShipMank_WPF.Models;
using QRCoder;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;

namespace ShipMank_WPF.Pages
{
    public partial class ViewDetails : Page
    {
        private int _bookingID;

        public ViewDetails(OrderHistoryItem historyItem)
        {
            InitializeComponent();
            _bookingID = historyItem.OriginalBookingID;

            QuestPDF.Settings.License = LicenseType.Community;

            LoadFullDetails();
        }

        public ViewDetails()
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private void LoadFullDetails()
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            b.bookingID, b.dateBooking, b.dateBerangkat, b.status,
                            k.namaKapal, s.typeName,
                            l.city, l.province,
                            u.name AS custName, u.email AS custEmail, u.noTelp AS custPhone,
                            p.paymentMethod, p.datePayment, p.va_number,
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
                        cmd.Parameters.AddWithValue("bid", _bookingID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // === 1. MAP ORDER & CUSTOMER INFO ===
                                string orderIdStr = $"BKG-{reader["bookingID"].ToString().PadLeft(5, '0')}";
                                TxtOrderID.Text = orderIdStr;

                                string custName = reader["custName"].ToString();
                                TxtCustName.Text = custName;
                                TxtCustEmail.Text = reader["custEmail"].ToString();
                                TxtCustPhone.Text = reader["custPhone"].ToString();

                                // === 2. MAP SHIP INFO ===
                                string shipName = reader["namaKapal"].ToString();
                                TxtShipName.Text = shipName;
                                TxtShipType.Text = reader["typeName"].ToString();
                                TxtRoute.Text = $"{reader["city"]}, {reader["province"]}";

                                DateTime depDate = Convert.ToDateTime(reader["dateBerangkat"]);
                                TxtDate.Text = depDate.ToString("dddd, dd MMM yyyy");

                                // === 3. MAP PAYMENT INFO ===
                                TxtBookingDate.Text = Convert.ToDateTime(reader["dateBooking"]).ToString("dd MMM yyyy, HH:mm");

                                decimal total = Convert.ToDecimal(reader["totalPaid"]);
                                TxtTotal.Text = $"Rp {total:N0}";

                                string method = reader["paymentMethod"]?.ToString();
                                TxtPaymentMethod.Text = string.IsNullOrEmpty(method) ? "-" : method;

                                if (reader["datePayment"] != DBNull.Value)
                                {
                                    TxtPaymentDate.Text = Convert.ToDateTime(reader["datePayment"]).ToString("dd MMM yyyy, HH:mm");
                                }
                                else
                                {
                                    TxtPaymentDate.Text = "Waiting Payment";
                                    TxtPaymentDate.Foreground = Brushes.OrangeRed;
                                }

                                // === 4. STATUS & LOGIC TAMPILAN ===
                                string status = reader["status"].ToString();
                                TxtStatus.Text = status.ToUpper();
                                UpdateUIBasedOnStatus(status, reader["va_number"]?.ToString());

                                // === 5. GENERATE QR CODE (Untuk Tampilan) ===
                                if (status == "Upcoming" || status == "Completed")
                                {
                                    GenerateQrCode(orderIdStr, custName, shipName, depDate.ToString("yyyy-MM-dd"));
                                    PanelQrCode.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    PanelQrCode.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading details: " + ex.Message);
            }
        }

        private void GenerateQrCode(string bookingID, string name, string ship, string date)
        {
            try
            {
                string payload = $"SHIPMANK|{bookingID}|{name}|{ship}|{date}";
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                ImgQrCode.Source = ByteToImage(qrCodeBytes);
            }
            catch { }
        }

        private BitmapImage ByteToImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private void UpdateUIBasedOnStatus(string status, string vaNumber)
        {
            var brushConverter = new BrushConverter();

            if (status == "Upcoming")
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#E0E7FF");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#3730A3");
                BtnCancel.Visibility = Visibility.Visible;
                BtnDownload.Visibility = Visibility.Visible;
                AlertUnpaid.Visibility = Visibility.Collapsed;
            }
            else if (status == "Completed")
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#DCFCE7");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#166534");
                BtnCancel.Visibility = Visibility.Collapsed;
                BtnDownload.Visibility = Visibility.Visible;
                AlertUnpaid.Visibility = Visibility.Collapsed;
            }
            else if (status == "Unpaid")
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#FEF9C3");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#854D0E");
                TxtVANumber.Text = string.IsNullOrEmpty(vaNumber) ? "Generating..." : vaNumber;
                AlertUnpaid.Visibility = Visibility.Visible;
                BtnCancel.Visibility = Visibility.Visible;
                BtnDownload.Visibility = Visibility.Collapsed;
            }
            else // Cancelled
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#FEE2E2");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#991B1B");
                BtnCancel.Visibility = Visibility.Collapsed;
                BtnDownload.Visibility = Visibility.Collapsed;
                AlertUnpaid.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel this booking?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                CancelOrder();
            }
        }

        private void CancelOrder()
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
                            string sqlBooking = "UPDATE Booking SET status = 'Cancelled' WHERE bookingID = @bid";
                            using (var cmd = new NpgsqlCommand(sqlBooking, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("bid", _bookingID);
                                cmd.ExecuteNonQuery();
                            }

                            string sqlPayment = "UPDATE Payment SET paymentStatus = 'Cancelled' WHERE bookingID = @bid";
                            using (var cmd = new NpgsqlCommand(sqlPayment, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("bid", _bookingID);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show("Order cancelled successfully.", "Info");
                            LoadFullDetails();
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
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            saveFileDialog.FileName = $"Receipt_{TxtOrderID.Text}.pdf";

            if (saveFileDialog.ShowDialog() == true)
            {
                GeneratePdfReceipt(saveFileDialog.FileName);
                MessageBox.Show("Receipt downloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GeneratePdfReceipt(string filePath)
        {
            // Mengambil data langsung dari UI
            var bookingId = TxtOrderID.Text;
            var status = TxtStatus.Text;
            var custName = TxtCustName.Text;
            var shipName = TxtShipName.Text;
            var route = TxtRoute.Text;
            var date = TxtDate.Text;
            var total = TxtTotal.Text;

            // Generate QR Code Byte Array khusus untuk PDF
            byte[] qrBytes = null;
            if (PanelQrCode.Visibility == Visibility.Visible)
            {
                string payload = $"SHIPMANK|{bookingId}|{custName}|{shipName}|{date}";
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                qrBytes = qrCode.GetGraphic(20);
            }

            // Mulai buat PDF dengan QuestPDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);

                    page.PageColor(QColors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SHIPMANK").SemiBold().FontSize(20).FontColor(QColors.Blue.Medium);
                                col.Item().Text("E-Ticket Receipt").FontSize(10).FontColor(QColors.Grey.Medium);
                            });

                            row.ConstantItem(100).AlignRight().Column(col =>
                            {
                                col.Item().Text(status).Bold().FontColor(status == "COMPLETED" ? QColors.Green.Medium : QColors.Blue.Medium);
                                col.Item().Text(DateTime.Now.ToString("dd MMM yyyy")).FontSize(8);
                            });
                        });

                    // --- CONTENT ---
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().Border(1).BorderColor(QColors.Grey.Lighten2).Padding(10).Column(inner =>
                        {
                            inner.Item().Text("Booking Details").Bold().FontSize(12).Underline();
                            inner.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Booking ID:");
                                row.RelativeItem().AlignRight().Text(bookingId).Bold();
                            });
                            inner.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Customer Name:");
                                row.RelativeItem().AlignRight().Text(custName);
                            });
                            inner.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Phone:");
                                row.RelativeItem().AlignRight().Text(TxtCustPhone.Text);
                            });
                        });

                        col.Item().Height(10);

                        col.Item().Border(1).BorderColor(QColors.Grey.Lighten2).Padding(10).Column(inner =>
                        {
                            inner.Item().Text("Trip Information").Bold().FontSize(12).Underline();
                            inner.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Ship Name:");
                                row.RelativeItem().AlignRight().Text(shipName);
                            });
                            inner.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Route:");
                                row.RelativeItem().AlignRight().Text(route);
                            });
                            inner.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Departure Date:");
                                row.RelativeItem().AlignRight().Text(date);
                            });
                            inner.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Ship Type:");
                                row.RelativeItem().AlignRight().Text(TxtShipType.Text);
                            });
                        });

                        col.Item().Height(10);

                        col.Item().Border(1).BorderColor(QColors.Grey.Lighten2).Padding(10).Column(inner =>
                        {
                            inner.Item().Text("Payment Summary").Bold().FontSize(12).Underline();
                            inner.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Payment Method:");
                                row.RelativeItem().AlignRight().Text(TxtPaymentMethod.Text);
                            });
                            inner.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Paid At:");
                                row.RelativeItem().AlignRight().Text(TxtPaymentDate.Text);
                            });
                            inner.Item().PaddingTop(10).LineHorizontal(1).LineColor(QColors.Grey.Lighten2);
                            inner.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Total Amount").Bold().FontSize(14);
                                row.RelativeItem().AlignRight().Text(total).Bold().FontSize(14).FontColor(QColors.Green.Darken2);
                            });
                        });

                        if (qrBytes != null)
                        {
                            col.Item().PaddingTop(20).AlignMiddle().AlignCenter().Column(c =>
                            {
                                c.Item().Width(100).Image(qrBytes);
                                c.Item().Text("Scan this QR at the port").FontSize(8).Italic().FontColor(QColors.Grey.Darken1);
                            });
                        }
                    });
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Thank you for choosing ShipMank. ");
                            x.Span("Contact support: support@shipmank.com");
                        });
                });
            }).GeneratePdf(filePath);
        }
    }
}