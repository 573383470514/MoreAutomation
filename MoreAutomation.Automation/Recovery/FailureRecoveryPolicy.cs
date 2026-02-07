namespace MoreAutomation.Automation.Recovery
{
    public enum RecoveryAction { Retry, Skip, Pause, Alert }

    public class FailureRecoveryPolicy
    {
        public int MaxRetries { get; set; } = 3;

        // 初始重试延迟（毫秒），可配置
        public int RetryDelayMs { get; set; } = 200;

        // 每次重试的乘法因子（指数退避），1.0 表示固定延迟
        public double RetryBackoffFactor { get; set; } = 1.5;

        // 当连续失败次数达到该阈值时，建议上游触发 circuit-breaker
        public int CircuitTripThreshold { get; set; } = 10;

        public FailureRecoveryPolicy() { }

        public FailureRecoveryPolicy(int maxRetries, int retryDelayMs, double backoffFactor)
        {
            MaxRetries = maxRetries;
            RetryDelayMs = retryDelayMs;
            RetryBackoffFactor = backoffFactor;
        }

        public RecoveryAction OnRecognitionFailed(int currentRetryCount)
        {
            if (currentRetryCount < MaxRetries) return RecoveryAction.Retry;
            return RecoveryAction.Pause; // 默认识别失败多次后暂停，等待用户介入
        }

        public int GetDelayForRetry(int attempt)
        {
            if (attempt <= 0) return RetryDelayMs;
            double delay = RetryDelayMs * Math.Pow(RetryBackoffFactor, attempt - 1);
            return (int)Math.Min(delay, int.MaxValue);
        }

        public bool ShouldTripCircuit(int consecutiveFailures)
        {
            return consecutiveFailures >= CircuitTripThreshold;
        }
    }
}