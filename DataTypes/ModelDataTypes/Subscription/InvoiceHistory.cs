using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class InvoiceHistory
    {
        public int InvoiceID { get; set; }
        public string? InvoiceDate { get; set;}
        public string? Amount { get; set; }
        public string? Status { get; set; }
        public string? PlanName { get; set; }
        public string? LoginStatus { get; set; }
        public string? DomainName { get; set; }
        public string? InvoicePDFLink { get; set; }
    }
}
