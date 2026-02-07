using System;
using System.IO;
using OpenCvSharp;
using MoreAutomation.Contracts.Models;
using MoreAutomation.Domain.ValueObjects;

namespace MoreAutomation.Vision.Matchers
{
    public class TemplateMatcher
    {
        public MatchResult FindImage(Mat screenMat, string templatePath, double threshold = 0.8)
        {
            if (screenMat == null || screenMat.Empty() || !File.Exists(templatePath))
                return MatchResult.Fail("输入无效");

            using var template = new Mat(templatePath, ImreadModes.Unchanged);
            if (template.Empty()) return MatchResult.Fail("无法加载模板");

            using var result = new Mat();
            // 处理透明通道遮罩 (关键：处理流光干扰)
            if (template.Channels() == 4)
            {
                using var mask = new Mat();
                Cv2.ExtractChannel(template, mask, 3);
                using var templateBgr = new Mat();
                Cv2.CvtColor(template, templateBgr, ColorConversionCodes.BGRA2BGR);
                Cv2.MatchTemplate(screenMat, templateBgr, result, TemplateMatchModes.CCoeffNormed, mask);
            }
            else
            {
                Cv2.MatchTemplate(screenMat, template, result, TemplateMatchModes.CCoeffNormed);
            }

            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

            if (maxVal >= threshold)
            {
                double centerX = maxLoc.X + template.Width / 2.0;
                double centerY = maxLoc.Y + template.Height / 2.0;

                return new MatchResult
                {
                    Success = true,
                    Confidence = maxVal,
                    PixelX = centerX,
                    PixelY = centerY,
                    // 自动转换成比例坐标
                    NormalizedCenter = new NormalizedPoint(centerX / screenMat.Width, centerY / screenMat.Height)
                };
            }

            return MatchResult.Fail();
        }
    }
}