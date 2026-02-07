using System;
using MoreAutomation.Domain.ValueObjects;
using MoreAutomation.Infrastructure.Platform; // 需要用到窗口信息

namespace MoreAutomation.Automation.Coordinates
{
    public class CoordinateTransformer
    {
        /// <summary>
        /// 将比例坐标 (0~1) 转换为窗口内的物理像素坐标
        /// </summary>
        public (int x, int y) TransformToPixel(NormalizedPoint normalized, int windowWidth, int windowHeight)
        {
            int x = (int)(normalized.X * windowWidth);
            int y = (int)(normalized.Y * windowHeight);
            return (x, y);
        }
    }
}