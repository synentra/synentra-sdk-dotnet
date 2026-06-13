using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vectra.Client.Abstractions;
using Vectra.Client.Configuration;
using Vectra.Client.Http;
using Vectra.Client.Internal;

namespace Vectra.Client.Extensions;

/// <summary>
/// Extension methods for registering the Vectra SDK with the .NET dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Vectra SDK services and configures the HTTP client.
    /// </summary>
    /// <param name="services">The service collection to add the SDK to.</param>
    /// <param name="configure">A delegate that configures <see cref="VectraClientOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddVectraClient(options =>
    /// {
    ///     options.BaseUrl     = "http://localhost:7080";
    ///     options.BearerToken = "your-jwt-token"; // optional
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddVectraClient(
        this IServiceCollection services,
        Action<VectraClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        // Shared delegating handler for Bearer token injection.
        services.AddTransient<BearerTokenHandler>();

        // Register a typed HttpClient for each sub-client, each sharing the same
        // base address and timeout resolved from VectraClientOptions.
        services
            .AddHttpClient<IVectraAgentClient, AgentClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services
            .AddHttpClient<IVectraPolicyClient, PolicyClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services
            .AddHttpClient<IVectraHitlClient, HitlClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services
            .AddHttpClient<IVectraTokenClient, TokenClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddTransient<IVectraClient, VectraClient>();

        return services;
    }

    /// <summary>
    /// Registers the Vectra SDK services using an already-configured
    /// <see cref="VectraClientOptions"/> section.
    /// </summary>
    /// <param name="services">The service collection to add the SDK to.</param>
    /// <param name="options">A pre-built options instance.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddVectraClient(
        this IServiceCollection services,
        VectraClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return services.AddVectraClient(o =>
        {
            o.BaseUrl = options.BaseUrl;
            o.BearerToken = options.BearerToken;
            o.Timeout = options.Timeout;
            o.ThrowOnError = options.ThrowOnError;
        });
    }

    private static void ConfigureClient(IServiceProvider sp, HttpClient client)
    {
        var options = sp.GetRequiredService<IOptions<VectraClientOptions>>().Value;

        var baseUrl = options.BaseUrl.TrimEnd('/') + '/';
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = options.Timeout;
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }
}
