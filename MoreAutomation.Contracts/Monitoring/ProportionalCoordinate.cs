using System.Collections.Generic;

namespace MoreAutomation.Contracts.Monitoring
{
    /// <summary>
    /// 比例坐标（相对于屏幕宽高的百分比）
    /// </summary>
    public class ProportionalCoordinate
    {
        /// <summary>
        /// X 百分比 (0.0 ~ 1.0)
        /// </summary>
        public double XPercent { get; set; }

        /// <summary>
        /// Y 百分比 (0.0 ~ 1.0)
        /// </summary>
        public double YPercent { get; set; }

        public ProportionalCoordinate() { }

        public ProportionalCoordinate(double xPercent, double yPercent)
        {
            XPercent = xPercent;
            YPercent = yPercent;
        }

        /// <summary>
        /// 根据当前屏幕尺寸转换为像素坐标
        /// </summary>
        public (int pixelX, int pixelY) ToPxielCoordinates(int screenWidth, int screenHeight)
        {
            return (
                (int)(XPercent * screenWidth),
                (int)(YPercent * screenHeight)
            );
        }

        public override string ToString() => $"({XPercent:P0}, {YPercent:P0})";
    }

    /// <summary>
    /// 镜像模式操作步骤（采集的点击序列）
    /// </summary>
    public class MirrorModeAction
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProportionalCoordinate> Coordinates { get; set; } = new();
        public int DelayBetweenClicksMs { get; set; } = 100; // 各点击间隔
    }
}
