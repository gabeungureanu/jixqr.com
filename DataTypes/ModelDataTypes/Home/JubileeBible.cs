using DataTypes.ModelDataTypes.ContentManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class JubileeBible
    {
        public Guid ID { get; set; }
        public string Description { get; set; } = null!;
        public List<JubileeBibleSeries> jubileeBibleSeries { get; set; }
    }

    public class JubileeBibleSeries
    {
        public Guid ID { get; set; }
        public string Description { get; set; } = null!;
        public string Prompt { get;set; } = null!;  
        public bool IsEvent { get;set; } 
        public List<JubileeBibleBook> jubileeBibleBooks { get; set; }
    }

    public class JubileeBibleBook
    {
        public Guid ID { get; set; }
        public string Prompt { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsEvent { get; set; }
    }
}
