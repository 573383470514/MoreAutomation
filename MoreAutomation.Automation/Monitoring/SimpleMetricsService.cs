using System.Threading;
using MoreAutomation.Contracts.Monitoring;

namespace MoreAutomation.Automation.Monitoring
{
    public class SimpleMetricsService : IMetricsService
    {
        private long _tickCount;
        private long _failureCount;

        public void IncrementTick() => Interlocked.Increment(ref _tickCount);
        public void IncrementFailure() => Interlocked.Increment(ref _failureCount);
        public long GetTickCount() => Interlocked.Read(ref _tickCount);
        public long GetFailureCount() => Interlocked.Read(ref _failureCount);
    }
}
