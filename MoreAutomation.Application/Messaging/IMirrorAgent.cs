namespace MoreAutomation.Application.Messaging
{
    public interface IMirrorAgent
    {
        bool IsMaster { get; }
        void SetMasterMode(bool isMaster);
        void BroadcastAction(string actionType, string payload);
        void ReceiveAction(string actionType, string payload);
    }
}
