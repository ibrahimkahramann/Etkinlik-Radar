using OpenQA.Selenium;
using HtmlAgilityPack;
using ScraperService.Core.Interfaces;
using EventBus.Messages;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration; // Added for IConfiguration

namespace ScraperService.Infrastructure.Adapters;

public class BubiletScraper : IScraperService
{
    private readonly ILogger<BubiletScraper> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _flareSolverrUrl;

    public string Name => "Bubilet";

    public BubiletScraper(ILogger<BubiletScraper> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _flareSolverrUrl = configuration["FlareSolverr:BaseUrl"] ?? "http://flaresolverr:8191";
    }

    public async Task<List<EventScraped>> ScrapeEventsAsync(string city)
    {
        var events = new List<EventScraped>();
        var targetUrl = $"https://www.bubilet.com.tr/{city}";
        
        _logger.LogInformation("Starting scraping from Bubilet for {City} using FlareSolverr...", city);

        try
        {
            var requestPayload = new
            {
                cmd = "request.get",
                url = targetUrl,
                maxTimeout = 60000
            };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_flareSolverrUrl}/v1", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("FlareSolverr returned error status: {Status}", response.StatusCode);
                return events;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            
            if (root.GetProperty("status").GetString() != "ok")
            {
                _logger.LogError("FlareSolverr failed to solve challenge: {Message}", root.GetProperty("message").GetString());
                return events;
            }

            var htmlContent = root.GetProperty("solution").GetProperty("response").GetString();
            
            if (string.IsNullOrEmpty(htmlContent))
            {
                _logger.LogWarning("FlareSolverr returned empty HTML content");
                return events;
            }

            _logger.LogInformation("Successfully retrieved HTML from FlareSolverr. Length: {Length}", htmlContent.Length);
            _logger.LogInformation("HTML Snippet: {Snippet}", htmlContent.Length > 1000 ? htmlContent.Substring(0, 1000) : htmlContent);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var allScripts = htmlDoc.DocumentNode.SelectNodes("//script");
            if (allScripts != null)
            {
                _logger.LogInformation("Found {Count} total script tags", allScripts.Count);
                foreach (var script in allScripts)
                {
                    var type = script.GetAttributeValue("type", "");
                    if (type == "application/ld+json")
                    {
                        var jsonContent = script.InnerText;
                    if (string.IsNullOrEmpty(jsonContent)) continue;
                    
                    _logger.LogInformation("JSON-LD Content Snippet: {Snippet}", jsonContent.Length > 200 ? jsonContent.Substring(0, 200) : jsonContent);

                    if (jsonContent.Contains("\"@type\":\"Event\"") || jsonContent.Contains("\"@type\": \"Event\""))
                    {
                        _logger.LogInformation("Found JSON-LD Event data. Parsing...");
                        var nameMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"name\":\\s*\"(.*?)\"");
                        var urlMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"url\":\\s*\"(.*?)\"");
                        var imageMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"image\":\\s*\\[?\"(.*?)\"");
                        var startDateMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"startDate\":\\s*\"(.*?)\"");
                        var locationMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"addressLocality\":\\s*\"(.*?)\"");

                        if (nameMatch.Success)
                        {
                            events.Add(new EventScraped
                            {
                                Name = nameMatch.Groups[1].Value,
                                Description = "Bubilet Event (JSON-LD)",
                                Date = startDateMatch.Success ? DateTime.Parse(startDateMatch.Groups[1].Value) : DateTime.Now,
                                Location = locationMatch.Success ? locationMatch.Groups[1].Value : city,
                                Url = urlMatch.Success ? urlMatch.Groups[1].Value : targetUrl,
                                ImageUrl = imageMatch.Success ? imageMatch.Groups[1].Value : "",
                                Source = "Bubilet",
                                City = city
                            });
                        }
                    }
                }
            }
            }


            if (events.Count == 0)
            {
                _logger.LogInformation("No events found via JSON-LD. Attempting HTML parsing fallback...");
                
                var eventNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'etkinlik-karti')]") 
                                 ?? htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'event-card')]")
                                 ?? htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, '/etkinlik/')]");

                if (eventNodes != null)
                {
                    _logger.LogInformation("Found {Count} potential event nodes via HTML parsing", eventNodes.Count);
                    foreach (var node in eventNodes)
                    {
                        try
                        {
                            string name = "", url = "", imageUrl = "", date = "", location = "";

                            if (node.Name == "a")
                            {
                                url = node.GetAttributeValue("href", "");
                                name = node.InnerText.Trim();
                                var imgNode = node.SelectSingleNode(".//img");
                                if (imgNode != null) imageUrl = imgNode.GetAttributeValue("src", "");
                            }
                            else
                            {
                                var linkNode = node.SelectSingleNode(".//a");
                                if (linkNode != null)
                                {
                                    url = linkNode.GetAttributeValue("href", "");
                                    name = linkNode.InnerText.Trim(); // Fallback name
                                }
                                
                                var nameNode = node.SelectSingleNode(".//*[contains(@class, 'name')]") ?? node.SelectSingleNode(".//*[contains(@class, 'title')]");
                                if (nameNode != null) name = nameNode.InnerText.Trim();

                                var imgNode = node.SelectSingleNode(".//img");
                                if (imgNode != null) imageUrl = imgNode.GetAttributeValue("src", "");

                                var dateNode = node.SelectSingleNode(".//*[contains(@class, 'date')]") ?? node.SelectSingleNode(".//*[contains(@class, 'tarih')]");
                                if (dateNode != null) date = dateNode.InnerText.Trim();
                                
                                var locNode = node.SelectSingleNode(".//*[contains(@class, 'location')]") ?? node.SelectSingleNode(".//*[contains(@class, 'mekan')]");
                                if (locNode != null) location = locNode.InnerText.Trim();
                            }

                            if (!string.IsNullOrEmpty(url) && !url.StartsWith("http"))
                            {
                                url = "https://www.bubilet.com.tr" + url;
                            }

                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                            {
                                if (name.Length > 100)
                                {
                                    var lines = name.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (lines.Length > 0) name = lines[0].Trim();
                                }

                                var parsedDate = ParseDate(date);
                                var finalDate = parsedDate ?? DateTime.MinValue;

                                events.Add(new EventScraped
                                {
                                    Name = name,
                                    Description = "Bubilet Event (HTML)",
                                    Date = finalDate,
                                    Location = !string.IsNullOrEmpty(location) ? location : city,
                                    Url = url,
                                    ImageUrl = imageUrl,
                                    Source = "Bubilet",
                                    City = city
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Error parsing event node: {Message}", ex.Message);
                        }
                    }
                }
            }
            
            _logger.LogInformation("Extracted {Count} events from Bubilet using FlareSolverr", events.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Bubilet with FlareSolverr");
        }

        return events;
    }

    private DateTime? ParseDate(string? dateText)
    {
        if (string.IsNullOrWhiteSpace(dateText)) return null;
        
        try
        {
            var datePart = dateText.Split('-')[0].Trim();
            
            var parts = datePart.Split(' ');
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
        catch
        {
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
