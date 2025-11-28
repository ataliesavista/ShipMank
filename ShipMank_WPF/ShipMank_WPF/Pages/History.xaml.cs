using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ShipMank_WPF.Components;
using ShipMank_WPF.Models;
using ShipMank_WPF.Models.Services; // Tambahkan namespace Service

namespace ShipMank_WPF.Pages
{
    public partial class History : Page
    {
        private int _currentUserID = 0;

        public History()
        {
            InitializeComponent();

            if (Application.Current.MainWindow is MainWindow mw && mw.CurrentUser != null)
            {
                _currentUserID = mw.CurrentUser.UserID;
            }
            else
            {
                _currentUserID = 1;
            }

            InitializeHistoryAsync();
        }

        private async void InitializeHistoryAsync()
        {
            // 1. Jalankan proses background (Logic ada di Service)
            HistoryService.CheckAndProcessCompletions();

            // 2. Load data awal
            RefreshUI();

            // 3. Sync Midtrans (Async)
            await HistoryService.SyncUnpaidBookingsAsync();

            // 4. Refresh lagi setelah sync
            RefreshUI();
        }

        private void RefreshUI()
        {
            // Panggil method static dari Service
            var data = HistoryService.GetHistoryByUser(_currentUserID);
            HistoryDataGrid.ItemsSource = data;
        }

        // ================= BUTTON HANDLERS =================

        private void BtnRate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderHistoryItem item)
            {
                RatingWindow ratingPopup = new RatingWindow();

                if (item.IsRated)
                {
                    int existing = HistoryService.GetExistingRating(item.OriginalBookingID);
                    ratingPopup.SetReadOnlyMode(existing);
                    ratingPopup.ShowDialog();
                }
                else
                {
                    if (ratingPopup.ShowDialog() == true)
                    {
                        try
                        {
                            HistoryService.SubmitRating(item.OriginalBookingID, item.KapalID, ratingPopup.SelectedRating);
                            RefreshUI(); // Refresh tampilan setelah rate
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("Gagal submit rating: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderHistoryItem selectedOrder)
            {
                NavigationService.Navigate(new ViewDetails(selectedOrder));
            }
        }

        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    }
}