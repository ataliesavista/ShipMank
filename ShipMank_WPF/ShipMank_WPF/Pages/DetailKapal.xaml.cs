using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Perlu untuk Cursors
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Npgsql;
using NpgsqlTypes;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Pages
{
    public partial class DetailKapal : Page
    {
        public Window HostWindow { get; set; }
        public ShipMank_WPF.Pages.BeliTiket.ShipData ShipInfo { get; set; }

        private List<string> _allImages = new List<string>();
        private int _currentImageIndex = 0;
        private int _kapalId;

        // Simpan background asli (Gradient) dari XAML di sini
        private Brush _originalButtonBackground;

        // Warna Status Teks
        private readonly SolidColorBrush _colorAvailable = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
        private readonly SolidColorBrush _colorUnavailable = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0392B"));

        public DetailKapal(int kapalId)
        {
            InitializeComponent();
            _kapalId = kapalId;
            this.DataContext = this;
            this.Loaded += DetailKapal_Loaded;
        }

        private void DetailKapal_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Simpan desain asli tombol (Gradient) saat pertama kali load
            if (_originalButtonBackground == null) _originalButtonBackground = BookNowButton.Background;

            if (ShipInfo != null)
            {
                LoadAllImages(_kapalId);

                // Setup DatePicker
                BookingDatePicker.SelectedDate = DateTime.Now;
                BookingDatePicker.DisplayDateStart = DateTime.Now;
                BookingDatePicker.SelectedDateChanged += (s, args) =>
                {
                    if (BookingDatePicker.SelectedDate.HasValue)
                        CheckAvailability(BookingDatePicker.SelectedDate.Value);
                };

                // Cek awal
                CheckAvailability(DateTime.Now);
            }
        }

        // =================================================================
        // LOGIC KETERSEDIAAN (DATABASE & UI)
        // =================================================================

        private void CheckAvailability(DateTime selectedDate)
        {
            // 1. Cek Status Global Kapal (Maintenance/Rusak)
            if (ShipInfo != null && !string.Equals(ShipInfo.KapalStatus, "Available", StringComparison.OrdinalIgnoreCase))
            {
                SetUiStatus(false, "Unavailable (Maintenance)");
                return;
            }

            // 2. Cek Database Booking
            bool isBooked = IsShipBookedOnDate(_kapalId, selectedDate);
            SetUiStatus(!isBooked, isBooked ? "Unavailable / Booked" : "Available");
        }

        private bool IsShipBookedOnDate(int kapalId, DateTime date)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Booking WHERE kapalID = @id AND dateBerangkat = @tgl";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", kapalId);
                        cmd.Parameters.Add(new NpgsqlParameter("@tgl", NpgsqlDbType.Date) { Value = date });

                        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Checking Availability: {ex.Message}");
                return true; // Anggap penuh jika error agar aman
            }
        }

        // === BAGIAN MODIFIKASI BUTTON YANG RINGKAS ===
        private void SetUiStatus(bool isAvailable, string textStatus)
        {
            if (BookNowButton != null)
            {
                BookNowButton.IsEnabled = isAvailable;
                BookNowButton.Content = isAvailable ? "Book Now" : "Fully Booked";
                // Jika Available -> Pakai Original (Gradient), Jika Tidak -> Pakai Abu-abu
                BookNowButton.Background = isAvailable ? _originalButtonBackground : Brushes.Gray;
                BookNowButton.Cursor = isAvailable ? Cursors.Hand : Cursors.Arrow;
            }

            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = textStatus;
                StatusTextBlock.Foreground = isAvailable ? _colorAvailable : _colorUnavailable;
            }
        }

        // =================================================================
        // LOGIC GAMBAR, NAVIGASI & UTILITIES
        // =================================================================

        private void BookNowButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime selectedDate = BookingDatePicker.SelectedDate ?? DateTime.Now;

            // Double Check Race Condition
            if (IsShipBookedOnDate(_kapalId, selectedDate))
            {
                MessageBox.Show("Maaf, tanggal ini baru saja dipesan orang lain.", "Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                CheckAvailability(selectedDate);
                return;
            }

            HostWindow?.Close();
            if (Application.Current.MainWindow is MainWindow mw)
                mw.MainFrame.Navigate(new Payment(ShipInfo, selectedDate));
        }

        private void LoadAllImages(int kapalId)
        {
            _allImages.Clear();
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = "SELECT imagePath FROM KapalImages WHERE kapalID = @KapalID ORDER BY isPrimary DESC, imageID ASC";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@KapalID", kapalId);
                        using (var reader = cmd.ExecuteReader())
                            while (reader.Read()) _allImages.Add(reader["imagePath"].ToString());
                    }
                }
            }
            catch { }

            if (_allImages.Count == 0 && !string.IsNullOrEmpty(ShipInfo?.ImageSource))
                _allImages.Add(ShipInfo.ImageSource);

            if (_allImages.Count > 0) { _currentImageIndex = 0; UpdateMainImage(); }
        }

        private void UpdateMainImage()
        {
            if (_allImages.Count > 0 && _currentImageIndex >= 0)
                try { MainImage.Source = new BitmapImage(new Uri(_allImages[_currentImageIndex], UriKind.RelativeOrAbsolute)); } catch { }
        }

        private void PrevImage_Click(object sender, RoutedEventArgs e)
        {
            if (_allImages.Count <= 1) return;
            _currentImageIndex = (_currentImageIndex - 1 < 0) ? _allImages.Count - 1 : _currentImageIndex - 1;
            UpdateMainImage();
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            if (_allImages.Count <= 1) return;
            _currentImageIndex = (_currentImageIndex + 1 >= _allImages.Count) ? 0 : _currentImageIndex + 1;
            UpdateMainImage();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => HostWindow?.Close();
    }
}