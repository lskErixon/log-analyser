using LogAnalyzerData.Models;
using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerBusiness.Services.ResultStore;

public interface IResultStore
{
    void AddTask(AnalysisResult result);
    AnalysisResult? GetResult(Guid taskId);
    void UpdateStatus(Guid taskId, AnalysisStatus status);
    void SetCompleted(Guid taskId, string summary, Dictionary<string, object> metrics);
    void SetFailed(Guid taskId, string errorMessage);
}