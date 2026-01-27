using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class ContentDetails
    {
        public Guid ContentID { get; set; }
        public string? ContentTitle { get; set; }
        public Guid? ContentType { get; set; }
        public string? ContentCode { get; set; }
        public string? ContentCategory { get; set; }
        public string? ContentTheme { get; set; }
        public string? NarrativeStory { get; set; }
        public string? NarrativeElements { get; set; }
    }
}
