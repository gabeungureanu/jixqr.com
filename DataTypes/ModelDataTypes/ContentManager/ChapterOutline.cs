using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class ChapterOutline
    {
        public string ID { get; set; } = null!;
        public string Title { get; set; } = null!;

        List<SectionOutline> Sections =new List<SectionOutline>();
    }
    public class SectionOutline
    {
        public string ID { get; set; } = null!;
        public string Title { get; set; } = null!;
    }
}
