using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LeanCode.AsyncInitializer.Tests
{
    public class AsyncInitializerTests
    {
        private readonly Counter globalCounter = new Counter();

        [Fact]
        public async Task StartAsync_calls_InitializeAsync_on_every_object()
        {
            var (sp, init) = Prepare(
                Init(0),
                Init(1),
                Init(2));

            await init.StartAsync(default);

            Assert.NotNull(sp.Initializers[0].InitOrder);
            Assert.NotNull(sp.Initializers[1].InitOrder);
            Assert.NotNull(sp.Initializers[2].InitOrder);
        }

        [Fact]
        public async Task StopAsync_calls_DeinitializeAsync_on_every_object()
        {
            var (sp, init) = Prepare(
                Init(0),
                Init(1),
                Init(2));

            await init.StopAsync(default);

            Assert.NotNull(sp.Initializers[0].DeinitOrder);
            Assert.NotNull(sp.Initializers[1].DeinitOrder);
            Assert.NotNull(sp.Initializers[2].DeinitOrder);
        }

        [Fact]
        public async Task StopAsync_orders_the_objects_in_reverse_order_before_calling_InitializeAsync()
        {
            var (sp, init) = Prepare(
                Init(1),
                Init(2),
                Init(0));

            await init.StopAsync(default);

            Assert.Equal(1, sp.Initializers[0].DeinitOrder);
            Assert.Equal(0, sp.Initializers[1].DeinitOrder);
            Assert.Equal(2, sp.Initializers[2].DeinitOrder);
        }

        [Fact]
        public async Task StartAsync_orders_the_objects_before_calling_InitializeAsync()
        {
            var (sp, init) = Prepare(
                Init(1),
                Init(2),
                Init(0));

            await init.StartAsync(default);

            Assert.Equal(1, sp.Initializers[0].InitOrder);
            Assert.Equal(2, sp.Initializers[1].InitOrder);
            Assert.Equal(0, sp.Initializers[2].InitOrder);
        }

        private static (StubProvider, AsyncInitializer) Prepare(params CountedInitializer[] inits)
        {
            var sp = new StubProvider(inits.ToList());
            return (sp, new AsyncInitializer(inits));
        }

        private CountedInitializer Init(int order)
        {
            return new CountedInitializer(order, globalCounter);
        }
    }
}
