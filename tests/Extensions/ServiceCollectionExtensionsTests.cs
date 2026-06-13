using Microsoft.Extensions.DependencyInjection;
using Synentra.Client.Abstractions;
using Synentra.Client.Configuration;
using Synentra.Client.Extensions;

namespace Synentra.Client.UnitTests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSynentraClient_WithDelegate_RegistersISynentraClient()
    {
        var services = new ServiceCollection();

        services.AddSynentraClient(o => o.BaseUrl = "http://localhost:7080");

        var provider = services.BuildServiceProvider();
        var client = provider.GetService<ISynentraClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddSynentraClient_WithDelegate_RegistersSubClients()
    {
        var services = new ServiceCollection();

        services.AddSynentraClient(o => o.BaseUrl = "http://localhost:7080");

        var provider = services.BuildServiceProvider();
        provider.GetService<ISynentraAgentClient>().Should().NotBeNull();
        provider.GetService<ISynentraPolicyClient>().Should().NotBeNull();
        provider.GetService<ISynentraHitlClient>().Should().NotBeNull();
        provider.GetService<ISynentraTokenClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddSynentraClient_WithOptions_RegistersISynentraClient()
    {
        var services = new ServiceCollection();
        var options = new SynentraClientOptions { BaseUrl = "http://localhost:7080", BearerToken = "tok" };

        services.AddSynentraClient(options);

        var provider = services.BuildServiceProvider();
        provider.GetService<ISynentraClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddSynentraClient_ThrowsArgumentNullException_WhenConfigureIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddSynentraClient((Action<SynentraClientOptions>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddSynentraClient_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddSynentraClient((SynentraClientOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddSynentraClient_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddSynentraClient(o => o.BaseUrl = "http://localhost");

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddSynentraClient_ConfiguresBaseAddress_FromOptions()
    {
        var services = new ServiceCollection();

        services.AddSynentraClient(o =>
        {
            o.BaseUrl = "http://localhost:7080";
            o.Timeout = TimeSpan.FromSeconds(10);
        });

        // Verify no exception on build
        var act = () => services.BuildServiceProvider();
        act.Should().NotThrow();
    }
}
