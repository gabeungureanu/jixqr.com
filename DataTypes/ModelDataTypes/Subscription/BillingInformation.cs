using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class BillingInformation
    {
        public string? BillingDetailID { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerID { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? LoginStatus { get; set; }
        public string? DomainName { get; set; }
    }
}
