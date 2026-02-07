using System;
using System.IO;
using System.Drawing;
using OpenCvSharp;
using Tesseract;
using MoreAutomation.Contracts.Models;
using MoreAutomation.Domain.ValueObjects;

namespace MoreAutomation.Vision.Ocr
{
    public class TesseractProvider : IDisposable
    {
        private readonly TesseractEngine? _engine;
        private readonly bool _isInitialized;

        public TesseractProvider()
        {
            string tessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            
            // 检查必需的训练数据文件
            string chiSimFile = Path.Combine(tessPath, "chi_sim.traineddata");
            string engFile = Path.Combine(tessPath, "eng.traineddata");
            
            if (File.Exists(chiSimFile) && File.Exists(engFile))
            {
                try
                {
                    _engine = new TesseractEngine(tessPath, "chi_sim+eng", EngineMode.Default);
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Tesseract 初始化失败: {ex.Message}");
                    _isInitialized = false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"缺少 Tesseract 训练数据: chi_sim.traineddata={File.Exists(chiSimFile)}, eng.traineddata={File.Exists(engFile)}");
                _isInitialized = false;
            }
        }

        public MatchResult FindText(Mat img, string targetText)
        {
            if (!_isInitialized || _engine == null)
            {
                // OCR 不可用，返回失败
                return MatchResult.Fail();
            }

            try
            {
                using var gray = new Mat();
                Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
                // 二值化增强
                Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                // 将 Mat 编码为 BMP 数据，再用 Pix 加载以供 Tesseract 处理
                Cv2.ImEncode(".bmp", gray, out byte[] bmpBytes);
                using var pix = Pix.LoadFromMemory(bmpBytes);
                using var page = _engine.Process(pix);

                var text = page.GetText();
                if (!string.IsNullOrEmpty(text) && text.Contains(targetText))
                {
                    return new MatchResult { Success = true, Confidence = page.GetMeanConfidence() };
                }

                return MatchResult.Fail();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tesseract 处理失败: {ex.Message}");
                return MatchResult.Fail();
            }
        }

        public void Dispose() => _engine?.Dispose();
    }
}