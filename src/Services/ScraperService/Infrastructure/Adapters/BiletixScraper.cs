using HtmlAgilityPack;
using ScraperService.Core.Interfaces;
using EventBus.Messages;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace ScraperService.Infrastructure.Adapters;

public class BiletixScraper : IScraperService
{
    private readonly ILogger<BiletixScraper> _logger;
    private readonly HttpClient _httpClient;
    private readonly IWebDriver _driver;

    public string Name => "Biletix";

    public BiletixScraper(ILogger<BiletixScraper> logger, IHttpClientFactory httpClientFactory, IWebDriver driver)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _driver = driver;
    }

    public async Task<List<EventScraped>> ScrapeEventsAsync(string city)
    {
        var events = new List<EventScraped>();
        
        var formattedCity = char.ToUpper(city[0]) + city.Substring(1).ToLower();
        if (formattedCity.Equals("Istanbul", StringComparison.OrdinalIgnoreCase)) formattedCity = "Istanbul";

        var targetUrl = $"https://www.biletix.com/search/TURKIYE/tr?category_sb=MUSIC&date_sb=-1&city_sb={formattedCity}#!category_sb:MUSIC,city_sb:{formattedCity}";
        
        _logger.LogInformation("Starting scraping from Biletix (Search - {City}) using Selenium...", city);

        try
        {
            // Navigate to URL
            _driver.Navigate().GoToUrl(targetUrl);
            
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            try 
            {
                wait.Until(d => d.FindElements(By.CssSelector(".searchResultEvent, .searchResultItem, .search_no_result_header")).Count > 0);
                wait.Until(d => {
                    var elements = d.FindElements(By.CssSelector(".searchResultEventName"));
                    return elements.Count > 0 && !string.IsNullOrEmpty(elements[0].Text);
                });
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("Timeout waiting for Biletix search results to populate.");
            }

            var htmlContent = _driver.PageSource;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var eventNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'searchResultEvent')]") 
                             ?? htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'listevent')]")
                             ?? htmlDoc.DocumentNode.SelectNodes("//li[contains(@class, 'searchResultItem')]")
                             ?? htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, 'eventname')]/ancestor::div[contains(@class, 'searchResult') or contains(@class, 'item') or position()=1]");

            if (eventNodes != null)
            {
                _logger.LogInformation("Found {Count} potential event nodes via Selenium", eventNodes.Count);
                int nodeIndex = 0;
                foreach (var node in eventNodes)
                {
                    nodeIndex++;
                    if (nodeIndex == 1)
                    {
                        _logger.LogInformation("DEBUG NODE HTML: {Html}", node.InnerHtml);
                    }
                    try
                    {
                        var nameNode = node.SelectSingleNode(".//a[contains(@class, 'searchResultEventName')]");
                        var name = nameNode?.InnerText.Trim() ?? "Unknown Event";
                        var url = nameNode?.GetAttributeValue("href", "") ?? "";
                        if (!string.IsNullOrEmpty(url) && !url.StartsWith("http"))
                        {
                            url = "https://www.biletix.com" + url;
                        }

                        var imgNode = node.SelectSingleNode(".//div[contains(@class, 'searchResultInfo1a')]//img") 
                                      ?? node.SelectSingleNode(".//img");
                        var imageUrl = imgNode?.GetAttributeValue("src", "") ?? "";
                        if (string.IsNullOrEmpty(imageUrl)) imageUrl = imgNode?.GetAttributeValue("data-src", "") ?? "";

                        var dateNode = node.SelectSingleNode(".//div[contains(@class, 'searchResultInfo2')]") 
                                       ?? node.SelectSingleNode(".//div[contains(@class, 'date')]");
                        var dateString = dateNode?.InnerText.Trim() ?? "";
                        DateTime date = DateTime.Now.AddDays(1);
                        var locationNode = node.SelectSingleNode(".//span[contains(@class, 'ln2')]");
                        var location = locationNode?.InnerText.Trim() ?? city;

                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url) && url != "#")
                        {
                            events.Add(new EventScraped
                            {
                                Name = name,
                                Description = "Biletix Event",
                                Date = date, // Parsing date from text is complex, keeping default for now or need better selector
                                Location = location,
                                Url = url,
                                ImageUrl = imageUrl,
                                Source = "Biletix",
                                City = city
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Error parsing Biletix event node: {Message}", ex.Message);
                    }
                }
            }
            else
            {
                _logger.LogWarning("No event nodes found on Biletix via Selenium.");
                var body = htmlDoc.DocumentNode.SelectSingleNode("//body")?.InnerHtml ?? "";
                _logger.LogInformation("Body Snippet: {Snippet}", body.Substring(0, Math.Min(1000, body.Length)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Biletix with Selenium");
        }
        finally
        {
            try
            {
                _driver.Quit();
                _logger.LogInformation("WebDriver session closed for Biletix");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error closing WebDriver: {Message}", ex.Message);
            }
        }

        return events;
    }
}
