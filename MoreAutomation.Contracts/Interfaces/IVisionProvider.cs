using MoreAutomation.Contracts.Models;
using System;

namespace MoreAutomation.Contracts.Interfaces
{
    public interface IVisionProvider
    {
        // 使用跨项目的轻量图像表示，避免 Contracts 直接依赖 OpenCvSharp
        MatchResult FindImage(VisionFrame screenFrame, string templatePath, double threshold = 0.8);
        MatchResult FindText(VisionFrame screenFrame, string targetText);
    }
}