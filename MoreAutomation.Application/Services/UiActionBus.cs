using System;
using MoreAutomation.Contracts.Models;

namespace MoreAutomation.Application.Services
{
    public class UiActionBus
    {
        // 定义 UI 动作触发的事件，UI 层订阅，Application 层发布或反之
        public event Action<UiActionType, object?>? OnActionTriggered;

        public void Publish(UiActionType action, object? data = null)
        {
            OnActionTriggered?.Invoke(action, data);
        }
    }

    public enum UiActionType
    {
        HideWindow,
        RestoreWindow,
        ToggleAdaptive,
        StartAutomation,
        StopAutomation
    }
}