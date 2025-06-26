using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace SAP.API.Tests.TestBase;

/// <summary>
/// Base class for API integration tests.
/// Provides common setup and utilities for testing API endpoints.
/// </summary>
public abstract class ApiTestBase : IDisposable
{
    private readonly WebApplicationFactory<global::Program> _factory;
    private readonly HttpClient _client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected HttpClient Client => _client;

    protected ApiTestBase()
    {
        _factory = new WebApplicationFactory<global::Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Override services for testing
                services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Warning));
            });
        });

        _client = _factory.CreateClient();
        
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Serialize an object to JSON for HTTP requests.
    /// </summary>
    protected StringContent ToJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Deserialize HTTP response content to an object.
    /// </summary>
    protected async Task<T?> FromJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Get error message from HTTP response.
    /// </summary>
    protected async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        try
        {
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(content, JsonOptions);
            return errorResponse?.GetValueOrDefault("error")?.ToString() ?? content;
        }
        catch
        {
            return content;
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
} 