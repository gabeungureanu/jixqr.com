using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes
{
    public class Album
    {
        public int ID { get; set; }
        public string? NanoId { get; set; }
        public int FileID { get; set; }
        public string? FileName { get; set; }
        public string? ShareDescription { get; set; }
        public string? Azurefilename { get; set; }
        public string? FileExtension { get; set; }
        public string? RedirectURL { get; set; }
        public string? RedirectLink { get; set; }
        public string? URLMapLink { get; set; }
        public string? Duration { get; set; }
        public string? TotalDuration { get; set; }
        public bool IsActive { get; set; }
        public string? Product { get; set; }
        public string? ProductImage { get; set; }
        public bool Archive { get; set; }
        public int ItemId { get; set; }
    }

    public class AlbumDetails
    {
        public string? FileName { get; set; }
        public string? ShareDescription { get; set; }       
        public string? FileExtension { get; set; }
        public int SongsCount { get; set; }
        public int SongsTime { get; set; }
        public string? SumDuration { get; set; }
        public List<Album> Albums { get; set; }
        public AlbumDetails()
        {
            Albums = new List<Album>();
        }
    }
}
