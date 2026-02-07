using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MoreAutomation.Automation.Input;
using MoreAutomation.Contracts.Automation;
using MoreAutomation.Contracts.Configuration;
using MoreAutomation.Contracts.Monitoring;

namespace MoreAutomation.Automation.Execution
{
    /// <summary>
    /// 坐标执行器：将保存的比例坐标转换为像素坐标并执行点击。
    /// 支持后台点击（不需要窗口激活）。
    /// </summary>
    public class CoordinateExecutor : ICoordinateExecutor
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        private readonly BackgroundInputSimulator _inputSimulator;
        private readonly AppConfig _config;
        private readonly Action<string> _logAction;

        public CoordinateExecutor(AppConfig config, Action<string> logAction)
        {
            _config = config;
            _logAction = logAction ?? (msg => System.Diagnostics.Debug.WriteLine(msg));
            _inputSimulator = new BackgroundInputSimulator();
        }

        public async Task<ExecutionResult> ExecuteActionAsync(MirrorModeAction action, IntPtr targetWindowHandle = default)
        {
            if (action?.Coordinates == null || action.Coordinates.Count == 0)
            {
                return new ExecutionResult
                {
                    Success = false,
                    Message = "动作坐标列表为空"
                };
            }

            // 获取目标窗口
            IntPtr hwnd = targetWindowHandle != IntPtr.Zero ? targetWindowHandle : GetActiveWindowHandle();
            if (hwnd == IntPtr.Zero)
            {
                return new ExecutionResult
                {
                    Success = false,
                    Message = "无法获取目标窗口"
                };
            }

            var sw = Stopwatch.StartNew();
            int successCount = 0;
            int failCount = 0;

            try
            {
                _logAction($"[Execution] 开始执行动作: {action.Name} ({action.Coordinates.Count} 个点)");

                foreach (var coord in action.Coordinates)
                {
                    bool success = await ExecuteCoordinateAsync(coord, hwnd, action.DelayBetweenClicksMs);
                    if (success)
                        successCount++;
                    else
                        failCount++;
                }

                sw.Stop();

                var executionResult = new ExecutionResult
                {
                    Success = failCount == 0,
                    SuccessfulClicks = successCount,
                    FailedClicks = failCount,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    Message = $"执行完成：成功 {successCount}，失败 {failCount}，耗时 {sw.ElapsedMilliseconds}ms"
                };

                _logAction($"[Execution] {executionResult.Message}");
                return executionResult;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logAction($"[Execution] 执行异常: {ex.Message}");
                return new ExecutionResult
                {
                    Success = false,
                    Message = $"执行失败: {ex.Message}",
                    ElapsedMs = sw.ElapsedMilliseconds
                };
            }
        }

        public async Task<bool> ExecuteCoordinateAsync(ProportionalCoordinate coordinate, IntPtr targetWindowHandle, int delayMs = 0)
        {
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }

            try
            {
                // 获取窗口尺寸
                (int width, int height) = GetWindowSize(targetWindowHandle);
                if (width <= 0 || height <= 0)
                {
                    _logAction($"[Execution] 窗口尺寸无效: {width}x{height}");
                    return false;
                }

                // 转换比例坐标到像素坐标
                int pixelX = (int)(coordinate.XPercent * width);
                int pixelY = (int)(coordinate.YPercent * height);

                // 后台点击（使用 PostMessage，无需窗口激活）
                _inputSimulator.SendBackgroundClick(targetWindowHandle, pixelX, pixelY);
                
                _logAction($"[Execution] 点击: ({coordinate.XPercent:P1}, {coordinate.YPercent:P1}) -> 像素 ({pixelX}, {pixelY})");
                return true;
            }
            catch (Exception ex)
            {
                _logAction($"[Execution] 点击失败: {ex.Message}");
                return false;
            }
        }

        public (int width, int height) GetWindowSize(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
                return (0, 0);

            if (GetClientRect(windowHandle, out RECT rect))
            {
                return (rect.Width, rect.Height);
            }

            return (0, 0);
        }

        public IntPtr GetActiveWindowHandle()
        {
            return GetForegroundWindow();
        }
    }
}
