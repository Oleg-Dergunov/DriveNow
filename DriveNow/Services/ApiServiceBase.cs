using System.Net;
using System.Text.Json;

namespace DriveNow.Services;

public abstract class ApiServiceBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    protected ApiServiceBase(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    protected async Task<T?> GetFromApiAsync<T>(string uri, string action)
    {
        using var response = await SendAsync(client => client.GetAsync(uri), action);

        try
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            _logger.LogError(ex, "Unexpected response while trying to {Action}", action);
            throw new ApiException($"The server returned unexpected data while trying to {action}.", response.StatusCode, ex);
        }
    }

    protected async Task PostToApiAsync<T>(string uri, T body, string action, string? conflictMessage = null)
    {
        using var response = await SendAsync(client => client.PostAsJsonAsync(uri, body), action, conflictMessage);
    }

    protected async Task PutToApiAsync<T>(string uri, T body, string action, string? conflictMessage = null)
    {
        using var response = await SendAsync(client => client.PutAsJsonAsync(uri, body), action, conflictMessage);
    }

    protected async Task DeleteFromApiAsync(string uri, string action, string? conflictMessage = null)
    {
        using var response = await SendAsync(client => client.DeleteAsync(uri), action, conflictMessage);
    }

    private async Task<HttpResponseMessage> SendAsync(
        Func<HttpClient, Task<HttpResponseMessage>> request,
        string action,
        string? conflictMessage = null)
    {
        HttpResponseMessage response;

        try
        {
            response = await request(_httpClient);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timed out while trying to {Action}", action);
            throw new ApiException($"The server took too long to respond while trying to {action}. Please try again.", innerException: ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Request failed while trying to {Action}", action);
            throw new ApiException($"Unable to reach the server to {action}. Please check your connection and try again.", innerException: ex);
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        using (response)
        {
            var detail = await ReadErrorDetailAsync(response);
            _logger.LogWarning("API returned {StatusCode} while trying to {Action}: {Detail}", (int)response.StatusCode, action, detail);

            throw new ApiException(BuildErrorMessage(response.StatusCode, action, detail, conflictMessage), response.StatusCode);
        }
    }

    private static string BuildErrorMessage(HttpStatusCode statusCode, string action, string? detail, string? conflictMessage) =>
        statusCode switch
        {
            HttpStatusCode.Conflict => conflictMessage ?? detail ?? $"Unable to {action} because it conflicts with existing data.",
            HttpStatusCode.NotFound => $"Unable to {action}: the item was not found. It may have been deleted.",
            HttpStatusCode.BadRequest => detail ?? $"Unable to {action}: the server rejected the data. Please check the form and try again.",
            _ => $"Unable to {action}. The server returned an error ({(int)statusCode}). Please try again later."
        };

    private static async Task<string?> ReadErrorDetailAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(content);

            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("detail", out var detail)
                && detail.ValueKind == JsonValueKind.String
                    ? detail.GetString()
                    : null;
        }
        catch (JsonException)
        {
            return content.Length <= 200 ? content.Trim() : null;
        }
    }
}