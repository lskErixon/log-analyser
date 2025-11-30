using LogAnalyzerData.Models;
using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerBusiness.Services.LogParsing;

// example of entry :
// ReSharper disable once InvalidXmlDocComment
/**
    [2025-11-23 10:15:30] ERROR: Database connection failed
    [2025-11-23 10:15:31] WARNING: Retry attempt 1
    [2025-11-23 10:15:35] ERROR: Connection timeout after 5 seconds
    [2025-11-23 10:15:40] INFO: Switching to backup database
    [2025-11-23 10:15:41] INFO: Connection restored
    [2025-11-23 10:16:00] CRITICAL: Data corruption detected
**/


// output :
/**
ParsedLogs
{
Entries = [
    LogEntry {
    Timestamp = 2025-11-23 10:15:30,
    Level = Error,
    Message = "Database connection failed"
},
LogEntry {
    Timestamp = 2025-11-23 10:15:31,
    Level = Warning,
    Message = "Retry attempt 1"
},
LogEntry {
    Timestamp = 2025-11-23 10:15:35,
    Level = Error,
    Message = "Connection timeout after 5 seconds"
},
LogEntry {
    Timestamp = 2025-11-23 10:15:40,
    Level = Info,
    Message = "Switching to backup database"
},
LogEntry {
    Timestamp = 2025-11-23 10:15:41,
    Level = Info,
    Message = "Connection restored"
},
LogEntry {
    Timestamp = 2025-11-23 10:16:00,
    Level = Critical,
    Message = "Data corruption detected"
}
],
    
CountByLevel = {
    [Info] = 2,
    [Warning] = 1,
    [Error] = 2,
    [Critical] = 1
}
}
**/

public class ParsingService : IParsingService
{
    public ParsedLogs ParseLogs(string logs)
    {
        string[] lines = logs.Split(
            new string[] { Environment.NewLine },
            StringSplitOptions.None
        );
        
        var result = new ParsedLogs();
        
        foreach (string line in lines)
        {
            var entry = ParseLine(line);

            if (entry != null)
            {
                result.Entries.Add(entry);

                if (!result.CountByLevel.ContainsKey(entry.Level))
                {
                    result.CountByLevel[entry.Level] = 0;
                }
                result.CountByLevel[entry.Level]++;
            }
        }
        return result;
    }

    private LogEntry? ParseLine(string line)
    {
        int endBracket = line.IndexOf(']');
        string timestampStr = line.Substring(1, endBracket - 1);
        DateTime timeStamp = DateTime.Parse(timestampStr);
            
        string rest = line.Substring(endBracket + 2);
        string[] restSplited = rest.Split(":", StringSplitOptions.RemoveEmptyEntries);
            
        string levelStr = restSplited[0];
        string messageStr = restSplited[1];
            
        Enum.TryParse<LogLevel>(levelStr, true, out var level);

        return new LogEntry()
        {
            Timestamp = timeStamp,
            Level = level,
            Message = messageStr
        };
    }
}