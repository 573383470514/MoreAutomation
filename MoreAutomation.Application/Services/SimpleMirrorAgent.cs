using System;
using System.Collections.Concurrent;

namespace MoreAutomation.Application.Services
{
    public class SimpleMirrorAgent : MoreAutomation.Application.Messaging.IMirrorAgent
    {
        private readonly string _clientId;
        private readonly ILogService _log;
        private bool _isMaster;
        private readonly ConcurrentQueue<(string actionType, string payload)> _actionQueue = new();

        public bool IsMaster => _isMaster;

        public SimpleMirrorAgent(string clientId, ILogService log)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _isMaster = false;
        }

        public void SetMasterMode(bool isMaster)
        {
            _isMaster = isMaster;
            _log.Append($"[Mirror] ClientId{_clientId} 角色设置为 {(isMaster ? "主控":"从应")}");
        }

        public void BroadcastAction(string actionType, string payload)
        {
            if (!_isMaster)
            {
                _log.Append("[Mirror] 非主控客户端，无法广播操作");
                return;
            }

            // TODO: 向所有从应客户端广播该操作（实际可通过网络/消息队列）
            _log.Append($"[Mirror] 主控广播操作: {actionType}");
        }

        public void ReceiveAction(string actionType, string payload)
        {
            if (_isMaster)
            {
                _log.Append("[Mirror] 主控客户端不接收来自其他客户端的操作");
                return;
            }

            _actionQueue.Enqueue((actionType, payload));
            _log.Append($"[Mirror] 从应收到操作: {actionType}");
        }

        public (string actionType, string payload)? DequeueAction()
        {
            if (_actionQueue.TryDequeue(out var action))
            {
                return action;
            }
            return null;
        }
    }
}
