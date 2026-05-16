namespace Vectra.Client.Models.Hitl;

/// <summary>
/// Represents the full status information for a HITL request, including pending details when available.
/// </summary>
public sealed class HitlStatusResponse
{
    /// <summary>Gets the unique identifier of the HITL request.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the current status of the request.</summary>
    public HitlRequestStatus Status { get; init; }

    /// <summary>
    /// Gets the full pending request details when <see cref="Status"/> is
    /// <see cref="HitlRequestStatus.Pending"/>; otherwise <see langword="null"/>.
    /// </summary>
    public PendingHitlRequest? Request { get; init; }
}
