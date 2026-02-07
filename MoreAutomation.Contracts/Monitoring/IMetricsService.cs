namespace MoreAutomation.Contracts.Monitoring
{
    public interface IMetricsService
    {
        void IncrementTick();
        void IncrementFailure();
        long GetTickCount();
        long GetFailureCount();
    }
}
