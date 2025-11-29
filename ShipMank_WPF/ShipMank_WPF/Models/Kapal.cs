using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ShipMank_WPF.Models.ViewModel;

namespace ShipMank_WPF.Models
{
    public partial class Kapal
    {
        public int KapalID { get; set; }
        public string NamaKapal { get; set; }
        public int ShipTypeID { get; set; }
        public int Kapasitas { get; set; }
        public decimal HargaPerjalanan { get; set; }
        public int LokasiID { get; set; }
        public KapalStatus Status { get; set; }
        public string Fasilitas { get; set; }

        public static List<ShipViewModel> GetAllKapalForDisplay()
        {
            var list = new List<ShipViewModel>();

            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            k.kapalID, k.namakapal, k.kapasitas, k.rating, k.hargaperjalanan, k.kapalStatus,
                            s.typename, 
                            l.city, l.address, l.province,
                            k.fasilitas,
                            ki.imagePath
                        FROM Kapal k
                        INNER JOIN ShipType s ON k.shiptype = s.typeid
                        LEFT JOIN Lokasi l ON k.lokasi = l.portid
                        LEFT JOIN KapalImages ki ON k.kapalID = ki.kapalID AND ki.isPrimary = TRUE;";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int kapasitas = reader["kapasitas"] is int cap ? cap : 0;
                            double rating = reader["rating"] is DBNull ? 0.0 : Convert.ToDouble(reader["rating"]);
                            decimal harga = reader["hargaperjalanan"] is DBNull ? 0M : (decimal)reader["hargaperjalanan"];
                            string typeName = reader["typename"].ToString();
                            string fasilitasText = reader["fasilitas"]?.ToString() ?? "";

                            var facilities = fasilitasText.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(f => f.Trim()).ToList();

                            list.Add(new ShipViewModel
                            {
                                KapalID = (int)reader["kapalID"],
                                ShipName = reader["namakapal"].ToString(),
                                ShipClass = typeName,
                                Location = reader["province"]?.ToString() ?? "-",
                                Address = reader["address"]?.ToString() ?? "-",
                                City = reader["city"]?.ToString() ?? "-",
                                Province = reader["province"]?.ToString() ?? "-",
                                KapalStatus = reader["kapalStatus"]?.ToString() ?? "Available",
                                Capacity = $"{kapasitas} Penumpang",
                                Rating = rating.ToString("F1", CultureInfo.InvariantCulture),
                                Price = "Rp " + harga.ToString("N0", new CultureInfo("id-ID")),
                                Facilities = facilities,
                                ImageSource = reader["imagePath"]?.ToString() ?? "/Resources/default_ship.jpg",
                                BadgeColor = ShipViewModel.GetBadgeColor(typeName)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list;
        }
        public static List<string> GetImages(int kapalId, string defaultImage)
        {
            var images = new List<string>();
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = "SELECT imagePath FROM KapalImages WHERE kapalID = @KapalID ORDER BY isPrimary DESC, imageID ASC";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@KapalID", kapalId);
                        using (var reader = cmd.ExecuteReader())
                            while (reader.Read()) images.Add(reader["imagePath"].ToString());
                    }
                }
            }
            catch { }

            if (images.Count == 0 && !string.IsNullOrEmpty(defaultImage))
                images.Add(defaultImage);

            return images;
        }
    }
}