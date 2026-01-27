using BusinessLogic.BLImplementation.AccountService;
using BusinessLogic.BLImplementation.AIService;
using BusinessLogic.BLImplementation.ContentManagerService;
using BusinessLogic.BLImplementation.CryptoService;
using BusinessLogic.BLImplementation.OpenAIClientService;
using BusinessLogic.BLImplementation.SMTP_Setting;
using BusinessLogic.BLImplementation.Subscription;
using BusinessLogic.BLImplementation.UtilityService;
using BusinessLogic.BLImplementation.WebsiteSettingsService;
using BusinessLogic.IBusinessLogic.IAccountService;
using BusinessLogic.IBusinessLogic.IAIService;
using BusinessLogic.IBusinessLogic.IContentManagerService;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IOpenAIClientService;
using BusinessLogic.IBusinessLogic.ISubscription;
using BusinessLogic.IBusinessLogic.IUtilityService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using BusinessLogic.IBusinessLogic.SMTP_Setting;
using DataAccess;
using JubileeGPT.Controllers;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using BusinessLogic.BLImplementation.ShareRedirector;
using BusinessLogic.IBusinessLogic.IShareRedirector;
//using Stripe;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();

#region Activate Service
builder.Services.AddScoped<IOpenAIApiClientServices, OpenAIApiClientServices>();
builder.Services.AddScoped<DbFactory, DbFactory>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IShareRedirector, ShareRedirector>();
builder.Services.AddScoped<ICryptographyService, CryptographyService>();
builder.Services.AddScoped<ISubsService, SubsService>();
builder.Services.AddScoped<ISMTP_SettingService, SMTP_SettingService>();
builder.Services.AddScoped<IWebsiteSettings, WebsiteSettings>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IContentManager, ContentManager>();
builder.Services.AddScoped<IAnthropicClaudeAIService, AnthropicClaudeAIService>();
builder.Services.AddScoped<IAICommunicationService, AICommunicationService>();
#endregion

// Configure services
builder.Services.AddHttpClient("AnthropicClient", client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/v1/");
    client.Timeout = TimeSpan.FromMinutes(30); // Example timeout of 1 minute
                                               // Add other configurations if needed
});

builder.Services.AddHttpContextAccessor();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize = 2147483647;// long.MaxValue; // Set the maximum request body size (in bytes)    
    options.Limits.KeepAliveTimeout = TimeSpan.FromHours(8);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromHours(8);
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 2147483647; //unit is bytes => 2GB
});

// Configure session services

int sessionIdleTimeoutMinutes = builder.Configuration.GetValue<int>("SessionSettings:IdleTimeoutMinutes");
builder.Services.AddDistributedMemoryCache(); // Use an in-memory cache for session (for demo purposes; consider using distributed cache in production)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionIdleTimeoutMinutes); // Set the session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//Fetch value from razor page.
builder.Services.AddSession();
builder.Services.AddSingleton<TokenStorageService>();
// Add configuration from appsettings.json
var configuration = builder.Configuration.AddJsonFile("appsettings.json");
//builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("StripeSettings"));

//------------Remove Language support code from here------------------
builder.Services.AddMemoryCache();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

//--------------------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//------------Remove Language support code from here------------------
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("es"), new CultureInfo("hi-IN") };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

//--------------------------------------------------------------------z
//--- Comment following lines on local----
if (!app.Environment.IsDevelopment())
{
    var rewriteOptions = new RewriteOptions()
        .AddRedirectToHttps()
        .AddIISUrlRewrite(new PhysicalFileProvider(Directory.GetCurrentDirectory()), "RedirectToWwwRule.xml");
    app.UseRewriter(rewriteOptions);
}
//--- Comment above lines on local----

// ... and so on

// Custom middleware to maintain session dynamically
//app.UseMiddleware<DynamicSessionTimeoutMiddleware>();   

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configure the session middleware

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
