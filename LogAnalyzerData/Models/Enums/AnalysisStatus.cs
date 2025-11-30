namespace LogAnalyzerData.Models.Enums;

public enum AnalysisStatus
{
    Queued, // waiting in queue
    Processing, // worker is parsing logs
    Analyzing, // sending to gpt
    Completed,
    Failed
}