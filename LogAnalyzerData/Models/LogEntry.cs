using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerData.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; }
}