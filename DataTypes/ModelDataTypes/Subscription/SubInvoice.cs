using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class SubInvoice
    {
        public string userID { get; set; }
        public string paymentPlanID { get; set; }
        public string InvoiceID { get; set; }
        public string SubscriptionID { get; set; }
        public string Customerid { get; set; }
        public string SesssionID { get; set; }
        public long Amount_paid { get; set; }
        public long Amount_due { get; set; }
        public string Customer_email { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerPostalCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Lineid { get; set; }
        public string Productid { get; set; }
        public string Priceid { get; set; }
        public string Status { get; set; }
        public string? ChargeID { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? PaymentIntentID { get; set; }
        public string? PaymentMethodID { get; set; }
        public string? CardBrand { get; set; }
        public string? CardLast4Digits { get; set; }
        public long? CardExpiryMonth { get; set; }
        public long? CardExpiryYear { get; set; }
        public string? CardCountry { get; set; }
        public string? CardFingerPrint { get; set; }
        public string? CardType { get; set; }
        public long AttemptCount { get; set; }
        public string? InvoicePDFLink { get; set; }
        public string? HostedInvoicePDFLink { get; set; }

    }
}
