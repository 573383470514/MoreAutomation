using System;

namespace MoreAutomation.Domain.ValueObjects
{
    public record NormalizedPoint
    {
        public double X { get; }
        public double Y { get; }

        public NormalizedPoint(double x, double y)
        {
            if (x < 0 || x > 1 || y < 0 || y > 1)
                throw new ArgumentOutOfRangeException("坐标必须为 0 到 1 之间的比例值");
            X = x;
            Y = y;
        }
    }
}