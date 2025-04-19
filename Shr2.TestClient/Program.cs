using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shr2.TestClient;

class Program
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly string _baseUrl = "http://localhost:5000";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Shr2 Test Client");
        Console.WriteLine("================");

        try
        {
            // Check if the service is running
            Console.WriteLine("Checking if Shr2 service is running...");
            var healthCheck = await CheckServiceHealth();
            if (!healthCheck)
            {
                Console.WriteLine("Shr2 service is not running. Please start the service and try again.");
                return;
            }

            Console.WriteLine("Shr2 service is running.");
            Console.WriteLine();

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Create a shortened URL");
                Console.WriteLine("2. Test a shortened URL");
                Console.WriteLine("3. Exit");
                Console.Write("\nEnter your choice (1-3): ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await CreateShortenedUrlInteractive();
                        break;
                    case "2":
                        await TestShortenedUrlInteractive();
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }

        Console.WriteLine("\nThank you for using Shr2 Test Client. Goodbye!");
    }

    static async Task CreateShortenedUrlInteractive()
    {
        Console.Write("Enter the URL to shorten: ");
        var longUrl = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(longUrl))
        {
            Console.WriteLine("URL cannot be empty.");
            return;
        }

        Console.WriteLine("\nCreating a shortened URL...");
        var shortenedUrl = await CreateShortenedUrl(longUrl);

        if (shortenedUrl != null)
        {
            Console.WriteLine($"Original URL: {shortenedUrl.LongUrl}");
            Console.WriteLine($"Shortened URL: {shortenedUrl.Id}");

            Console.Write("\nWould you like to test this shortened URL? (y/n): ");
            var testChoice = Console.ReadLine()?.ToLower();

            if (testChoice == "y" || testChoice == "yes")
            {
                await TestShortenedUrl(shortenedUrl.Id, true);
            }
        }
    }

    static async Task TestShortenedUrlInteractive()
    {
        Console.Write("Enter the shortened URL to test: ");
        var shortenedUrl = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(shortenedUrl))
        {
            Console.WriteLine("URL cannot be empty.");
            return;
        }

        Console.WriteLine("\nTesting the shortened URL...");
        await TestShortenedUrl(shortenedUrl, true);
    }

    static async Task<bool> CheckServiceHealth()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    static async Task<ShortenedUrl?> CreateShortenedUrl(string longUrl)
    {
        var request = new UrlRequest { LongUrl = longUrl };

        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/url", request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {content}");

            return JsonSerializer.Deserialize<ShortenedUrl>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        else
        {
            Console.WriteLine($"Error creating shortened URL: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {content}");
            return null;
        }
    }

    static async Task<string?> TestShortenedUrl(string shortenedUrl, bool interactive = false)
    {
        try
        {
            // Extract the path from the full URL
            Uri uri;
            string path;

            try
            {
                uri = new Uri(shortenedUrl);
                path = uri.PathAndQuery;
            }
            catch
            {
                // If it's not a valid URI, assume it's just the path/code
                path = shortenedUrl.StartsWith("/") ? shortenedUrl : "/" + shortenedUrl;
            }

            // Create a request with AllowAutoRedirect = false to see the redirect location
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler);

            var response = await client.GetAsync($"{_baseUrl}{path}");

            if (interactive)
            {
                Console.WriteLine($"Status code: {(int)response.StatusCode} ({response.StatusCode})");
            }

            if (response.IsSuccessStatusCode)
            {
                // If successful, it means we got the content directly
                var content = await response.Content.ReadAsStringAsync();
                if (interactive)
                {
                    Console.WriteLine($"Received direct content: {content}");
                }
                return content;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                     response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
            {
                // If it's a redirect, get the location header
                var location = response.Headers.Location?.ToString();
                if (interactive)
                {
                    Console.WriteLine($"Redirects to: {location}");
                    Console.WriteLine("Success! The shortened URL redirects correctly.");
                }
                return location;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                if (interactive)
                {
                    Console.WriteLine($"Error testing shortened URL: {response.StatusCode}");
                    Console.WriteLine($"Response: {content}");
                }
                return null;
            }
        }
        catch (Exception ex)
        {
            if (interactive)
            {
                Console.WriteLine($"Error testing shortened URL: {ex.Message}");
            }
            return null;
        }
    }
}

public class UrlRequest
{
    [JsonPropertyName("longUrl")]
    public string LongUrl { get; set; } = string.Empty;
}

public class ShortenedUrl
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("longUrl")]
    public string LongUrl { get; set; } = string.Empty;
}
