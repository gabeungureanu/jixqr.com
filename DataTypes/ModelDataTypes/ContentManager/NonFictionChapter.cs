using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class ChapterNonFiction
    {
        public int ChapterId { get; set; }
        public string? ID { get; set; }
        public string? Title { get; set; }
        public List<SectionNonFiction> Sections { get; set; }
    }

    public class SectionNonFiction
    {
        public int SectionId { get; set; }
        public string? ID { get; set; }
        public string? Title { get; set; }
        public int ChapterId { get; set; }
        public ChapterNonFiction Chapter { get; set; }
    }
}
