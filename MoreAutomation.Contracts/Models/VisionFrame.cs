using System;

namespace MoreAutomation.Contracts.Models
{
    // 轻量跨项目图像表示，避免在 Contracts 层引用第三方库（如 OpenCvSharp）
    public class VisionFrame
    {
        // 原始像素数据（BGR/BGRA/灰度等，根据 Channels 含义解释）
        public byte[]? Data { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public int Channels { get; set; }

        public VisionFrame() { }

        public VisionFrame(byte[] data, int width, int height, int channels)
        {
            Data = data;
            Width = width;
            Height = height;
            Channels = channels;
        }
    }
}
