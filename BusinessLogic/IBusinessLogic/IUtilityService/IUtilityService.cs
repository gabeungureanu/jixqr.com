using DataTypes.ModelDataTypes.Account;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.WebsiteSettings;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.IUtilityService
{
    public interface IUtilityService
    {
        string CheckPlanUsingUserIDAsync(string UserID);
        string InsertErrorLogs(Guid userID, string errorPage, string methodName, string errorMessage, string errorDescription, string errorMode, string errorCode, bool active = true);
        bool SetRememberMeCookies(HttpContext context, RememberMe rememberMe);
        bool RemoveRememberMeCookies(HttpContext context);
        Task<RememberMe> GetRememberMeCookies(HttpContext context);
        bool SetWebsiteConfigurationCookies(WebsiteConfiguration websiteConfiguration, HttpContext context);
        Task<WebsiteConfiguration> GetWebsiteConfigurationCookies(HttpContext context);
        bool RemoveWebsiteConfigurationCookies(HttpContext context);
        string GetUserIPAddress(HttpContext context);
        string WebsiteVisitorsAdd(string UserID, string UserIPAddress);
        VisitorRevenueQuestionData WebsiteVisitorRevenueQuestion_GetCount();
        Task<List<Language>> GetLanguagesAsync(string UserID);
        Task<List<Language>> GetVoiceLanguagesAsync(string UserID);
        Task<Language> GetLanguagesAsync_LanguageID(string LanguageID);
        Message SaveLanguageToUserAsync(string LanguageID, string UserID);
    }
}
