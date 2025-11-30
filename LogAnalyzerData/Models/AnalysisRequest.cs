using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LogAnalyzerData.Models.Enums;

namespace LogAnalyzerData.Models;

public class AnalysisRequest
{
    [Required]
    public string Logs { get; set; }
    public AnalysisType Type { get; set; } = AnalysisType.Full;
}
