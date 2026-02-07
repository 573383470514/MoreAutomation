using System.Threading.Tasks;
using MoreAutomation.Application.Messaging;
using MoreAutomation.Application.Commands;
using MoreAutomation.Application.Models;
using MoreAutomation.Contracts.Monitoring;

namespace MoreAutomation.Application.Handlers
{
    public class MetricsQueryHandler : IQueryHandler<GetSchedulerMetricsQuery, SchedulerMetrics>
    {
        private readonly IMetricsService _metrics;

        public MetricsQueryHandler(IMetricsService metrics)
        {
            _metrics = metrics;
        }

        public Task<SchedulerMetrics> HandleAsync(GetSchedulerMetricsQuery query)
        {
            var m = new SchedulerMetrics
            {
                TickCount = _metrics.GetTickCount(),
                FailureCount = _metrics.GetFailureCount()
            };
            return Task.FromResult(m);
        }
    }
}
