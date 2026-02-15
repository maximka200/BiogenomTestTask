using BiogenomTestTask.Services.Interfaces;

namespace BiogenomTestTask.Services.GigaChatServices;

public class GigaChatTokenProvider : IGigaChatTokenProvider
{
    private string? accessToken;

    public string? GetToken() => accessToken;

    public void SetToken(string token)
    {
        accessToken = token;
    }
}