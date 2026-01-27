using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ShareRedirector
{
    public class Product
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public string? Thumbnail { get; set; }
        public string? Path { get; set; }
    }
}
