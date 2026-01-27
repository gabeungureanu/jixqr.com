using BusinessLogic.IBusinessLogic.IAccountService;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IOpenAIClientService;
using BusinessLogic.IBusinessLogic.IUtilityService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using BusinessLogic.IBusinessLogic.SMTP_Setting;
using DataTypes.ModelDataTypes.Account;
using Microsoft.AspNetCore.Mvc;


namespace Jubilee.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory; // Inject HttpClient factory
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnv;
        private IOpenAIApiClientServices _openAIApiClientServices;
        private IAccountService _accountService;
        private ISMTP_SettingService _smtp_SettingService;
        private IWebsiteSettings _websiteSettings;
        private readonly ICryptographyService _cryptographyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUtilityService _utilityService;

        public AccountController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, ICryptographyService cryptographyService, IHostEnvironment hostEnv, IConfiguration configuration, IOpenAIApiClientServices openAIApiClientServices, IAccountService accountService, ISMTP_SettingService smtp_SettingService, IHttpContextAccessor httpContextAccessor, IWebsiteSettings websiteSettings, IWebHostEnvironment hostingEnvironment, IUtilityService utilityService)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _openAIApiClientServices = openAIApiClientServices;
            _hostEnv = hostEnv;
            _accountService = accountService;
            _cryptographyService = cryptographyService;
            _smtp_SettingService = smtp_SettingService;
            _httpContextAccessor = httpContextAccessor;
            _websiteSettings = websiteSettings;
            _hostingEnvironment = hostingEnvironment;
            _utilityService = utilityService;
           
        }
        public List<UserInformation> Get_Denomination()
        {
            List<UserInformation> userInformation = new List<UserInformation>();
            userInformation = _accountService.Get_Denomination();
            return userInformation;
        }
        public List<UserInformation> Get_Language()
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            Guid userId = Guid.TryParse(userIdString, out var parsedUserId) ? parsedUserId : Guid.Empty;
            List<UserInformation> userInformation = new List<UserInformation>();
            userInformation = _accountService.Get_Language(userId);
            return userInformation;
        }
        public List<UserInformation> Get_Timezone()
        {

            List<UserInformation> userInformation = new List<UserInformation>();
            userInformation = _accountService.Get_TimeZone();
            return userInformation;
        }
        public List<UserInformation> Get_DateFormat()
        {

            List<UserInformation> userInformation = new List<UserInformation>();
            userInformation = _accountService.Get_DateFormat();
            return userInformation;
        }
     }
}
