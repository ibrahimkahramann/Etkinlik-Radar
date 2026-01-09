using OpenQA.Selenium;
using EventBus.Messages;
using ScraperService.Core.Interfaces;
using Microsoft.Extensions.Options;
using ScraperService.Core.Options;

namespace ScraperService.Infrastructure.Adapters;

public class BiletinialScraper : IScraperService
{
    private readonly ILogger<BiletinialScraper> _logger;
    private readonly IWebDriver _driver;
    private readonly ScraperOptions _options;

    public BiletinialScraper(ILogger<BiletinialScraper> logger, IWebDriver driver, IOptions<ScraperOptions> options)
    {
        _logger = logger;
        _driver = driver;
        _options = options.Value;
    }

    public string Name => "Biletinial";

    public async Task<List<EventScraped>> ScrapeEventsAsync(string city)
    {
        _logger.LogInformation("Starting scraping from Biletinial for {City} using Selenium...", city);
        var events = new List<EventScraped>();
        var uniqueUrls = new HashSet<string>();

        try
        {
            var url = $"https://biletinial.com/tr-tr/muzik/{city}";
            _driver.Navigate().GoToUrl(url);
            
            var js = (IJavaScriptExecutor)_driver; // Cast _driver to IJavaScriptExecutor once
            long lastHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
            for (int i = 0; i < 5; i++)
            {
                js.ExecuteScript("window.scrollBy(0, 1000);");
                Thread.Sleep(1000);
                
                long newHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
                if (newHeight > lastHeight)
                {
                    lastHeight = newHeight;
                    i = 0; // Reset counter if new content loaded
                }
            }
            var pageSource = _driver.PageSource;
            if (pageSource.Contains("Rafet El Roman"))
            {
                _logger.LogInformation("DEBUG: 'Rafet El Roman' FOUND in page source.");
            }
            else
            {
                _logger.LogWarning("DEBUG: 'Rafet El Roman' NOT FOUND in page source. Content might not be loaded.");
            }
            var eventElements = new List<IWebElement>();
            

            
            eventElements.AddRange(_driver.FindElements(By.CssSelector(".kategori__slider__popularvenue__content")));
            
            eventElements.AddRange(_driver.FindElements(By.XPath("//li/figure/a")));

            var genericLinks = _driver.FindElements(By.XPath($"//a[contains(@href, '/muzik/') and .//img]"));
            foreach (var link in genericLinks)
            {
                var href = link.GetAttribute("href");
                if (href != null && !href.EndsWith("/muzik/istanbul") && !href.EndsWith("/muzik"))
                {
                    eventElements.Add(link);
                }
            }

            _logger.LogInformation("Found {Count} potential event elements on Biletinial.", eventElements.Count);

            foreach (var element in eventElements)
            {
                try
                {
                    IWebElement linkElement = null;
                    IWebElement imgElement = null;

                    try 
                    { 
                        if (element.TagName.ToLower() == "a")
                        {
                            linkElement = element;
                        }
                        else
                        {
                            linkElement = element.FindElement(By.TagName("a")); 
                        }
                    } catch {}
                    try { imgElement = element.FindElement(By.TagName("img")); } catch {}

                    var name = linkElement?.GetAttribute("title")?.Trim() ?? linkElement?.Text?.Trim();
                    var link = linkElement?.GetAttribute("href") ?? "";
                    var imageUrl = imgElement?.GetAttribute("src") ?? imgElement?.GetAttribute("data-src") ?? "";

                    if (string.IsNullOrEmpty(name)) continue;
                    if (string.IsNullOrEmpty(link)) continue;

                    if (!link.StartsWith("http"))
                    {
                        link = "https://biletinial.com" + link;
                    }

                    if (uniqueUrls.Contains(link)) continue;
                    uniqueUrls.Add(link);

                    events.Add(new EventScraped
                    {
                        Name = name,
                        Description = "Biletinial Event",
                        Date = DateTime.Now.AddDays(Random.Shared.Next(1, 60)), // Placeholder
                        Location = city, // Placeholder
                        Url = link,
                        ImageUrl = imageUrl,
                        Source = "Biletinial"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error parsing a Biletinial event element: {Message}", ex.Message);
                }
            }

            _logger.LogInformation("Successfully scraped {Count} unique events from Biletinial", events.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Biletinial with Selenium");
        }
        finally
        {
            try
            {
                _driver.Quit();
                _logger.LogInformation("WebDriver session closed for Biletinial");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error closing WebDriver: {Message}", ex.Message);
            }
        }

        return events;
    }
}
