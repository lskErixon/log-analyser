namespace LogAnalyzerBusiness.Services.RateLimit;

public class RateLimitService : IRateLimitService
{
    private readonly SemaphoreSlim _semaphoreSlim;

    public RateLimitService(int maxConcurrentRequests)
    {
        _semaphoreSlim = new SemaphoreSlim(maxConcurrentRequests, maxConcurrentRequests);
    }

    public async Task WaitForSlotAsync(CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
    }
    
    public void ReleaseSlot()
    {
        _semaphoreSlim.Release();
    }
}