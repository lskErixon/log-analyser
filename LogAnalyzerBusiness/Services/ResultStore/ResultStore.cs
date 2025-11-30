using System.Collections.Concurrent;
using LogAnalyzerData.Models;
using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerBusiness.Services.ResultStore;

public class ResultStore : IResultStore
{
    private readonly ConcurrentDictionary<Guid, AnalysisResult> _results = new();
    
    public void AddTask(AnalysisResult result)
    {
        _results.TryAdd(result.TaskId, result);
    }

    public AnalysisResult? GetResult(Guid taskId)
    {
        _results.TryGetValue(taskId, out var result);
        return result;
    }

    public void UpdateStatus(Guid taskId, AnalysisStatus status)
    {
        if (_results.TryGetValue(taskId, out var result))
        {
            result.Status = status;
        }
    }

    public void SetCompleted(Guid taskId, string summary, Dictionary<string, object> metrics)
    {
        if (_results.TryGetValue(taskId, out var result))
        {
            result.Status = AnalysisStatus.Completed;
            result.Summary = summary;
            result.Metrics = metrics;
            result.Completed = DateTime.UtcNow;
        }
    }

    public void SetFailed(Guid taskId, string errorMessage)
    {
        if (_results.TryGetValue(taskId, out var result))
        {
            result.Status = AnalysisStatus.Failed;
            result.ErrorMessage = errorMessage;
            result.Completed = DateTime.UtcNow;
        }
    }
}