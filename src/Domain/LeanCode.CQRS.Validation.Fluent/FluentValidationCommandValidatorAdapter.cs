using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;

namespace LeanCode.CQRS.Validation.Fluent
{
    public class FluentValidationCommandValidatorAdapter<TAppContext, TCommand>
        : ICommandValidator<TAppContext, TCommand>
        where TCommand : ICommand
        where TAppContext : notnull
    {
        private readonly IValidator fluentValidator;
        private readonly IComponentContext componentContext;

        public FluentValidationCommandValidatorAdapter(IValidator fluentValidator, IComponentContext componentContext)
        {
            this.fluentValidator = fluentValidator;
            this.componentContext = componentContext;
        }

        public async Task<Contracts.Validation.ValidationResult> ValidateAsync(TAppContext appContext, TCommand command)
        {
            var ctx = PrepareContext(appContext, command);

            var fluentValidationResult = await fluentValidator.ValidateAsync(ctx);

            var mappedResult = fluentValidationResult.Errors
                .Select(MapFluentError)
                .ToList();

            return new(mappedResult);
        }

        private ValidationError MapFluentError(ValidationFailure failure)
        {
            var state = failure.CustomState as FluentValidatorErrorState;

            return new ValidationError(
                failure.PropertyName,
                failure.ErrorMessage,
                state?.ErrorCode ?? 0);
        }

        private ValidationContext<TCommand> PrepareContext(TAppContext appContext, TCommand command)
        {
            var ctx = new ValidationContext<TCommand>(
                command,
                new PropertyChain(),
                ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory());

            ctx.RootContextData[ValidationContextExtensions.AppContextKey] = appContext;
            ctx.RootContextData[ValidationContextExtensions.ComponentContextKey] = componentContext;

            return ctx;
        }
    }
}
