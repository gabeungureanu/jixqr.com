using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class ReportFeedback
    {
        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
        public string? SelectedReason { get; set; }
        public string? OptionalComment { get; set; }
        public string? Message { get; set; }
        public bool Status { get; set; }

    }
}
