using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ShipMank_WPF.Components;
using Npgsql;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Pages
{
    public partial class BeliTiket : Page
    {
        // ... (Class ShipData tidak berubah) ...
        public class ShipData
        {
            public int KapalID { get; set; }
            public string ShipName { get; set; }
            public string ShipClass { get; set; }
            public string Location { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string Province { get; set; }
            public string FullLocation => $"{Address}, {City}, {Province}";
            public string Capacity { get; set; }
            public string Rating { get; set; }
            public string Price { get; set; }
            public string PriceUnit { get; set; } = "/day";
            public string KapalStatus { get; set; }
            public List<string> Facilities { get; set; } = new List<string>();
            public string ImageSource { get; set; }
            public string BadgeColor { get; set; } = "#2980B9";
        }

        private List<ShipData> masterShipList = new List<ShipData>();

        // Constants
        private const string SearchPlaceholder = "Search boat name...";
        private const string LocationPlaceholder = "e.g. Bali, Jakarta";

        public BeliTiket()
        {
            InitializeComponent();

            // 1. Load Filter Data (Ship Types) DULUAN
            LoadShipTypes();

            // 2. Load Data Kapal
            LoadInitialData();

            InitializePlaceholders();

            // Wiring Events
            TypeComboBox.SelectionChanged += Filter_Changed;
            CapacityComboBox.SelectionChanged += Filter_Changed;
            RateComboBox.SelectionChanged += Filter_Changed;
            PriceSlider.ValueChanged += Filter_Changed;

            // Initial Filter Application
            ApplyFilters();
        }

        // =========================================================
        // [BARU] LOAD SHIP TYPE DARI DATABASE
        // =========================================================
        private void LoadShipTypes()
        {
            try
            {
                // Bersihkan item jika ada sisa dari XAML atau reload
                TypeComboBox.Items.Clear();

                // Tambahkan opsi Default "All Types"
                ComboBoxItem defaultItem = new ComboBoxItem { Content = "All Types", IsSelected = true };
                TypeComboBox.Items.Add(defaultItem);

                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    // Ambil nama tipe kapal dari tabel ShipType
                    string sql = "SELECT typeName FROM ShipType ORDER BY typeName ASC";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string typeName = reader["typeName"].ToString();

                                // Tambahkan ke ComboBox sebagai ComboBoxItem
                                // Kita bungkus dalam ComboBoxItem agar fungsi GetComboBoxValue tetap bekerja
                                TypeComboBox.Items.Add(new ComboBoxItem { Content = typeName });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat tipe kapal: {ex.Message}");
            }
        }

        private void LoadInitialData()
        {
            masterShipList = new List<ShipData>();

            try
            {
                string connString = DBHelper.GetConnectionString();

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            k.kapalID, k.namakapal, k.kapasitas, k.rating, k.hargaperjalanan, k.kapalStatus,
                            s.typename, 
                            l.city, l.address, l.province,
                            k.fasilitas,
                            ki.imagePath
                        FROM Kapal k
                        INNER JOIN ShipType s ON k.shiptype = s.typeid
                        LEFT JOIN Lokasi l ON k.lokasi = l.portid
                        LEFT JOIN KapalImages ki ON k.kapalID = ki.kapalID AND ki.isPrimary = TRUE;
                    ";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int kapasitas = reader["kapasitas"] is int cap ? cap : 0;
                                object ratingObj = reader["rating"];
                                double rating = ratingObj is DBNull ? 0.0 : Convert.ToDouble(ratingObj);
                                object hargaObj = reader["hargaperjalanan"];
                                decimal hargaDecimal = hargaObj is DBNull ? 0M : (decimal)hargaObj;

                                string fasilitasText = reader["fasilitas"]?.ToString() ?? "";
                                List<string> facilitiesList = fasilitasText.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                                           .Select(f => f.Trim())
                                                                           .ToList();

                                string dbAddress = reader["address"] is DBNull ? "-" : reader["address"].ToString();
                                string dbCity = reader["city"] is DBNull ? "-" : reader["city"].ToString();
                                string dbProvince = reader["province"] is DBNull ? "-" : reader["province"].ToString();
                                string dbStatus = reader["kapalStatus"] is DBNull ? "Available" : reader["kapalStatus"].ToString();
                                string dbImage = reader["imagePath"] is DBNull ? "/Resources/default_ship.jpg" : reader["imagePath"].ToString();

                                masterShipList.Add(new ShipData
                                {
                                    KapalID = (int)reader["kapalID"],
                                    ShipName = reader["namakapal"].ToString(),
                                    ShipClass = reader["typename"].ToString(),
                                    Location = dbProvince,
                                    Address = dbAddress,
                                    City = dbCity,
                                    Province = dbProvince,
                                    KapalStatus = dbStatus,
                                    Capacity = $"{kapasitas} Penumpang",
                                    Rating = rating.ToString("F1", CultureInfo.InvariantCulture),
                                    Price = "Rp " + hargaDecimal.ToString("N0", new CultureInfo("id-ID")),
                                    Facilities = facilitiesList,
                                    ImageSource = dbImage,
                                    BadgeColor = GetBadgeColor(reader["typename"].ToString())
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data: {ex.Message}");
            }
        }

        private string GetBadgeColor(string shipType)
        {
            // Update logika warna agar dinamis (fallback warna default jika tipe baru ditambah di DB)
            string type = shipType.ToLower();
            if (type.Contains("phinisi")) return "#8E44AD";
            else if (type.Contains("speedboat")) return "#2980B9";
            else if (type.Contains("yacht")) return "#F39C12";
            else if (type.Contains("ferry")) return "#27AE60";
            else return "#16A085"; // Warna default untuk tipe kapal baru
        }

        // ... (ShowDetailKapalPopup TIDAK BERUBAH) ...
        private void ShowDetailKapalPopup(ShipData ship)
        {
            Window modalWindow = new Window
            {
                Title = "Detail Kapal",
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowState = WindowState.Maximized,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                ShowInTaskbar = false
            };

            Grid overlayGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
            };

            DetailKapal detailPage = new DetailKapal(ship.KapalID)
            {
                HostWindow = modalWindow,
                ShipInfo = ship
            };

            Frame contentFrame = new Frame
            {
                Content = detailPage,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Background = Brushes.Transparent
            };

            overlayGrid.Children.Add(contentFrame);
            modalWindow.Content = overlayGrid;
            modalWindow.ShowDialog();
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBoatNameTextBox.Text = SearchPlaceholder;
            SearchBoatNameTextBox.Foreground = Brushes.Gray;
            LocationTextBox.Text = LocationPlaceholder;
            LocationTextBox.Foreground = Brushes.Gray;

            // Index 0 adalah "All Types" yang kita tambahkan secara coding
            TypeComboBox.SelectedIndex = 0;

            CapacityComboBox.SelectedIndex = 0;
            RateComboBox.SelectedIndex = 0;
            PriceSlider.Value = 20000000;
            ApplyFilters();
        }

        // ... (Sisa method helper lainnya TIDAK BERUBAH) ...
        private void InitializePlaceholders()
        {
            if (string.IsNullOrEmpty(SearchBoatNameTextBox.Text)) { SearchBoatNameTextBox.Text = SearchPlaceholder; SearchBoatNameTextBox.Foreground = Brushes.Gray; }
            if (string.IsNullOrEmpty(LocationTextBox.Text)) { LocationTextBox.Text = LocationPlaceholder; LocationTextBox.Foreground = Brushes.Gray; }
        }

        private void Filter_Changed(object sender, object e) => ApplyFilters();

        private void ApplyFilters()
        {
            if (ShipCardGrid == null) return;

            string searchText = (SearchBoatNameTextBox.Text == SearchPlaceholder) ? "" : SearchBoatNameTextBox.Text;
            string locationText = (LocationTextBox.Text == LocationPlaceholder) ? "" : LocationTextBox.Text;

            string selectedType = GetComboBoxValue(TypeComboBox);

            string selectedCapacity = GetComboBoxValue(CapacityComboBox);
            string selectedRate = GetComboBoxValue(RateComboBox);
            double maxPrice = PriceSlider.Value;

            var filteredList = masterShipList.Where(ship =>
            {
                bool matchName = string.IsNullOrEmpty(searchText) || ship.ShipName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchLocation = string.IsNullOrEmpty(locationText) ||
                                     ship.Province.IndexOf(locationText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     ship.City.IndexOf(locationText, StringComparison.OrdinalIgnoreCase) >= 0;

                // Logic "All Types"
                bool matchType = selectedType == "All Types" || selectedType == null || ship.ShipClass.Equals(selectedType, StringComparison.OrdinalIgnoreCase);

                bool matchRate = true;
                if (selectedRate != "Any" && selectedRate != null && double.TryParse(ship.Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out double shipRating))
                {
                    if (double.TryParse(selectedRate, out double filterRate)) matchRate = shipRating >= filterRate;
                }
                bool matchPrice = ParsePrice(ship.Price) <= maxPrice;
                int shipCapVal = ParseCapacity(ship.Capacity);
                bool matchCapacity = true;
                if (selectedCapacity == "2 - 5 People") matchCapacity = (shipCapVal >= 2 && shipCapVal <= 5);
                else if (selectedCapacity == "5 - 10 People") matchCapacity = (shipCapVal > 5 && shipCapVal <= 10);
                else if (selectedCapacity == "10+ People") matchCapacity = (shipCapVal > 10);

                return matchName && matchLocation && matchType && matchRate && matchPrice && matchCapacity;
            }).ToList();

            UpdateShipCardGrid(filteredList);
        }

        private void UpdateShipCardGrid(List<ShipData> ships)
        {
            ShipCardGrid.Children.Clear();
            if (ships.Count == 0) return;

            foreach (var ship in ships)
            {
                ShipCard newCard = new ShipCard
                {
                    ShipName = ship.ShipName,
                    ShipClass = ship.ShipClass,
                    Location = ship.Location,
                    Capacity = ship.Capacity,
                    Rating = ship.Rating,
                    Price = ship.Price,
                    PriceUnit = ship.PriceUnit,
                    Facilities = ship.Facilities,
                    KapalID = ship.KapalID,
                    ImageSource = ship.ImageSource
                };

                newCard.DetailButtonClicked += ShipCard_ButtonClicked;
                ShipCardGrid.Children.Add(newCard);
            }
        }

        private void ShipCard_ButtonClicked(object sender, int kapalID)
        {
            ShipData selectedShip = masterShipList.FirstOrDefault(s => s.KapalID == kapalID);
            if (selectedShip != null)
            {
                ShowDetailKapalPopup(selectedShip);
            }
        }

        private string GetComboBoxValue(ComboBox cb)
        {
            // Karena kita memasukkan ComboBoxItem secara manual di LoadShipTypes, logic ini tetap aman
            if (cb.SelectedItem is ComboBoxItem item) return item.Content.ToString();
            return null;
        }

        private double ParsePrice(string priceStr)
        {
            string clean = priceStr.Replace("Rp", "").Replace(".", "").Replace(" ", "").Trim();
            if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)) return result;
            return 0;
        }

        private int ParseCapacity(string capacityStr)
        {
            string numberPart = new string(capacityStr.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(numberPart, out int result)) return result;
            return 0;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;
            if (tb.Text == SearchPlaceholder || tb.Text == LocationPlaceholder) { tb.Text = ""; tb.Foreground = Brushes.Black; }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                if (tb.Name == "SearchBoatNameTextBox") { tb.Text = SearchPlaceholder; tb.Foreground = Brushes.Gray; }
                else if (tb.Name == "LocationTextBox") { tb.Text = LocationPlaceholder; tb.Foreground = Brushes.Gray; }
            }
        }

        private void MainScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 3.0);
            e.Handled = true;
        }
    }
}