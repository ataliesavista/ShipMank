using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using System;
using System.IO;
using QColors = QuestPDF.Helpers.Colors;

namespace ShipMank_WPF.Models.Services
{
    public static class PdfService
    {
        public static void GenerateReceipt(string filePath, BookingDetailInfo info)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Generate QR Code untuk PDF
            byte[] qrBytes = GenerateQrBytes(info);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);
                    page.PageColor(QColors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SHIPMANK").SemiBold().FontSize(20).FontColor(QColors.Blue.Medium);
                            col.Item().Text("E-Ticket Receipt").FontSize(10).FontColor(QColors.Grey.Medium);
                        });
                        row.ConstantItem(100).AlignRight().Column(col =>
                        {
                            col.Item().Text(info.Status.ToUpper()).Bold().FontColor(info.Status == "Completed" ? QColors.Green.Medium : QColors.Blue.Medium);
                            col.Item().Text(DateTime.Now.ToString("dd MMM yyyy")).FontSize(8);
                        });
                    });

                    // ISIIII
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().Border(1).BorderColor(QColors.Grey.Lighten2).Padding(10).Column(inner =>
                        {
                            inner.Item().Text("Booking Details").Bold().FontSize(12).Underline();
                            AddRow(inner, "Booking ID:", info.OrderID);
                            AddRow(inner, "Customer Name:", info.CustName);
                            AddRow(inner, "Phone:", info.CustPhone);
                        });

                        col.Item().Height(10);

                        col.Item().Border(1).BorderColor(QColors.Grey.Lighten2).Padding(10).Column(inner =>
                        {
                            inner.Item().Text("Trip Information").Bold().FontSize(12).Underline();
                            AddRow(inner, "Ship Name:", info.ShipName);
                            AddRow(inner, "Route:", info.Route);
                            AddRow(inner, "Departure Date:", info.DepartureDate.ToString("dd MMM yyyy"));
                            AddRow(inner, "Ship Type:", info.ShipType);
                        });

                        col.Item().Height(10);

                        col.Item().Border(1).BorderColor(QColors.Grey.Lighten2).Padding(10).Column(inner =>
                        {
                            inner.Item().Text("Payment Summary").Bold().FontSize(12).Underline();
                            AddRow(inner, "Payment Method:", info.PaymentMethod ?? "-");
                            AddRow(inner, "Paid At:", info.PaymentDate?.ToString("dd MMM yyyy, HH:mm") ?? "Waiting Payment");

                            inner.Item().PaddingTop(10).LineHorizontal(1).LineColor(QColors.Grey.Lighten2);
                            inner.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Total Amount").Bold().FontSize(14);
                                row.RelativeItem().AlignRight().Text($"Rp {info.TotalPaid:N0}").Bold().FontSize(14).FontColor(QColors.Green.Darken2);
                            });
                        });

                        // QR Code
                        if (qrBytes != null)
                        {
                            col.Item().PaddingTop(20).AlignMiddle().AlignCenter().Column(c =>
                            {
                                c.Item().Width(100).Image(qrBytes);
                                c.Item().Text("Scan this QR at the port").FontSize(8).Italic().FontColor(QColors.Grey.Darken1);
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Thank you for choosing ShipMank. Contact support: support@shipmank.com");
                    });
                });
            }).GeneratePdf(filePath);
        }

        private static void AddRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().PaddingTop(2).Row(row =>
            {
                row.RelativeItem().Text(label);
                row.RelativeItem().AlignRight().Text(value);
            });
        }

        private static byte[] GenerateQrBytes(BookingDetailInfo info)
        {
            if (info.Status != "Upcoming" && info.Status != "Completed") return null;
            try
            {
                string payload = $"SHIPMANK|{info.OrderID}|{info.CustName}|{info.ShipName}|{info.DepartureDate:yyyy-MM-dd}";
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                return qrCode.GetGraphic(20);
            }
            catch { return null; }
        }
    }
}