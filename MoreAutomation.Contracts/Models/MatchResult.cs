using MoreAutomation.Domain.ValueObjects;

namespace MoreAutomation.Contracts.Models
{
    public class MatchResult
    {
        public bool Success { get; set; }

        // 原始像素坐标 (仅用于内部调试或 Vision 层)
        public double PixelX { get; set; }
        public double PixelY { get; set; }

        // 核心：比例坐标 (0~1)，用于自动化逻辑
        public NormalizedPoint? NormalizedCenter { get; set; }

        public double Confidence { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static MatchResult Fail(string msg = "") => new MatchResult { Success = false, ErrorMessage = msg };
    }
}