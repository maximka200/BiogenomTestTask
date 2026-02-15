namespace BiogenomTestTask.Services.Interfaces;

public interface IGigaChatTokenProvider
{
    string? GetToken();
    void SetToken(string token);
}
