using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public  class PaymentMethodAttachedDetails
    {
        public string? paymentMethodId { get; set; }
        public string? customerId { get; set; }
        public DateTime? createdDate { get; set; }
        public string? paymentType { get; set; }
        public bool? paymentLiveMode { get; set; }
        public string? billingName { get; set; }
        public string? billingEmail { get; set; }
        public string? billingPhone { get; set; }
        public string? billingAddressLine1 { get; set; }
        public string? billingAddressLine2 { get; set; }
        public string? billingCity { get; set; }
        public string? billingState { get; set; }
        public string? billingCountry { get; set; }
        public string? billingPostalCode { get; set; }
        public string? cardBrand { get; set; }
        public string? cardCVVCheck { get; set; }
        public string? cardCountry { get; set; }
        public string? cardDescription { get; set; }
        public int? cardExpMonth { get; set; }
        public int? cardExpYear { get; set; }
        public string? cardFingerprint { get; set; }
        public string? cardFunding { get; set; }
        public string? cardIssuer { get; set; }
        public string? cardLast4Digits { get; set; }
    }
}
