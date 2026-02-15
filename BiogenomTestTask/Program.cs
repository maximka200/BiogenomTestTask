using BiogenomTestTask.Repository;
using BiogenomTestTask.Services;
using BiogenomTestTask.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using BiogenomTestTask.Controllers.Interfaces;
using BiogenomTestTask.Models;
using BiogenomTestTask.Services.GigaChatServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<AiSettings>(
    builder.Configuration.GetSection("AiSettings"));


builder.Services.AddHttpClient<IAiService, AiServiceGigaChatService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<AiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
}).ConfigurePrimaryHttpMessageHandler(
    () =>
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
        handler.SslProtocols = 
            System.Security.Authentication.SslProtocols.Tls12;
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        return handler;
    });

builder.Services.AddScoped<IImageAnalysisService, ImageAnalysisService>();
builder.Services.AddSingleton<IGigaChatTokenProvider, GigaChatTokenProvider>();
builder.Services.AddHostedService<GigaChatTokenWorker>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();