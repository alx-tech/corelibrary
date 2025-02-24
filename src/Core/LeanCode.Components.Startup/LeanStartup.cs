using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LeanCode.Components.Startup;

public abstract class LeanStartup
{
    protected IConfiguration Configuration { get; }
    protected ILogger Logger { get; }
    protected virtual bool CloseAndFlushLogger { get; } = true;

    protected abstract IReadOnlyList<IAppModule> Modules { get; }

    protected LeanStartup(IConfiguration config)
    {
        Configuration = config;
        Logger = Log.ForContext(GetType());

        Logger.Information("Configuration loaded, starting app");
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        Logger.Debug("Loading common services");
        services.AddOptions();

        foreach (var component in Modules)
        {
            Logger.Debug("Loading services of {Component}", component.GetType());
            component.ConfigureServices(services);
        }
    }

    public virtual void ConfigureContainer(ContainerBuilder builder)
    {
        foreach (var component in Modules)
        {
            Logger.Debug("Loading module {Component}", component.GetType());
            builder.RegisterModule(component);
        }
    }

    public void Configure(IApplicationBuilder app)
    {
        ConfigureApp(app);

        var appLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        if (CloseAndFlushLogger)
        {
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }
    }

    protected abstract void ConfigureApp(IApplicationBuilder app);
}
