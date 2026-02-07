using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Monitoring;

namespace MoreAutomation.Contracts.Automation
{
    /// <summary>
    /// 坐标执行器接口：负责执行采集的比例坐标序列。
    /// </summary>
    public interface ICoordinateExecutor
    {
        /// <summary>
        /// 执行一个保存的动作（坐标点击序列）。
        /// </summary>
        /// <param name="action">要执行的动作（包含比例坐标列表）</param>
        /// <param name="targetWindowHandle">目标窗口句柄；如果为 IntPtr.Zero，则使用当前活跃窗口</param>
        /// <returns>执行状态：成功/失败的点次数</returns>
        Task<ExecutionResult> ExecuteActionAsync(MirrorModeAction action, IntPtr targetWindowHandle = default);

        /// <summary>
        /// 执行一个单独的比例坐标点击。
        /// </summary>
        /// <param name="coordinate">比例坐标 (0.0~1.0)</param>
        /// <param name="targetWindowHandle">目标窗口句柄</param>
        /// <param name="delayMs">执行前延迟（毫秒）</param>
        Task<bool> ExecuteCoordinateAsync(ProportionalCoordinate coordinate, IntPtr targetWindowHandle, int delayMs = 0);

        /// <summary>
        /// 获取窗口的实际尺寸（宽、高）。
        /// </summary>
        (int width, int height) GetWindowSize(IntPtr windowHandle);

        /// <summary>
        /// 获取当前活跃窗口句柄。
        /// </summary>
        IntPtr GetActiveWindowHandle();
    }

    /// <summary>
    /// 执行结果。
    /// </summary>
    public class ExecutionResult
    {
        public bool Success { get; set; }
        public int SuccessfulClicks { get; set; }
        public int FailedClicks { get; set; }
        public string Message { get; set; } = string.Empty;
        public long ElapsedMs { get; set; }
    }
}
