using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Account
{
    public class Testimonies
    {
        public Guid TestimonyID { get; set; }
        public string TestimonyText { get; set; } = null!;
        public string TestimonyName { get; set; } = null!;
        public string TestimonyCityState { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
