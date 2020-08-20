using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            return CreateWebHostBuilder(args).Build().RunAsync();
        }

        [SuppressMessage("?", "IDE0060", Justification = "`args` are required by convention.")]
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return LeanProgram
                .BuildMinimalWebHost<Startup>()
                .UseKestrel()
                .AddAppConfigurationFromAzureKeyVault()
                .ConfigureDefaultLogging(
                    projectName: "test",
                    destructurers: new TypesCatalog(typeof(Program)));
        }
    }
}
