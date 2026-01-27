using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class ResponseFeedback
    {
        public Guid Userid { get; set; }
        public Guid Messageid { get; set; }
        public string? FeedbackType { get; set; }
        public bool Status { get; set; }
        public string? Message { get; set; }
    }
}
