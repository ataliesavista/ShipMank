using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ShipMank_WPF.Pages
{
    public class ShipTypeModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }

    /// <summary>
    /// Interaction logic for Home2.xaml
    /// </summary>
    public partial class Home2 : Page
    {
        public List<ShipTypeModel> ShipTypes { get; set; }

        public Home2()
        {
            InitializeComponent();
            LoadMockData();
            this.DataContext = this;
        }

        private void LoadMockData()
        {
            ShipTypes = new List<ShipTypeModel>
            {
                new ShipTypeModel
                {
                    Title = "Fast Boat",
                    Description = "High-speed vessels designed for quick inter-island travel.",
                    ImagePath = "/Assets/hero2.jpg"
                },
                new ShipTypeModel
                {
                    Title = "Ro-Ro Ferry",
                    Description = "Large vessels capable of carrying passengers, cars, and logistics trucks.",
                    ImagePath = "/Assets/hero2.jpg"
                },
                new ShipTypeModel
                {
                    Title = "Phinisi",
                    Description = "Traditional Indonesian wooden sailing vessels for leisure luxury experience.",
                    ImagePath = "/Assets/hero2.jpg"
                },
                new ShipTypeModel
                {
                    Title = "Yacht",
                    Description = "Premium private vessels for exclusive travel experiences.",
                    ImagePath = "/Assets/hero2.jpg"
                }
            };
        }
        private void BtnBookNow_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BeliTiket());
        }

    }
}