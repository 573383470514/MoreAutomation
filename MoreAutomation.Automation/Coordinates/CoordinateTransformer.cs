using System;
using MoreAutomation.Domain.ValueObjects;

namespace MoreAutomation.Automation.Coordinates
{
    public class CoordinateTransformer
    {
        /// <summary>
        /// 将比例坐标 (0~1) 转换为窗口内的物理像素坐标。
        /// </summary>
        public (int x, int y) TransformToPixel(NormalizedPoint normalized, int windowWidth, int windowHeight)
        {
            if (windowWidth <= 0) throw new ArgumentOutOfRangeException(nameof(windowWidth));
            if (windowHeight <= 0) throw new ArgumentOutOfRangeException(nameof(windowHeight));

            int x = (int)(normalized.X * windowWidth);
            int y = (int)(normalized.Y * windowHeight);

            x = Math.Clamp(x, 0, Math.Max(0, windowWidth - 1));
            y = Math.Clamp(y, 0, Math.Max(0, windowHeight - 1));

            return (x, y);
        }
    }
}
