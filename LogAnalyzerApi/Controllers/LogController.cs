using System.Threading.Channels;
using LogAnalyzerBusiness.Services.ResultStore;
using LogAnalyzerData.Models;
using LogAnalyzerData.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LogAnalyzerApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class LogController(IResultStore resultStore, Channel<(Guid,string)> channel) : ControllerBase
{
    [HttpPost("add-task")]
    public async Task<IActionResult> AddTask([FromBody] AnalysisRequest  analysisRequest)
    {
        var taskId = Guid.NewGuid();
        AnalysisResult analysisResult = new()
        {
            TaskId = taskId,
            Status = AnalysisStatus.Queued,
            Created = DateTime.UtcNow,
            
        };
        resultStore.AddTask(analysisResult);
        await channel.Writer.WriteAsync((taskId, analysisRequest.Logs));
        return Ok(taskId);
    }
    [HttpGet("result/{taskId}")]
    public IActionResult GetResult(Guid taskId)
    {
        var result = resultStore.GetResult(taskId);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    // [HttpGet("task/{taskId}")]
    // public IActionResult GetTaskStatus(Guid taskId)
    // {
    //     
    // }
    
}