using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BusinessLogic.IBusinessLogic.IAccountService;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IOpenAIClientService;
using BusinessLogic.IBusinessLogic.IShareRedirector;
using BusinessLogic.IBusinessLogic.IUtilityService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using BusinessLogic.IBusinessLogic.SMTP_Setting;
using Dapper;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.Player;
using DataTypes.ModelDataTypes.ShareRedirector;
using DataTypes.ModelDataTypes.SMTPSetting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using NAudio.Wave;
using QRCoder;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JubileeGPT.Controllers
{
    public class ShareRedirectorController : Controller
    {
        private IUtilityService _utilityService;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnv;
        private IWebsiteSettings _websiteSettings;
        private ICryptographyService _cryptographyService;
        private IShareRedirector _shareRedirector;
        //Configure Azure blob 
        private readonly string _azureconnectionString;
        private readonly string _azurecontainerName;
        private readonly string _QRDefaultColor;
        private readonly string _QRDefaultLogo;
        private IAccountService _accountService;
        private IOpenAIApiClientServices _openAIApiClientServices;
        private ISMTP_SettingService _smtp_SettingService;

        public ShareRedirectorController(ILogger<ShareRedirectorController> logger, ICryptographyService cryptographyService, IHttpClientFactory clientFactory,
            IHostEnvironment hostEnv, IConfiguration configuration, IOpenAIApiClientServices openAIApiClientServices, IUtilityService utilityService,
            IWebsiteSettings websiteSettings, ISMTP_SettingService smtp_SettingService, IAccountService accountService,
            IShareRedirector shareRedirector)
        {
            _configuration = configuration;
            _hostEnv = hostEnv;
            _websiteSettings = websiteSettings;
            _cryptographyService = cryptographyService;
            _utilityService = utilityService;
            _shareRedirector = shareRedirector;
            _smtp_SettingService = smtp_SettingService;
            _accountService = accountService;
            //Get Azure blob
            _azureconnectionString = configuration["AzureBlobStorage:ConnectionString"] ?? "";
            _azurecontainerName = configuration["AzureBlobStorage:ContainerName"] ?? "";
            _openAIApiClientServices = openAIApiClientServices;
            _QRDefaultColor = configuration["DefaultQRSetting:Color"] ?? "";
            _QRDefaultLogo = configuration["DefaultQRSetting:Logo"] ?? "";
        }

        /// <summary>
        /// Generate blob content URL
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public string GetBlobSasUrl(string containerName, string blobName)
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(_azureconnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

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
                return sasUri.ToString();
            }
            return null;
        }
        /// <summary>
        /// For the purpose to remove uploaded file from DB.
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RemoveUploadedFile(int FileID)
        {
            var IsDelete = _shareRedirector.RemoveUploadedFile(FileID);
            return Json(new { success = true, message = "Delete success" });
        }

        /// <summary>
        /// For the purpose to rename folder name in DB.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RenameFolderName(int ID, string FolderName, bool IsFile)
        {
            ShareRediretor objmodel = new ShareRediretor();
            try
            {
                objmodel = _shareRedirector.RenameFolderName(ID, FolderName, IsFile);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "RenameFolderName", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(objmodel);
        }

        /// <summary>
        /// For the purpose to open the redirect URL.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Route("Redirect/{fileName}")]
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
        /// For the purpose to active & deactive file,structure.
        /// </summary>
        /// <param name="IsActive"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task<IActionResult> ActiveStructure(bool IsActive, int ID)
        {
            List<ShareRediretor> lstModel = new List<ShareRediretor>();
            try
            {
                lstModel = await _shareRedirector.ActiveStructure(IsActive, ID);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "ActiveStructure", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(lstModel);
        }

        /// <summary>
        ///  Create valid blob container name
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// For the purpose to fetch Series for drop down
        /// </summary>
        /// <param name="authorID"></param>
        /// <returns></returns>
        public IActionResult Fetch_Series(int authorID)
        {
            List<SeriesModel> lstObj = new List<SeriesModel>();
            try
            {
                lstObj = _shareRedirector.Fetch_Series(authorID);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "Fetch_Series", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(lstObj);
        }

        /// <summary>
        ///   For the purpose to fetch Product for drop down
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public IActionResult Fetch_Product(int seriesId)
        {
            List<ProductModel> lstObj = new List<ProductModel>();
            try
            {
                lstObj = _shareRedirector.Fetch_Product(seriesId);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "Fetch_Product", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(lstObj);
        }

        /// <summary>
        /// For the purpose to fetch Delivery for drop down
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IActionResult Fetch_Delivery(int productId)
        {
            List<DeliveryModel> lstObj = new List<DeliveryModel>();
            try
            {
                lstObj = _shareRedirector.Fetch_Delivery(productId);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "Fetch_Delivery", ex.Message, ex.Message, "Frontend", "", true);
            }
            return Json(lstObj);
        }

        /// <summary>
        /// For the purpose to insert product thumbnail.
        /// </summary>
        /// <param name="thumbnail"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> UploadThumbnail(IFormFile thumbnail, [FromForm] string id)
        {
            Product product = new Product();
            try
            {
                if (thumbnail == null || thumbnail.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded." });
                }

                // Create a folder for uploads if it doesn't exist
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductThumbnail");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                int productID = Convert.ToInt32(id);
                // Create a unique filename
                var fileName = $"{productID}_{Path.GetFileName(thumbnail.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await thumbnail.CopyToAsync(stream);
                }

                product = _shareRedirector.Insert_ProductThumbnail(productID, fileName, filePath);

                return Json(product);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "UploadThumbnail", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest($"An error occurred: {ex.Message}");
            }


        }

        /// <summary>
        /// For the purpose to show preview of product thumbnail.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Fetch_Productthumbnail([FromForm] string id)
        {
            Product product = new Product();
            try
            {
                var part1 = id.Split('_');
                int productID = Convert.ToInt32(part1[1]);
                if (productID != 0)
                {
                    product = _shareRedirector.Fetch_Productthumbnail(productID);
                }
                return Json(product);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "Fetch_Productthumbnail", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        #region Player
        /// <summary>
        /// Download file from azure blob.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Route("Share/{fileName}")]
        public async Task<IActionResult> Share(string fileName)
        {
            ShareRediretor objmodel = new ShareRediretor();
            string sharedLink = string.Empty;
          
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

                var ShareURL = "";
                string domainName = _websiteSettings.GetDomain();
                // Normalize to lowercase for consistent comparison
                string lowerDomain = domainName.ToLowerInvariant();
                if (lowerDomain.Contains("jubileechat") || lowerDomain.Contains("uat.askjubileegpt"))
                {
                    domainName = "jixqr.com"; // new target domain
                    //domainName = "jixqr.logixportfolio.in"; // new target domain
                    ShareURL = $"https://{domainName}/share/{fileName}";
                    return Redirect(ShareURL);
                }
                objmodel = _shareRedirector.FetchFileExtension(fileName);
                if (objmodel == null)
                {
                    return BadRequest("File extension not found or the file does not exist.");
                }
                ShareURL = $"https://{domainName}/share/{fileName}";
                

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
                TempData["Id"] = ViewBag.hdnItemId;

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

        [Route("FPDirectory/{fileName}")]
        public async Task<IActionResult> FPDirectory(string fileName)

        {
            ShareRediretor objmodel = new ShareRediretor();
            string sharedLink = string.Empty;

            try
            {
                List<PromptResponseReturn> promptResponseReturn = new List<PromptResponseReturn>();
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
                string currentUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
                if (currentUrl.Contains("FPDirectory"))
                {
                    currentUrl = currentUrl.Replace("FPDirectory/", "");
                    return Redirect(currentUrl);
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
                    ViewBag.FileShareName = fileName;
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

        /// <summary>
        /// For the purpose to show album.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Route("Album/{fileName}")]
        public async Task<IActionResult> GetAlbumList(string fileName)
        {
            ShareRediretor shareRediretor = new ShareRediretor();
            ShareRediretor Objshare = new ShareRediretor();
            try
            {
                ViewBag.FileShareName = fileName;
                //Get blob Id by nano id.
                Objshare = await _shareRedirector.GetBlobId_ByNanoId(fileName);
                if (Objshare.Status)
                {
                    fileName = Objshare.Azurefilename ?? "";
                }
                //redirect to jixqr domain.
                string ShareURL = string.Empty;
                string domainName = _websiteSettings.GetDomain();
                // Normalize to lowercase for consistent comparison
                string lowerDomain = domainName.ToLowerInvariant();
                if (lowerDomain.Contains("jubileechat") || lowerDomain.Contains("uat.askjubileegpt"))
                {
                    domainName = "jixqr.com"; // new target domain
                    //domainName = "jixqr.logixportfolio.in"; // new target domain
                    ShareURL = $"https://{domainName}/Album/{fileName}";
                    return Redirect(ShareURL);
                }

                string? paymentStatus = TempData["IsPayment"]?.ToString();
              
                ViewBag.IsPayment = paymentStatus switch
                {
                    "success" => "true",
                    "fail" => "false",
                    _ => null // do nothing
                };

                var objAlbum = await _shareRedirector.FetchAlbumDetails(Guid.Parse(fileName));
                if (objAlbum == null)
                {
                    return NotFound("notfound");
                }
                shareRediretor = _shareRedirector.FetchContainerName(objAlbum.Albums[0].FileID);
                if (objAlbum.Albums.Count > 0)
                {
                    foreach (var album in objAlbum.Albums)
                    {
                        if (album.FileExtension.Equals(".mp3"))
                        {
                            if (album.Duration == null)
                            {
                                var blobFileName = album.Azurefilename + album.FileExtension;
                                var duration = GetAudioDurationFromAzure(
                                    _azureconnectionString,
                                    shareRediretor.FolderName,
                                    blobFileName);

                                album.Duration = duration?.ToString(@"mm\:ss"); // Or save as TimeSpan

                                var FileDuration = album.Duration;
                                if (!string.IsNullOrEmpty(FileDuration))
                                {
                                    Objshare = _shareRedirector.InsertDuration(album.FileID, FileDuration);
                                    album.Duration = Objshare.Duration;
                                }
                            }
                        }
                    }
                }
                ViewBag.AlbumPoster = "ProductThumbnail/" + objAlbum.Albums[0].ProductImage;
                string? popupPoster = objAlbum.Albums[0].ProductImage ?? "";
                HttpContext.Session.SetString("Poster", popupPoster);

                ViewBag.containerName = shareRediretor.FolderName;
                return View("~/Views/Player/AlbumItem.cshtml", objAlbum);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "GetAlbumList", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest(ex);
            }

            //return Ok(lstAlbum);
        }

        [Route("AlbumSongs/{fileName}")]
        public async Task<IActionResult> AlbumSong(string fileName)
        {
            ShareRediretor shareRediretor = new ShareRediretor();
            ShareRediretor Objshare = new ShareRediretor();
            try
            {
                ViewBag.FileShareName = fileName;
                //Get blob Id by nano id.
                Objshare = await _shareRedirector.GetBlobId_ByNanoId(fileName);
                if (Objshare.Status)
                {
                    fileName = Objshare.Azurefilename ?? "";
                }
                //redirect to jixqr domain.
                string ShareURL = string.Empty;
                string domainName = _websiteSettings.GetDomain();
                // Normalize to lowercase for consistent comparison
                string lowerDomain = domainName.ToLowerInvariant();
                if (lowerDomain.Contains("jubileechat") || lowerDomain.Contains("uat.askjubileegpt"))
                {
                    domainName = "jixqr.com"; // new target domain
                    //domainName = "jixqr.logixportfolio.in"; // new target domain
                    ShareURL = $"https://{domainName}/Album/{fileName}";
                    return Redirect(ShareURL);
                }

                string? paymentStatus = TempData["IsPayment"]?.ToString();
              
                ViewBag.IsPayment = paymentStatus switch
                {
                    "success" => "true",
                    "fail" => "false",
                    _ => null // do nothing
                };

                var objAlbum = await _shareRedirector.FetchAlbumDetails(Guid.Parse(fileName));
                if (objAlbum == null)
                {
                    return NotFound("notfound");
                }
                shareRediretor = _shareRedirector.FetchContainerName(objAlbum.Albums[0].FileID);
                if (objAlbum.Albums.Count > 0)
                {
                    foreach (var album in objAlbum.Albums)
                    {
                        if (album.FileExtension.Equals(".mp3"))
                        {
                            if (album.Duration == null)
                            {
                                var blobFileName = album.Azurefilename + album.FileExtension;
                                var duration = GetAudioDurationFromAzure(
                                    _azureconnectionString,
                                    shareRediretor.FolderName,
                                    blobFileName);

                                album.Duration = duration?.ToString(@"mm\:ss"); // Or save as TimeSpan

                                var FileDuration = album.Duration;
                                if (!string.IsNullOrEmpty(FileDuration))
                                {
                                    Objshare = _shareRedirector.InsertDuration(album.FileID, FileDuration);
                                    album.Duration = Objshare.Duration;
                                }
                            }
                        }
                    }
                }
                ViewBag.AlbumPoster = "ProductThumbnail/" + objAlbum.Albums[0].ProductImage;
                string? popupPoster = objAlbum.Albums[0].ProductImage ?? "";
                HttpContext.Session.SetString("Poster", popupPoster);
                ViewBag.IsFPDirectory = true;
                ViewBag.containerName = shareRediretor.FolderName;
                return View("~/Views/Player/AlbumItem.cshtml", objAlbum);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "GetAlbumList", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest(ex);
            }

            //return Ok(lstAlbum);
        }

        /// <summary>
        /// For the purpose to get album details
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public TimeSpan? GetAudioDurationFromAzure(string connectionString, string containerName, string blobName)
        {
            var blobClient = new BlobClient(connectionString, containerName, blobName);

            using (var memoryStream = new MemoryStream())
            {
                blobClient.DownloadTo(memoryStream);
                memoryStream.Position = 0;

                using (var mp3Reader = new Mp3FileReader(memoryStream))
                {
                    return mp3Reader.TotalTime;
                }
            }
        }

        /// <summary>
        /// Download file from azure blob.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Route("GetFileDetails/{fileName}")]
        public async Task<IActionResult> GetFileDetails(string fileName)
        {
            ShareRediretor objmodel = new ShareRediretor();
            string sharedLink = string.Empty;

            try
            {
                List<PromptResponseReturn> promptResponseReturn = new List<PromptResponseReturn>();
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
                    Console.WriteLine("File extension not found or the file does not exist.");
                }
                var ShareURL = $"https://{domainName}/share/{fileName}";
                var containerName = objmodel?.FolderName;
                var AzureFileName = fileName + objmodel?.FileExtension;
                var FileName = objmodel?.FileName + objmodel?.FileExtension;
                var IsPublicAccess = objmodel?.IsPublic;

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
                if (redirectUrl != null && objmodel != null)
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

                        objmodel.RedirectURL = redirectUrl;
                    }
                }
                else
                {
                    objmodel.URLMapLink = sharedLink;
                    return Ok(objmodel);

                }

                return NotFound("File not found.");

            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "GetFileDetails", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// For the purpose to show album page.
        /// </summary>
        /// <returns></returns>
        [Route("Albums/{containerName}")]
        public IActionResult Albums(string containerName)
        {
            try
            {
                ViewBag.IsAlbum = true;
                if (!string.IsNullOrEmpty(containerName))
                {
                    ViewBag.ConatinerName = containerName;
                }
                return View("~/Views/Player/Album.cshtml");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "Album", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest();
            }

        }
        /// <summary>
        /// For the purpose to filter songs by search.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="conatinerName"></param>
        /// <returns></returns>
        public PartialViewResult SearchSongs([FromForm] string text, string conatinerName)
        {
            Product product = new Product();
            try
            {
                var Album = _shareRedirector.FetchAlbumSongsBySearch(text, conatinerName);
                return PartialView("~/Views/Shared/Partial/AlbumList.cshtml", Album);

            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "SearchSongs", ex.Message, ex.Message, "Frontend", "", true);
                return PartialView("404");
            }


        }

        /// <summary>
        /// For the purpose to show blob's content preview.
        /// </summary>
        /// <param name="sharelink"></param>
        /// <param name="previewfilename"></param>
        /// <returns></returns>
        public IActionResult SongPreview(string sharelink, string previewfilename)
        {
            try
            {
                var ShareURL = sharelink;
                string Azurefile = ShareURL.Split('/').Last();
                ShareRediretor objmodel = new ShareRediretor();
                objmodel = _shareRedirector.FetchFileExtension(Azurefile);

                var containerName = objmodel?.FolderName;
                var FileName = objmodel?.FileName + objmodel?.FileExtension;
                var ItemName = objmodel?.FileName;
                var IsPublicAccess = objmodel?.IsPublic;
                var Duration = objmodel?.Duration ?? "0.00";
                var IsActive = objmodel?.IsActive;
                var ProductName = objmodel?.Product;
                var ProductImage = objmodel?.ProductImage;
                var redirectUrl = objmodel?.RedirectURL;

                var BlobFileName = Azurefile + objmodel?.FileExtension;

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

                var AzurefileLink = GetBlobSasUrl(containerName, BlobFileName);
                //
                ViewBag.Content = true;
                ViewBag.FileName = previewfilename;
                if (objmodel?.FileExtension != ".mp3")
                {
                    ViewBag.SharedLink = AzurefileLink;
                }
                else
                {
                    ViewBag.SharedLink = ShareURL;
                }
                return PartialView("~/Views/Shared/Partial/Preview.cshtml");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "SongPreview", ex.Message, ex.Message, "Frontend", "", true);
                return PartialView("404");
            }
        }

        /// <summary>
        /// For the purpose to play video on song's playlist.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public IActionResult VideoPlayer(string extension, string file)
        {
            try
            {
                string Azurefile = file;
                ShareRediretor objmodel = new ShareRediretor();
                objmodel = _shareRedirector.FetchFileExtension(Azurefile);

                var containerName = objmodel?.FolderName;
                var FileName = objmodel?.FileName + objmodel?.FileExtension;
                var ItemName = objmodel?.FileName;
                var IsPublicAccess = objmodel?.IsPublic;
                var Duration = objmodel?.Duration ?? "0.00";
                var IsActive = objmodel.IsActive;
                var ProductName = objmodel?.Product;
                var ProductImage = objmodel?.ProductImage;
                var redirectUrl = objmodel?.RedirectURL;

                var BlobFileName = Azurefile + objmodel?.FileExtension;

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

                var AzurefileLink = GetBlobSasUrl(containerName, BlobFileName);
                //
                ViewBag.Content = true;
                ViewBag.FileName = FileName;
                if (objmodel?.FileExtension != ".mp3")
                {
                    ViewBag.SharedLink = AzurefileLink;
                }
                else
                {
                    //ViewBag.SharedLink = ShareURL;
                }
                return PartialView("~/Views/Shared/Partial/Preview.cshtml");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "VideoPlayer", ex.Message, ex.Message, "Frontend", "", true);
                return PartialView("404");
            }
        }

        /// <summary>
        /// To bind the store drop down.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetStoreDetails(int id)
        {
            List<StoreModel> lstobj = new List<StoreModel>();
            try
            {
                if (id != 0)
                {
                    lstobj = _shareRedirector.GetStoreDetails(id);
                    return Json(lstobj);
                }
                return Json("Error");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "GetStoreDetails", ex.Message, ex.Message, "Frontend", "", true);
                return Json("Error");
            }
        }

        /// <summary>
        /// Update Author's store from blob and DB.
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MoveAuthor(int storeID, int authorId)
        {
            ShareRediretor shareRediretor = new ShareRediretor();
            List<Blobfiles> lstBlob = new List<Blobfiles>();
            BlobConatiner blobConatiner = new BlobConatiner();
            try
            {
                if (storeID == 0)
                {
                    return Json(new { success = false, message = "Store id is blank." });
                }
                else if (authorId == 0)
                {
                    return Json(new { success = false, message = "Author id is blank." });
                }
                blobConatiner = _shareRedirector.FetchBlobConatiners(storeID, authorId);
                string connectionString = _azureconnectionString;
                string sourceContainer = blobConatiner.SourceContainer;
                string targetContainer = blobConatiner.TargetContainer;
                lstBlob = _shareRedirector.FetchBlobFiles_byAuthor(authorId);

                //MoveBlobFilesAsync(lstBlob, sourceContainer, targetContainer, connectionString);
                var moveResult = await MoveBlobFilesAsync(lstBlob, sourceContainer, targetContainer, connectionString);
                if (moveResult.IsSuccess)
                {
                    shareRediretor = _shareRedirector.MoveAuthorToStore(storeID, authorId);
                }

                return Json(shareRediretor);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "SubmitStore", ex.Message, ex.Message, "Frontend", "", true);
                return Json(ex.Message);
            }
        }

        /// <summary>
        /// For the Purpose to move file from one container to another container in azure blob.
        /// </summary>
        /// <param name="lstBlob"></param>
        /// <param name="sourceContainerName"></param>
        /// <param name="targetContainerName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<MoveBlobResult> MoveBlobFilesAsync(List<Blobfiles> lstBlob, string sourceContainerName, string targetContainerName, string connectionString)
        {
            MoveBlobResult result = new MoveBlobResult();

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient sourceContainer = blobServiceClient.GetBlobContainerClient(sourceContainerName);
            BlobContainerClient targetContainer = blobServiceClient.GetBlobContainerClient(targetContainerName);

            foreach (var blob in lstBlob)
            {
                string blobName = blob.BlobFileName + blob.FileExtension;

                try
                {
                    BlobClient sourceBlob = sourceContainer.GetBlobClient(blobName);
                    BlobClient targetBlob = targetContainer.GetBlobClient(blobName);

                    if (await sourceBlob.ExistsAsync())
                    {
                        var copyResult = await targetBlob.StartCopyFromUriAsync(sourceBlob.Uri);

                        BlobProperties properties;
                        do
                        {
                            await Task.Delay(200);
                            properties = await targetBlob.GetPropertiesAsync();
                        }
                        while (properties.CopyStatus == CopyStatus.Pending);

                        if (properties.CopyStatus == CopyStatus.Success)
                        {
                            await sourceBlob.DeleteIfExistsAsync();
                            result.SuccessFiles.Add(blobName);
                        }
                        else
                        {
                            result.FailedFiles.Add(blobName);
                        }
                    }
                    else
                    {
                        result.FailedFiles.Add(blobName); // File does not exist
                    }
                }
                catch
                {
                    result.FailedFiles.Add(blobName); // Any unexpected error
                }
            }

            result.IsSuccess = result.FailedFiles.Count == 0;
            return result;
        }
        #endregion

        #region Viral Music Player

        /// <summary>
        /// Bind the viral music player shared popups.
        /// </summary>
        /// <param name="popupType"></param>
        /// <returns></returns>

        [HttpPost]
        public PartialViewResult ShareMusicDetails([FromForm] string popupType)
        {
            ShareMusicModel objShareMusic = new ShareMusicModel();
            try
            {
                objShareMusic = _shareRedirector.FetchShareMusicPopupByPopupType(popupType);
                return PartialView("~/Views/Shared/Partial/ViralMusicPlayer.cshtml", objShareMusic);

            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "ShareMusic", ex.Message, ex.Message, "Frontend", "", true);
                return PartialView("404");
            }

        }

        /// <summary>
        /// For the purpose to unlock album by ID
        /// </summary>
        /// <param name="AlbumID"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UnlockAlbumByID(int AlbumID)
        {
            ShareMusicModel objShareMusic = new ShareMusicModel();
            bool isAlbumPaid = false;
            try
            {
                if (Request.Cookies.TryGetValue("JubileeAlbum", out var cookie))
                {
                    var decryptJsonCookie = _cryptographyService.Decrypt(cookie);
                    var decryptCookie = JsonSerializer.Deserialize<List<KeyValuePair<int, DateTime>>>(decryptJsonCookie);
                    bool exists = decryptCookie.Any(x => x.Key == AlbumID);

                    if (!exists)
                    {
                        isAlbumPaid = false;
                    }
                    else
                    {
                        isAlbumPaid = true;
                    }
                }
                // Success response
                return Json(new { isAlbumPaid });

            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "ShareMusic", ex.Message, ex.Message, "Frontend", "", true);
                return PartialView("404");
            }

        }

        /// <summary>
        /// For the purpose to show album page.
        /// </summary>
        /// <returns></returns>
        [Route("ShareMusic/{containerName}")]
        public IActionResult ShareMusic(int SongIndex)
        {
            try
            {
                ViewBag.ShareMusic = true;
                if (SongIndex > 0)
                {
                    ViewBag.SongIndex = SongIndex;
                }
                return View("~/Views/Player/ViralMusicPopup.cshtml");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "ShareMusic", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest();
            }

        }

        /// <summary>
        /// add hit count .
        /// </summary>
        /// <returns></returns>
        [Route("ShareHit/{ItemId}")]
        public IActionResult AddHitCount(string ItemId)
        {
            try
            {
                ViewBag.ShareMusic = true;
                int retVal = 0;
                if (!string.IsNullOrEmpty(ItemId))
                {
                    retVal = _shareRedirector.AddHitCount(ItemId);
                }
                return Json(retVal);
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "AddHitCount", "Share", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest();
            }

        }


        /// <summary>
        /// For the purpose to sent OTP to verify email.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/SendShareOTP")]
        public async Task<IActionResult> SendShareOTP([FromBody] EmailSendOTP model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string DomainName = _websiteSettings.GetDomain();
                    ShareRediretor objShareRediretor = _shareRedirector.Fetch_AlbumDetails(Guid.Parse(model.shareID));
                    if (objShareRediretor != null && !"Blob Id is not exists".Equals(objShareRediretor.Message))
                    {
                        //Get Album Details
                        var paymentDetails = _shareRedirector.Fetch_PaymentDetails(objShareRediretor.ID, model.Email);
                        if (paymentDetails != null)
                        {
                            //Get SMTP settings from database
                            _SMTPSetting objSMTP = _smtp_SettingService.Fetch_SMTP_Settings_By_SMTPName("uat.askjubileegpt.com");
                            paymentDetails.AlbumName = objShareRediretor.FolderName;
                            //Send email to user
                            int code = await _accountService.Send_OTPToUser(paymentDetails, objSMTP, DomainName, _hostEnv.ContentRootPath);
                            if (code > 0)
                            {
                                //update code
                                _shareRedirector.UpdateShareOTP(objShareRediretor.ID, model.Email, code);
                            }
                            return Ok("success");
                        }
                        else
                        {
                            return Ok("no-payment-exist");
                        }
                    }
                    else
                    {
                        return Ok("albumid-not-exist");
                    }
                }
                return Ok("fail");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "SendOTPVerification", ex.Message, ex.Message, "Frontend", "", true);
                return Ok("fail");
            }
        }

        


        /// <summary>
        /// Resend the email verify OTP.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/ResendOTP")]
        public async Task<IActionResult> ResendOTP([FromBody] EmailSendOTP model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string DomainName = _websiteSettings.GetDomain();
                    ShareRediretor objShareRediretor = _shareRedirector.Fetch_AlbumDetails(Guid.Parse(model.shareID));
                    if (objShareRediretor != null && !"Blob Id is not exists".Equals(objShareRediretor.Message))
                    {
                        //Get Album Details
                        var paymentDetails = _shareRedirector.Fetch_PaymentDetails(objShareRediretor.ID, model.Email);
                        if (paymentDetails != null)
                        {
                            //Get SMTP settings from database
                            _SMTPSetting objSMTP = _smtp_SettingService.Fetch_SMTP_Settings_By_SMTPName(DomainName);
                            paymentDetails.AlbumName = objShareRediretor.FolderName;
                            //Send email to user
                            int code = await _accountService.Send_OTPToUser(paymentDetails, objSMTP, DomainName, _hostEnv.ContentRootPath);
                            if (code > 0)
                            {
                                //update code
                                _shareRedirector.UpdateShareOTP(objShareRediretor.ID, model.Email, code);
                            }
                            return Ok("success");
                        }
                        else
                        {
                            return Ok("no-payment-exist");
                        }
                    }
                    else
                    {
                        return Ok("albumid-not-exist");
                    }
                }
                return Ok("fail");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "SendOTPVerification", ex.Message, ex.Message, "Frontend", "", true);
                return Ok("fail");
            }
        }

        /// <summary>
        /// OTP Verify
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="ShareID"></param>
        /// <param name="OTP"></param>
        /// <returns></returns>
        [Route("api/VerifyShareOTP")]
        public IActionResult VerifyShareOTP([FromBody] EmailSendOTP model)
        {
            try
            {
                if (model != null && !string.IsNullOrEmpty(model.shareID) && !string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.OTP))
                {
                    string DomainName = _websiteSettings.GetDomain();
                    ShareRediretor objShareRediretor = _shareRedirector.Fetch_AlbumDetails(Guid.Parse(model.shareID));
                    if (objShareRediretor != null && !"Blob Id is not exists".Equals(objShareRediretor.Message))
                    {
                        //Get Album Details
                        var paymentDetails = _shareRedirector.Fetch_PaymentDetails(objShareRediretor.ID, model.Email);
                        if (paymentDetails != null)
                        {
                            //Send email to user
                            var result = _shareRedirector.OTPVerify(objShareRediretor.ID, model.Email, int.Parse(model.OTP));
                            if (result != null && result == "success")
                            {
                                var newcookie = new List<KeyValuePair<int, DateTime>>();
                                if (Request.Cookies.TryGetValue("JubileeAlbum", out var cookie))
                                {
                                    var decryptJsonCookie = _cryptographyService.Decrypt(cookie);
                                    var decryptCookie = JsonSerializer.Deserialize<List<KeyValuePair<int, DateTime>>>(decryptJsonCookie);
                                    bool exists = decryptCookie.Any(x => x.Key == objShareRediretor.ID);

                                    if (!exists)
                                    {
                                        decryptCookie.Add(new KeyValuePair<int, DateTime>(objShareRediretor.ID, DateTime.Now.AddYears(1)));
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
                                    newcookie.Add(new KeyValuePair<int, DateTime>(objShareRediretor.ID, DateTime.Now.AddYears(1)));
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
                                return Ok(result);
                            }
                        }
                        else
                        {
                            return Ok("no-payment-exist");
                        }
                    }
                    else
                    {
                        return Ok("albumid-not-exist");
                    }
                }
                return Ok("fail");
            }
            catch (Exception ex)
            {
                _utilityService.InsertErrorLogs(Guid.Empty, "ShareRedirectorController", "SendOTPVerification", ex.Message, ex.Message, "Frontend", "", true);
                return BadRequest("server-error");
            }
        }



 
        #endregion




    }
}
