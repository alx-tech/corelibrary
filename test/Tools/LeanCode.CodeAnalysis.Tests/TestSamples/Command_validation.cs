using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;

namespace ValidatedCommands
{
    public class Context { }

    public class ValidatedCommand : ICommand { }
    public class Validator : ContextualValidator<ValidatedCommand> { }
    public class ValidatedHandler : ICommandHandler<Context, ValidatedCommand>
    {
        public Task ExecuteAsync(Context ctx, ValidatedCommand cmd) => Task.CompletedTask;
    }

    public class NotValidatedCommand : ICommand { }
    public class NotValidatedHandler : ICommandHandler<Context, NotValidatedCommand>
    {
        public Task ExecuteAsync(Context ctx, NotValidatedCommand cmd) => Task.CompletedTask;
    }
}
