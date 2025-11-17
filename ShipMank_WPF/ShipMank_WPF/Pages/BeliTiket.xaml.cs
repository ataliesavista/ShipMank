using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows; // Ditambahkan untuk GotFocus/LostFocus
using ShipMank_WPF.Components; // <-- PERBAIKAN 1: Tambahkan using directive ini

namespace ShipMank_WPF.Pages
{
    /// <summary>
    /// Interaction logic for BeliTiket.xaml
    /// </summary>
    public partial class BeliTiket : Page
    {
        // Kelas internal untuk menyimpan data mock
        private class ShipData
        {
            public string ShipName { get; set; }
            public string ShipClass { get; set; }
            public string Location { get; set; }
            public string Capacity { get; set; }
            public string Rating { get; set; }
            public string Price { get; set; }
            public string PriceUnit { get; set; }
            public string Route { get; set; }
            public string Seats { get; set; }
            public string DepartureTime { get; set; }
            public string Duration { get; set; }
            public string BadgeColor { get; set; }
        }

        // Daftar master untuk semua data kapal (data asli)
        private List<ShipData> masterShipList = new List<ShipData>();

        // Teks placeholder
        private const string SearchPlaceholder = "Search boat name...";
        private const string LocationPlaceholder = "Enter location...";

        public BeliTiket()
        {
            InitializeComponent();
            LoadInitialData(); // Memuat data mock
            InitializePlaceholders(); // Mengatur placeholder awal
            ApplyFilters(); // Menampilkan semua data saat pertama kali dimuat
        }

        /// <summary>
        /// Mengisi daftar master dengan data mock
        /// </summary>
        private void LoadInitialData()
        {
            masterShipList = new List<ShipData>
            {
                new ShipData
                {
                    ShipName = "Kapal Karam",
                    ShipClass = "Yacht",
                    Location = "Jakarta",
                    Capacity = "50 Penumpang",
                    Rating = "4.8",
                    Price = "Rp 15.000.000",
                    PriceUnit = "/hari",
                    Route = "Jakarta – Surabaya",
                    Seats = "50",
                    DepartureTime = "08:30",
                    Duration = "12 Jam",
                    BadgeColor = "Green"
                },
                new ShipData
                {
                    ShipName = "Speedy Boat",
                    ShipClass = "Boat",
                    Location = "Bali",
                    Capacity = "20 Penumpang",
                    Rating = "4.5",
                    Price = "Rp 5.000.000",
                    PriceUnit = "/hari",
                    Route = "Bali – Lombok",
                    Seats = "20",
                    DepartureTime = "10:00",
                    Duration = "3 Jam",
                    BadgeColor = "Blue"
                },
                new ShipData
                {
                    ShipName = "Perahu Nelayan",
                    ShipClass = "Perahu",
                    Location = "Jakarta",
                    Capacity = "10 Penumpang",
                    Rating = "4.2",
                    Price = "Rp 1.000.000",
                    PriceUnit = "/hari",
                    Route = "P. Seribu",
                    Seats = "10",
                    DepartureTime = "06:00",
                    Duration = "8 Jam",
                    BadgeColor = "Orange"
                },
                 new ShipData
                {
                    ShipName = "Yacht Mewah",
                    ShipClass = "Yacht",
                    Location = "Bali",
                    Capacity = "30 Penumpang",
                    Rating = "5.0",
                    Price = "Rp 25.000.000",
                    PriceUnit = "/hari",
                    Route = "Bali – Gili",
                    Seats = "30",
                    DepartureTime = "09:00",
                    Duration = "5 Jam",
                    BadgeColor = "Purple"
                }
                // Tambahkan data kapal lainnya di sini
            };
        }

        /// <summary>
        /// Mengatur teks placeholder awal
        /// </summary>
        private void InitializePlaceholders()
        {
            // Atur placeholder untuk Search
            SearchBoatNameTextBox.Text = SearchPlaceholder;
            SearchBoatNameTextBox.Foreground = Brushes.Gray;

            // Atur placeholder untuk Location
            LocationTextBox.Text = LocationPlaceholder;
            LocationTextBox.Foreground = Brushes.Gray;
        }

        /// <summary>
        /// Menerapkan filter berdasarkan input pengguna dan memperbarui UI
        /// </summary>
        private void ApplyFilters()
        {
            // Pastikan UI sudah siap
            if (ShipCardGrid == null) return;

            // Ambil teks filter, anggap string kosong jika masih placeholder
            string searchText = (SearchBoatNameTextBox.Text == SearchPlaceholder) ? "" : SearchBoatNameTextBox.Text;
            string locationText = (LocationTextBox.Text == LocationPlaceholder) ? "" : LocationTextBox.Text;

            // Terapkan filter menggunakan LINQ
            var filteredList = masterShipList
                .Where(ship => 
                    // Filter berdasarkan nama kapal (case-insensitive)
                    ship.ShipName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                    // Filter berdasarkan lokasi (case-insensitive)
                    ship.Location.IndexOf(locationText, StringComparison.OrdinalIgnoreCase) >= 0
                )
                .ToList();

            // Perbarui Grid UI
            UpdateShipCardGrid(filteredList);
        }

        /// <summary>
        /// Mengosongkan dan mengisi ulang ShipCardGrid dengan data yang difilter
        /// </summary>
        /// <param name="ships">Daftar kapal yang akan ditampilkan</param>
        private void UpdateShipCardGrid(List<ShipData> ships)
        {
            ShipCardGrid.Children.Clear(); // Hapus semua kartu yang ada

            foreach (var ship in ships)
            {
                // <-- PERBAIKAN 2: Hapus 'components.' dari baris di bawah
                ShipCard newCard = new ShipCard
                {
                    ShipName = ship.ShipName,
                    ShipClass = ship.ShipClass,
                    Location = ship.Location,
                    Capacity = ship.Capacity,
                    Rating = ship.Rating,
                    Price = ship.Price,
                    PriceUnit = ship.PriceUnit,
                    Route = ship.Route,
                    Seats = ship.Seats,
                    DepartureTime = ship.DepartureTime,
                    Duration = ship.Duration,
                    // BadgeColor = (SolidColorBrush)(new BrushConverter().ConvertFrom(ship.BadgeColor))
                };
                
                // Tambahkan kartu baru ke grid
                ShipCardGrid.Children.Add(newCard);
            }
        }

        // -- Event Handler untuk Kontrol --

        /// <summary>
        /// Dipanggil setiap kali teks di TextBox berubah
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Menghapus placeholder saat TextBox mendapatkan fokus
        /// </summary>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            // Cek apakah itu Search box
            if (textBox == SearchBoatNameTextBox && textBox.Text == SearchPlaceholder)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
            // Cek apakah itu Location box
            else if (textBox == LocationTextBox && textBox.Text == LocationPlaceholder)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        /// <summary>
        /// Mengembalikan placeholder jika TextBox kosong dan kehilangan fokus
        /// </summary>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Cek apakah itu Search box
                if (textBox == SearchBoatNameTextBox)
                {
                    textBox.Text = SearchPlaceholder;
                    textBox.Foreground = Brushes.Gray;
                }
                // Cek apakah itu Location box
                else if (textBox == LocationTextBox)
                {
                    textBox.Text = LocationPlaceholder;
                    textBox.Foreground = Brushes.Gray;
                }
            }
        }
    }
}