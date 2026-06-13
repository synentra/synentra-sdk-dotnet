using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Synentra.Client.Abstractions;
using Synentra.Client.Configuration;
using Synentra.Client.Http;
using Synentra.Client.Internal;

namespace Synentra.Client.Extensions;

/// <summary>
/// Extension methods for registering the Synentra SDK with the .NET dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Synentra SDK services and configures the HTTP client.
    /// </summary>
    /// <param name="services">The service collection to add the SDK to.</param>
    /// <param name="configure">A delegate that configures <see cref="SynentraClientOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddSynentraClient(options =>
    /// {
    ///     options.BaseUrl     = "http://localhost:7080";
    ///     options.BearerToken = "your-jwt-token"; // optional
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddSynentraClient(
        this IServiceCollection services,
        Action<SynentraClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        // Shared delegating handler for Bearer token injection.
        services.AddTransient<BearerTokenHandler>();

        // Register a typed HttpClient for each sub-client, each sharing the same
        // base address and timeout resolved from SynentraClientOptions.
        services
            .AddHttpClient<ISynentraAgentClient, AgentClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services
            .AddHttpClient<ISynentraPolicyClient, PolicyClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services
            .AddHttpClient<ISynentraHitlClient, HitlClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services
            .AddHttpClient<ISynentraTokenClient, TokenClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddTransient<ISynentraClient, SynentraClient>();

        return services;
    }

    /// <summary>
    /// Registers the Synentra SDK services using an already-configured
    /// <see cref="SynentraClientOptions"/> section.
    /// </summary>
    /// <param name="services">The service collection to add the SDK to.</param>
    /// <param name="options">A pre-built options instance.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSynentraClient(
        this IServiceCollection services,
        SynentraClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return services.AddSynentraClient(o =>
        {
            o.BaseUrl = options.BaseUrl;
            o.BearerToken = options.BearerToken;
            o.Timeout = options.Timeout;
            o.ThrowOnError = options.ThrowOnError;
        });
    }

    private static void ConfigureClient(IServiceProvider sp, HttpClient client)
    {
        var options = sp.GetRequiredService<IOptions<SynentraClientOptions>>().Value;

        var baseUrl = options.BaseUrl.TrimEnd('/') + '/';
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = options.Timeout;
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }
}
