using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Infrastructure.External.TestingModule;

public class TestingHttpClient(string endpointUrl, ILogger<TestingHttpClient> logger)
{
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri(endpointUrl),
    };
    private readonly ILogger<TestingHttpClient> _logger = logger;

    public async Task SubmitAsync(SubmitForTesterModel payload, CancellationToken cancellationToken = default)
    {
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
    }
}