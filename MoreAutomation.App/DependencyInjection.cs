using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreAutomation.Contracts.Interfaces;
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
                    services.AddSingleton<IAccountRepository, AccountRepository>();
                    services.AddSingleton<JsonConfigService>();

                    services.AddSingleton(sp => sp.GetRequiredService<JsonConfigService>().GetConfig());

                    services.AddSingleton<IAutomationEngine, AutomationScheduler>();

                    services.AddSingleton<AccountService>();
                    services.AddSingleton<StartupOrchestrator>();
                    services.AddSingleton<UiActionBus>();
                    services.AddSingleton<ModuleRegistry>();
                    services.AddSingleton<FeatureGateService>();

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<AccountViewModel>();
                    services.AddSingleton<MainWindow>();
                });
    }
}
