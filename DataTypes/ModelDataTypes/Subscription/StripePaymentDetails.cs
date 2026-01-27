using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class StripePaymentDetails
    {
        public string? LibraryID { get; set; }
        public string? AlbumName { get; set; }
        public string? ShareID { get; set; }
        public string? SessionID { get; set; }
        public string? PaymentIntentId { get; set; }
        public long? Amount { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PostalCode { get; set; }
        public string? PaymentStatus { get; set; }
        public string? ChargeID { get; set; }
        public string? CardBrand { get; set; }
        public string? CardLast4 { get; set; }
    }

}
