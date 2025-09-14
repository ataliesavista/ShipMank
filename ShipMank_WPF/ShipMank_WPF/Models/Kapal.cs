using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Kapal
    {
        public int KapalID { get; private set; }
        public string NamaKapal { get; set; }
        public ShipType Jenis { get; private set; }
        public int Kapasitas { get; set; }
        public double HargaPerjalanan { get; set; }
        public string Lokasi { get; set; }
        public string Deskripsi { get; set; }
        public KapalStatus Status { get; private set; }
        public double Rating { get; private set; }
        public string Fasilitas { get; set; }

        public List<Review> Reviews { get; private set; }

        public Kapal(int kapalID, string namaKapal, ShipType jenis, int kapasitas,
                     double hargaPerjalanan, string lokasi, string deskripsi, string fasilitas)
        {
            KapalID = kapalID;
            NamaKapal = namaKapal;
            Jenis = jenis;
            Kapasitas = kapasitas;
            HargaPerjalanan = hargaPerjalanan;
            Lokasi = lokasi;
            Deskripsi = deskripsi;
            Status = KapalStatus.Available;
            Fasilitas = fasilitas;
            Reviews = new List<Review>();
            Rating = 0.0;
        }

        //Contoh Tampilan Detail Kapal
        public string TampilDetail()
        {
            return $"ID: {KapalID}\n" +
                   $"Nama: {NamaKapal}\n" +
                   $"Jenis: {Jenis.TypeName}\n" +
                   $"Kapasitas: {Kapasitas} orang\n" +
                   $"Harga: Rp {HargaPerjalanan:N0}\n" +
                   $"Lokasi: {Lokasi}\n" +
                   $"Status: {Status}\n" +
                   $"Rating: {Rating:F1}/5.0\n" +
                   $"Fasilitas: {Fasilitas}\n" +
                   $"Deskripsi: {Deskripsi}";
        }

        public bool Availability() => Status == KapalStatus.Available;

        public void UpdateStatus(KapalStatus newStatus) => Status = newStatus;

        public void UpdateRating()
        {
            if (Reviews.Count > 0)
            {
                Rating = Reviews.Average(r => r.Rating);
            }
        }

        public override string ToString()
        {
            return $"{NamaKapal} ({Jenis.TypeName}) - Rp {HargaPerjalanan:N0}";
        }
    }
}
