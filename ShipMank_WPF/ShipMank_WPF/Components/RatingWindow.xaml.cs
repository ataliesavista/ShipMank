using MahApps.Metro.IconPacks;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ShipMank_WPF.Components
{
    public partial class RatingWindow : Window
    {
        public int SelectedRating { get; private set; } = 0;
        private bool _isReadOnly = false;

        private readonly Brush _goldColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
        private readonly Brush _lightYellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9C4"));
        private readonly Brush _grayColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

        public RatingWindow()
        {
            InitializeComponent();
        }
        public void SetReadOnlyMode(int existingRating)
        {
            _isReadOnly = true;
            SelectedRating = existingRating;

            TitleText.Text = "Your Rating";
            SubTitleText.Text = "You have already rated this trip";
            BtnSubmit.Visibility = Visibility.Collapsed;

            StarsPanel.Cursor = Cursors.Arrow;
            Star1.Cursor = Cursors.Arrow; Star2.Cursor = Cursors.Arrow;
            Star3.Cursor = Cursors.Arrow; Star4.Cursor = Cursors.Arrow; Star5.Cursor = Cursors.Arrow;

            UpdateStars(SelectedRating, false);
        }

        private void Star_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_isReadOnly) return;

            if (sender is PackIconMaterial icon && int.TryParse(icon.Tag.ToString(), out int hoverRating))
            {
                UpdateStars(hoverRating, true);
            }
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isReadOnly) return;
            UpdateStars(SelectedRating, false);
        }

        private void Star_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isReadOnly) return;

            if (sender is PackIconMaterial icon && int.TryParse(icon.Tag.ToString(), out int rating))
            {
                SelectedRating = rating;
                UpdateStars(SelectedRating, false);
                BtnSubmit.IsEnabled = true;
            }
        }

        private void UpdateStars(int value, bool isHover)
        {
            Brush activeColor = isHover ? _lightYellow : _goldColor;

            SetStarColor(Star1, 1, value, activeColor);
            SetStarColor(Star2, 2, value, activeColor);
            SetStarColor(Star3, 3, value, activeColor);
            SetStarColor(Star4, 4, value, activeColor);
            SetStarColor(Star5, 5, value, activeColor);
        }

        private void SetStarColor(PackIconMaterial star, int starValue, int targetValue, Brush color)
        {
            star.Foreground = (starValue <= targetValue) ? color : _grayColor;
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}