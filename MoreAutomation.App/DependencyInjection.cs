using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Contracts.Automation;
using MoreAutomation.Infrastructure.Persistence.Repositories;
using MoreAutomation.Infrastructure.Config;
using MoreAutomation.Infrastructure.Platform;
using MoreAutomation.Application.UseCases.AccountManagement;
using MoreAutomation.Application.UseCases.DeveloperTools;
using MoreAutomation.Application.Orchestration;
using MoreAutomation.Application.Services;
using MoreAutomation.UI.Shell;
using MoreAutomation.UI.Modules.AccountManagement;
using MoreAutomation.UI.Modules.DeveloperTools;
using MoreAutomation.Automation.Scheduler;
using MoreAutomation.Automation.Execution;

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

                    // proxy manager keeps in-memory mapping of per-account proxy ports
                    services.AddSingleton<ProxyManager>();

                    var cfg = services.BuildServiceProvider().GetRequiredService<JsonConfigService>().GetConfig();
                    services.AddSingleton(cfg);

                    // register module registry and feature gate early so defaults can be initialized
                    services.AddSingleton<ModuleRegistry>();
                    services.AddSingleton<FeatureGateService>();

                    // feature gate defaults initialization
                    var spTemp = services.BuildServiceProvider();
                    var fg = spTemp.GetService<MoreAutomation.Application.Services.FeatureGateService>();
                    fg?.RegisterDefaults();

                    services.AddSingleton<IAutomationEngine, AutomationScheduler>();
                    services.AddSingleton<MoreAutomation.Contracts.Monitoring.IMetricsService, MoreAutomation.Automation.Monitoring.SimpleMetricsService>();
                    services.AddSingleton<ICoordinateExecutor, CoordinateExecutor>();
                    services.AddSingleton<AccountService>(sp => new AccountService(sp.GetRequiredService<IAccountRepository>(), sp.GetRequiredService<ProxyManager>(), sp.GetRequiredService<MoreAutomation.Application.Services.ILogService>()));
                    services.AddSingleton<MoreAutomation.Application.Services.ILogService, MoreAutomation.Application.Services.SimpleLogService>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.IMirrorAgent>(sp => new MoreAutomation.Application.Services.SimpleMirrorAgent(
                        cfg.ClientPath ?? "localhost", 
                        sp.GetRequiredService<MoreAutomation.Application.Services.ILogService>()));
                    services.AddSingleton<MoreAutomation.Automation.Recovery.FailureRecoveryPolicy>();
                    // Command bus and handlers
                        services.AddSingleton<StartupSelfCheckService>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandBus, MoreAutomation.Application.Messaging.CommandBus>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.DeleteAccountCommand>, MoreAutomation.Application.Handlers.AccountCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.AddAccountCommand>, MoreAutomation.Application.Handlers.AccountCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.SetMasterCommand>, MoreAutomation.Application.Handlers.AccountCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.UpdateAccountCommand>, MoreAutomation.Application.Handlers.AccountCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.IQueryHandler<MoreAutomation.Application.Commands.LoadAccountsQuery, System.Collections.Generic.List<MoreAutomation.Domain.Entities.Account>>, MoreAutomation.Application.Handlers.AccountCommandHandler>();
                    // Runtime command handler
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.StartAutomationCommand>, MoreAutomation.Application.Handlers.RuntimeCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.StopAutomationCommand>, MoreAutomation.Application.Handlers.RuntimeCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.ForceStopCommand>, MoreAutomation.Application.Handlers.RuntimeCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.HideWindowCommand>, MoreAutomation.Application.Handlers.RuntimeCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.LoginAccountCommand>, MoreAutomation.Application.Handlers.RuntimeCommandHandler>();
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandHandler<MoreAutomation.Application.Commands.LogoutAccountCommand>, MoreAutomation.Application.Handlers.RuntimeCommandHandler>();
                    services.AddSingleton<StartupOrchestrator>();
                    services.AddSingleton<UiActionBus>();
                    // metrics query handler
                    services.AddSingleton<MoreAutomation.Application.Messaging.IQueryHandler<MoreAutomation.Application.Commands.GetSchedulerMetricsQuery, MoreAutomation.Application.Models.SchedulerMetrics>, MoreAutomation.Application.Handlers.MetricsQueryHandler>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<AccountViewModel>();
                    services.AddSingleton<MoreAutomation.UI.Modules.DailyTasks.DailyTasksViewModel>();
                    services.AddSingleton<MoreAutomation.UI.Modules.MirrorMode.MirrorModeViewModel>(sp => new MoreAutomation.UI.Modules.MirrorMode.MirrorModeViewModel(
                        sp.GetRequiredService<MoreAutomation.Application.Messaging.IMirrorAgent>(),
                        sp.GetRequiredService<MoreAutomation.Application.Services.ILogService>(),
                        sp.GetRequiredService<MoreAutomation.Contracts.Configuration.AppConfig>(),
                        sp.GetRequiredService<JsonConfigService>(),
                        sp.GetRequiredService<ICoordinateExecutor>()));
                    services.AddSingleton<DeveloperToolsService>();
                    services.AddSingleton<DeveloperToolsViewModel>();
                    services.AddSingleton<DeveloperToolsView>();
                    // ensure MainViewModel resolves with all dependencies
                    services.AddSingleton<MainViewModel>(sp => new MainViewModel(sp.GetRequiredService<UiActionBus>(), sp.GetRequiredService<FeatureGateService>(), sp.GetRequiredService<AccountViewModel>(), sp.GetRequiredService<MoreAutomation.Application.Messaging.ICommandBus>(), sp.GetRequiredService<MoreAutomation.Application.Services.ILogService>(), sp.GetRequiredService<IAutomationEngine>(), sp.GetRequiredService<MoreAutomation.UI.Modules.DailyTasks.DailyTasksViewModel>(), sp.GetRequiredService<MoreAutomation.UI.Modules.MirrorMode.MirrorModeViewModel>()));
                    // ensure ICommandBus is resolved for UI viewmodels
                    services.AddSingleton<MoreAutomation.Application.Messaging.ICommandBus>(sp => (MoreAutomation.Application.Messaging.ICommandBus)sp.GetRequiredService(typeof(MoreAutomation.Application.Messaging.ICommandBus)));
                    services.AddSingleton<MainWindow>();
                });
    }
}
