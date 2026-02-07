using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreAutomation.Application.Orchestration;
using MoreAutomation.UI.Shell;

namespace MoreAutomation.App
{
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = DependencyInjection.CreateHostBuilder(new string[0]).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // 在启动服务前运行自检，若自检失败则给出提示并退出
            try
            {
                var selfCheck = _host.Services.GetRequiredService<MoreAutomation.Application.Services.StartupSelfCheckService>();
                selfCheck.RunChecks();
            }
            catch (MoreAutomation.Application.Services.StartupValidationException ex)
            {
                System.Windows.MessageBox.Show($"启动自检失败: {ex.Message}", "启动失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Shutdown();
                return;
            }

            await _host.StartAsync();

            // Initialize proxy mappings from repository into in-memory manager
            try
            {
                var proxyMgr = _host.Services.GetService<MoreAutomation.Infrastructure.Platform.ProxyManager>();
                proxyMgr?.Initialize();
            }
            catch
            {
                // non-fatal: continue startup even if proxy init fails
            }

            var orchestrator = _host.Services.GetRequiredService<StartupOrchestrator>();
            var step = orchestrator.GetNextStep();

            // 根据启动编排器的结果决定下一步
            switch (step)
            {
                case StartupStep.ShowAgreement:
                    var agreement = new MoreAutomation.UI.Shell.UserAgreementWindow();
                    agreement.Owner = null;
                    agreement.ShowDialog();
                    if (!agreement.Accepted)
                    {
                        Shutdown();
                        return;
                    }
                    // 标记已同意并保存配置
                    var cfgService = _host.Services.GetRequiredService<MoreAutomation.Infrastructure.Config.JsonConfigService>();
                    var cfg = cfgService.GetConfig();
                    cfg.IsAgreed = true;
                    cfgService.SaveConfigAsync(cfg).GetAwaiter().GetResult();

                    // 重新评估下一步
                    step = orchestrator.GetNextStep();
                    goto case StartupStep.ShowPathSelection;

                case StartupStep.ShowPathSelection:
                    var cfg2 = _host.Services.GetRequiredService<MoreAutomation.Infrastructure.Config.JsonConfigService>().GetConfig();
                    var pathDlg = new MoreAutomation.UI.Shell.PathSelectionWindow(cfg2.ClientPath);
                    pathDlg.Owner = null;
                    pathDlg.ShowDialog();
                    if (string.IsNullOrWhiteSpace(pathDlg.SelectedPath))
                    {
                        Shutdown();
                        return;
                    }
                    cfg2.ClientPath = pathDlg.SelectedPath;
                    _host.Services.GetRequiredService<MoreAutomation.Infrastructure.Config.JsonConfigService>().SaveConfigAsync(cfg2).GetAwaiter().GetResult();

                    // 进入主壳
                    var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                    mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
                    mainWindow.Show();
                    break;

                case StartupStep.EnterMainShell:
                    var mainWindow2 = _host.Services.GetRequiredService<MainWindow>();
                    mainWindow2.DataContext = _host.Services.GetRequiredService<MainViewModel>();
                    mainWindow2.Show();
                    break;
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}