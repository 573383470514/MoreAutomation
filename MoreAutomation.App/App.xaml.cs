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
            await _host.StartAsync();

            var orchestrator = _host.Services.GetRequiredService<StartupOrchestrator>();
            var step = orchestrator.GetNextStep();

            // 根据启动编排器的结果决定下一步
            switch (step)
            {
                case StartupStep.ShowAgreement:
                    // 弹出协议窗口逻辑 (此处简化，实际应从 DI 获取窗口)
                    MessageBox.Show("请先同意协议");
                    break;
                case StartupStep.EnterMainShell:
                    var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                    mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
                    mainWindow.Show();
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