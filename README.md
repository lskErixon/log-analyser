# Log Analyzer System

A high-performance REST API for parallel log analysis powered by AI. Built with .NET and designed to showcase efficient multithreading patterns including Producer-Consumer, thread-safe operations, and rate limiting.

## Architecture

The system implements a robust multithreaded pipeline:

- **Controllers** receive log data via REST endpoints
- **Channel-based Queue** distributes tasks to worker threads (bounded to 100 tasks)
- **Multiple Workers** process logs concurrently (3-5 parallel threads)
- **Thread-Safe Storage** using ConcurrentDictionary for result management
- **Rate Limiter** controls AI API requests using Semaphore
- **AI Integration** with OpenAI GPT for intelligent log summarization

## Key Features

**Multithreading Patterns**
- Producer-Consumer pattern via System.Threading.Channels
- Thread-safe shared state management
- Semaphore-based rate limiting
- Deadlock and starvation prevention

**Real-World Application**
- Asynchronous log processing
- Parallel AI analysis
- Status tracking and metrics collection
- Error handling and graceful degradation

## Project Structure
```
LogAnalyzerSystem/
├── LogAnalyzer.API/          # REST API endpoints
├── LogAnalyzer.Business/     # Core business logic
│   ├── Services/             # Processing services
│   └── Workers/              # Background worker threads
└── LogAnalyzer.Data/         # Data models and entities
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- OpenAI API key

### Installation

1. Clone the repository
```bash
git clone https://github.com/aboba555/LogAnalyzerSystem.git
cd LogAnalyzerSystem
```

2. Configure API key using User Secrets
```bash
cd LogAnalyzerApi
dotnet user-secrets init
dotnet user-secrets set "Gpt:ApiKey" "your-openai-api-key"
```

3. Run the application
```bash
dotnet run --project LogAnalyzerApi
```

The API will be available at `http://localhost:5045`

## API Endpoints

### Create Analysis Task
```http
POST /api/log/add-task
Content-Type: application/json

{
  "logs": "[2025-11-23 10:00:00] ERROR: Database connection failed\n[2025-11-23 10:00:01] WARNING: Retry attempt",
  "type": 0
}
```

Response:
```json
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### Get Analysis Result
```http
GET /api/log/result/{taskId}
```

Response:
```json
{
  "taskId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Completed",
  "summary": "Analysis identified 5 critical errors primarily related to database connectivity...",
  "metrics": {
    "totalEntries": 100,
    "errors": 5,
    "warnings": 12,
    "critical": 2
  },
  "created": "2025-11-23T10:00:00Z",
  "completed": "2025-11-23T10:00:15Z"
}
```

### Check Task Status
```http
GET /api/log/status/{taskId}
```

## Multithreading Implementation

### Channel-Based Queue
```csharp
Channel.CreateBounded<(Guid, string)>(100)
```
Bounded channel prevents memory overflow and implements backpressure when capacity is reached.

### Parallel Workers
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var workers = new[]
    {
        ProcessTasksAsync(1, stoppingToken),
        ProcessTasksAsync(2, stoppingToken),
        ProcessTasksAsync(3, stoppingToken)
    };
    
    await Task.WhenAll(workers);
}
```

### Thread-Safe Storage
```csharp
ConcurrentDictionary<Guid, AnalysisResult>
```
Ensures safe concurrent access to shared result storage.

### Rate Limiting
```csharp
SemaphoreSlim(5, 5)
```
Limits concurrent API requests to prevent rate limit violations.

## Technology Stack

- **.NET 9.0** - Modern C# features and performance improvements
- **ASP.NET Core** - REST API framework
- **System.Threading.Channels** - High-performance async queues
- **OpenAI GPT API** - AI-powered log analysis
- **ConcurrentDictionary** - Thread-safe collections

## Development

### Running Tests
```bash
dotnet test
```

### Building
```bash
dotnet build
```

### Configuration

Application settings can be configured in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "Gpt": {
    "ApiKey": ""
  }
}
```

Use User Secrets for sensitive data in development.

## Design Decisions

**Why Channel over Queue?**  
Channels provide native async/await support and better backpressure handling compared to BlockingCollection.

**Why ConcurrentDictionary?**  
Built-in thread safety eliminates manual locking overhead while maintaining high performance.

**Why Multiple Workers in One BackgroundService?**  
Simplifies dependency injection while achieving parallel processing through Task.WhenAll.

**Why Rate Limiting with Semaphore?**  
Prevents API quota exhaustion and enables graceful degradation under load.

## Contributing

Contributions are welcome. Please ensure all tests pass and follow the existing code style.

## License

MIT License - see LICENSE file for details

## Author

Created as a demonstration of advanced multithreading patterns in .NET

---

**Note**: This project was developed as an educational exercise in concurrent programming. It demonstrates real-world patterns for building scalable, thread-safe applications.
