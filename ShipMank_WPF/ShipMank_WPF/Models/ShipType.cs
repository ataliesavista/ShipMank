using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class ShipType
    {
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        public ShipType(int typeID, string typeName, string description, string imagePath)
        {
            TypeID = typeID;
            TypeName = typeName;
            Description = description;
            ImagePath = imagePath;
        }

        public override string ToString() => TypeName;
        public static List<string> GetAllTypeNames()
        {
            var list = new List<string>();
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = "SELECT typeName FROM ShipType ORDER BY typeName ASC";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader["typeName"].ToString());
                        }
                    }
                }
            }
            catch
            {
                
            }
            return list;
        }
    }
}