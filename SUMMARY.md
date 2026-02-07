# 更改汇总与补齐建议

概览：本次变更集中在提高可维护性与长期稳定性，主要内容包括：

- 引入命令/查询总线（`ICommand`/`IQuery` + `CommandBus`），将 UI 从业务逻辑中解耦。
- 将 `MoreAutomation.Contracts` 与 OpenCvSharp 解耦，新增 `VisionFrame`（视觉契约）。
- 加强 `Account` 管理：仓储异常映射（`RepositoryException`/`DuplicateAccountException`）、`AccountService.MoveAccountAsync` 完整实现（含主控选举与组容量校验）。
- 自动化稳定性改进：新增 `FailureRecoveryPolicy` 可配置回退和指数退避、在 `AutomationScheduler` 中接入并实现熔断逻辑（`CircuitTripThreshold`）。
- 指标导出：添加 `IMetricsService`、`SimpleMetricsService`，并提供 `GetSchedulerMetricsQuery` / `MetricsQueryHandler` 以供查询 tick/failure 计数。
- 启动自检：新增 `StartupSelfCheckService`，并在启动时执行以验证外部依赖（tessdata、客户端路径、可写权限）。

主要变更文件（非穷尽）：

- MoreAutomation.Application: `AccountService.cs`, `BusinessException.cs`, `CommandBus.cs`, `AccountCommandHandler.cs`, `MetricsQueryHandler.cs`, `SchedulerMetrics.cs`
- MoreAutomation.Automation: `AutomationScheduler.cs`, `FailureRecoveryPolicy.cs`, `SimpleMetricsService.cs`
- MoreAutomation.Contracts: `AppConfig.cs`（新增 CircuitTripThreshold）、`FeatureToggleKeys.cs`、`IMetricsService.cs`
- MoreAutomation.App: `DependencyInjection.cs`（DI 调整与新服务注册）
- MoreAutomation.Vision: TemplateMatcher/TesseractProvider/Converter 调整（Contracts 解耦）

Feature toggles 使用点扫描：

（代码中发现的关键使用点）
- `FeatureToggleKeys.RuntimeCircuitBreaker`：在 `AutomationScheduler` 启动周期前检查（默认 `false`）。
- `FeatureToggleKeys.RuntimeForceStop`：在 `RuntimeCommandHandler.ForceStop` 调用前检查（默认 `true`）。
- UI 与模块开关：`FeatureGateService` 在 DI 初始化时通过 `RegisterDefaults()` 初始化多个键，默认行为已设置（见 `FeatureGateService.RegisterDefaults()`）。

未决与建议补齐项：

1. 发布治理：添加 `dotnet publish` 的 CI 配置（单文件、自包含选项）以及签名/版本化流程。建议在 `MoreAutomation.App` 添加发布配置文件与 README 指南。
2. 监控/告警：当前只提供本地内存指标实现（`SimpleMetricsService`）。建议将其替换或扩展为将指标推送到 Prometheus/Influx 或引入日志上报（Application Insights）。
3. 配置管理：`AppConfig` 当前为 JSON 本地配置。建议提供运行时热加载和配置校验端点。
4. 单元/集成测试：未添加测试（按你的要求）。建议至少为 `AccountService`、`AutomationScheduler` 的关键行为补充单元测试以防回归。
5. 文档：为新增的总线（Command/Query）与恢复策略编写简短的开发者指南，便于模块扩展与维护。

如何验证本地改动：

```powershell
dotnet build MoreAutomation.slnx -c Debug
dotnet run --project MoreAutomation.App\MoreAutomation.App.csproj
```

若需我现在把上述第 1-4 项中的任一项继续实现，请直接指示。现在我会把 `功能开关矩阵` 做一次完整核对（确认默认值与使用点一致）。

— 已完成的工作由自动化代理整理
