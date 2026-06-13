using System.ComponentModel.DataAnnotations;

namespace Synentra.Client.Configuration;

/// <summary>
/// Configuration options for the Synentra SDK HTTP client.
/// </summary>
/// <remarks>
/// Register via <c>services.AddSynentraClient(options => { ... })</c>.
/// </remarks>
public sealed class SynentraClientOptions
{
    /// <summary>
    /// Gets or sets the base URL of the Synentra gateway (e.g. <c>http://localhost:7080</c>).
    /// </summary>
    /// <remarks>
    /// A trailing slash is automatically normalised. This property is required.
    /// </remarks>
    [Required]
    public required string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets an optional static Bearer token to attach to every outgoing request.
    /// </summary>
    /// <remarks>
    /// When set, the SDK injects this value as the <c>Authorization: Bearer &lt;token&gt;</c>
    /// header automatically. Leave <see langword="null"/> to manage authentication manually.
    /// </remarks>
    public string? BearerToken { get; set; }

    /// <summary>
    /// Gets or sets the request timeout. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to throw a <see cref="Synentra.Client.Exceptions.SynentraApiException"/>
    /// automatically when the server returns a non-success HTTP status code.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool ThrowOnError { get; set; } = true;
}
