using LogAnalyzerData.Models;

namespace LogAnalyzerBusiness.Services.Gpt;

public interface IGptService
{
    Task<string> AnalyzeLogs(ParsedLogs logs);
}