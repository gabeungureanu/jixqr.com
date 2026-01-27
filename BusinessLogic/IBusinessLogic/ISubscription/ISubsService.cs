using DataTypes.ModelDataTypes.Account;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.ISubscription
{
    public interface ISubsService
    {
        Message CreateSubscription(UserSubscription userSubscription);
        List<PaymentPlan> BindPlansAsync(string UserID);
        List<PaymentPlanType> BindPlanTypeAsync(string DomainName);
        List<PaymentPlanItem> BindPlanItemsAsync(Guid? PlanTypeID);
        PaymentPlan BindPlansUsingPlanIDAsync(string PaymentPlanID);
        Message AddUserPlan(string UserID, string PlanID);
        Message RemoveUserPlan(string UserID);
        PaymentPlan BindPlanUsingUserIDAsync(string UserID);
        StripeOptions BindStripeOptions(string DomainName);
        bool UpdateUserSessionIDASync(string UserID, string sessionID);
        List<Features> BindFeaturesAsync(string DomainName);
        //SubInvoice SaveInvoice(SubInvoice subInvModel);
        ShowMessage SaveInvoiceSuccessFailed(InvoiceDetail invoiceDetail);
        //SubInvoice SaveInvoiceData(SubInvoice subInvModel);
        Message CancelUserSubscription(string UserID);
        string GetUserSubscriptionID(string UserID);
        Message UpdateUserCancelSubscription(string UserID, string Status);
        SubscriptionPlanInfo GetSubscriptionPlanInfo(string UserID,string subscriptionID);
        SubscriptionPlanInfo GetSubscriptionPlanInfoUsingSubscriptionID(string subscriptionID);
        SubscriptionInfo CheckPlanUsingUserIDAsync(string UserID);
        List<InvoiceHistory> BindInvoiceHistoryAsync(string DomainName, string UserID);
        List<PaymentMethodDetail> BindPaymentMethodAsync(string DomainName, string UserID);
        List<BillingInformation> BindBillingInformationAsync(string DomainName, string UserID);
        Task<bool> CheckValidCustomerIDAsync(string userID, string customerID);
        Task<ShowMessage> UpdateCustomerBillingInformationAsync(string userID, string customerID, string customerName, string emailAddress, string phoneNumber, string city, string state, string country, string postalCode);
        ShowMessage AddCustomerBillingInformation(PaymentMethodAttachedDetails paymentMethodAttachedDetails, string UserID);
        string GetStripeCustomerIDByUserID(string UserID);
        ShowMessage Webhook_SaveInvoice(InvoiceDetail invoiceDetail);
        void Webhook_SaveResponseJson(string jsonText);
        Message UpdateUserCancelSubscriptionForUpgradePlan(string UserID, string Status, string SubscriptionID, Guid PaymentPlanID);

        TokenInfo FetchUsedToken(Guid AccountUserID, Guid UserID);
        ShowMessage SaveSubscriptionTokenStatus(Guid UserID, Guid AccountUserID, SubscriptionPlanInfo subscriptionPlanInfo, DateTime InvoiveDate, Int64 RechargeToken);

        Message SaveUserSubscription(SubscriptionInfo subscription);

        TokenRecharge FetchCurrentPlanDetails(Guid AccountUserID);
        ShowMessage SaveInvoiceDetails(InvoiceDetail invoiceDetail);
        Guid GetPaymentPlanID(string AccountUserID, string DomainName);
        bool FetchRechargeTokenOnSubscriptionID(string subscriptionID);
        string GetSubscriptionStatus(string AccountUserID, string DomainName);

        Task<string> SaveStripePaymentAsync(StripePaymentDetails details);
        StripeOptions BindShareStripeOptions(string DomainName);
    }
}
