using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BiogenomTestTask.Models;
using BiogenomTestTask.Models.DTOs;
using BiogenomTestTask.Services.Interfaces;
using Microsoft.Extensions.Options;
using HttpMethod = System.Net.Http.HttpMethod;

namespace BiogenomTestTask.Services.GigaChatServices;

public class GigaChatTokenWorker(IOptions<AiSettings> settings, IGigaChatTokenProvider tokenProvider, 
    ILogger<GigaChatTokenWorker> logger)
    : BackgroundService
{
    private readonly AiSettings settings = settings.Value;

    private static readonly SemaphoreSlim Locker = new(1, 1);

    private const string ReqUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
    private const string Scope = "GIGACHAT_API_PERS";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateTokenAsync(stoppingToken);
            
            await Task.Delay(TimeSpan.FromMinutes(29), stoppingToken);
        }
    }

    private async Task UpdateTokenAsync(CancellationToken cancellationToken)
    {
        await Locker.WaitAsync(cancellationToken);

        try
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var client = new HttpClient(handler);
            
            var rqUid = Guid.NewGuid().ToString();

            var request = new HttpRequestMessage(HttpMethod.Post, ReqUrl);
            
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("RqUID", rqUid);

            var authString = $"{settings.ClientId}:{settings.ClientSecret}";
            var authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authBase64);
            
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "scope", Scope }
            });

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            if (tokenResponse?.AccessToken is null)
            {
                throw new InvalidOperationException("Server not return access_token");
            }

            tokenProvider.SetToken(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token update failed");
            if (ex.InnerException != null)
            {
                logger.LogError("{Message}", ex.InnerException.Message);
            }
        }
        finally
        {
            Locker.Release();
        }
    }
}
