using System.Net;
using System.Text.Json;

namespace MiniMarketCRM.Api.SystemTests.Infrastructure;

public static class HttpTestHelper
{
    public static void AssertSuccess(HttpResponseMessage res, string stepName)
    {
        if (res.IsSuccessStatusCode) return;

        var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        throw new Exception($"{stepName} FAILED => {(int)res.StatusCode} {res.StatusCode}\nBody:\n{body}");
    }

    public static void AssertOkOrCreated(HttpResponseMessage res, string stepName)
    {
        if (res.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created) return;

        var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        throw new Exception($"{stepName} FAILED => {(int)res.StatusCode} {res.StatusCode}\nBody:\n{body}");
    }

    public static async Task<T> ReadJsonOrThrow<T>(HttpResponseMessage res, string stepName)
    {
        var text = await res.Content.ReadAsStringAsync();

        try
        {
            var obj = JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (obj is null) throw new Exception("Deserialize null döndü.");
            return obj;
        }
        catch (Exception ex)
        {
            throw new Exception($"{stepName} JSON parse edilemedi.\nStatus: {(int)res.StatusCode} {res.StatusCode}\nBody:\n{text}\nHata: {ex.Message}");
        }
    }
}
