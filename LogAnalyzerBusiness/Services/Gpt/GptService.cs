using System.Net.Http.Json;
using System.Text;
using LogAnalyzerData.Models;
using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerBusiness.Services.Gpt;

public class GptService(HttpClient httpClient) : IGptService
{
    public async Task<string> AnalyzeLogs(ParsedLogs logs)
    {
        var prompt = BuildPrompt(logs);

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = prompt },
            },
            max_tokens = 1000
        };
        var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", requestBody);

        var result = await response.Content.ReadFromJsonAsync<GptResponse>();

        return result?.Choices[0].Message.Content ?? "No summary";
    }

    private string BuildPrompt(ParsedLogs logs)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# System Log Analysis");
        sb.AppendLine();
        sb.AppendLine("You are an expert DevOps engineer specializing in production incident analysis.");
        sb.AppendLine(
            "Analyze the following application logs and provide a **professional, actionable report in Markdown format**.");
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## 📊 Log Overview");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| **Total Entries** | {logs.Entries.Count} |");

        foreach (var kvp in logs.CountByLevel.OrderByDescending(x => (int)x.Key))
        {
            var percentage = (kvp.Value * 100.0 / logs.Entries.Count).ToString("F1");
            var icon = GetLevelIcon(kvp.Key);
            sb.AppendLine($"| {icon} **{kvp.Key}** | {kvp.Value} ({percentage}%) |");
        }

        sb.AppendLine();

        if (logs.Entries.Any())
        {
            var firstLog = logs.Entries.MinBy(e => e.Timestamp);
            var lastLog = logs.Entries.MaxBy(e => e.Timestamp);

            sb.AppendLine("## ⏱️ Time Window");
            sb.AppendLine();
            sb.AppendLine(
                $"- **Period**: `{firstLog?.Timestamp:yyyy-MM-dd HH:mm:ss}` to `{lastLog?.Timestamp:yyyy-MM-dd HH:mm:ss}`");

            if (firstLog != null && lastLog != null)
            {
                var duration = lastLog.Timestamp - firstLog.Timestamp;

                if (duration.TotalHours >= 1)
                    sb.AppendLine($"- **Duration**: {duration.TotalHours:F1} hours");
                else if (duration.TotalMinutes >= 1)
                    sb.AppendLine($"- **Duration**: {duration.TotalMinutes:F1} minutes");
                else
                    sb.AppendLine($"- **Duration**: {duration.TotalSeconds:F0} seconds");
            }

            sb.AppendLine();
        }

        var errorAndCritical = logs.Entries
            .Where(e => e.Level >= LogLevel.Error)
            .ToList();

        if (errorAndCritical.Any())
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## 🚨 Error & Critical Events");
            sb.AppendLine();

            var errorGroups = errorAndCritical
                .Take(15)
                .GroupBy(e => e.Message.Length > 100 ? e.Message.Substring(0, 100) + "..." : e.Message)
                .Select(g => new
                {
                    Message = g.Key,
                    Count = g.Count(),
                    FirstOccurrence = g.Min(e => e.Timestamp),
                    Level = g.First().Level
                })
                .OrderByDescending(x => (int)x.Level)
                .ThenByDescending(x => x.Count)
                .Take(10);

            foreach (var error in errorGroups)
            {
                var icon = GetLevelIcon(error.Level);
                var timestamp = error.FirstOccurrence.ToString("HH:mm:ss");

                if (error.Count > 1)
                {
                    sb.AppendLine($"- {icon} **[{timestamp}]** {error.Message}");
                    sb.AppendLine($"  - _Occurred {error.Count} times_");
                }
                else
                {
                    sb.AppendLine($"- {icon} **[{timestamp}]** {error.Message}");
                }
            }

            sb.AppendLine();

            var errorKeywords = errorAndCritical
                .SelectMany(e => e.Message.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(w => w.Length > 5 && !w.All(char.IsDigit))
                .GroupBy(w => w.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => $"`{g.Key}` ({g.Count()})");

            if (errorKeywords.Any())
            {
                sb.AppendLine("### 🔍 Common Error Keywords");
                sb.AppendLine();
                sb.AppendLine(string.Join(" • ", errorKeywords));
                sb.AppendLine();
            }
        }

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## 📋 Required Analysis");
        sb.AppendLine();
        sb.AppendLine("Based on the logs above, provide a **structured incident report** with these sections:");
        sb.AppendLine();

        sb.AppendLine("### 1. 🎯 Executive Summary");
        sb.AppendLine("_Provide a 2-3 sentence overview of system health and critical issues._");
        sb.AppendLine();

        sb.AppendLine("### 2. ⚠️ Critical Issues Identified");
        sb.AppendLine("_List top 3-5 problems in order of severity:_");
        sb.AppendLine("- **Issue name** - Brief description");
        sb.AppendLine("  - **Severity**: Critical/High/Medium/Low");
        sb.AppendLine("  - **Impact**: What systems/users are affected");
        sb.AppendLine("  - **Frequency**: How often it occurred");
        sb.AppendLine();

        sb.AppendLine("### 3. 💡 Root Cause Hypothesis");
        sb.AppendLine("_What patterns suggest the underlying cause?_");
        sb.AppendLine();

        sb.AppendLine("### 4. 🔧 Immediate Actions Required");
        sb.AppendLine("_Prioritized list of fixes:_");
        sb.AppendLine("1. **[URGENT]** - Actions needed within 1 hour");
        sb.AppendLine("2. **[HIGH]** - Actions needed today");
        sb.AppendLine("3. **[MEDIUM]** - Actions needed this week");
        sb.AppendLine();

        sb.AppendLine("### 5. 🛡️ Prevention Recommendations");
        sb.AppendLine("_How to prevent this in the future:_");
        sb.AppendLine("- Monitoring improvements");
        sb.AppendLine("- Code/infrastructure changes");
        sb.AppendLine("- Process improvements");
        sb.AppendLine();

        sb.AppendLine("### 6. 📊 System Health Assessment");
        sb.AppendLine("_Provide overall health rating:_");
        sb.AppendLine();
        sb.AppendLine("| Aspect | Status | Notes |");
        sb.AppendLine("|--------|--------|-------|");
        sb.AppendLine("| Availability | 🟢/🟡/🔴 | ... |");
        sb.AppendLine("| Performance | 🟢/🟡/🔴 | ... |");
        sb.AppendLine("| Error Rate | 🟢/🟡/🔴 | ... |");
        sb.AppendLine("| **Overall** | 🟢/🟡/🔴 | ... |");
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## 📝 Formatting Guidelines");
        sb.AppendLine();
        sb.AppendLine("- Use **bold** for critical terms and actions");
        sb.AppendLine("- Use `code blocks` for technical terms, error codes, and file paths");
        sb.AppendLine("- Use emojis strategically for visual hierarchy");
        sb.AppendLine("- Keep language professional but concise");
        sb.AppendLine("- Focus on **actionable insights**, not just descriptions");
        sb.AppendLine("- If unsure about root cause, say so explicitly");
        sb.AppendLine();

        sb.AppendLine("**Severity Definitions:**");
        sb.AppendLine("- 🔴 **Critical**: Service down, data loss, security breach");
        sb.AppendLine("- 🟠 **High**: Major functionality impaired, many users affected");
        sb.AppendLine("- 🟡 **Medium**: Minor functionality issues, workarounds available");
        sb.AppendLine("- 🟢 **Low**: Cosmetic issues, no user impact");

        return sb.ToString();
    }

    private static string GetLevelIcon(LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => "🔴",
            LogLevel.Error => "❌",
            LogLevel.Warning => "⚠️",
            LogLevel.Info => "ℹ️",
            _ => "📝"
        };
    }
}