using MoreAutomation.Application.Models;
using MoreAutomation.Application.Messaging;

namespace MoreAutomation.Application.Commands
{
    public record GetSchedulerMetricsQuery() : IQuery<SchedulerMetrics>;
}
