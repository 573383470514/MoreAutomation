using System;
using System.Runtime.InteropServices;
using MoreAutomation.Infrastructure.Platform;

namespace MoreAutomation.Automation.Input
{
    public class BackgroundInputSimulator
    {
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;

        public void SendBackgroundClick(IntPtr handle, int x, int y)
        {
            // 保证坐标在 16-bit 范围并按无符号方式打包，避免符号扩展和平台差异
            uint px = (uint)(x & 0xFFFF);
            uint py = (uint)(y & 0xFFFF);
            uint packed = (py << 16) | px;
            IntPtr lParam = new IntPtr((long)packed);

            PostMessage(handle, WM_LBUTTONDOWN, (IntPtr)1, lParam);
            PostMessage(handle, WM_LBUTTONUP, IntPtr.Zero, lParam);
        }

        public void SendKey(IntPtr handle, int keyCode)
        {
            const uint WM_KEYDOWN = 0x0100;
            const uint WM_KEYUP = 0x0101;
            PostMessage(handle, WM_KEYDOWN, (IntPtr)keyCode, IntPtr.Zero);
            PostMessage(handle, WM_KEYUP, (IntPtr)keyCode, IntPtr.Zero);
        }
    }
}