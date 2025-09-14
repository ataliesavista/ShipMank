using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class User
    {
        public int UserID { get; private set; }
        public string Username { get; set; }
        private string Password { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string NoTelp { get; set; }
        public string Alamat { get; set; }

        public List<Booking> Bookings { get; private set; }
        public List<Review> Reviews { get; private set; }

        public User(int userID, string username, string password, string email,
                    string name, string noTelp, string alamat)
        {
            UserID = userID;
            Username = username;
            Password = password;
            Email = email;
            Name = name;
            NoTelp = noTelp;
            Alamat = alamat;
            Bookings = new List<Booking>();
            Reviews = new List<Review>();
        }

        public bool Register(string username, string password, string email,
                             string name, string noTelp, string alamat)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                return false;

            Username = username;
            Password = password;
            Email = email;
            Name = name;
            NoTelp = noTelp;
            Alamat = alamat;

            return true;
        }

        public bool Login(string username, string password)
        {
            return Username == username && Password == password;
        }

        public List<Kapal> SearchKapal(List<Kapal> allKapals, string lokasi = null,
                                       ShipTypeEnum? jenis = null, double maxPrice = double.MaxValue)
        {
            var results = new List<Kapal>();

            foreach (var kapal in allKapals)
            {
                bool matchLokasi = string.IsNullOrEmpty(lokasi) || kapal.Lokasi.Contains(lokasi);
                bool matchJenis = !jenis.HasValue || kapal.Jenis.TypeName == jenis.Value;
                bool matchPrice = kapal.HargaPerjalanan <= maxPrice;
                bool available = kapal.Availability();

                if (matchLokasi && matchJenis && matchPrice && available)
                    results.Add(kapal);
            }

            return results;
        }

        public List<Booking> Riwayat() => Bookings;

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if (Password == oldPassword)
            {
                Password = newPassword;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{Name} ({Username})";
        }
    }
}
