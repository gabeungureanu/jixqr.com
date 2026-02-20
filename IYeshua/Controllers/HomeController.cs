using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BusinessLogic.BLImplementation.UtilityService;
using BusinessLogic.IBusinessLogic.IAccountService;
using BusinessLogic.IBusinessLogic.IAIService;
using BusinessLogic.IBusinessLogic.IContentManagerService;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IOpenAIClientService;
using BusinessLogic.IBusinessLogic.IShareRedirector;
using BusinessLogic.IBusinessLogic.ISubscription;
using BusinessLogic.IBusinessLogic.IUtilityService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.ShareRedirector;
//using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Text;
using System.Text.RegularExpressions;


namespace Jubilee.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer? _localizer;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnv;
        private IOpenAIApiClientServices _openAIApiClientServices;
        private IWebsiteSettings _websiteSettings;
        private ICryptographyService _cryptographyService;
        private IUtilityService _utilityService;
        private IAccountService _accountService;
        private ISubsService _subsService;
        private IContentManager _contentManager;
        private IAICommunicationService _aiCommunication;
        private static string _systemPrompt = string.Empty;
        private static string _voicePrompt = string.Empty;
        private static string _languageName = string.Empty;
        // Initialize a conversation history (in-memory for now)
        private static List<ChatMessage> messageHistory = new List<ChatMessage>();
        private static bool isLastVisit = false;
        private string systemPrompt = string.Empty;
        // Open AI ChatGPT impelmentation and maintain session
        private readonly IMemoryCache _memoryCache;
        private List<object> _conversationSession = new List<object>();
        private List<object> _userMemorySession = new List<object>();
        //private List<object> _userMemory = new List<object>();
        private string ConversationCacheKey = "ConversationSession";
        private const int MaxRetries = 6;
        private const int InitialDelayMilliseconds = 2000;
        //Configure Azure blob 
        private readonly string _azureconnectionString;
        private readonly string _azurecontainerName;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenStorageService _tokenStorage;
        private IShareRedirector _shareRedirector;

        public HomeController(ILogger<HomeController> logger, ICryptographyService cryptographyService, IHttpClientFactory clientFactory,
            IHostEnvironment hostEnv, IConfiguration configuration, IOpenAIApiClientServices openAIApiClientServices,
            IAICommunicationService aICommunicationService, IWebsiteSettings websiteSettings, IUtilityService utilityService,
            IAccountService accountService, ISubsService subsService, IContentManager contentManager, IMemoryCache memoryCache, TokenStorageService tokenStorage, IShareRedirector shareRedirector)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _openAIApiClientServices = openAIApiClientServices;
            _hostEnv = hostEnv;
            _websiteSettings = websiteSettings;
            _cryptographyService = cryptographyService;
            _utilityService = utilityService;
            _accountService = accountService;
            _subsService = subsService;
            _contentManager = contentManager;
            _aiCommunication = aICommunicationService;
            _memoryCache = memoryCache;
            //Get Azure blob
            _azureconnectionString = configuration["AzureBlobStorage:ConnectionString"] ?? "";
            _azurecontainerName = configuration["AzureBlobStorage:ContainerName"] ?? "";
            _tokenStorage = tokenStorage;
            _shareRedirector = shareRedirector;

            systemPrompt = _configuration["SearchPrefixText:Prefix"] ?? "";
            if (!string.IsNullOrEmpty(systemPrompt) && isLastVisit == false)
                messageHistory.Add(new ChatMessage(ChatRole.System, systemPrompt));
            isLastVisit = true;
        }

        /// <summary>
        /// This is Home page. In this function we are maintining the getting the Cookie for Login user, and the website settings.
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("{id?}")]
        public async Task<IActionResult> Index(string id)

        {
            try
            {
                
                bool isSuccess = (TempData["IsSuccess"] as bool?) ?? false;
                //var  objmodel = await _shareRedirector.GetBlobId_ByNanoId(id);
                //ViewBag.hdnItemId = objmodel.ID;
                if (!string.IsNullOrEmpty(id) && !isSuccess)
                {
                    if (id.Contains("re-"))
                    {
                        id = id.Replace("re-", "");
                        return await RedirectFile(id);
                    }
                }
                ViewBag.hdnItemId = TempData["Id"];
                if (!string.IsNullOrEmpty(id) && !isSuccess)
                {
                    return await FPDirectory(id);
                }
                ViewBag.SharedLink = TempData["SharedLink"];
                ViewBag.FileName = TempData["FileName"];
                ViewBag.ShareURL = TempData["ShareURL"];
                ViewBag.fileExtension = TempData["fileExtension"];
                ViewBag.Content = isSuccess;
                ViewBag.IsPreview = Convert.ToBoolean(TempData["Preview"]);
                TempData["Preview"] = ViewBag.IsPreview;
                //--------------------Website cookies section----------------------
                string domainName = _websiteSettings.GetDomain();
                // Full URL including path
                string currentUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
                // Normalize trailing slash
                currentUrl = currentUrl.TrimEnd('/');
                ViewBag.DomainName = domainName;
                ViewBag.PageName = "index";
                
                
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "HomeController", "Index", ex.Message, ex.Message, "Frontend", "", true);
            }
            return View();
        }

        /// <summary>
        /// Free song redirect here
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<IActionResult> FPDirectory(string fileName)

        {
            ShareRediretor objmodel = new ShareRediretor();
            string sharedLink = string.Empty;
            string blobId = string.Empty;
            try
            {
                List<PromptResponseReturn> promptResponseReturn = new List<PromptResponseReturn>();
                ViewBag.FileShareName = fileName;
                //Get blob Id by nano id.
                objmodel = await _shareRedirector.GetBlobId_ByNanoId(fileName);
                ViewBag.hdnItemId = objmodel.ID;
                if (objmodel.Status)
                {
                    fileName = objmodel.Azurefilename ?? "";
                }

                promptResponseReturn = await _openAIApiClientServices.ShareChat(Guid.Parse(fileName));
                if (promptResponseReturn != null)
                {
                    ViewBag.OGTitle = "";
                    string desc = "";
                    ViewBag.Description = ""; // Assign the cleaned-up text to ViewBag
                }

                string domainName = _websiteSettings.GetDomain();
                // Normalize to lowercase for consistent comparison
                string lowerDomain = domainName.ToLowerInvariant();
                if (lowerDomain.Contains("jubileechat") || lowerDomain.Contains("uat.askjubileegpt"))
                {
                    domainName = "jixqr.com"; // new target domain
                    //domainName = "jixqr.logixportfolio.in"; // new target domain
                }

                objmodel = _shareRedirector.FetchFileExtension(fileName);
                if (objmodel == null)
                {
                    return BadRequest("File extension not found or the file does not exist.");
                }
                var ShareURL = $"https://{domainName}/share/{fileName}";

                var containerName = objmodel?.FolderName;
                var AzureFileName = fileName + objmodel?.FileExtension;
                var FileName = objmodel?.FileName + objmodel?.FileExtension;
                var ItemName = objmodel?.FileName;
                var IsPublicAccess = objmodel?.IsPublic;
                var Duration = objmodel?.Duration ?? "0.00";
                var IsActive = objmodel.IsActive;
                var ProductName = objmodel?.Product;
                var ProductImage = objmodel?.ProductImage;
                var BlobFileName = fileName;

                if (!IsActive)
                {
                    return View("~/Views/Home/ContentRestricted.cshtml");
                }

                // Convert to lowercase and remove invalid characters
                containerName = Regex.Replace(containerName.ToLower(), @"[^a-z0-9-]", "-");

                // Ensure the name starts with a letter or number
                if (!char.IsLetterOrDigit(containerName[0]))
                {
                    containerName = "c" + containerName;  // Prefix with 'c' if the first character is invalid
                }

                // Trim to max 63 characters
                if (containerName.Length > 63)
                {
                    containerName = containerName.Substring(0, 63);
                }

                // Ensure the name doesn't end with a hyphen
                containerName = containerName.Trim('-');

                containerName = GenerateValidBlobContainerName(containerName);

                sharedLink = GetBlobSasUrl(containerName, AzureFileName);
                // Download file when redirecURL is blank
                var redirectUrl = objmodel?.RedirectURL;
                if (redirectUrl != null)
                {
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        //return Redirect(redirectUrl);

                        // Ensure the URL is absolute
                        if (!Uri.IsWellFormedUriString(redirectUrl, UriKind.Absolute))
                        {
                            // Check if it's missing a scheme (http or https)
                            if (!redirectUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                                !redirectUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                // Default to http:// for non-SSL users, change if necessary
                                redirectUrl = "http://" + redirectUrl;
                            }
                        }

                        return Redirect(redirectUrl);
                    }
                    //sharedLink = GetBlobSasUrl(containerName, FileName);
                }
                else
                {

                    TempData["SharedLink"] = sharedLink;
                    TempData["FileName"] = FileName;
                    TempData["ShareURL"] = ShareURL;
                    ViewBag.IsPreview = Convert.ToBoolean(TempData["Preview"]);

                    ViewBag.SharedLink = sharedLink;
                    ViewBag.FileName = FileName;
                    ViewBag.ItemName = ItemName;
                    ViewBag.ShareURL = ShareURL;

                    ViewBag.Duration = Duration;
                    ViewBag.ProductName = ProductName;
                    ViewBag.ProductImage = ProductImage;
                    ViewBag.IsFPDirectory = true;

                    if (objmodel != null && objmodel.FileExtension != null)
                    {
                        if (objmodel.FileExtension.Equals(".mp3"))
                        {
                            ViewBag.containerName = containerName;
                            return View("~/Views/Player/Index.cshtml");
                        }
                        else
                        {
                            TempData["IsSuccess"] = true;
                            return RedirectToAction("Index", "Home");
                        }
                    }

                }

                return NotFound("File not found in Azure and no redirect URL available.");

            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "Share", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        public static string GenerateValidBlobContainerName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Container name cannot be empty");

            // Convert to lowercase
            string lower = input.ToLowerInvariant();

            // Replace invalid characters with a dash
            string valid = Regex.Replace(lower, @"[^a-z0-9-]", "-");

            // Remove consecutive dashes
            valid = Regex.Replace(valid, @"-+", "-");

            // Trim dashes from start and end
            valid = valid.Trim('-');

            // Ensure length is within 3 to 63 characters
            if (valid.Length < 3)
                valid = valid.PadRight(3, 'a'); // pad with 'a'
            else if (valid.Length > 63)
                valid = valid.Substring(0, 63);

            // Ensure it starts and ends with a letter or number
            if (!char.IsLetterOrDigit(valid[0]))
                valid = "a" + valid;
            if (!char.IsLetterOrDigit(valid[^1]))
                valid = valid + "a";

            return valid;
        }

        public string GetBlobSasUrl(string containerName, string blobName)
        {
            try
            {
                if (string.IsNullOrEmpty(_azureconnectionString))
                {
                    _logger.LogError("Azure connection string is not configured");
                    return null;
                }

                BlobServiceClient blobServiceClient = new BlobServiceClient(_azureconnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                if (!blobClient.Exists())
                {
                    _logger.LogWarning($"Blob not found: {containerName}/{blobName}");
                    return null;
                }

                if (blobClient.CanGenerateSasUri)
                {
                    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                    {
                        BlobName = blobName,
                        ExpiresOn = DateTimeOffset.UtcNow.AddYears(100),
                        Resource = "b"
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                    _logger.LogInformation($"Generated SAS URL for: {containerName}/{blobName}");
                    return sasUri.ToString();
                }

                _logger.LogError($"Cannot generate SAS URI for blob: {containerName}/{blobName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating SAS URL for: {containerName}/{blobName}");
                return null;
            }
        }

        public async Task<IActionResult> RedirectFile(string fileName)
        {
            ShareRediretor objmodel = new ShareRediretor();
            ShareRediretor objmodelShare = new ShareRediretor();
            try
            {
                //Get blob Id by nano id.
                objmodel = await _shareRedirector.GetBlobId_ByNanoId(fileName);
                if (objmodel.Status)
                {
                    fileName = objmodel.Azurefilename ?? "";
                }
                string domainName = _websiteSettings.GetDomain();
                objmodelShare = await _shareRedirector.FetchAzureURL(fileName);

                objmodel = _shareRedirector.FetchRedirectURL(fileName);
                if (!string.IsNullOrEmpty(objmodelShare.Azurefilename))
                {
                    return RedirectToAction("Share", new { fileName = objmodelShare.Azurefilename });
                }
                var MapURL = objmodel.URLMapLink;
                if (objmodel == null)
                {
                    Console.WriteLine("Map URL not found or the file does not exist.");
                }

                if (!string.IsNullOrEmpty(MapURL))
                {
                    // Ensure the URL is absolute
                    if (!Uri.IsWellFormedUriString(MapURL, UriKind.Absolute))
                    {
                        // Check if it's missing a scheme (http or https)
                        if (!MapURL.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                            !MapURL.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            // Default to http:// for non-SSL users, change if necessary
                            MapURL = "http://" + MapURL;
                        }
                    }

                    return Redirect(MapURL);
                }
                return NotFound("Map URL found and no redirect URL available.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// To bind language on the Right panel
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> BindLanguages()
        {
            List<Language> languages = new List<Language>();
            try
            {
                string? userID = HttpContext.Session.GetString("UserID");
                if (!string.IsNullOrEmpty(userID))
                {
                    languages = await _utilityService.GetLanguagesAsync(userID);
                }
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "HomeController", "BindLanguages", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(languages);
        }

        [HttpGet]
        public async Task<IActionResult> BindAllVoice()
        {
            List<TextToSpeachVoice> textToSpeachVoices = new List<TextToSpeachVoice>();
            try
            {
                textToSpeachVoices = await _aiCommunication.GetVoice();
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "HomeController", "BindVoiceAsync", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(textToSpeachVoices);
        }

        //[HttpGet]
        public async Task<IActionResult> BindVoiceByUser()
        {
            TextToSpeachVoice textToSpeachVoice = new TextToSpeachVoice();
            try
            {
                string? UserID = HttpContext?.Session?.GetString("UserID");
                if (!string.IsNullOrEmpty(UserID))
                {
                    textToSpeachVoice = await _aiCommunication.GetVoiceByUserID(UserID);
                }

            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "HomeController", "BindVoiceByUserAsync", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(textToSpeachVoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetWebSiteConfiguration()
        {
            try
            {
                string domainName = Request.Host.Host;
                var websiteConfig = await _websiteSettings.GetWebsiteAsync(domainName);
                if (websiteConfig != null)
                {
                    return Json(new
                    {
                        system_WebsiteName = websiteConfig.System_WebsiteName,
                        system_DomainName = websiteConfig.System_DomainName,
                        faviconImagePath = websiteConfig.FaviconImagePath,
                        mainImagePath = websiteConfig.MainImagePath,
                        brandImagePath = websiteConfig.BrandImagePath,
                        mainImageAltText = websiteConfig.MainImageAltText,
                        resReplacedText = websiteConfig.ResReplacedText
                    });
                }
                return Json(new { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWebSiteConfiguration");
                _utilityService.InsertErrorLogs(Guid.Empty, "HomeController", "GetWebSiteConfiguration", ex.Message, ex.StackTrace ?? "", "Frontend", "", true);
                return Json(new { });
            }
        }
    }
}
