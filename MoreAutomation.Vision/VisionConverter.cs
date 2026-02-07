using OpenCvSharp;

namespace MoreAutomation.Vision
{
    public static class VisionConverter
    {
        // WPF 相关的 BitmapSource 扩展在某些构建环境不可用。
        // 如需在 UI 层做转换，请在 MoreAutomation.UI 项目中实现对应的适配器。
        public static Mat ToMat(object source)
        {
            throw new System.NotSupportedException("BitmapSource conversion not available in this build. Implement in UI project if needed.");
        }

        public static object ToBitmapSource(Mat mat)
        {
            throw new System.NotSupportedException("BitmapSource conversion not available in this build. Implement in UI project if needed.");
        }
    }
}