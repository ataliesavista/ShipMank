using System;
using System.Collections.Generic;
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

        public static readonly DependencyProperty KapalIDProperty =
            DependencyProperty.Register("KapalID", typeof(int), typeof(ShipCard), new PropertyMetadata(0));

        public int KapalID
        {
            get { return (int)GetValue(KapalIDProperty); }
            set { SetValue(KapalIDProperty, value); }
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

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(ShipCard), new PropertyMetadata(null));

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty FacilitiesProperty =
            DependencyProperty.Register("Facilities", typeof(List<string>), typeof(ShipCard), new PropertyMetadata(null));

        public List<string> Facilities
        {
            get { return (List<string>)GetValue(FacilitiesProperty); }
            set { SetValue(FacilitiesProperty, value); }
        }

        public event EventHandler<int> DetailButtonClicked;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DetailButtonClicked?.Invoke(this, this.KapalID);
        }
    }
}
