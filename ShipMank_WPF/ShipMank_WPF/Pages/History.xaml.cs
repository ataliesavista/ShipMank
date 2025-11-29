using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ShipMank_WPF.Components;
using ShipMank_WPF.Models;
using ShipMank_WPF.Models.Services;

namespace ShipMank_WPF.Pages
{
    public partial class History : Page
    {
        private int _currentUserID = 0;

        public History()
        {
            InitializeComponent();
            this.Loaded += History_Loaded;
        }

        private async void History_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mw && mw.CurrentUser != null)
            {
                _currentUserID = mw.CurrentUser.UserID;
            }
            else
            {
                _currentUserID = 0;
            }

            if (_currentUserID == 0)
            {
                HistoryDataGrid.ItemsSource = null;
                return;
            }

            HistoryService.CheckAndProcessCompletions();
            RefreshUI();
            await HistoryService.SyncUnpaidBookingsAsync();
            RefreshUI();
        }
        private async void InitializeHistoryAsync()
        {
            HistoryService.CheckAndProcessCompletions();
            RefreshUI();
            await HistoryService.SyncUnpaidBookingsAsync();
            RefreshUI();
        }

        private void RefreshUI()
        {
            var data = HistoryService.GetHistoryByUser(_currentUserID);
            HistoryDataGrid.ItemsSource = data;
        }

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
                            RefreshUI(); // Refresh tampilan abis rating
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