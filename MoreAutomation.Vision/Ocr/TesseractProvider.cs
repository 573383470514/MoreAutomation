using System;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;
using MoreAutomation.Contracts.Models;
using MoreAutomation.Domain.ValueObjects;

namespace MoreAutomation.Vision.Ocr
{
    public class TesseractProvider : IDisposable
    {
        private readonly TesseractEngine _engine;

        public TesseractProvider()
        {
            string tessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            _engine = new TesseractEngine(tessPath, "chi_sim+eng", EngineMode.Default);
        }

        public MatchResult FindText(Mat img, string targetText)
        {
            using var gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
            // 二值化增强
            Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            using var bitmap = gray.ToBitmap();
            using var page = _engine.Process(bitmap);

            var text = page.GetText();
            if (!string.IsNullOrEmpty(text) && text.Contains(targetText))
            {
                // 这里简化处理，实际可使用 PageIterator 获取具体坐标
                // 返回结果时同样需转换 NormalizedPoint
                return new MatchResult { Success = true, Confidence = page.GetMeanConfidence() };
            }

            return MatchResult.Fail();
        }

        public void Dispose() => _engine?.Dispose();
    }
}