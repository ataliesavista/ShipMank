using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShipMank_WPF.Models;
using ShipMank_WPF.Models.ViewModel;
using ShipMank_WPF.Models.Services;

namespace ShipMank_WPF.Pages
{
    public partial class DetailKapal : Page
    {
        public Window HostWindow { get; set; }
        public ShipViewModel ShipInfo { get; set; }

        private List<string> _allImages;
        private int _currentImageIndex = 0;
        private int _kapalId;
        private Brush _originalButtonBackground;

        public DetailKapal(int kapalId)
        {
            InitializeComponent();
            _kapalId = kapalId;
            DataContext = this;
            Loaded += (s, e) => {
                if (_originalButtonBackground == null) _originalButtonBackground = BookNowButton.Background;
                if (ShipInfo != null)
                {
                    // 1. Panggil Method Static di Model Kapal
                    _allImages = Kapal.GetImages(_kapalId, ShipInfo.ImageSource);
                    UpdateMainImage();

                    BookingDatePicker.SelectedDate = DateTime.Now;
                    BookingDatePicker.DisplayDateStart = DateTime.Now;
                    CheckAvailability(DateTime.Now);
                }
            };

            BookingDatePicker.SelectedDateChanged += (s, e) => {
                if (BookingDatePicker.SelectedDate.HasValue)
                    CheckAvailability(BookingDatePicker.SelectedDate.Value);
            };
        }

        private void CheckAvailability(DateTime date)
        {
            if (ShipInfo?.KapalStatus != "Available") { SetUiStatus(false, "Unavailable (Maintenance)"); return; }

            // 2. Panggil Method Static di Model Booking
            bool isBooked = Booking.IsDateBooked(_kapalId, date);
            SetUiStatus(!isBooked, isBooked ? "Unavailable / Booked" : "Available");
        }

        private void SetUiStatus(bool isAvailable, string text)
        {
            BookNowButton.IsEnabled = isAvailable;
            BookNowButton.Content = isAvailable ? "Book Now" : "Fully Booked";
            BookNowButton.Background = isAvailable ? _originalButtonBackground : Brushes.Gray;
            StatusTextBlock.Text = text;
            StatusTextBlock.Foreground = isAvailable ? Brushes.Green : Brushes.Red;
        }

        private void BookNowButton_Click(object sender, RoutedEventArgs e)
        {
            var date = BookingDatePicker.SelectedDate ?? DateTime.Now;
            if (Booking.IsDateBooked(_kapalId, date)) // Cek lagi
            {
                MessageBox.Show("Tanggal ini sudah dipesan.", "Full", MessageBoxButton.OK, MessageBoxImage.Warning);
                CheckAvailability(date);
                return;
            }

            HostWindow?.Close();
            (Application.Current.MainWindow as MainWindow)?.MainFrame.Navigate(new Payment(ShipInfo, date));
        }

        // Logic Gambar UI
        private void UpdateMainImage()
        {
            if (_allImages?.Count > 0) try { MainImage.Source = new BitmapImage(new Uri(_allImages[_currentImageIndex], UriKind.RelativeOrAbsolute)); } catch { }
        }
        private void PrevImage_Click(object sender, RoutedEventArgs e)
        {
            if (_allImages.Count > 1) { _currentImageIndex = (_currentImageIndex - 1 < 0) ? _allImages.Count - 1 : _currentImageIndex - 1; UpdateMainImage(); }
        }
        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            if (_allImages.Count > 1) { _currentImageIndex = (_currentImageIndex + 1 >= _allImages.Count) ? 0 : _currentImageIndex + 1; UpdateMainImage(); }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e) => HostWindow?.Close();
    }
}