using BusinessLogic.IBusinessLogic.ICryptoService;
using System.Security.Cryptography;
using System.Text;
using Stripe;
using DataTypes.ModelDataTypes.Account;
using Microsoft.Extensions.Configuration;
using DataTypes.ModelDataTypes.Common;
using BusinessLogic.IBusinessLogic.ISubscription;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using DataAccess;
using DataTypes.ModelDataTypes.Subscription;
using BusinessLogic.BLImplementation.CryptoService;
using Dapper;
using System.Net.Mail;
using System.Reflection.Metadata;

namespace BusinessLogic.BLImplementation.Subscription
{
    public class SubsService : ISubsService
    {
        private readonly IConfiguration _configuration;
        private DbFactory _dBFactory;
        private IWebsiteSettings _websiteSettings;
        private readonly string? SecretKey;
        public SubsService(IConfiguration configuration, DbFactory dbFactory, IWebsiteSettings websiteSettings)
        {
            _configuration = configuration;
            _websiteSettings = websiteSettings;
            _dBFactory = dbFactory;
            SecretKey = _configuration.GetSection("StripeSettings")["SecretKey"];
        }

        public Message CreateSubscription(UserSubscription userSubscription)
        {
            Message msgPayment = new Message();
            Message msgProduct = new Message();
            Message msgPrice = new Message();
            Message msgCustomer = new Message();
            Message msgInvoice = new Message();
            try
            {
                msgPayment = CreatePaymentMethod(userSubscription);
                if (msgPayment != null && msgPayment.Status == "success")
                {
                    string paymentID = msgPayment.Id!;
                    msgProduct = CreateProductMethod();
                    if (msgProduct != null && msgProduct.Status == "success")
                    {
                        string productID = msgProduct.Id!;
                        msgPrice = CreatePriceMethod(productID);
                        if (msgPrice != null && msgPrice.Status == "success")
                        {
                            string priceID = msgPrice.Id!;
                            msgCustomer = CreateCustomerMethod(userSubscription);
                            if (msgCustomer != null && msgCustomer.Status == "success")
                            {
                                var options = new SubscriptionCreateOptions
                                {
                                    Customer = msgCustomer.Id,
                                    Items = new List<SubscriptionItemOptions>
                                    {
                                        new SubscriptionItemOptions
                                        {
                                            Price = msgPrice.Id,
                                        },
                                    },
                                };
                                var service = new SubscriptionService();
                                var subscription = service.Create(options);
                                var details = subscription;
                                Invoice invService = CreateAndPayInvoiceMethod(msgCustomer.Id);
                                if (invService != null && invService.Paid == true)
                                {
                                    string invoiceID = invService.Id;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msgPayment.Msg = ex.Message;
                msgPayment.Status = "fail";
            }
            return msgPayment;
        }

        public Message CreatePaymentMethod(UserSubscription userSubscription)
        {
            Message msg = new Message();
            try
            {
                StripeConfiguration.ApiKey = SecretKey;
                var options = new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Number = userSubscription.CardNumber,
                        ExpMonth = userSubscription.ExpireMonth,
                        ExpYear = userSubscription.ExpireYear,
                        Cvc = userSubscription.CVC,
                    },
                    BillingDetails = new PaymentMethodBillingDetailsOptions
                    {
                        Address = new AddressOptions
                        {
                            City = userSubscription.City,
                            State = userSubscription.State,
                            Country = userSubscription.Country,
                        },
                        Email = userSubscription.EmailAddress,
                        Name = userSubscription.FirstName + " " + userSubscription.LastName,
                    }
                };
                var service = new PaymentMethodService();
                var option = service.Create(options);
                msg.Id = option.Id;
                msg.Status = "success";
            }
            catch (Exception ex)
            {
                msg.Msg = ex.Message;
                msg.Status = "fail";
            }
            return msg;
        }

        public Message CreateProductMethod()
        {
            Message msg = new Message();
            try
            {
                StripeConfiguration.ApiKey = SecretKey;
                var options = new ProductCreateOptions
                {
                    Name = "Jubilee",
                };
                var service = new ProductService();
                var option = service.Create(options);
                msg.Id = option.Id;
                msg.Status = "success";
            }
            catch (Exception ex)
            {
                msg.Msg = ex.Message;
                msg.Status = "fail";
            }
            return msg;
        }

        public List<Features> BindFeaturesAsync(string DomainName)
        {
            List<Features> features = new List<Features>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@DomainName", DomainName.Trim());
                features = _dBFactory.SelectCommand_SP(features, "system_Features_Get", parameters);
                return features;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Message CreatePriceMethod(string productID)
        {
            Message msg = new Message();
            try
            {
                StripeConfiguration.ApiKey = SecretKey;
                var options = new PriceCreateOptions
                {
                    UnitAmount = 995,
                    Currency = "usd",
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = "month",
                    },
                    Product = productID,
                };
                var service = new PriceService();
                var option = service.Create(options);
                msg.Id = option.Id;
                msg.Status = "success";
            }
            catch (Exception ex)
            {
                msg.Msg = ex.Message;
                msg.Status = "fail";
            }
            return msg;
        }

        public Message CreateCustomerMethod(UserSubscription userSubscription)
        {
            Message msg = new Message();
            try
            {
                StripeConfiguration.ApiKey = SecretKey;
                var options = new CustomerCreateOptions
                {
                    Description = userSubscription.FirstName + " " + userSubscription.LastName,
                    Name = userSubscription.FirstName + " " + userSubscription.LastName,
                    Email = userSubscription.EmailAddress,
                    Address = new AddressOptions
                    {
                        City = userSubscription.City,
                        State = userSubscription.State,
                        Country = userSubscription.Country,
                    }
                };
                var service = new CustomerService();
                var option = service.Create(options);
                msg.Id = option.Id;
                msg.Status = "success";
            }
            catch (Exception ex)
            {
                msg.Msg = ex.Message;
                msg.Status = "fail";
            }
            return msg;
        }

        public Invoice CreateAndPayInvoiceMethod(string customerID)
        {
            InvoiceService serviceInvoice = new InvoiceService();
            Invoice invoice = new Invoice();
            try
            {
                StripeConfiguration.ApiKey = SecretKey;
                var options = new InvoiceCreateOptions
                {
                    Customer = customerID,
                };
                var service = new InvoiceService();
                var option = service.Create(options);
                serviceInvoice = new InvoiceService();
                invoice = service.Pay(option.Id);
            }
            catch (Exception ex)
            {
            }
            return invoice;
        }

        public List<PaymentPlan> BindPlansAsync(string UserID)
        {
            List<PaymentPlan> paymentPlans = new List<PaymentPlan>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID.Trim());
                paymentPlans = _dBFactory.SelectCommand_SP(paymentPlans, "system_PaymentPlans_Get", parameters);
                return paymentPlans;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PaymentPlanType> BindPlanTypeAsync(string DomainName)
        {
            List<PaymentPlanType> paymentPlanTypes = new List<PaymentPlanType>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@DomainName", DomainName.Trim());
                paymentPlanTypes = _dBFactory.SelectCommand_SP<PaymentPlanType>(paymentPlanTypes, "system_PlanType_Get", parameters);
                return paymentPlanTypes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PaymentPlan BindPlansUsingPlanIDAsync(string PaymentPlanID)
        {
            PaymentPlan paymentPlan = new PaymentPlan();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@PaymentPlanID", PaymentPlanID.Trim());
                paymentPlan = _dBFactory.SelectCommand_SP(paymentPlan, "system_PaymentPlans_ID_Get", parameters);
                return paymentPlan;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PaymentPlanItem> BindPlanItemsAsync(Guid? PlanTypeID)
        {
            List<PaymentPlanItem> paymentPlanItems = new List<PaymentPlanItem>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@PlanTypeID", PlanTypeID);
                paymentPlanItems = _dBFactory.SelectCommand_SP(paymentPlanItems, "system_PaymentPlanItems_Get", parameters);
                return paymentPlanItems;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Message AddUserPlan(string UserID, string PlanID)
        {
            Message message = new Message();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                dParam.Add("@PlanID", PlanID);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "System_Users_AddPlan", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Message RemoveUserPlan(string UserID)
        {
            Message message = new Message();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "System_Users_RemovePlan", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PaymentPlan BindPlanUsingUserIDAsync(string UserID)
        {
            PaymentPlan paymentPlans = new PaymentPlan();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID);
                paymentPlans = _dBFactory.SelectCommand_SP(paymentPlans, "system_User_PlanGetByUserID", parameters);
                return paymentPlans;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SubscriptionInfo CheckPlanUsingUserIDAsync(string UserID)
        {
            SubscriptionInfo subscriptionInfo = new SubscriptionInfo();
            //string Status = "";
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID);
                subscriptionInfo = _dBFactory.SelectCommand_SP(subscriptionInfo, "system_User_GetPlanStatusByUserID", parameters);
                return subscriptionInfo;
            }
            catch (Exception)
            {
                return subscriptionInfo;
            }
        }

        public bool UpdateUserSessionIDASync(string UserID, string sessionID)
        {
            PaymentPlan paymentPlans = new PaymentPlan();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID);
                parameters.Add("@SessionID", sessionID);
                paymentPlans = _dBFactory.SelectCommand_SP(paymentPlans, "system_User_SetSessionIDByUserID", parameters);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public StripeOptions BindStripeOptions(string DomainName)
        {
            StripeOptions stripeOptions = new StripeOptions();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@DomainName", DomainName);
                stripeOptions = _dBFactory.SelectCommand_SP(stripeOptions, "system_StripeOptions_Get", parameters);
                return stripeOptions;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ShowMessage Webhook_SaveInvoice(InvoiceDetail invoice)
        {
            ShowMessage message = new ShowMessage();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                //=====Invoice Details==================
                parameters.Add("@UserID", invoice.userID);
                parameters.Add("@PaymentPlanID", invoice.paymentPlanID);
                parameters.Add("@PayMethodID", invoice.paymentMethodID);
                parameters.Add("@SubscriptionID", invoice.subscriptionID);
                parameters.Add("@InvoiceID", invoice.invoiceID);
                parameters.Add("@CustomerID", invoice.customerID);
                parameters.Add("@SessionID", invoice.sessionID);
                parameters.Add("@LineID", invoice.lineID);
                parameters.Add("@PaymentIntentID", invoice.paymentIntentID);

                parameters.Add("@ProductID", invoice.productID);
                parameters.Add("@PlanCreatedDate", invoice.planCreatedDate);
                parameters.Add("@PlanInterval", invoice.planInterval);

                parameters.Add("@PriceID", invoice.priceID);
                parameters.Add("@PriceCreatedDate", invoice.priceCreatedDate);
                parameters.Add("@PriceInterval", invoice.priceInterval);

                parameters.Add("@ChargeID", invoice.chargeID);
                parameters.Add("@AmountPaid", invoice.amountPaid);
                parameters.Add("@AmountDue", invoice.amountDue);
                parameters.Add("@AmountRemaining", invoice.amountRemaining);
                parameters.Add("@Status", invoice.status);
                parameters.Add("@RechargeType", invoice.rechargeType);
                parameters.Add("@InvoiceNumber", invoice.invoiceNumber);
                parameters.Add("@InvoicePDFLink", invoice.invoicePDFLink);
                parameters.Add("@HostedInvoicePDFLink", invoice.hostedInvoicePDFLink);
                parameters.Add("@AttemptCount", invoice.attemptCount);
                parameters.Add("@InvoiceDate", invoice.invoiceDate);
                parameters.Add("@EffectiveDate", invoice.effectiveDate);
                parameters.Add("@PeriodStartDate", invoice.periodStartDate);
                parameters.Add("@PeriodEndDate", invoice.periodEndDate);
                parameters.Add("@Currency", invoice.currency);

                //============Customer Details================
                parameters.Add("@CustomerName", invoice.customerName);
                parameters.Add("@CustomerEmail", invoice.customerEmail);
                parameters.Add("@CustomerPhone", invoice.customerPhone);
                parameters.Add("@CustomerAddressLine1", invoice.customerAddressLine1);
                parameters.Add("@CustomerAddressLine2", invoice.customerAddressLine2);
                parameters.Add("@CustomerAddressCity", invoice.customerAddressCity);
                parameters.Add("@CustomerAddressState", invoice.customerAddressState);
                parameters.Add("@CustomerAddressCountry", invoice.customerAddressCountry);
                parameters.Add("@CustomerAddressPostalCode", invoice.customerAddressPostalCode);

                //=============Card Info=======================
                parameters.Add("@CardBrand", invoice.cardBrand);
                parameters.Add("@CardType", invoice.cardType);
                parameters.Add("@Last4Digits", invoice.last4Digits);
                parameters.Add("@ExpiryMonth", invoice.expiryMonth);
                parameters.Add("@ExpiryYear", invoice.expiryYear);
                parameters.Add("@CardCountry", invoice.country);
                parameters.Add("@Fingerprint", invoice.fingerprint);

                message = _dBFactory.SelectCommand_SP(message, "system_Invoice_Insert", parameters);
            }
            catch (Exception) { throw; }
            return message;
        }

        //public SubInvoice SaveInvoice(SubInvoice subInvModel)
        //{
        //    SubInvoice objSubInvoice = new SubInvoice();
        //    try
        //    {
        //        DynamicParameters parameters = new DynamicParameters();
        //        parameters.Add("@UserID", subInvModel.userID);
        //        parameters.Add("@PaymentPlanID", subInvModel.paymentPlanID);
        //        parameters.Add("@PayMethodID", subInvModel.PaymentMethodID);
        //        parameters.Add("@InvoiceID", subInvModel.InvoiceID);
        //        parameters.Add("@SesssionID", subInvModel.SesssionID);
        //        parameters.Add("@SubscriptionID", subInvModel.SubscriptionID);
        //        //============Customer Info===================
        //        parameters.Add("@Customer_email", subInvModel.Customer_email);
        //        parameters.Add("@CustomerName", subInvModel.CustomerName);
        //        parameters.Add("@CustomerPhone", subInvModel.CustomerPhone);
        //        parameters.Add("@CustomerCountry", subInvModel.CustomerCountry);
        //        parameters.Add("@CustomerCity", subInvModel.CustomerCity);
        //        parameters.Add("@CustomerState", subInvModel.CustomerState);
        //        parameters.Add("@CustomerPostalCode", subInvModel.CustomerPostalCode);
        //        //=============Card Info=======================
        //        parameters.Add("@CardBrand", subInvModel.CardBrand);
        //        parameters.Add("@CardLast4Digits", subInvModel.CardLast4Digits);
        //        parameters.Add("@CardExpiryMonth", subInvModel.CardExpiryMonth);
        //        parameters.Add("@CardExpiryYear", subInvModel.CardExpiryYear);
        //        parameters.Add("@CardCountry", subInvModel.CardCountry);
        //        parameters.Add("@CardFingerPrint", subInvModel.CardFingerPrint);
        //        parameters.Add("@CardType", subInvModel.CardType);

        //        parameters.Add("@Customerid", subInvModel.Customerid);
        //        parameters.Add("@Amount_paid", subInvModel.Amount_paid);
        //        parameters.Add("@Amount_due", subInvModel.Amount_due);
        //        parameters.Add("@InvoiceDate", subInvModel.InvoiceDate);
        //        parameters.Add("@Lineid", subInvModel.Lineid);
        //        parameters.Add("@Productid", subInvModel.Productid);
        //        parameters.Add("@Priceid", subInvModel.Priceid);
        //        parameters.Add("@PaymentIntentID", subInvModel.PaymentIntentID);
        //        parameters.Add("@ChargeID", subInvModel.ChargeID);
        //        parameters.Add("@InvoiceNumber", subInvModel.InvoiceNumber);
        //        parameters.Add("@Status", subInvModel.Status);
        //        parameters.Add("@AttemptCount", subInvModel.AttemptCount);
        //        parameters.Add("@InvoicePDFLink", subInvModel.InvoicePDFLink);
        //        parameters.Add("@HostedInvoicePDFLink", subInvModel.HostedInvoicePDFLink);
        //        objSubInvoice = _dBFactory.SelectCommand_SP(objSubInvoice, "system_Invoice_Insert", parameters);
        //        return objSubInvoice;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public ShowMessage SaveInvoiceSuccessFailed(InvoiceDetail invoiceDetail)
        {
            ShowMessage message = new ShowMessage();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@SubscriptionID", invoiceDetail.subscriptionID);
                parameters.Add("@InvoiceID", invoiceDetail.invoiceID);
                parameters.Add("@CustomerID", invoiceDetail.customerID);
                parameters.Add("@SessionID", invoiceDetail.sessionID);
                parameters.Add("@LineID", invoiceDetail.lineID);
                parameters.Add("@PaymentIntentID", invoiceDetail.paymentIntentID);
                //Plan details
                parameters.Add("@ProductID", invoiceDetail.productID);
                parameters.Add("@PlanCreatedDate", invoiceDetail.planCreatedDate);
                parameters.Add("@PlanInterval", invoiceDetail.planInterval);
                //Price details
                parameters.Add("@PriceID", invoiceDetail.priceID);
                parameters.Add("@PriceCreatedDate", invoiceDetail.priceCreatedDate);
                parameters.Add("@PriceInterval", invoiceDetail.priceInterval);
                parameters.Add("@Currency", invoiceDetail.currency);
                //Invoice details
                parameters.Add("@ChargeID", invoiceDetail.chargeID);
                parameters.Add("@AmountPaid", invoiceDetail.amountPaid);
                parameters.Add("@AmountDue", invoiceDetail.amountDue);
                parameters.Add("@AmountRemaining", invoiceDetail.amountRemaining);
                parameters.Add("@Status", invoiceDetail.status);
                parameters.Add("@RechargeType", invoiceDetail.rechargeType);
                parameters.Add("@InvoiceNumber", invoiceDetail.invoiceNumber);
                parameters.Add("@AttemptCount", invoiceDetail.attemptCount);
                parameters.Add("@InvoiceDate", invoiceDetail.invoiceDate);
                parameters.Add("@EffectiveDate", invoiceDetail.effectiveDate);
                parameters.Add("@PeriodStartDate", invoiceDetail.periodStartDate);
                parameters.Add("@PeriodEndDate", invoiceDetail.periodEndDate);
                parameters.Add("@InvoicePDFLink", invoiceDetail.invoicePDFLink);
                parameters.Add("@HostedInvoicePDFLink", invoiceDetail.hostedInvoicePDFLink);
                //Customer details
                parameters.Add("@CustomerName", invoiceDetail.customerName);
                parameters.Add("@CustomerEmail", invoiceDetail.customerEmail);
                parameters.Add("@CustomerPhone", invoiceDetail.customerPhone);
                parameters.Add("@CustomerAddressLine1", invoiceDetail.customerAddressLine1);
                parameters.Add("@CustomerAddressLine2", invoiceDetail.customerAddressLine2);
                parameters.Add("@CustomerAddressCity", invoiceDetail.customerAddressCity);
                parameters.Add("@CustomerAddressState", invoiceDetail.customerAddressState);
                parameters.Add("@CustomerAddressCountry", invoiceDetail.customerAddressCountry);
                parameters.Add("@CustomerAddressPostalCode", invoiceDetail.customerAddressPostalCode);
                //Card details
                parameters.Add("@CardBrand", invoiceDetail.cardBrand);
                parameters.Add("@CardType", invoiceDetail.cardType);
                parameters.Add("@Last4Digits", invoiceDetail.last4Digits);
                parameters.Add("@ExpiryMonth", invoiceDetail.expiryMonth);
                parameters.Add("@ExpiryYear", invoiceDetail.expiryYear);
                parameters.Add("@Country", invoiceDetail.country);
                parameters.Add("@Fingerprint", invoiceDetail.fingerprint);

                message = _dBFactory.InsertCommand_SPQuery(message, "system_Invoice_SaveSuccessFailed", parameters);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ShowMessage SaveInvoiceDetails(InvoiceDetail invoiceDetail)
        {
            ShowMessage message = new ShowMessage();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@SubscriptionID", invoiceDetail.subscriptionID);
                parameters.Add("@InvoiceID", invoiceDetail.invoiceID);
                parameters.Add("@CustomerID", invoiceDetail.customerID);
                parameters.Add("@SessionID", invoiceDetail.sessionID);
                parameters.Add("@LineID", invoiceDetail.lineID);
                parameters.Add("@PaymentIntentID", invoiceDetail.paymentIntentID);
                //Plan details
                parameters.Add("@ProductID", invoiceDetail.productID);
                parameters.Add("@PlanCreatedDate", invoiceDetail.planCreatedDate);
                parameters.Add("@PlanInterval", invoiceDetail.planInterval);
                //Price details
                parameters.Add("@PriceID", invoiceDetail.priceID);
                parameters.Add("@PriceCreatedDate", invoiceDetail.priceCreatedDate);
                parameters.Add("@PriceInterval", invoiceDetail.priceInterval);
                parameters.Add("@Currency", invoiceDetail.currency);
                //Invoice details
                parameters.Add("@ChargeID", invoiceDetail.chargeID);
                parameters.Add("@AmountPaid", invoiceDetail.amountPaid);
                parameters.Add("@AmountDue", invoiceDetail.amountDue);
                parameters.Add("@AmountRemaining", invoiceDetail.amountRemaining);
                parameters.Add("@Status", invoiceDetail.status);
                parameters.Add("@RechargeType", invoiceDetail.rechargeType);
                parameters.Add("@InvoiceNumber", invoiceDetail.invoiceNumber);
                parameters.Add("@AttemptCount", invoiceDetail.attemptCount);
                parameters.Add("@InvoiceDate", invoiceDetail.invoiceDate);
                parameters.Add("@EffectiveDate", invoiceDetail.effectiveDate);
                parameters.Add("@PeriodStartDate", invoiceDetail.periodStartDate);
                parameters.Add("@PeriodEndDate", invoiceDetail.periodEndDate);
                parameters.Add("@InvoicePDFLink", invoiceDetail.invoicePDFLink);
                parameters.Add("@HostedInvoicePDFLink", invoiceDetail.hostedInvoicePDFLink);
                //Customer details
                parameters.Add("@CustomerName", invoiceDetail.customerName);
                parameters.Add("@CustomerEmail", invoiceDetail.customerEmail);
                parameters.Add("@CustomerPhone", invoiceDetail.customerPhone);
                parameters.Add("@CustomerAddressLine1", invoiceDetail.customerAddressLine1);
                parameters.Add("@CustomerAddressLine2", invoiceDetail.customerAddressLine2);
                parameters.Add("@CustomerAddressCity", invoiceDetail.customerAddressCity);
                parameters.Add("@CustomerAddressState", invoiceDetail.customerAddressState);
                parameters.Add("@CustomerAddressCountry", invoiceDetail.customerAddressCountry);
                parameters.Add("@CustomerAddressPostalCode", invoiceDetail.customerAddressPostalCode);
                //Card details
                parameters.Add("@CardBrand", invoiceDetail.cardBrand);
                parameters.Add("@CardType", invoiceDetail.cardType);
                parameters.Add("@Last4Digits", invoiceDetail.last4Digits);
                parameters.Add("@ExpiryMonth", invoiceDetail.expiryMonth);
                parameters.Add("@ExpiryYear", invoiceDetail.expiryYear);
                parameters.Add("@Country", invoiceDetail.country);
                parameters.Add("@Fingerprint", invoiceDetail.fingerprint);
                parameters.Add("@AccountUserID", invoiceDetail.userID);

                message = _dBFactory.InsertCommand_SPQuery(message, "system_InvoiceDetails_save", parameters);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public SubInvoice SaveInvoiceData(SubInvoice subInvModel)
        //{
        //    SubInvoice objSubInvoice = new SubInvoice();
        //    try
        //    {
        //        DynamicParameters parameters = new DynamicParameters();
        //        parameters.Add("@UserID", subInvModel.userID);
        //        parameters.Add("@InvoiceID", subInvModel.InvoiceID);
        //        parameters.Add("@SubscriptionID", subInvModel.SubscriptionID);
        //        parameters.Add("@Customer_email", subInvModel.Customer_email);
        //        parameters.Add("@CustomerName", subInvModel.CustomerName);
        //        parameters.Add("@Customerid", subInvModel.Customerid);
        //        parameters.Add("@SesssionID", subInvModel.SesssionID);
        //        parameters.Add("@Amount_paid", subInvModel.Amount_paid);
        //        parameters.Add("@Amount_due", subInvModel.Amount_due);
        //        parameters.Add("@InvoiceDate", subInvModel.InvoiceDate);
        //        parameters.Add("@Lineid", subInvModel.Lineid);
        //        parameters.Add("@Productid", subInvModel.Productid);
        //        parameters.Add("@Priceid", subInvModel.Priceid);
        //        parameters.Add("@PaymentIntentID", subInvModel.PaymentIntentID);
        //        parameters.Add("@ChargeID", subInvModel.ChargeID);
        //        parameters.Add("@InvoiceNumber", subInvModel.InvoiceNumber);
        //        parameters.Add("@Status", subInvModel.Status);
        //        parameters.Add("@AttemptCount", subInvModel.AttemptCount);
        //        parameters.Add("@InvoicePDFLink", subInvModel.InvoicePDFLink);
        //        parameters.Add("@HostedInvoicePDFLink", subInvModel.HostedInvoicePDFLink);
        //        objSubInvoice = _dBFactory.SelectCommand_SP(objSubInvoice, "system_Invoice_InsertInvoiceStatusData", parameters);
        //        return objSubInvoice;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public Message CancelUserSubscription(string UserID)
        {
            Message message = new Message();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "System_Users_RemovePlan", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Message SaveUserSubscription(SubscriptionInfo subscription)
        {
            Message message = new Message();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@SubscriptionID", subscription.subscriptionID);
                dParam.Add("@UserID", subscription.userId);
                dParam.Add("@SessionID", subscription.sessionID);
                dParam.Add("@ProductID", subscription.productID);
                dParam.Add("@PriceID", subscription.priceID);
                dParam.Add("@Status", subscription.Status);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "system_Users_subscription_Add", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetUserSubscriptionID(string UserID)
        {
            string SubscriptionID = string.Empty;
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                SubscriptionID = _dBFactory.InsertCommand_SPQuery(SubscriptionID, "System_Users_GetSubscriptionID", dParam);
                return SubscriptionID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Message UpdateUserCancelSubscription(string UserID, string Status)
        {
            Message message = new Message();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                dParam.Add("@SubscriptionStatus", Status);
                message = _dBFactory.InsertCommand_SPQuery(message, "System_Users_UpdateUserCancelSubscription", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Message UpdateUserCancelSubscriptionForUpgradePlan(string UserID, string Status, string SubscriptionID,Guid PaymentPlanID)
        {
            Message message = new Message();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                dParam.Add("@SubscriptionStatus", Status);
                dParam.Add("@SubscriptionID", SubscriptionID);
                dParam.Add("@PaymentPlanID", PaymentPlanID);
                message = _dBFactory.InsertCommand_SPQuery(message, "System_CancelSubscriptionForUpgradePlan_update", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TokenInfo FetchUsedToken(Guid AccountUserID, Guid UserID)
        {
            TokenInfo tokenInfo = new TokenInfo();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                dParam.Add("@AccountUserID", AccountUserID);
                tokenInfo = _dBFactory.InsertCommand_SPQuery(tokenInfo, "System_AccountToken_Get", dParam);
                return tokenInfo;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SubscriptionPlanInfo GetSubscriptionPlanInfo(string UserID,string SubscriptionId)
        {
            SubscriptionPlanInfo subscriptionPlanInfo = new SubscriptionPlanInfo();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                dParam.Add("@PreviousSubcriptionId", SubscriptionId);
                subscriptionPlanInfo = _dBFactory.InsertCommand_SPQuery(subscriptionPlanInfo, "system_Users_GetSubscriptionPlan", dParam);
                return subscriptionPlanInfo;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SubscriptionPlanInfo GetSubscriptionPlanInfoUsingSubscriptionID(string subscriptionID)
        {
            SubscriptionPlanInfo subscriptionPlanInfo = new SubscriptionPlanInfo();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@subscriptionID", subscriptionID);
                subscriptionPlanInfo = _dBFactory.InsertCommand_SPQuery(subscriptionPlanInfo, "system_Users_GetSubscriptionPlan_SubscriptionID", dParam);
                return subscriptionPlanInfo;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<InvoiceHistory> BindInvoiceHistoryAsync(string DomainName, string UserID)
        {
            List<InvoiceHistory> invoiceHistories = new List<InvoiceHistory>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID.Trim());
                invoiceHistories = _dBFactory.SelectCommand_SP(invoiceHistories, "system_Invoice_GetHistory", parameters);
                return invoiceHistories;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PaymentMethodDetail> BindPaymentMethodAsync(string DomainName, string UserID)
        {
            List<PaymentMethodDetail> paymentMethodDetails = new List<PaymentMethodDetail>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID.Trim());
                paymentMethodDetails = _dBFactory.SelectCommand_SP(paymentMethodDetails, "system_PaymentMethods_Get", parameters);
                if (paymentMethodDetails != null && paymentMethodDetails.Count > 0)
                {
                    for (int i = 0; i < paymentMethodDetails.Count; i++)
                    {
                        paymentMethodDetails[i].DomainName = DomainName;
                    }
                }
                return paymentMethodDetails?? new List<PaymentMethodDetail>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<BillingInformation> BindBillingInformationAsync(string DomainName, string UserID)
        {
            List<BillingInformation> billingInformation = new List<BillingInformation>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID.Trim());
                billingInformation = _dBFactory.SelectCommand_SP(billingInformation, "system_BillingInformation_Get", parameters);
                if (billingInformation != null && billingInformation.Count > 0)
                {
                    foreach (var item in billingInformation)
                    {
                        item.CustomerName = item.CustomerName ?? "";
                        item.EmailAddress = item.EmailAddress ?? "";
                        item.PhoneNumber = item.PhoneNumber ?? "";
                        item.City = item.City ?? "";
                        item.State = item.State ?? "";
                        item.Country = item.Country ?? "";
                        item.PostalCode = item.PostalCode ?? "";
                        item.DomainName = DomainName;
                    }
                }
                return billingInformation ?? new List<BillingInformation>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckValidCustomerIDAsync(string userID, string customerID)
        {
            bool status = false;
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", userID.Trim());
                parameters.Add("@CustomerID", customerID.Trim());
                status = await _dBFactory.SelectCommand_SPAsync(status, "system_BillingInformation_CheckValidCustomer", parameters);
                return status;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ShowMessage> UpdateCustomerBillingInformationAsync(string userID, string customerID, string customerName, string emailAddress, string phoneNumber, string city, string state, string country, string postalCode)
        {
            try
            {
                ShowMessage message = new ShowMessage();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", userID.Trim());
                parameters.Add("@CustomerID", customerID.Trim());
                parameters.Add("@customerName", customerName != null ? customerName.Trim() : "");
                parameters.Add("@emailAddress", emailAddress != null ? emailAddress.Trim() : "");
                parameters.Add("@phoneNumber", phoneNumber != null ? phoneNumber.Trim() : "");
                parameters.Add("@city", city != null ? city.Trim() : "");
                parameters.Add("@state", state != null ? state.Trim() : "");
                parameters.Add("@country", country != null ? country.Trim() : "");
                parameters.Add("@postalCode", postalCode != null ? postalCode.Trim() : "");
                message = await _dBFactory.InsertCommand_SPQueryAsync(message, "system_BillingInformation_UpdateBillingInfo", parameters);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ShowMessage AddCustomerBillingInformation(PaymentMethodAttachedDetails pDetails, string UserID)
        {
            ShowMessage showMessage = new ShowMessage();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID);
                parameters.Add("@paymentMethodId", pDetails.paymentMethodId);
                parameters.Add("@customerId", pDetails.customerId);
                parameters.Add("@createdDate", pDetails.createdDate);

                parameters.Add("@cardBrand", pDetails.cardBrand);
                parameters.Add("@cardCVVCheck", pDetails.cardCVVCheck);
                parameters.Add("@cardCountry", pDetails.cardCountry);
                parameters.Add("@cardDescription", pDetails.cardDescription);
                parameters.Add("@cardExpMonth", pDetails.cardExpMonth);
                parameters.Add("@cardExpYear", pDetails.cardExpYear);
                parameters.Add("@cardFingerprint", pDetails.cardFingerprint);
                parameters.Add("@cardFunding", pDetails.cardFunding);
                parameters.Add("@cardIssuer", pDetails.cardIssuer);
                parameters.Add("@cardLast4Digits", pDetails.cardLast4Digits);

                parameters.Add("@paymentType", pDetails.paymentType);
                parameters.Add("@paymentLiveMode", pDetails.paymentLiveMode);
                parameters.Add("@billingName", pDetails.billingName);
                parameters.Add("@billingEmail", pDetails.billingEmail);
                parameters.Add("@billingPhone", pDetails.billingPhone);
                parameters.Add("@billingAddressLine1", pDetails.billingAddressLine1);
                parameters.Add("@billingAddressLine2", pDetails.billingAddressLine2);
                parameters.Add("@billingCity", pDetails.billingCity);
                parameters.Add("@billingState", pDetails.billingState);
                parameters.Add("@billingCountry", pDetails.billingCountry);
                parameters.Add("@billingPostalCode", pDetails.billingPostalCode);

                showMessage = _dBFactory.SelectCommand_SP(showMessage, "system_PaymentMethods_BillingInformation_Add", parameters);
                return showMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetStripeCustomerIDByUserID(string UserID)
        {
            string customerID = string.Empty;
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID);
                customerID = _dBFactory.SelectCommand_SP(customerID, "system_BillingInformation_GetCustomerID", parameters);
                return customerID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Webhook_SaveResponseJson(string jsonText)
        {
            Message message = new Message();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                //=====Invoice Details==================
                parameters.Add("@jsonText", jsonText);
                message = _dBFactory.SelectCommand_SP(message, "webhookLogs_Insert", parameters);
            }
            catch (Exception) { throw; }           
        }

        public ShowMessage SaveSubscriptionTokenStatus (Guid UserID, Guid AccountUserID, SubscriptionPlanInfo subscriptionPlanInfo, DateTime InvoiveDate,Int64 RechargeToken)
        {
            try
            {
                ShowMessage message = new ShowMessage();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@AccountUserID", AccountUserID);
                dParam.Add("@UserID", UserID);
                dParam.Add("@InvoiceID", subscriptionPlanInfo.InvoiceID);
                dParam.Add("@SubscriptionID", subscriptionPlanInfo.SubscriptionID);
                dParam.Add("@PaymentPlanID", subscriptionPlanInfo.PaymentPlanID);
                dParam.Add("@InvoiceDate", InvoiveDate);
                dParam.Add("@RechargeToken", RechargeToken);

                message = _dBFactory.SelectCommand_SP(message, "Subscription_TokenStatus_Insert", dParam);
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public TokenRecharge FetchCurrentPlanDetails(Guid AccountUserID)
        {
            TokenRecharge tokenRecharge = new TokenRecharge(); 
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@AccountUserID", AccountUserID);
                tokenRecharge = _dBFactory.SelectCommand_SP(tokenRecharge, "subscription_currentPlanDetails_get", parameters);
                return tokenRecharge;

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        public Guid GetPaymentPlanID(string AccountUserID,string DomainName)
        {
            Guid PaymentPlanID = Guid.Empty;
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@AccountUserID", AccountUserID);
                parameters.Add("@DomainName", DomainName);
                PaymentPlanID = _dBFactory.SelectCommand_SP(PaymentPlanID, "System_PlanID_Get", parameters);
                return PaymentPlanID;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }                
        public string GetSubscriptionStatus(string AccountUserID,string DomainName)
        {
            string SubscriptionStatus = "";
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@AccountUserID", AccountUserID);
                parameters.Add("@DomainName", DomainName);
                SubscriptionStatus = _dBFactory.SelectCommand_SP(SubscriptionStatus, "System_SubscriptionStatus_Get", parameters);
                return SubscriptionStatus;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }        
        public bool FetchRechargeTokenOnSubscriptionID(string subscriptionID)
        {
            bool IsTokenRecharged = false;
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@subscriptionID", subscriptionID);
                IsTokenRecharged = _dBFactory.SelectCommand_SP(IsTokenRecharged, "system_RechargeTokenOnSubscriptionID_Get", parameters);
                return IsTokenRecharged;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        public async Task<string> SaveStripePaymentAsync(StripePaymentDetails details)
        {
            string retval = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@LibraryID", details.LibraryID);
                parameters.Add("@ShareID", details.ShareID);
                parameters.Add("@SessionID", details.SessionID);
                parameters.Add("@PaymentIntentId", details.PaymentIntentId);
                parameters.Add("@Amount", details.Amount);
                parameters.Add("@Email", details.Email);
                parameters.Add("@Name", details.Name);
                parameters.Add("@City", details.City);
                parameters.Add("@Country", details.Country);
                parameters.Add("@AddressLine1", details.AddressLine1);
                parameters.Add("@AddressLine2", details.AddressLine2);
                parameters.Add("@PostalCode", details.PostalCode);
                parameters.Add("@PaymentStatus", details.PaymentStatus);
                parameters.Add("@ChargeID", details.ChargeID);
                parameters.Add("@CardBrand", details.CardBrand);
                parameters.Add("@CardLast4", details.CardLast4);

                retval = await _dBFactory.InsertCommand_SPQueryAsync(retval, "SaveStripePaymentDetails", parameters);
                return retval;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DomainName"></param>
        /// <returns></returns>
        public StripeOptions BindShareStripeOptions(string DomainName)
        {
            StripeOptions stripeOptions = new StripeOptions();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@DomainName", DomainName);
                stripeOptions = _dBFactory.SelectCommand_SP(stripeOptions, "system_Share_StripeOptions_Get", parameters);
                return stripeOptions;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
