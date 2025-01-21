using System.Threading.Tasks;
using Xunit;

namespace LeanCode.IntegrationTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1001", Justification = "Disposed with `IAsyncLifetime`.")]
public class Tests : IAsyncLifetime
{
    private readonly AuthenticatedTestApp app;

    public Tests()
    {
        app = new AuthenticatedTestApp();
    }

    [Fact]
    public async Task TestQuery()
    {
        var res = await app.Query.GetAsync(new App.SampleQuery());

        Assert.Equal($"{App.AuthConfig.UserId}-", res?.Data);
    }

    public ValueTask InitializeAsync() => app.InitializeAsync();

    public ValueTask DisposeAsync() => app.DisposeAsync();
}
