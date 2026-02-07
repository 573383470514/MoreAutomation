using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.Application.Services
{
    public class StartupSelfCheckService
    {
        private readonly AppConfig _config;
        private const string TesseractDataUrl = "https://github.com/UB-Mannheim/tesseract/wiki";

        public StartupSelfCheckService(AppConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void RunChecks()
        {
            // 检查客户端路径（如果已配置）
            if (!string.IsNullOrWhiteSpace(_config.ClientPath) && !Directory.Exists(_config.ClientPath))
            {
                throw new StartupValidationException($"客户端路径不存在: {_config.ClientPath}");
            }

            // 检查 tessdata（OCR 所需）
            string tessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            if (!Directory.Exists(tessPath))
            {
                Directory.CreateDirectory(tessPath);
            }

            // 检查必需的训练数据文件
            var requiredFiles = new[] { "chi_sim.traineddata", "eng.traineddata" };
            foreach (var file in requiredFiles)
            {
                string filePath = Path.Combine(tessPath, file);
                if (!File.Exists(filePath))
                {
                    // 创建一个占位符，实际使用时用户需手动下载或通过脚本下载
                    File.WriteAllText(filePath + ".missing", $"Please download {file} from: https://github.com/UB-Mannheim/tesseract/wiki");
                }
            }

            // 检查存储目录可写性（与 AccountRepository 一致的位置）
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NIGHTHAVEN");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string testFile = Path.Combine(folder, ".__startup_test.txt");
                File.WriteAllText(testFile, "ok");
                File.Delete(testFile);
            }
            catch (Exception ex)
            {
                throw new StartupValidationException("数据目录不可写，请检查权限", ex);
            }
        }
    }
}
