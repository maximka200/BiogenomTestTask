using BiogenomTestTask.Repository;
using BiogenomTestTask.Services;
using BiogenomTestTask.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using BiogenomTestTask.Controllers.Interfaces;
using BiogenomTestTask.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<AiSettings>(
    builder.Configuration.GetSection("GigaChat"));


builder.Services.AddHttpClient<IAiService, AiServiceGigaChatService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<AiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", settings.ApiKey);
});

builder.Services.AddScoped<IImageAnalysisService, ImageAnalysisService>();


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