using System.ComponentModel;
using System.Threading.Channels;
using LogAnalyzerBusiness.Services.Gpt;
using LogAnalyzerBusiness.Services.LogParsing;
using LogAnalyzerBusiness.Services.RateLimit;
using LogAnalyzerBusiness.Services.ResultStore;
using LogAnalyzerData.Models.Enums;
using Microsoft.Extensions.Hosting;

namespace LogAnalyzerBusiness.Worker;

public class LogProcessorWorker(IResultStore resultStore, Channel<(Guid,string)> channel, IParsingService parsingService, IGptService gptService, IRateLimitService rateLimitService) : BackgroundService
{
    

    private async Task ProcessTasksAsync(int workerId, CancellationToken stoppingToken)
    {
        Console.WriteLine($"Worker {workerId} started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var (taskId, logs) = await channel.Reader.ReadAsync(stoppingToken);
            Console.WriteLine($"Get task: {taskId}");
            resultStore.UpdateStatus(taskId, AnalysisStatus.Processing);
            
            try
            {
                var parsedLogs = parsingService.ParseLogs(logs);
                string gptSummary;
                
                await rateLimitService.WaitForSlotAsync();
                try
                {
                    gptSummary = await gptService.AnalyzeLogs(parsedLogs);
                }
                finally
                {
                    rateLimitService.ReleaseSlot();
                }
                

                var metrics = new Dictionary<string, object>
                {
                    ["totalEntries"] = parsedLogs.Entries.Count,
                    ["errors"] = parsedLogs.CountByLevel.GetValueOrDefault(LogLevel.Error, 0),
                    ["warnings"] = parsedLogs.CountByLevel.GetValueOrDefault(LogLevel.Warning, 0),
                    ["critical"] = parsedLogs.CountByLevel.GetValueOrDefault(LogLevel.Critical, 0)
                };

                resultStore.SetCompleted(taskId, gptSummary, metrics);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                resultStore.SetFailed(taskId,ex.ToString());
            }
            
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workers = new[]
        {
            ProcessTasksAsync(1, stoppingToken),
            ProcessTasksAsync(2, stoppingToken),
            ProcessTasksAsync(3, stoppingToken),
        };
        return Task.WhenAll(workers);
    }
}