using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    sealed class RemoteCommandHandler<TAppContext>
        : BaseRemoteObjectHandler<TAppContext>
    {
        private static readonly MethodInfo ExecCommandMethod = typeof(RemoteCommandHandler<TAppContext>)
            .GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly IServiceProvider serviceProvider;

        public RemoteCommandHandler(
            IServiceProvider serviceProvider,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
            : base(catalog, contextTranslator)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task<ExecutionResult> ExecuteObjectAsync(
            TAppContext context, object obj)
        {
            var type = obj.GetType();
            if (!typeof(IRemoteCommand).IsAssignableFrom(type))
            {
                Logger.Warning("The type {Type} is not an IRemoteCommand", type);
                return ExecutionResult.Failed(StatusCodes.Status404NotFound);
            }

            var method = methodCache.GetOrAdd(type, MakeExecutorMethod);
            var result = (Task<CommandResult>)method.Invoke(this, new[] { context, obj });

            CommandResult cmdResult;
            try
            {
                cmdResult = await result.ConfigureAwait(false);
            }
            catch (CommandHandlerNotFoundException)
            {
                return ExecutionResult.Failed(StatusCodes.Status404NotFound);
            }

            if (cmdResult.WasSuccessful)
            {
                return ExecutionResult.Success(cmdResult);
            }
            else
            {
                return ExecutionResult.Success(cmdResult, StatusCodes.Status422UnprocessableEntity);
            }
        }

        private Task<CommandResult> ExecuteCommand<TContext, TCommand>(
            TAppContext appContext,
            object cmd)
            where TCommand : IRemoteCommand<TContext>
        {
            var commandExecutor = serviceProvider.GetService<ICommandExecutor<TAppContext>>();
            return commandExecutor.RunAsync(appContext, (TCommand)cmd);
        }

        private static MethodInfo MakeExecutorMethod(Type commandType)
        {
            var types = commandType
                .GetInterfaces()
                .Single(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .GenericTypeArguments;
            var contextType = types[0];
            return ExecCommandMethod.MakeGenericMethod(contextType, commandType);
        }
    }
}
