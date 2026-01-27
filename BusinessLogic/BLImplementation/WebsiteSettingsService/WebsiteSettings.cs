using Azure.Core;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes.Account;
using DataTypes.ModelDataTypes.Subscription;
using DataTypes.ModelDataTypes.WebsiteSettings;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using DataTypes.ModelDataTypes.Home;

namespace BusinessLogic.BLImplementation.WebsiteSettingsService
{
    public class WebsiteSettings : IWebsiteSettings
    {
        private DbFactory _dbFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public WebsiteSettings(DbFactory dbFactory, IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetDomain()
        {
            string? currentDomain = _httpContextAccessor?.HttpContext?.Request.Host.Host;
            if (!string.IsNullOrEmpty(currentDomain))
            {
                if (currentDomain.StartsWith("www."))
                {
                    currentDomain = currentDomain.Substring(4);
                }
            }
            return currentDomain;
        }

        public async Task<PrefixPrompt> GetPromptPrefixAsync(string domainName)
        {
            PrefixPrompt prefixPrompt = new PrefixPrompt();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@DomainName", domainName);
                prefixPrompt = await _dbFactory.SelectCommand_SPAsync(prefixPrompt, "system_Prompts_Get", dParam);
                return prefixPrompt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<WebsiteConfiguration> GetWebsiteAsync(string domainName)
        {
            WebsiteConfiguration website = new WebsiteConfiguration();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@DomainName", domainName);
                website = await _dbFactory.SelectCommand_SPAsync(website, "system_Websites_Get", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return website;
        }

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
                parameters.Add("@Description", errorDescription.Trim());
                parameters.Add("@Active", active);
                status = _dbFactory.SelectCommand_SP(status, "system_ErrorLog_Add", parameters);
                return status;                
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
