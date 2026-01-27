using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class InvoiceDetail
    {
        public string? userID { get; set; }
        public string? paymentPlanID { get; set; }
        public string? paymentMethodID { get; set; }
        public string? subscriptionID { get; set; }
        public string? invoiceID { get; set; }
        public string? customerID { get; set; }
        public string? sessionID { get; set; }
        public string? lineID { get; set; }
        public string? paymentIntentID { get; set; }
        public string? productID { get; set; }
        public DateTime? planCreatedDate { get; set; }
        public string? planInterval { get; set; }
        public string? priceID { get; set; }
        public DateTime? priceCreatedDate { get; set; }
        public string? priceInterval { get; set; }
        public string? chargeID { get; set; }
        public double? amountPaid { get; set; }
        public long? amountDue { get; set; }
        public long? amountRemaining { get; set; }
        public string? status { get; set; }
        public string? rechargeType { get; set; }
        public string? invoiceNumber { get; set; }
        public string? invoicePDFLink { get; set; }
        public string? hostedInvoicePDFLink { get; set; }
        public long? attemptCount { get; set; }

        public DateTime? invoiceDate { get; set; }
        public DateTime? effectiveDate { get; set; }
        public DateTime? periodStartDate { get; set; }
        public DateTime? periodEndDate { get; set; }

        public string? currency { get; set; }
        //Customer Details
        public string? customerName { get; set; }
        public string? customerEmail { get; set; }
        public string? customerPhone { get; set; }
        public string? customerAddressLine1 { get; set; }
        public string? customerAddressLine2 { get; set; }
        public string? customerAddressCity { get; set; }
        public string? customerAddressState { get; set; }
        public string? customerAddressCountry { get; set; }
        public string? customerAddressPostalCode { get; set; }

        //Payment card details
        public string? cardBrand { get; set; }
        public string? cardType { get; set; }
        public string? last4Digits { get; set; }
        public long? expiryMonth { get; set; }
        public long? expiryYear { get; set; }
        public string? country { get; set; }
        public string? fingerprint { get; set; }

    }
}
