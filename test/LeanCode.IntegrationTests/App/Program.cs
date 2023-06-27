using System.Diagnostics.CodeAnalysis;
using LeanCode.AzureIdentity;
using LeanCode.Components;
using LeanCode.Logging;
using LeanCode.Startup.MicrosoftDI;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTests.App;

public static class Program
{
    [SuppressMessage("?", "IDE0060", Justification = "`args` are required by convention.")]
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return LeanProgram
            .BuildMinimalHost<Startup>()
            .AddAppConfigurationFromAzureKeyVaultOnNonDevelopmentEnvironment()
            .ConfigureDefaultLogging(projectName: "test", destructurers: new[] { typeof(Program).Assembly });
    }
}
