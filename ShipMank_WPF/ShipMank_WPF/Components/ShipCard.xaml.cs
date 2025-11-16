using System.Windows;
using System.Windows.Controls;

namespace ShipMank_WPF.Components
{
    public partial class ShipCard : UserControl
    {
        public ShipCard()
        {
            InitializeComponent();
        }

        public string ShipName
        {
            get => (string)GetValue(ShipNameProperty);
            set => SetValue(ShipNameProperty, value);
        }
        public static readonly DependencyProperty ShipNameProperty =
            DependencyProperty.Register("ShipName", typeof(string), typeof(ShipCard));

        public string ShipClass
        {
            get => (string)GetValue(ShipClassProperty);
            set => SetValue(ShipClassProperty, value);
        }
        public static readonly DependencyProperty ShipClassProperty =
            DependencyProperty.Register("ShipClass", typeof(string), typeof(ShipCard));

        public string Location
        {
            get => (string)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(string), typeof(ShipCard));

        public string Capacity
        {
            get => (string)GetValue(CapacityProperty);
            set => SetValue(CapacityProperty, value);
        }
        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register("Capacity", typeof(string), typeof(ShipCard));

        public string Rating
        {
            get => (string)GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
        }
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register("Rating", typeof(string), typeof(ShipCard));

        public string Price
        {
            get => (string)GetValue(PriceProperty);
            set => SetValue(PriceProperty, value);
        }
        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register("Price", typeof(string), typeof(ShipCard));

        public string PriceUnit
        {
            get => (string)GetValue(PriceUnitProperty);
            set => SetValue(PriceUnitProperty, value);
        }
        public static readonly DependencyProperty PriceUnitProperty =
            DependencyProperty.Register("PriceUnit", typeof(string), typeof(ShipCard));

        public string Seats
        {
            get => (string)GetValue(SeatsProperty);
            set => SetValue(SeatsProperty, value);
        }
        public static readonly DependencyProperty SeatsProperty =
            DependencyProperty.Register("Seats", typeof(string), typeof(ShipCard));

        public string Route
        {
            get => (string)GetValue(RouteProperty);
            set => SetValue(RouteProperty, value);
        }
        public static readonly DependencyProperty RouteProperty =
            DependencyProperty.Register("Route", typeof(string), typeof(ShipCard));

        public string DepartureTime
        {
            get => (string)GetValue(DepartureTimeProperty);
            set => SetValue(DepartureTimeProperty, value);
        }
        public static readonly DependencyProperty DepartureTimeProperty =
            DependencyProperty.Register("DepartureTime", typeof(string), typeof(ShipCard));

        public string Duration
        {
            get => (string)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(string), typeof(ShipCard));

        public string BadgeColor
        {
            get => (string)GetValue(BadgeColorProperty);
            set => SetValue(BadgeColorProperty, value);
        }
        public static readonly DependencyProperty BadgeColorProperty =
            DependencyProperty.Register("BadgeColor", typeof(string), typeof(ShipCard));

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
