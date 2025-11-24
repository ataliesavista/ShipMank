using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class ShipType
    {
        public int TypeID { get; private set; }
        public string TypeName { get; private set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        public ShipType(int typeID, string typeName, string description, string imagePath)
        {
            TypeID = typeID;
            TypeName = typeName;
            Description = description;
            ImagePath = imagePath;
        }

        public override string ToString()
        {
            return $"{TypeName}: {Description}";
        }
    }
}