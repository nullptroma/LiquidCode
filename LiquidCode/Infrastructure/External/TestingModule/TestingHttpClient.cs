using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Infrastructure.External.TestingModule;

public class TestingHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger<TestingHttpClient> _logger;
    private readonly string _endpointUrl;

    public TestingHttpClient(string endpointUrl, ILogger<TestingHttpClient> logger)
    {
        _endpointUrl = endpointUrl;
        _logger = logger;
        _client = new HttpClient
        {
            BaseAddress = new Uri(endpointUrl),
        };

        _logger.LogInformation("Initialized testing HTTP client for endpoint {Endpoint}", endpointUrl);
    }

    public async Task SubmitAsync(SubmitForTesterModel payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting tester payload {SubmitId} to {Endpoint}", payload.Id, _endpointUrl);

        string serializedPayload;
        try
        {
            serializedPayload = JsonSerializer.Serialize(payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize payload for submit {SubmitId}", payload.Id);
            serializedPayload = "<serialization_failed>";
        }

        _logger.LogInformation("Tester payload body: {PayloadBody}", serializedPayload);

        var response = await _client.PostAsJsonAsync("api/submit", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Testing module returned {StatusCode} for submit {SubmitId}: {Body}",
                (int)response.StatusCode,
                payload.Id,
                string.IsNullOrWhiteSpace(content) ? "<empty>" : content);
            response.EnsureSuccessStatusCode();
        }
        else
        {
            _logger.LogInformation("Testing module accepted submit {SubmitId} with status {StatusCode}",
                payload.Id,
                (int)response.StatusCode);
        }
    }
}