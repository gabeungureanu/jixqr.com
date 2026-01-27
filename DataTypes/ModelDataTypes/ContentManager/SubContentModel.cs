using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class SubContentModel
    {
        public Guid ID { get; set; }
        public Guid SubContentID { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ContentBody { get; set; } = null!;

    }
}
