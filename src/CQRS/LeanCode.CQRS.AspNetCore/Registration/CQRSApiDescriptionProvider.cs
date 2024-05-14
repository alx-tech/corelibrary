using System.Collections.Immutable;
using System.Text;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace LeanCode.CQRS.AspNetCore.Registration;

internal sealed class CQRSApiDescriptionProvider : IApiDescriptionProvider
{
    private const string ApplicationJson = "application/json";

    private readonly EndpointDataSource endpointDataSource;
    private readonly CQRSApiDescriptionConfiguration configuration;

    public int Order => -1200;

    public CQRSApiDescriptionProvider(
        EndpointDataSource endpointDataSource,
        CQRSApiDescriptionConfiguration configuration
    )
    {
        this.endpointDataSource = endpointDataSource;
        this.configuration = configuration;
    }

    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (
                endpoint is RouteEndpoint routeEndpoint
                && routeEndpoint.Metadata.GetMetadata<CQRSObjectMetadata>() is { } metadata
            )
            {
                context.Results.Add(CreateApiDescription(routeEndpoint, metadata));
            }
        }
    }

    public void OnProvidersExecuted(ApiDescriptionProviderContext context) { }

    private ApiDescription CreateApiDescription(RouteEndpoint routeEndpoint, CQRSObjectMetadata metadata)
    {
        var apiDescription = new ApiDescription
        {
            HttpMethod = HttpMethod.Post.Method,
            GroupName = null,
            RelativePath = BuildPath(routeEndpoint.RoutePattern),
            ActionDescriptor = new ActionDescriptor
            {
                DisplayName = routeEndpoint.DisplayName,
                RouteValues = configuration.RouteValuesMapping(routeEndpoint),
            },
        };
        apiDescription.SupportedRequestFormats.Add(new() { MediaType = ApplicationJson });

        if (metadata.ObjectKind == CQRSObjectKind.Command)
        {
            DefineCommand(metadata, apiDescription);
        }
        else if (metadata.ObjectKind == CQRSObjectKind.Query)
        {
            DefineQuery(metadata, apiDescription);
        }
        else if (metadata.ObjectKind == CQRSObjectKind.Operation)
        {
            DefineOperation(metadata, apiDescription);
        }

        AddCommonResponses(apiDescription);

        return apiDescription;
    }

    private static string BuildPath(RoutePattern pattern)
    {
        var sb = new StringBuilder();
        foreach (var seg in pattern.PathSegments)
        {
            if (!seg.IsSimple || !seg.Parts[0].IsLiteral)
            {
                throw new InvalidOperationException("CQRS Endpoints can only contain literal parts.");
            }
            sb.Append(((RoutePatternLiteralPart)seg.Parts[0]).Content);
            sb.Append('/');
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    private static void DefineCommand(CQRSObjectMetadata metadata, ApiDescription apiDescription)
    {
        apiDescription.ParameterDescriptions.Add(
            new()
            {
                IsRequired = true,
                Source = BindingSource.Body,
                Type = metadata.ObjectType,
                ModelMetadata = CreateModelMetadata(metadata.ObjectType),
            }
        );

        apiDescription.SupportedResponseTypes.Add(
            new()
            {
                ModelMetadata = CreateModelMetadata(typeof(CommandResult)),
                ApiResponseFormats = [new() { MediaType = ApplicationJson }],
                StatusCode = 200,
                Type = typeof(CommandResult),
            }
        );

        apiDescription.SupportedResponseTypes.Add(
            new()
            {
                ModelMetadata = CreateModelMetadata(typeof(CommandResult)),
                ApiResponseFormats = [new() { MediaType = ApplicationJson }],
                StatusCode = 422,
                Type = typeof(CommandResult),
            }
        );
    }

    private static void DefineQuery(CQRSObjectMetadata metadata, ApiDescription apiDescription)
    {
        apiDescription.ParameterDescriptions.Add(
            new()
            {
                IsRequired = true,
                Source = BindingSource.Body,
                Type = metadata.ObjectType,
                ModelMetadata = CreateModelMetadata(metadata.ObjectType),
            }
        );

        apiDescription.SupportedResponseTypes.Add(
            new()
            {
                ModelMetadata = CreateModelMetadata(metadata.ResultType),
                ApiResponseFormats = [new() { MediaType = ApplicationJson }],
                StatusCode = 200,
                Type = metadata.ResultType,
            }
        );
    }

    private static void DefineOperation(CQRSObjectMetadata metadata, ApiDescription apiDescription)
    {
        apiDescription.ParameterDescriptions.Add(
            new()
            {
                IsRequired = true,
                Source = BindingSource.Body,
                Type = metadata.ObjectType,
                ModelMetadata = CreateModelMetadata(metadata.ObjectType),
            }
        );

        apiDescription.SupportedResponseTypes.Add(
            new()
            {
                ModelMetadata = CreateModelMetadata(metadata.ResultType),
                ApiResponseFormats = [new() { MediaType = ApplicationJson }],
                StatusCode = 200,
                Type = metadata.ResultType,
            }
        );
    }

    private static void AddCommonResponses(ApiDescription apiDescription)
    {
        apiDescription.SupportedResponseTypes.Add(
            new()
            {
                ModelMetadata = CreateModelMetadata(typeof(void)),
                ApiResponseFormats = [new() { MediaType = ApplicationJson }],
                StatusCode = 400,
                Type = typeof(void),
            }
        );

        apiDescription.SupportedResponseTypes.Add(
            new()
            {
                ModelMetadata = CreateModelMetadata(typeof(void)),
                ApiResponseFormats = [new() { MediaType = ApplicationJson }],
                StatusCode = 401,
                Type = typeof(void),
            }
        );

        apiDescription.SupportedResponseTypes.Add(
            new()
            {
                ModelMetadata = CreateModelMetadata(typeof(void)),
                ApiResponseFormats = [new() { MediaType = ApplicationJson }],
                StatusCode = 403,
                Type = typeof(void),
            }
        );
    }

    private static CQRSBodyModelMetadata CreateModelMetadata(Type type) => new(ModelMetadataIdentity.ForType(type));
}

internal sealed class CQRSBodyModelMetadata : ModelMetadata
{
    public CQRSBodyModelMetadata(ModelMetadataIdentity identity)
        : base(identity) { }

    public override IReadOnlyDictionary<object, object> AdditionalValues { get; } =
        ImmutableDictionary<object, object>.Empty;
    public override string? BinderModelName { get; }
    public override Type? BinderType { get; }
    public override BindingSource? BindingSource => BindingSource.Body;
    public override bool ConvertEmptyStringToNull { get; }
    public override string? DataTypeName { get; }
    public override string? Description { get; }
    public override string? DisplayFormatString { get; }
    public override string? DisplayName { get; }
    public override string? EditFormatString { get; }
    public override ModelMetadata? ElementMetadata { get; }
    public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues { get; }
    public override IReadOnlyDictionary<string, string>? EnumNamesAndValues { get; }
    public override bool HasNonDefaultEditFormat { get; }
    public override bool HideSurroundingHtml { get; }
    public override bool HtmlEncode { get; }
    public override bool IsBindingAllowed => true;
    public override bool IsBindingRequired => true;
    public override bool IsEnum { get; }
    public override bool IsFlagsEnum { get; }
    public override bool IsReadOnly { get; }
    public override bool IsRequired => true;
    public override ModelBindingMessageProvider ModelBindingMessageProvider { get; } =
        new DefaultModelBindingMessageProvider();
    public override string? NullDisplayText { get; }
    public override int Order { get; }
    public override string? Placeholder { get; }
    public override ModelPropertyCollection Properties { get; } = new(Enumerable.Empty<ModelMetadata>());
    public override IPropertyFilterProvider? PropertyFilterProvider { get; }
    public override Func<object, object>? PropertyGetter { get; }
    public override Action<object, object?>? PropertySetter { get; }
    public override bool ShowForDisplay { get; }
    public override bool ShowForEdit { get; }
    public override string? SimpleDisplayProperty { get; }
    public override string? TemplateHint { get; }
    public override bool ValidateChildren { get; }
    public override IReadOnlyList<object> ValidatorMetadata { get; } = Array.Empty<object>();
}
