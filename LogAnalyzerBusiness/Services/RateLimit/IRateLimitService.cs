namespace LogAnalyzerBusiness.Services.RateLimit;

public interface IRateLimitService
{
    Task WaitForSlotAsync(CancellationToken cancellationToken = default);
    void ReleaseSlot();
}