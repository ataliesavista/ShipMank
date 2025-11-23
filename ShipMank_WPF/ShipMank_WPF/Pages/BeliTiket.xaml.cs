using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShipMank_WPF.Components;

namespace ShipMank_WPF.Pages
{
    public partial class BeliTiket : Page
    {
        // Data Model
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

        private List<ShipData> masterShipList = new List<ShipData>();

        // Konstanta Placeholder
        private const string SearchPlaceholder = "Search boat name...";
        private const string LocationPlaceholder = "e.g. Bali, Jakarta";

        public BeliTiket()
        {
            InitializeComponent();
            LoadInitialData();
            InitializePlaceholders();

            // Wiring Events
            TypeComboBox.SelectionChanged += Filter_Changed;
            CapacityComboBox.SelectionChanged += Filter_Changed;
            RateComboBox.SelectionChanged += Filter_Changed;
            PriceSlider.ValueChanged += Filter_Changed;

            // Tampilkan data awal
            ApplyFilters();
        }

        private void LoadInitialData()
        {
            masterShipList = new List<ShipData>
            {
                new ShipData
                {
                    ShipName = "The Black Pearl",
                    ShipClass = "Phinisi",
                    Location = "Labuan Bajo",
                    Capacity = "12 Penumpang",
                    Rating = "5.0",
                    Price = "Rp 15.000.000",
                    PriceUnit = "/day",
                    Route = "Labuan Bajo – Komodo",
                    Seats = "12",
                    DepartureTime = "08:00",
                    Duration = "3 Days",
                    BadgeColor = "#8E44AD"
                },
                new ShipData
                {
                    ShipName = "Ocean Sprinter",
                    ShipClass = "Speedboat",
                    Location = "Bali",
                    Capacity = "4 Penumpang",
                    Rating = "4.5",
                    Price = "Rp 2.500.000",
                    PriceUnit = "/trip",
                    Route = "Sanur – Nusa Penida",
                    Seats = "4",
                    DepartureTime = "07:30",
                    Duration = "45 Mins",
                    BadgeColor = "#2980B9"
                },
                new ShipData
                {
                    ShipName = "Royal Horizon",
                    ShipClass = "Yacht",
                    Location = "Jakarta",
                    Capacity = "25 Penumpang",
                    Rating = "4.8",
                    Price = "Rp 18.000.000",
                    PriceUnit = "/day",
                    Route = "Ancol – P. Seribu",
                    Seats = "25",
                    DepartureTime = "09:00",
                    Duration = "8 Hours",
                    BadgeColor = "#F39C12"
                },
                new ShipData
                {
                    ShipName = "KMP Ferry",
                    ShipClass = "Ferry",
                    Location = "Banyuwangi",
                    Capacity = "100 Penumpang",
                    Rating = "4.0",
                    Price = "Rp 50.000",
                    PriceUnit = "/pax",
                    Route = "Ketapang – Gilimanuk",
                    Seats = "100",
                    DepartureTime = "Every Hour",
                    Duration = "1 Hour",
                    BadgeColor = "#27AE60"
                },
                new ShipData
                {
                    ShipName = "Sunset Chaser",
                    ShipClass = "Boat",
                    Location = "Lombok",
                    Capacity = "8 Penumpang",
                    Rating = "4.3",
                    Price = "Rp 1.200.000",
                    PriceUnit = "/day",
                    Route = "Gili Trawangan Tour",
                    Seats = "8",
                    DepartureTime = "10:00",
                    Duration = "6 Hours",
                    BadgeColor = "#16A085"
                }
            };
        }

        // ===========================================================
        // LOGIKA RESET FILTER (YANG BARU DITAMBAHKAN)
        // ===========================================================
        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Reset TextBox ke Placeholder
            SearchBoatNameTextBox.Text = SearchPlaceholder;
            SearchBoatNameTextBox.Foreground = Brushes.Gray;

            LocationTextBox.Text = LocationPlaceholder;
            LocationTextBox.Foreground = Brushes.Gray;

            // 2. Reset Dropdowns (Index 0 biasanya 'All' atau 'Any')
            TypeComboBox.SelectedIndex = 0;
            CapacityComboBox.SelectedIndex = 0;
            RateComboBox.SelectedIndex = 0;

            // 3. Reset Slider ke Harga Max (20 Juta)
            PriceSlider.Value = 20000000;

            // ApplyFilters akan otomatis terpanggil karena event change/selection
            // Tapi untuk memastikan sinkronisasi teks placeholder:
            ApplyFilters();
        }
        // ===========================================================

        private void InitializePlaceholders()
        {
            if (string.IsNullOrEmpty(SearchBoatNameTextBox.Text))
            {
                SearchBoatNameTextBox.Text = SearchPlaceholder;
                SearchBoatNameTextBox.Foreground = Brushes.Gray;
            }

            if (string.IsNullOrEmpty(LocationTextBox.Text))
            {
                LocationTextBox.Text = LocationPlaceholder;
                LocationTextBox.Foreground = Brushes.Gray;
            }
        }

        private void Filter_Changed(object sender, object e)
        {
            ApplyFilters();
        }

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
                bool matchLocation = string.IsNullOrEmpty(locationText) || ship.Location.IndexOf(locationText, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchType = selectedType == "All Types" || selectedType == null || ship.ShipClass.Equals(selectedType, StringComparison.OrdinalIgnoreCase);

                bool matchRate = true;
                if (selectedRate != "Any" && selectedRate != null && double.TryParse(ship.Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out double shipRating))
                {
                    if (double.TryParse(selectedRate, out double filterRate))
                    {
                        matchRate = shipRating >= filterRate;
                    }
                }

                bool matchPrice = true;
                double shipPriceVal = ParsePrice(ship.Price);
                if (shipPriceVal > maxPrice) matchPrice = false;

                bool matchCapacity = true;
                int shipCapVal = ParseCapacity(ship.Capacity);
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
                    Route = ship.Route,
                    Seats = ship.Seats,
                    DepartureTime = ship.DepartureTime,
                    Duration = ship.Duration
                };
                ShipCardGrid.Children.Add(newCard);
            }
        }

        private string GetComboBoxValue(ComboBox cb)
        {
            if (cb.SelectedItem is ComboBoxItem item) return item.Content.ToString();
            return null;
        }

        private double ParsePrice(string priceStr)
        {
            string clean = priceStr.Replace("Rp", "").Replace(".", "").Replace(" ", "").Trim();
            if (double.TryParse(clean, out double result)) return result;
            return 0;
        }

        private int ParseCapacity(string capacityStr)
        {
            string numberPart = new string(capacityStr.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(numberPart, out int result)) return result;
            return 0;
        }

        // TextBox Focus Events
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;

            if (tb.Name == "SearchBoatNameTextBox" && tb.Text == SearchPlaceholder)
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
            else if (tb.Name == "LocationTextBox" && tb.Text == LocationPlaceholder)
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;

            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                if (tb.Name == "SearchBoatNameTextBox")
                {
                    tb.Text = SearchPlaceholder;
                    tb.Foreground = Brushes.Gray;
                }
                else if (tb.Name == "LocationTextBox")
                {
                    tb.Text = LocationPlaceholder;
                    tb.Foreground = Brushes.Gray;
                }
            }
        }
    }
}