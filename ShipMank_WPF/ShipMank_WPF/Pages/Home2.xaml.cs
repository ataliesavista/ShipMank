using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Npgsql;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Pages
{
    public class ShipTypeModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }

    public partial class Home2 : Page
    {
        public List<ShipTypeModel> ShipTypes { get; set; }

        public Home2()
        {
            InitializeComponent();
            LoadDataFromDatabase();
            this.DataContext = this;
        }

        private void LoadDataFromDatabase()
        {
            ShipTypes = new List<ShipTypeModel>();

            try
            {
                string connString = DBHelper.GetConnectionString();

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = "SELECT typename, description, imagepath FROM shiptype";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string typeName = reader["typename"].ToString();
                                string description = reader["description"].ToString();
                                string imagePath = reader["imagepath"].ToString();

                                var ship = new ShipTypeModel
                                {
                                    Title = typeName,
                                    Description = description,
                                    ImagePath = imagePath
                                };

                                ShipTypes.Add(ship);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengambil data kapal: {ex.Message}");
            }
        }

        private void BtnBookNow_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigasi ke Booking (Fitur belum aktif di kode contoh ini)");
        }

        private void MainScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;

            double speedFactor = 3.0;
            double scrollAmount = e.Delta / speedFactor;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - scrollAmount);

            e.Handled = true;
        }
    }
}