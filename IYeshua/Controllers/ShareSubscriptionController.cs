using BusinessLogic.IBusinessLogic.IAccountService;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IShareRedirector;
using BusinessLogic.IBusinessLogic.ISubscription;
using BusinessLogic.IBusinessLogic.IUtilityService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using BusinessLogic.IBusinessLogic.SMTP_Setting;
using DataAccess;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.ShareRedirector;
using DataTypes.ModelDataTypes.SMTPSetting;
using DataTypes.ModelDataTypes.Subscription;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace JubileeGPT.Controllers
{
    public class ShareSubscriptionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISubsService _subsService;
        private IAccountService _accountService;
        public readonly IOptions<StripeOptions> options;
        private readonly IStripeClient client;
        private DbFactory _dBFactory;
        private readonly IHostEnvironment _hostEnv;
        private IWebsiteSettings _websiteSettings;
        private string _domainName;
        private IShareRedirector _shareRedirector;
        private ISMTP_SettingService _smtp_SettingService;
        private readonly ICryptographyService _cryptographyService;

        public ShareSubscriptionController(IConfiguration configuration, ISubsService subsService,
            IAccountService accountService, IOptions<StripeOptions> options, DbFactory dbFactory,
            IWebsiteSettings websiteSettings, IUtilityService utilityService, IHostEnvironment hostEnv, IShareRedirector shareRedirector, ISMTP_SettingService smtp_SettingService, ICryptographyService cryptographyService)
        {
            _configuration = configuration;
            _subsService = subsService;
            _accountService = accountService;
            this.options = options;
            _dBFactory = dbFactory;
            _hostEnv = hostEnv;
            _websiteSettings = websiteSettings;
            this._domainName = _websiteSettings.GetDomain();
            _shareRedirector = shareRedirector;
            _smtp_SettingService = smtp_SettingService;
            _cryptographyService = cryptographyService;
            try
            {
                StripeOptions option = _subsService.BindShareStripeOptions(_domainName);
                if (option != null)
                {
                    this.options.Value.SecretKey = option.SecretKey;
                    this.options.Value.PublishableKey = option.PublishableKey;
                    this.options.Value.WebhookSecret = option.WebhookSecret;
                    this.options.Value.Domain = option.Domain;
                    this.client = new StripeClient(this.options.Value.SecretKey);
                }
                else
                {
                    _websiteSettings.InsertErrorLogs(Guid.Empty, "ShareSubscriptionController", "Constructor", "Secret Key not set", "Secret Key not set", "API", "", true);
                }
            }
            catch (Exception ex)
            {
                _websiteSettings.InsertErrorLogs(Guid.Empty, "ShareSubscriptionController", "Constructor", ex.Message, ex.Message, "API", "", true);
            }

        }

        //Create Session 
        [HttpGet("checkout-session/{shareID}")]
        public async Task<IActionResult> CreateCheckoutSession(Guid shareID)
        {
            StripeConfiguration.ApiKey = this.options.Value.SecretKey;
            ShareRediretor objmodel = new ShareRediretor();
            HttpContext.Session.SetString("ShareID", shareID.ToString());
            objmodel = _shareRedirector.Fetch_AlbumDetails(shareID);
            if (objmodel == null && !objmodel.Status)
            {
                return BadRequest();
            }
            var options = new SessionCreateOptions
            {
                // Success and cancel URLs
                SuccessUrl = $"{this.options.Value.Domain}/share-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{this.options.Value.Domain}/share-canceled",
                Mode = "payment",
                // Line items (what you're selling)
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = objmodel.DisplayOrder, // Price as DisplayOrder for Dynamic value.
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = objmodel.FolderName,
                            },
                        },
                        Quantity = 1,
                    },
                },

                // Metadata (additional information for later reference)
                Metadata = new Dictionary<string, string>
                {
                    { "shareID", shareID.ToString() },
                    { "LibraryID", objmodel.ID.ToString() },
                    { "AlbumName", objmodel.FolderName.ToString() }
                },

                // Expand the payment method to handle payment-related information
                Expand = new List<string> { "payment_intent.payment_method" },
            };

            // Stripe session service
            var service = new SessionService();
            try
            {
                // Create the session asynchronously
                var session = await service.CreateAsync(options);

                return Redirect(session.Url);
            }
            catch (StripeException ex)
            {
                // Handle any Stripe API errors here
                return BadRequest(new { Error = ex.Message });
            }
        }
        /// <summary>
        /// success event
        /// </summary>
        /// <param name="session_id"></param>
        /// <returns></returns>
        [HttpGet("share-success")]
        public async Task<IActionResult> ShareSuccess(string session_id)
        {
            var shareID = string.Empty;
            try
            {
                var service = new SessionService();
                var session = service.Get(session_id);
                if (session is not null)
                {
                    TempData["IsPayment"] = "success";
                    if (session.Metadata != null && session.Metadata.Count > 0)
                    {
                        //string emailID = session.Metadata["emailID"];
                        shareID = session.Metadata["shareID"];

                        var paymentIntentService = new PaymentIntentService();
                        var payIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

                        var paymentMethodService = new PaymentMethodService();
                        var paymentMethod = await paymentMethodService.GetAsync(payIntent.PaymentMethodId);
                        string AlbumID = session.Metadata["LibraryID"];
                        var paymentDetails = new StripePaymentDetails
                        {
                            LibraryID = AlbumID,
                            AlbumName = session.Metadata["AlbumName"],
                            ShareID = session.Metadata["shareID"],
                            SessionID = session.Id,
                            PaymentIntentId = session.PaymentIntentId,
                            Amount = session.AmountTotal,
                            Email = session.CustomerDetails?.Email,
                            Name = session.CustomerDetails?.Name,
                            City = session.CustomerDetails?.Address?.City,
                            Country = session.CustomerDetails?.Address?.Country,
                            AddressLine1 = session.CustomerDetails?.Address?.Line1,
                            AddressLine2 = session.CustomerDetails?.Address?.Line2,
                            PostalCode = session.CustomerDetails?.Address?.PostalCode,
                            PaymentStatus = session.PaymentStatus,
                            ChargeID = payIntent.LatestChargeId,
                            CardBrand = paymentMethod.Card?.Brand,
                            CardLast4 = paymentMethod.Card?.Last4
                        };

                        var paymentDetais = await _subsService.SaveStripePaymentAsync(paymentDetails);
                        if (paymentDetais == "success")
                        {
                            string DomainName = _websiteSettings.GetDomain();
                            //Get SMTP settings from database
                            _SMTPSetting objSMTP = _smtp_SettingService.Fetch_SMTP_Settings_By_SMTPName(DomainName);
                            if (objSMTP != null)
                            {
                                //Send email to user
                                bool sendStatus = await Send_MailToUser(paymentDetails, objSMTP, DomainName);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _websiteSettings.InsertErrorLogs(Guid.Empty, "ShareSubscriptionController", "Share-Success", ex.Message, ex.Message, "Frontend", "", true);
            }
            return RedirectToAction("GetAlbumList", "ShareRedirector", new { fileName = shareID });

        }
        /// <summary>
        /// share cancelled event
        /// </summary>
        /// <returns></returns>
        [Route("/share-canceled")]
        public IActionResult ShareCanceled()
        {
            ViewBag.DomainName = _websiteSettings.GetDomain();
            try
            {
                TempData["IsPayment"] = "fail";
                string shareID = HttpContext.Session.GetString("ShareID") ?? "";
                return RedirectToAction("GetAlbumList", "ShareRedirector", new { fileName = shareID });
                //-------------------------Remove plan to user---------------------
                Message messageUser = new Message();
                //messageUser = _subsService.RemoveUserPlan(UserID);
                //-------------------------End Remove plan to user-----------------
            }
            catch (Exception ex)
            {
                _websiteSettings.InsertErrorLogs(Guid.Empty, "ShareSubscriptionController", "Share-Canceled", ex.Message, ex.Message, "Frontend", "", true);
            }
            return View("~/Views/Shared/Cancel.cshtml");
        }

        /// <summary>
        /// share cancelled event
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/checkalbumcookie")]
        public IActionResult CheckPaymentToken([FromBody] CookieValue request)
        {
            ViewBag.DomainName = _websiteSettings.GetDomain();
            try
            {
                if (request == null)
                {
                    return Ok(new { success = false, message = "No valid payment token found." });
                }
                if (request.Cookie != null && request.ShareId != null)
                {
                    ShareRediretor objShareRediretor = _shareRedirector.Fetch_AlbumDetails(Guid.Parse(request.ShareId));
                    if (objShareRediretor != null && !"Blob Id is not exists".Equals(objShareRediretor.Message))
                    {
                        var decrypeCookie = _cryptographyService.Decrypt(request.Cookie);
                        var items = JsonSerializer.Deserialize<List<KeyValuePair<int, DateTime>>>(decrypeCookie);
                        if (items != null && items.Count > 0)
                        {
                            if (items.Where(x => x.Key == objShareRediretor.ID).ToList().Count > 0)
                            {
                                return Ok(new { success = true, message = "Valid payment token found." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _websiteSettings.InsertErrorLogs(Guid.Empty, "ShareSubscriptionController", "Share-Canceled", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Ok(new { success = false, message = "No valid payment token." });
        }

        /// <summary>
        /// share cancelled event
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/updatealbumcookie")]
        public IActionResult UpdateAlbumCookie([FromBody] CookieValue request)
        {
            ViewBag.DomainName = _websiteSettings.GetDomain();
            try
            {
                if (request == null)
                {
                    return Ok(new { success = false, message = "No valid payment token found." });
                }
                if (request.ShareId != null)
                {
                    //Create Cookie for Payment
                    var newcookie = new List<KeyValuePair<int, DateTime>>();
                    var shareModel = _shareRedirector.Fetch_AlbumDetails(Guid.Parse(request.ShareId));
                    if (shareModel != null)
                    {
                        if (Request.Cookies.TryGetValue("JubileeAlbum", out var cookie))
                        {
                            var decryptJsonCookie = _cryptographyService.Decrypt(cookie);
                            var decryptCookie = JsonSerializer.Deserialize<List<KeyValuePair<int, DateTime>>>(decryptJsonCookie);
                            bool exists = decryptCookie.Any(x => x.Key == shareModel.ID);

                            if (!exists)
                            {
                                decryptCookie.Add(new KeyValuePair<int, DateTime>(shareModel.ID, DateTime.Now.AddYears(1)));
                            }
                            newcookie = decryptCookie;
                            var cookieValue = JsonSerializer.Serialize(newcookie);
                            var encryptCookie = _cryptographyService.Encrypt(cookieValue);
                            Response.Cookies.Append("JubileeAlbum", encryptCookie, new CookieOptions
                            {
                                Expires = DateTime.UtcNow.AddYears(1),
                                HttpOnly = false,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Path = "/"
                            });
                        }
                        else
                        {
                            newcookie.Add(new KeyValuePair<int, DateTime>(shareModel.ID, DateTime.Now.AddYears(1)));
                            var cookieValue = JsonSerializer.Serialize(newcookie);
                            var encryptCookie = _cryptographyService.Encrypt(cookieValue);
                            Response.Cookies.Append("JubileeAlbum", encryptCookie, new CookieOptions
                            {
                                Expires = DateTime.UtcNow.AddYears(1),
                                HttpOnly = false,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Path = "/"
                            });
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                _websiteSettings.InsertErrorLogs(Guid.Empty, "ShareSubscriptionController", "Share-Canceled", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Ok(new { success = false, message = "No valid payment token." });
        }
        public async Task<bool> Send_MailToUser(StripePaymentDetails paymentDetails, _SMTPSetting objSMTP, string DomainName)
        {
            bool retval = true;
            try
            {
                string hostName = Request.Scheme + "://" + Request.Host.ToString();
                var TokenID = Convert.ToString(Guid.NewGuid());
                //var link = hostName + "/Account/VerifyAccount?UserID=" + UserDetails.UserID + "&Token=" + TokenID;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (SmtpClient client = new SmtpClient(objSMTP.SMTPServer))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(
                        objSMTP.SenderEmail,
                        _cryptographyService.Decrypt(objSMTP.SenderPassword ?? "")
                    );
                    client.EnableSsl = objSMTP.SSlEnable;
                    client.Port = objSMTP.SMTPPort;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(objSMTP.SenderEmail!, objSMTP.DisplayName);
                        mailMessage.To.Add(paymentDetails.Email!);
                        mailMessage.Subject = "Payment Confirmed – Your Album is Ready to Enjoy!";
                        mailMessage.IsBodyHtml = true;

                        var webRoot = _hostEnv.ContentRootPath;
                        var pathToFile = _hostEnv.ContentRootPath
                                  + "wwwroot"
                                  + Path.DirectorySeparatorChar.ToString()
                                  + "EmailTemplate" + Path.DirectorySeparatorChar.ToString()
                                  + "paymentsuccess.html";

                        string AlbumLink = "https://" + DomainName + "/Album/" + paymentDetails.ShareID;
                        StreamReader reader = new StreamReader(pathToFile);
                        string readFile = reader.ReadToEnd();
                        mailMessage.Body = readFile;
                        mailMessage.Body = mailMessage.Body.ToString()
                            .Replace("<%DomainName%>", Convert.ToString(DomainName))
                            .Replace("<%UserName%>", Convert.ToString(paymentDetails.Name))
                            .Replace("<%AlbumName%>", Convert.ToString(paymentDetails.AlbumName))
                            .Replace("<%AlbumLink%>", Convert.ToString(AlbumLink))
                            .Replace("<%CurrentYear%>", Convert.ToString(System.DateTime.Now.Year));
                        await client.SendMailAsync(mailMessage); // ✅ Await here
                    }
                }
                retval = true;
            }
            catch (Exception ex)
            {
                retval = false;
                _websiteSettings.InsertErrorLogs(Guid.Empty, "ShareSubscriptionController", "Send_MailToUser", ex.Message, ex.Message, "Frontend", "", true);
            }
            return retval;
        }
    }//CLASS ENDS HERE
}
