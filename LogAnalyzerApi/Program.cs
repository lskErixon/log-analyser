using System.Text.Json.Serialization;
using System.Threading.Channels;
using LogAnalyzerBusiness;
using LogAnalyzerBusiness.Services.Gpt;
using LogAnalyzerBusiness.Services.ResultStore;
using LogAnalyzerBusiness.Worker;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
// our channel
builder.Services.AddSingleton(Channel.CreateBounded<(Guid, string)>(100));

// business logic services
builder.Services.AddBusinessLogic();

builder.Services.AddHttpClient<IGptService, GptService>(client =>
    {
        client.BaseAddress = new Uri("https://api.openai.com");
        var apiKey = configuration["Gpt:ApiKey"];
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.Timeout = TimeSpan.FromSeconds(90);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();