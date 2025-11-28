using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using QRCoder;
using ShipMank_WPF.Models;
using ShipMank_WPF.Models.Services; // Tambahkan namespace Service

namespace ShipMank_WPF.Pages
{
    public partial class ViewDetails : Page
    {
        private int _bookingID;
        // Simpan data detail di variabel class agar bisa dipakai saat download PDF
        private BookingDetailInfo _currentDetail;

        public ViewDetails(OrderHistoryItem historyItem)
        {
            InitializeComponent();
            _bookingID = historyItem.OriginalBookingID;
            RefreshUI();
        }

        public ViewDetails() { InitializeComponent(); }

        private void RefreshUI()
        {
            // 1. PANGGIL SERVICE (Load Data)
            _currentDetail = BookingDetailService.GetBookingDetails(_bookingID);

            if (_currentDetail != null)
            {
                // 2. Mapping ke UI
                TxtOrderID.Text = _currentDetail.OrderID;
                TxtCustName.Text = _currentDetail.CustName;
                TxtCustEmail.Text = _currentDetail.CustEmail;
                TxtCustPhone.Text = _currentDetail.CustPhone;
                TxtShipName.Text = _currentDetail.ShipName;
                TxtShipType.Text = _currentDetail.ShipType;
                TxtRoute.Text = _currentDetail.Route;
                TxtDate.Text = _currentDetail.DepartureDate.ToString("dddd, dd MMM yyyy");
                TxtBookingDate.Text = _currentDetail.BookingDate.ToString("dd MMM yyyy, HH:mm");
                TxtTotal.Text = $"Rp {_currentDetail.TotalPaid:N0}";
                TxtStatus.Text = _currentDetail.Status.ToUpper();
                TxtPaymentMethod.Text = string.IsNullOrEmpty(_currentDetail.PaymentMethod) ? "-" : _currentDetail.PaymentMethod;

                if (_currentDetail.PaymentDate.HasValue)
                    TxtPaymentDate.Text = _currentDetail.PaymentDate.Value.ToString("dd MMM yyyy, HH:mm");
                else
                {
                    TxtPaymentDate.Text = "Waiting Payment";
                    TxtPaymentDate.Foreground = Brushes.OrangeRed;
                }

                UpdateUIBasedOnStatus(_currentDetail.Status, _currentDetail.VaNumber);
                GenerateQrCodeForUI(_currentDetail);
            }
        }

        private void UpdateUIBasedOnStatus(string status, string vaNumber)
        {
            var brushConverter = new BrushConverter();
            if (status == "Upcoming")
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#E0E7FF");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#3730A3");
                BtnCancel.Visibility = Visibility.Visible; BtnDownload.Visibility = Visibility.Visible; AlertUnpaid.Visibility = Visibility.Collapsed;
            }
            else if (status == "Completed")
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#DCFCE7");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#166534");
                BtnCancel.Visibility = Visibility.Collapsed; BtnDownload.Visibility = Visibility.Visible; AlertUnpaid.Visibility = Visibility.Collapsed;
            }
            else if (status == "Unpaid")
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#FEF9C3");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#854D0E");
                TxtVANumber.Text = string.IsNullOrEmpty(vaNumber) ? "Generating..." : vaNumber;
                AlertUnpaid.Visibility = Visibility.Visible; BtnCancel.Visibility = Visibility.Visible; BtnDownload.Visibility = Visibility.Collapsed;
            }
            else // Cancelled
            {
                StatusBadge.Background = (Brush)brushConverter.ConvertFrom("#FEE2E2");
                TxtStatus.Foreground = (Brush)brushConverter.ConvertFrom("#991B1B");
                BtnCancel.Visibility = Visibility.Collapsed; BtnDownload.Visibility = Visibility.Collapsed; AlertUnpaid.Visibility = Visibility.Collapsed;
            }
        }

        private void GenerateQrCodeForUI(BookingDetailInfo info)
        {
            if (info.Status == "Upcoming" || info.Status == "Completed")
            {
                try
                {
                    string payload = $"SHIPMANK|{info.OrderID}|{info.CustName}|{info.ShipName}|{info.DepartureDate:yyyy-MM-dd}";
                    var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new PngByteQRCode(qrCodeData);

                    using (var ms = new MemoryStream(qrCode.GetGraphic(20)))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        ImgQrCode.Source = image;
                    }
                    PanelQrCode.Visibility = Visibility.Visible;
                }
                catch { }
            }
            else
            {
                PanelQrCode.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel this booking?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // 3. PANGGIL SERVICE (Cancel)
                if (BookingDetailService.CancelBooking(_bookingID))
                {
                    MessageBox.Show("Order cancelled successfully.", "Info");
                    RefreshUI(); // Refresh data
                }
            }
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDetail == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            saveFileDialog.FileName = $"Receipt_{_currentDetail.OrderID}.pdf";

            if (saveFileDialog.ShowDialog() == true)
            {
                // 4. PANGGIL SERVICE (Generate PDF)
                try
                {
                    PdfService.GenerateReceipt(saveFileDialog.FileName, _currentDetail);
                    MessageBox.Show("Receipt downloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to generate PDF: {ex.Message}");
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }
}