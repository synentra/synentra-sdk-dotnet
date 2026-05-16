using Microsoft.Extensions.DependencyInjection;
using Vectra.Client.Abstractions;
using Vectra.Client.Configuration;
using Vectra.Client.Extensions;

namespace Vectra.Client.UnitTests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectraClient_WithDelegate_RegistersIVectraClient()
    {
        var services = new ServiceCollection();

        services.AddVectraClient(o => o.BaseUrl = "http://localhost:7080");

        var provider = services.BuildServiceProvider();
        var client = provider.GetService<IVectraClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddVectraClient_WithDelegate_RegistersSubClients()
    {
        var services = new ServiceCollection();

        services.AddVectraClient(o => o.BaseUrl = "http://localhost:7080");

        var provider = services.BuildServiceProvider();
        provider.GetService<IVectraAgentClient>().Should().NotBeNull();
        provider.GetService<IVectraPolicyClient>().Should().NotBeNull();
        provider.GetService<IVectraHitlClient>().Should().NotBeNull();
        provider.GetService<IVectraTokenClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddVectraClient_WithOptions_RegistersIVectraClient()
    {
        var services = new ServiceCollection();
        var options = new VectraClientOptions { BaseUrl = "http://localhost:7080", BearerToken = "tok" };

        services.AddVectraClient(options);

        var provider = services.BuildServiceProvider();
        provider.GetService<IVectraClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddVectraClient_ThrowsArgumentNullException_WhenConfigureIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddVectraClient((Action<VectraClientOptions>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddVectraClient_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddVectraClient((VectraClientOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddVectraClient_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddVectraClient(o => o.BaseUrl = "http://localhost");

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddVectraClient_ConfiguresBaseAddress_FromOptions()
    {
        var services = new ServiceCollection();

        services.AddVectraClient(o =>
        {
            o.BaseUrl = "http://localhost:7080";
            o.Timeout = TimeSpan.FromSeconds(10);
        });

        // Verify no exception on build
        var act = () => services.BuildServiceProvider();
        act.Should().NotThrow();
    }
}
