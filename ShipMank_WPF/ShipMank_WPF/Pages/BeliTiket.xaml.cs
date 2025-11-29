using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ShipMank_WPF.Components;
using ShipMank_WPF.Models;
using ShipMank_WPF.Models.ViewModel;

namespace ShipMank_WPF.Pages
{
    public partial class BeliTiket : Page
    {
        private List<ShipViewModel> masterShipList = new List<ShipViewModel>();
        private const string SearchPlaceholder = "Search boat name...";
        private const string LocationPlaceholder = "e.g. Bali, Jakarta";

        public BeliTiket()
        {
            InitializeComponent();

            LoadShipTypes();
            LoadFilterOptions();   
            LoadInitialData();
            InitializePlaceholders();

            if (TypeComboBox != null) TypeComboBox.SelectionChanged += Filter_Changed;
            if (CapacityComboBox != null) CapacityComboBox.SelectionChanged += Filter_Changed;
            if (RateComboBox != null) RateComboBox.SelectionChanged += Filter_Changed;
            if (PriceSlider != null) PriceSlider.ValueChanged += Filter_Changed;

            ApplyFilters();
        }

        private void LoadShipTypes()
        {
            try
            {
                if (TypeComboBox == null) return;

                TypeComboBox.Items.Clear();
                TypeComboBox.Items.Add(new ComboBoxItem { Content = "All Types", IsSelected = true });
                List<string> types = ShipType.GetAllTypeNames();
                foreach (string type in types) TypeComboBox.Items.Add(new ComboBoxItem { Content = type });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LoadFilterOptions()
        {
            try
            {
                if (CapacityComboBox != null)
                {
                    CapacityComboBox.Items.Clear();
                    CapacityComboBox.Items.Add(new ComboBoxItem { Content = "Any Capacity", IsSelected = true });
                    CapacityComboBox.Items.Add(new ComboBoxItem { Content = "2 - 5 People" });
                    CapacityComboBox.Items.Add(new ComboBoxItem { Content = "5 - 10 People" });
                    CapacityComboBox.Items.Add(new ComboBoxItem { Content = "10+ People" });
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading filters: " + ex.Message); }
        }

        private void LoadInitialData()
        {
            try
            {
                masterShipList = Kapal.GetAllKapalForDisplay();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); masterShipList = new List<ShipViewModel>(); }
        }

        private void Filter_Changed(object sender, object e) => ApplyFilters();
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void ApplyFilters()
        {
            if (ShipCardGrid == null || masterShipList == null) return;

            string searchText = (SearchBoatNameTextBox.Text == SearchPlaceholder) ? "" : SearchBoatNameTextBox.Text;
            string locationText = (LocationTextBox.Text == LocationPlaceholder) ? "" : LocationTextBox.Text;

            string selectedType = GetComboBoxValue(TypeComboBox);
            string selectedCapacity = GetComboBoxValue(CapacityComboBox);
            string selectedRate = GetComboBoxValue(RateComboBox);
            double maxPrice = PriceSlider != null ? PriceSlider.Value : double.MaxValue;

            var filteredList = masterShipList.Where(ship =>
            {
                // 1. Filter Nama
                bool matchName = string.IsNullOrEmpty(searchText) || ship.ShipName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                // 2. Filter Lokasi
                bool matchLocation = string.IsNullOrEmpty(locationText) || ship.Province.IndexOf(locationText, StringComparison.OrdinalIgnoreCase) >= 0 || ship.City.IndexOf(locationText, StringComparison.OrdinalIgnoreCase) >= 0;

                // 3. Filter Tipe
                bool matchType = selectedType == "All Types" || selectedType == null || ship.ShipClass.Equals(selectedType, StringComparison.OrdinalIgnoreCase);

                // 4. Filter Rating
                bool matchRate = true;
                if (selectedRate != "Any" && selectedRate != null && !string.IsNullOrEmpty(selectedRate))
                {
                    if (double.TryParse(ship.Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out double shipRating) &&
                        double.TryParse(selectedRate, NumberStyles.Any, CultureInfo.InvariantCulture, out double filterRate))
                    {
                        matchRate = shipRating >= filterRate;
                    }
                }

                // 5. Filter Harga
                bool matchPrice = ParsePrice(ship.Price) <= maxPrice;

                // 6. Filter Kapasitas
                int shipCapVal = ParseCapacity(ship.Capacity);
                bool matchCapacity = true;

                if (selectedCapacity == "2 - 5 People") matchCapacity = (shipCapVal >= 2 && shipCapVal <= 5);
                else if (selectedCapacity == "5 - 10 People") matchCapacity = (shipCapVal > 5 && shipCapVal <= 10);
                else if (selectedCapacity == "10+ People") matchCapacity = (shipCapVal > 10);

                return matchName && matchLocation && matchType && matchRate && matchPrice && matchCapacity;
            }).ToList();

            UpdateShipCardGrid(filteredList);
        }

        private void UpdateShipCardGrid(List<ShipViewModel> ships)
        {
            ShipCardGrid.Children.Clear();
            if (ships.Count == 0) return;

            foreach (var ship in ships)
            {
                ShipCard newCard = new ShipCard
                {
                    KapalID = ship.KapalID,
                    ShipName = ship.ShipName,
                    ShipClass = ship.ShipClass,
                    Location = ship.Location,
                    Capacity = ship.Capacity,
                    Rating = ship.Rating,
                    Price = ship.Price,
                    PriceUnit = ship.PriceUnit,
                    Facilities = ship.Facilities,
                    ImageSource = ship.ImageSource
                };
                newCard.DetailButtonClicked += ShipCard_ButtonClicked;
                ShipCardGrid.Children.Add(newCard);
            }
        }

        private void ShipCard_ButtonClicked(object sender, int kapalID)
        {
            var selectedShip = masterShipList.FirstOrDefault(s => s.KapalID == kapalID);
            if (selectedShip != null) ShowDetailKapalPopup(selectedShip);
        }

        private void ShowDetailKapalPopup(ShipViewModel ship)
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

            Grid overlayGrid = new Grid { Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) };

            DetailKapal detailPage = new DetailKapal(ship.KapalID);

            detailPage.ShipInfo = ship;
            detailPage.HostWindow = modalWindow;

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

        // Helpers
        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBoatNameTextBox.Text = SearchPlaceholder; SearchBoatNameTextBox.Foreground = Brushes.Gray;
            LocationTextBox.Text = LocationPlaceholder; LocationTextBox.Foreground = Brushes.Gray;

            if (TypeComboBox != null) TypeComboBox.SelectedIndex = 0;
            if (CapacityComboBox != null) CapacityComboBox.SelectedIndex = 0;
            if (RateComboBox != null) RateComboBox.SelectedIndex = 0;
            if (PriceSlider != null) PriceSlider.Value = PriceSlider.Maximum;

            ApplyFilters();
        }

        private void InitializePlaceholders()
        {
            if (string.IsNullOrEmpty(SearchBoatNameTextBox.Text)) { SearchBoatNameTextBox.Text = SearchPlaceholder; SearchBoatNameTextBox.Foreground = Brushes.Gray; }
            if (string.IsNullOrEmpty(LocationTextBox.Text)) { LocationTextBox.Text = LocationPlaceholder; LocationTextBox.Foreground = Brushes.Gray; }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && (tb.Text == SearchPlaceholder || tb.Text == LocationPlaceholder))
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                if (tb.Name == "SearchBoatNameTextBox") { tb.Text = SearchPlaceholder; tb.Foreground = Brushes.Gray; }
                else if (tb.Name == "LocationTextBox") { tb.Text = LocationPlaceholder; tb.Foreground = Brushes.Gray; }
            }
        }

        private string GetComboBoxValue(ComboBox cb)
        {
            if (cb == null) return null;
            return cb.SelectedItem is ComboBoxItem item ? item.Content.ToString() : null;
        }

        private double ParsePrice(string priceStr) => double.TryParse(priceStr?.Replace("Rp", "").Replace(".", "").Replace(" ", "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double res) ? res : 0;

        private int ParseCapacity(string capacityStr) => int.TryParse(new string((capacityStr ?? "").TakeWhile(char.IsDigit).ToArray()), out int res) ? res : 0;

        private void MainScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scv)
            {
                scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }
    }
}