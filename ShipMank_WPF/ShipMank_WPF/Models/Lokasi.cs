using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Lokasi
    {
        public int PortID { get; private set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public Lokasi(int portID, string name, string address, string city, string country)
        {
            PortID = portID;
            Name = name;
            Address = address;
            City = city;
            Country = country;
        }

        public List<Kapal> GetAvailableShips()
        {
            // TODO: Hubungkan ke database untuk implementasinya
            return new List<Kapal>();
        }

        public override string ToString()
        {
            return $"{Name}, {City}, {Country}";
        }
    }
}
