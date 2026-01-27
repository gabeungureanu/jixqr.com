using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class Act
    {
        public string ActTitle { get; set; }
        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
    }

    public class Chapter
    {
        public string ChapterTitle { get; set; }
        public List<string> Scenes { get; set; } = new List<string>();
    }
}
