using LogAnalyzerBusiness.Services.Gpt;
using LogAnalyzerBusiness.Services.LogParsing;
using LogAnalyzerBusiness.Services.RateLimit;
using LogAnalyzerBusiness.Services.ResultStore;
using LogAnalyzerBusiness.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace LogAnalyzerBusiness;

public static class Extension
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddSingleton<IResultStore, ResultStore>();
        services.AddSingleton<IParsingService, ParsingService>();
        services.AddSingleton<IRateLimitService>(
            _ => new RateLimitService(maxConcurrentRequests: 3)
        );
        services.AddHostedService<LogProcessorWorker>();
        return services;
    }
}