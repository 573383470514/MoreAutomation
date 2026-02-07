namespace MoreAutomation.Automation.Recovery
{
    public enum RecoveryAction { Retry, Skip, Pause, Alert }

    public class FailureRecoveryPolicy
    {
        public int MaxRetries { get; set; } = 3;

        public RecoveryAction OnRecognitionFailed(int currentRetryCount)
        {
            if (currentRetryCount < MaxRetries) return RecoveryAction.Retry;
            return RecoveryAction.Pause; // 默认识别失败多次后暂停，等待用户介入
        }
    }
}