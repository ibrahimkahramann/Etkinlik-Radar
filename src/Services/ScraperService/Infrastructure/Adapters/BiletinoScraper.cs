using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using EventBus.Messages;
using ScraperService.Core.Interfaces;
using Microsoft.Extensions.Options;
using ScraperService.Core.Options;

namespace ScraperService.Infrastructure.Adapters;

public class BiletinoScraper : IScraperService
{
    private readonly ILogger<BiletinoScraper> _logger;
    private readonly IWebDriver _driver;
    private readonly ScraperOptions _options;

    public BiletinoScraper(ILogger<BiletinoScraper> logger, IWebDriver driver, IOptions<ScraperOptions> options)
    {
        _logger = logger;
        _driver = driver;
        _options = options.Value;
    }

    public string Name => "Biletino";

    public async Task<List<EventScraped>> ScrapeEventsAsync(string city)
    {
        _logger.LogInformation("Starting scraping from Biletino for {City} using Selenium...", city);
        var events = new List<EventScraped>();

        try
        {
            var normalizedCity = city.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            if (normalizedCity == "istanbul") normalizedCity = "istanbul"; // Explicit check if needed
            
            var url = $"https://biletino.com/tr/city/{normalizedCity}/muzik/";
            
            await Task.Run(() => _driver.Navigate().GoToUrl(url));
            
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until(d => 
                    d.FindElements(By.CssSelector(".event-card")).Count > 0 ||
                    d.FindElements(By.CssSelector(".event-list-item")).Count > 0 ||
                    d.FindElements(By.XPath("//a[contains(@href, '/event/')]")).Count > 0
                );
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("Timeout waiting for Biletino events. Page title: {Title}", _driver.Title);
            }

            long lastHeight = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.scrollHeight");
            int noChangeCount = 0;
            
            while (true)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                Thread.Sleep(2000); // Wait for content to load

                long newHeight = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.scrollHeight");
                if (newHeight == lastHeight)
                {
                    noChangeCount++;
                    if (noChangeCount >= 3) // Stop if no height change for 3 consecutive attempts
                    {
                        break;
                    }
                }
                else
                {
                    noChangeCount = 0;
                    lastHeight = newHeight;
                }
            }

            var eventBodies = _driver.FindElements(By.CssSelector("a.card-body.event-url")); 
            
            _logger.LogInformation("Found {Count} potential event bodies on Biletino.", eventBodies.Count);

            foreach (var bodyElement in eventBodies)
            {
                try
                {
                    var name = bodyElement.FindElement(By.TagName("h3")).Text;
                    var link = bodyElement.GetAttribute("href");
                    
                    var dateText = bodyElement.FindElements(By.TagName("p")).FirstOrDefault()?.Text;
                    
                    var location = bodyElement.FindElements(By.TagName("p")).LastOrDefault()?.Text;

                    string imageUrl = "";
                    try 
                    {
                        var imageLink = bodyElement.FindElement(By.XPath("./preceding-sibling::a[contains(@class, 'card-image')][1]"));
                        var img = imageLink.FindElement(By.TagName("img"));
                        imageUrl = img?.GetAttribute("src") ?? img?.GetAttribute("data-src") ?? "";
                    }
                    catch (NoSuchElementException)
                    {
                        _logger.LogWarning("Could not find image for event: {EventName}", name);
                    }

                    if (!string.IsNullOrEmpty(link))
                    {
                        if (!link.StartsWith("http"))
                        {
                            link = "https://biletino.com" + link;
                        }

                        events.Add(new EventScraped
                        {
                            Name = !string.IsNullOrWhiteSpace(name) ? name : "Biletino Event",
                            Description = location ?? "Biletino Event", // Use location as description for now
                            Date = ParseDate(dateText) ?? DateTime.Now.AddDays(Random.Shared.Next(1, 30)), 
                            Location = location ?? city,
                            Url = link,
                            ImageUrl = imageUrl,
                            Source = "Biletino"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting event details from Biletino");
                }
            }


            _logger.LogInformation("Scraped {Count} events from Biletino", events.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Biletino with Selenium");
        }
        finally
        {
            try
            {
                _driver.Quit();
                _logger.LogInformation("WebDriver session closed for Biletino");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error closing WebDriver: {Message}", ex.Message);
            }
        }

        return events;
    }

    private DateTime? ParseDate(string? dateText)
    {
        if (string.IsNullOrWhiteSpace(dateText)) return null;
        
        try
        {
            var parts = dateText.Trim().Split(' ');
            if (parts.Length < 2) return null;

            if (int.TryParse(parts[0], out int day))
            {
                string monthName = parts[1].ToLower(new System.Globalization.CultureInfo("tr-TR"));
                int month = GetMonthNumber(monthName);
                
                if (month > 0)
                {
                    int year = DateTime.Now.Year;
                    var date = new DateTime(year, month, day);

                    if (date < DateTime.Now.AddMonths(-2))
                    {
                        date = date.AddYears(1);
                    }
                    
                    return date;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error parsing date '{DateText}': {Message}", dateText, ex.Message);
        }

        return null; 
    }

    private int GetMonthNumber(string monthName)
    {
        return monthName switch
        {
            "ocak" => 1,
            "şubat" => 2,
            "mart" => 3,
            "nisan" => 4,
            "mayıs" => 5,
            "haziran" => 6,
            "temmuz" => 7,
            "ağustos" => 8,
            "eylül" => 9,
            "ekim" => 10,
            "kasım" => 11,
            "aralık" => 12,
            _ => 0
        };
    }
}
