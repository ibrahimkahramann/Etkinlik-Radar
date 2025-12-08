using MassTransit;
using ScraperService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ScraperService.Core.Interfaces;
using ScraperService.Infrastructure.Adapters;
using ScraperService.Application.Features.Scraping.Consumers;
using ScraperService.Core.Options;
using ScraperService.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure ScraperOptions (city selection) from configuration
builder.Services.Configure<ScraperOptions>(builder.Configuration.GetSection("ScraperOptions"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddTransient<OpenQA.Selenium.IWebDriver>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    try
    {
        var options = new OpenQA.Selenium.Chrome.ChromeOptions();
        options.AddArgument("--headless=new"); // New headless mode to bypass Cloudflare
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);

        var seleniumHost = builder.Configuration["Selenium:Host"] ?? "localhost";
        var seleniumUrl = $"http://{seleniumHost}:4444/wd/hub";
        
        logger.LogInformation("Connecting to Selenium Grid at {Url}", seleniumUrl);

        return new OpenQA.Selenium.Remote.RemoteWebDriver(new Uri(seleniumUrl), options.ToCapabilities(), TimeSpan.FromMinutes(10));
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize Selenium WebDriver");
        throw;
    }
});

builder.Services.AddTransient<BubiletScraper>();
builder.Services.AddTransient<BiletinialScraper>();
builder.Services.AddTransient<BiletinoScraper>();
builder.Services.AddTransient<BiletixScraper>();

builder.Services.AddSingleton<IScrapeJobTracker, ScrapeJobTracker>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/etkinlik-radar";
        var audience = builder.Configuration["Keycloak:Audience"] ?? "scraper-api";
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ScrapeAllConsumer>();
    x.AddConsumer<ScrapeSiteConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        var rabbitUser = builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
        var rabbitPass = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<ScraperBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


