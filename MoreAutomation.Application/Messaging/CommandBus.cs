using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreAutomation.Infrastructure.Persistence;

namespace MoreAutomation.Application.Messaging
{
    public interface ICommandBus
    {
        Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand;
        Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>;
    }

    public class CommandBus : ICommandBus
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<CommandBus> _logger;

        public CommandBus(IServiceProvider provider, ILogger<CommandBus> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handler = _provider.GetService<ICommandHandler<TCommand>>();
            if (handler == null) throw new InvalidOperationException($"No handler registered for {typeof(TCommand).FullName}");

            _logger.LogDebug("Dispatching command {CommandType}", typeof(TCommand).FullName);
            try
            {
                await handler.HandleAsync(command);
            }
            catch (RepositoryException rex)
            {
                _logger.LogWarning(rex, "Repository error while handling {CommandType}", typeof(TCommand).FullName);
                throw new InvalidOperationException(rex.Message, rex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while handling {CommandType}", typeof(TCommand).FullName);
                throw;
            }
        }

        public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            var handler = _provider.GetService<IQueryHandler<TQuery, TResult>>();
            if (handler == null) throw new InvalidOperationException($"No query handler registered for {typeof(TQuery).FullName}");

            _logger.LogDebug("Dispatching query {QueryType}", typeof(TQuery).FullName);
            try
            {
                return await handler.HandleAsync(query);
            }
            catch (RepositoryException rex)
            {
                _logger.LogWarning(rex, "Repository error while querying {QueryType}", typeof(TQuery).FullName);
                throw new InvalidOperationException(rex.Message, rex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while querying {QueryType}", typeof(TQuery).FullName);
                throw;
            }
        }
    }
}
