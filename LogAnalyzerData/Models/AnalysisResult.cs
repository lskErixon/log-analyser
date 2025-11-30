using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerData.Models;

public class AnalysisResult
{
    public Guid TaskId { get; set; }
    public AnalysisStatus Status { get; set; }
    public string? Summary { get; set; }
    public Dictionary<string,object>? Metrics { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Completed { get; set; }
    public string? ErrorMessage { get; set; } 
}