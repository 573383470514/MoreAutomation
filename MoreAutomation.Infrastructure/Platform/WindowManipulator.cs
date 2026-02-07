using System;
using System.Text;

namespace MoreAutomation.Infrastructure.Platform
{
    public class WindowManipulator
    {
        public void HideWindow(IntPtr handle)
        {
            if (NativeMethods.IsWindow(handle))
                NativeMethods.ShowWindow(handle, NativeMethods.SW_HIDE);
        }

        public void RestoreWindow(IntPtr handle)
        {
            if (NativeMethods.IsWindow(handle))
            {
                NativeMethods.ShowWindow(handle, NativeMethods.SW_RESTORE);
                NativeMethods.SetForegroundWindow(handle);
            }
        }

        public string GetWindowTitle(IntPtr handle)
        {
            StringBuilder sb = new StringBuilder(256);
            int len = NativeMethods.GetWindowText(handle, sb, sb.Capacity);
            if (len <= 0) return string.Empty;
            return sb.ToString(0, len);
        }
    }
}