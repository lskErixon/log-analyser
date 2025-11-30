using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerData.Models;

public class ParsedLogs
{
    public List<LogEntry> Entries { get; set; } = new();
    public Dictionary<LogLevel, int> CountByLevel { get; set; } = new();
}

