using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class BookIntroduction
    {
        public Guid ID { get; set; }
        public Guid BookContentID { get; set; }
        public string? Description { get; set; }
    }
}
