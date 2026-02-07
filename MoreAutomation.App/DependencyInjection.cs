using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Contracts.Configuration;
using MoreAutomation.Infrastructure.Persistence.Repositories;
using MoreAutomation.Infrastructure.Config;
using MoreAutomation.Application.UseCases.AccountManagement;
using MoreAutomation.Application.Orchestration;
using MoreAutomation.Application.Services;
using MoreAutomation.UI.Shell;
using MoreAutomation.UI.Modules.AccountManagement;
using MoreAutomation.Automation.Scheduler;

namespace MoreAutomation.App
{
    public static class DependencyInjection
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // 1. Infrastructure 层
                    services.AddSingleton<IAccountRepository, AccountRepository>();
                    services.AddSingleton<JsonConfigService>();

                    // 2. Contracts 层配置模型 (从 JsonConfigService 加载)
                    services.AddSingleton(sp => sp.GetRequiredService<JsonConfigService>().GetConfig());

                    // 3. Automation & Vision 层
                    services.AddSingleton<IAutomationEngine, AutomationScheduler>();
                    // 这里可以按需注册 IVisionProvider 等

                    // 4. Application 层 (业务逻辑)
                    services.AddSingleton<AccountService>();
                    services.AddSingleton<StartupOrchestrator>();
                    services.AddSingleton<UiActionBus>();
                    services.AddSingleton<ModuleRegistry>();

                    // 5. UI 层 (ViewModels & Main Window)
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<AccountViewModel>();
                    services.AddSingleton<MainWindow>();
                });
    }
}