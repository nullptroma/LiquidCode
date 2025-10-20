using System.Text;
using System.Text.Json;
using NuGet.Protocol;

namespace LiquidCode.Infrastructure.External.TestingModule;

public class TestingHttpClient(string endpointUrl)
{
    private HttpClient _client = new()
    {
        BaseAddress = new Uri(endpointUrl),
    };

    public async Task PostData(int id, int missionId, string sourceCode, string language)
    {
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                id,
                problemId = missionId,
                sourceCode,
                language
            }),
            Encoding.UTF8,
            "application/json");

        await _client.PostAsync(
            "api/submit",
            jsonContent);
    }
}