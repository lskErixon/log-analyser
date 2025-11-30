using LogAnalyzerData.Models;

namespace LogAnalyzerBusiness.Services.LogParsing;

public interface IParsingService
{
    ParsedLogs ParseLogs(string logs);
}