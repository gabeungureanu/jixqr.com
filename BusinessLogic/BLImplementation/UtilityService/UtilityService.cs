using Azure.Core;
using Azure;
using BusinessLogic.IBusinessLogic.IUtilityService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes.WebsiteSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using DataTypes.ModelDataTypes.Account;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.BLImplementation.CryptoService;
using DataTypes.ModelDataTypes.Common;

namespace BusinessLogic.BLImplementation.UtilityService
{
    public class UtilityService : IUtilityService
    {
        private readonly IConfiguration _configuration;
        private DbFactory _dbFactory;
        private IWebsiteSettings _websiteSettings;
        private readonly ICryptographyService _cryptographyService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="dbFactory"></param>
        /// <param name="websiteSettings"></param>
        /// <param name="cryptographyService"></param>
        public UtilityService(IConfiguration configuration, DbFactory dbFactory, IWebsiteSettings websiteSettings, ICryptographyService cryptographyService)
        {
            _configuration = configuration;
            _dbFactory = dbFactory;
            _websiteSettings = websiteSettings;
            _cryptographyService = cryptographyService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="errorPage"></param>
        /// <param name="methodName"></param>
        /// <param name="errorMessage"></param>
        /// <param name="errorDescription"></param>
        /// <param name="errorMode"></param>
        /// <param name="errorCode"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public string InsertErrorLogs(Guid userID, string errorPage, string methodName, string errorMessage, string errorDescription, string errorMode, string errorCode, bool active = true)
        {
            try
            {
                string status = string.Empty;
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", userID);
                parameters.Add("@ErrorMode", errorMode.Trim());
                parameters.Add("@ErrorCode", errorCode.Trim());
                parameters.Add("@ErrorPage", errorPage.Trim());
                parameters.Add("@MethodName", methodName.Trim());
                parameters.Add("@ErrorMessage", errorMessage.Trim());
                parameters.Add("@Description", errorDescription == null ? "" : errorDescription.Trim());
                parameters.Add("@Active", active);
                status = _dbFactory.SelectCommand_SP(status, "system_ErrorLog_Add", parameters);
                return status;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Check plans for the login user
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public string CheckPlanUsingUserIDAsync(string UserID)
        {
            string Status = "";
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID);
                Status = _dbFactory.SelectCommand_SP(Status, "system_User_GetPlanStatusByUserID", parameters);
                return Status;
            }
            catch (Exception ex)
            {
                InsertErrorLogs(Guid.Empty, "UtilityService", "CheckPlanUsingUserIDAsync", ex.Message, ex.Message, "Frontend", "", true);
                return Status;
            }
        }

        #region Remember Me Cookies
        public bool SetRememberMeCookies(HttpContext context, RememberMe rememberMe)
        {
            bool status = false;
            string serializedModel = string.Empty;
            string encModel = string.Empty;
            try
            {
                string domainName = _websiteSettings.GetDomain().ToLower();
                var ExpiresTimeoutDays = _configuration.GetSection("CookieSettings")["RememberMeExpiresTimeoutDays"];
                if (ExpiresTimeoutDays == null)
                    ExpiresTimeoutDays = "30";
                if (!context.Request.Cookies.TryGetValue("encremme" + domainName, out string? encRemMe))
                {
                    CookieOptions options = new CookieOptions
                    {
                        IsEssential = true,
                        Path = "/",
                        Secure = true,
                        HttpOnly = true,
                        Expires = DateTime.Now.AddDays(Convert.ToInt32(ExpiresTimeoutDays))
                    };

                    if (rememberMe != null)
                    {
                        serializedModel = JsonConvert.SerializeObject(rememberMe);
                        if (!string.IsNullOrEmpty(serializedModel))
                        {
                            encModel = _cryptographyService.Encrypt(serializedModel);
                            if (encModel != null)
                            {
                                context.Response.Cookies.Append("encremme" + domainName, encModel, options);
                                status = context.Request.Cookies.ContainsKey("encremme" + domainName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InsertErrorLogs(Guid.Empty, "UtilityService", "SetRememberMeCookies", ex.Message, ex.Message, "Frontend", "", true);
            }
            return status;
        }

        /// <summary>
        /// Remember me functionality
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<RememberMe> GetRememberMeCookies(HttpContext context)
        {
            RememberMe rememberMe = new RememberMe();
            string domainName = _websiteSettings.GetDomain().ToLower();
            string deserializedModel = string.Empty;
            try
            {
                if (context.Request.Cookies != null)
                {
                    if (context.Request.Cookies.Count > 0)
                    {
                        if (context.Request.Cookies.TryGetValue("encremme" + domainName, out string? encRemMe))
                        {
                            if (encRemMe != null)
                            {
                                deserializedModel = _cryptographyService.Decrypt(encRemMe);
                                if (!string.IsNullOrEmpty(deserializedModel))
                                {
                                    rememberMe = JsonConvert.DeserializeObject<RememberMe>(deserializedModel);
                                    await Task.Delay(1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InsertErrorLogs(Guid.Empty, "UtilityService", "GetRememberMeCookies", ex.Message, ex.Message, "Frontend", "", true);
            }
            return rememberMe;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool RemoveRememberMeCookies(HttpContext context)
        {
            bool status = false;
            string domainName = _websiteSettings.GetDomain().ToLower();
            try
            {
                if (context.Request.Cookies != null)
                {
                    if (context.Request.Cookies.Count > 0)
                    {
                        if (context.Request.Cookies.TryGetValue("encremme" + domainName, out string? encRemMe))
                        {
                            if (encRemMe != null)
                            {
                                context.Response.Cookies.Delete("encremme" + domainName);
                                // Check if the cookie was successfully deleted
                                bool statusCookie = !context.Request.Cookies.ContainsKey("encremme" + domainName);
                                if (!statusCookie)
                                {
                                    status = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InsertErrorLogs(Guid.Empty, "UtilityService", "RemoveRememberMeCookies", ex.Message, ex.Message, "Frontend", "", true);
            }
            return status;
        }
        
        #endregion Remember Me Cookies

        #region Website Configuration Cookies

        /// <summary>
        /// Get website configuration model from cookie
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<WebsiteConfiguration> GetWebsiteConfigurationCookies(HttpContext context)
        {
            WebsiteConfiguration websiteConfiguration = new WebsiteConfiguration();
            string domainName = _websiteSettings.GetDomain();
            string deserializedModel = string.Empty;

            try
            {
                if (context.Request.Cookies != null)
                {
                    if (context.Request.Cookies.Count > 0)
                    {
                        if (context.Request.Cookies.TryGetValue("encwebmodel" + domainName, out string? encWebModel))
                        {
                            if (encWebModel != null)
                            {
                                deserializedModel = _cryptographyService.Decrypt(encWebModel);
                                if (!string.IsNullOrEmpty(deserializedModel))
                                {
                                    websiteConfiguration = JsonConvert.DeserializeObject<WebsiteConfiguration>(deserializedModel);
                                    await Task.Delay(1);
                                }
                            }
                        }
                    }
                }
                if (websiteConfiguration == null || string.IsNullOrEmpty(websiteConfiguration.System_WebsiteName))
                {
                    websiteConfiguration = await _websiteSettings.GetWebsiteAsync(domainName);
                    if (websiteConfiguration != null)
                    {
                        SetWebsiteConfigurationCookies(websiteConfiguration, context);
                    }
                }
            }
            catch (Exception ex)
            {
                InsertErrorLogs(Guid.Empty, "UtilityService", "GetWebsiteConfigurationCookies", ex.Message, ex.StackTrace ?? "", "Frontend", "", true);
            }
            return websiteConfiguration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="websiteConfiguration"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool SetWebsiteConfigurationCookies(WebsiteConfiguration websiteConfiguration, HttpContext context)
        {
            bool status = false;
            string serializedModel = string.Empty;
            string encWebModel = string.Empty;
            try
            {
                string domainName = _websiteSettings.GetDomain();
                var ExpiresTimeoutHours = _configuration.GetSection("CookieSettings")["WebsiteSettingExpiresTimeoutHours"];
                if (ExpiresTimeoutHours == null)
                    ExpiresTimeoutHours = "24";

                if (!context.Request.Cookies.TryGetValue("encwebmodel" + domainName, out string? encWebName))
                {
                    CookieOptions options = new CookieOptions();
                    options.IsEssential = true;
                    options.Path = "/";
                    options.Secure = true;
                    options.HttpOnly = true;
                    options.Expires = DateTime.Now.AddHours(Convert.ToInt32(ExpiresTimeoutHours));
                    if (websiteConfiguration != null)
                    {
                        serializedModel = JsonConvert.SerializeObject(websiteConfiguration);
                        if (!string.IsNullOrEmpty(serializedModel))
                        {
                            encWebModel = _cryptographyService.Encrypt(serializedModel);
                            if (encWebModel != null)
                            {
                                context.Response.Cookies.Append("encwebmodel" + domainName, encWebModel, options);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InsertErrorLogs(Guid.Empty, "UtilityService", "SetWebsiteConfigurationCookies", ex.Message, ex.Message, "Frontend", "", true);
            }
            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool RemoveWebsiteConfigurationCookies(HttpContext context)
        {
            bool status = false;
            string domainName = _websiteSettings.GetDomain().ToLower();
            try
            {
                if (context.Request.Cookies != null)
                {
                    if (context.Request.Cookies.Count > 0)
                    {
                        if (context.Request.Cookies.TryGetValue("encwebmodel" + domainName, out string? encWebModel))
                        {
                            if (encWebModel != null)
                            {
                                context.Response.Cookies.Delete("encwebmodel" + domainName);
                                // Check if the cookie was successfully deleted
                                bool statusCookie = !context.Request.Cookies.ContainsKey("encwebmodel" + domainName);
                                if (!statusCookie)
                                {
                                    status = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InsertErrorLogs(Guid.Empty, "UtilityService", "RemoveWebsiteConfigurationCookies", ex.Message, ex.Message, "Frontend", "", true);
            }
            return status;
        }
        #endregion Website Configuration Cookies

        #region UserIPAddress

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetUserIPAddress(HttpContext context)
        {
            try
            {
                //string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                //if (string.IsNullOrEmpty(ipAddress))
                //{
                //    ipAddress = context.Request.ServerVariables["REMOTE_ADDR"];
                //}
                //return ipAddress;

                string ipAddress = context.Connection.RemoteIpAddress.ToString();
                return ipAddress;

                //string ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                //if (string.IsNullOrEmpty(ipAddress))
                //{
                //    ipAddress = context.Connection.RemoteIpAddress?.ToString();
                //}
                //return ipAddress;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion UserIPAddress

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserIPAddress"></param>
        /// <returns></returns>
        public string WebsiteVisitorsAdd(string UserID, string UserIPAddress)
        {
            string Status = "";
            string DomainName = _websiteSettings.GetDomain();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserID", UserID);
                parameters.Add("@UserIPAddress", UserIPAddress.Trim());
                parameters.Add("@DomainName", DomainName.Trim());
                Status = _dbFactory.SelectCommand_SP(Status, "system_WebsiteVisitors_Add", parameters);
                return Status;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public VisitorRevenueQuestionData WebsiteVisitorRevenueQuestion_GetCount()
        {
            string DateFrom;
            string DateTo;

            VisitorRevenueQuestionData visitors = new VisitorRevenueQuestionData();
            List<RevenueGraphData> revenueGraphData = new List<RevenueGraphData>();
            string DomainName = _websiteSettings.GetDomain();
            try
            {
                //====================Get current month dates=================
                DateTime currentDate = DateTime.UtcNow;
                string currentDay = currentDate.Day.ToString();
                string currentMonth = currentDate.Month.ToString();
                string currentYear = currentDate.Year.ToString();
                DateFrom = currentYear + "-" + currentMonth + "-1";
                DateTo = currentYear + "-" + currentMonth + "-" + currentDay;
                //============================================================

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@DomainName", DomainName.Trim());
                parameters.Add("@DateFrom", DateFrom.Trim());
                parameters.Add("@DateTo", DateTo.Trim());
                visitors = _dbFactory.SelectCommand_SP(visitors, "WebsiteVisitorRevenueQuestion_Get", parameters);
                if (visitors != null)
                {
                    DynamicParameters parametersGraph = new DynamicParameters();
                    parametersGraph.Add("@DomainName", DomainName.Trim());
                    parametersGraph.Add("@DateFrom", DateFrom.Trim());
                    parametersGraph.Add("@DateTo", DateTo.Trim());
                    revenueGraphData = _dbFactory.SelectCommand_SP<RevenueGraphData>(revenueGraphData, "WebsiteRevenue_GetGraphData", parametersGraph);
                    if (revenueGraphData != null)
                    {
                        visitors.revenueGraphDatas = revenueGraphData;
                    }
                }
                return visitors;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region language-section

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public async Task<List<Language>> GetLanguagesAsync(string UserID)
        {
            List<Language> languages=new List<Language>();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@UserID", UserID);
            languages = await _dbFactory.SelectCommand_SP_List_Async<Language>(languages, "website_Language_Get", parameters);
            return languages;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public async Task<List<Language>> GetVoiceLanguagesAsync(string UserID)
        {
            List<Language> languages = new List<Language>();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@UserID", UserID);
            languages = await _dbFactory.SelectCommand_SP_List_Async<Language>(languages, "website_VoiceLanguage_Get", parameters);
            return languages;
        }

        /// <summary>
        /// Get language prompt by passing language ID
        /// </summary>
        /// <param name="LanguageID"></param>
        /// <returns></returns>
        public async Task<Language> GetLanguagesAsync_LanguageID(string LanguageID)
        {
            Language language = new Language();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@LanguageID", LanguageID);
            language = await _dbFactory.SelectCommand_SPAsync<Language>(language, "website_Language_LanguageID_Get", parameters);
            return language;
        }

        /// <summary>
        /// Update language to user
        /// </summary>
        /// <param name="LanguageID"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Message SaveLanguageToUserAsync(string LanguageID, string UserID)
        {
            Message message = new Message();
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@LanguageID", LanguageID);
                param.Add("@UserID", UserID);
                message = _dbFactory.InsertCommand_SPQuery(message, "website_LanguageVisitor_Insert", param);
                return message;
            }
            catch(Exception)
            {
                throw;
            }
        }

        #endregion language-section
    }
}
