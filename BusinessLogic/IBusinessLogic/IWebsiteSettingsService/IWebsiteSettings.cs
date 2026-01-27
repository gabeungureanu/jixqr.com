using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.WebsiteSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.IWebsiteSettingsService
{
    public interface IWebsiteSettings
    {
        // Return current domain name
        string GetDomain();
        Task<WebsiteConfiguration> GetWebsiteAsync(string domainName);
        Task<PrefixPrompt> GetPromptPrefixAsync(string domainName);
        string InsertErrorLogs(Guid userID, string errorPage, string methodName, string errorMessage, string errorDescription, string errorMode, string errorCode, bool active = true);
    }
}
