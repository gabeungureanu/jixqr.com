using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Player
{
    public class AlbumItems
    {
        public List<IsSpotLight>  IsSpotLights { get; set; }
        public List<ProductDetails> ProductDetails { get; set; }
    }

    public class IsSpotLight
    {
        public string? FileName { get; set; }
        public string? AzurefileName { get; set; }
        public string? FileExtension { get; set; }
        public int FileId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
    }

    public class ProductDetails
    {
        public string? ProductName { get; set; }
        public int ProductID { get; set; }
        public List<ProductsSong> ProductsSong { get; set; }
    }

    public class ProductsSong
    {
        public string? FileName { get; set; }
        public string? AzurefileName { get; set; }
        public string? NanoId { get; set; }
        public string? FileExtension { get; set; }
        public int FileId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public int SongsProductID { get; set; }
    }
}
