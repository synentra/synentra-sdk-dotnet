namespace Synentra.Client.Models.Policies;

/// <summary>
/// Represents a summary of a policy returned in list responses.
/// </summary>
public sealed class PolicySummary
{
    /// <summary>Gets the name of the policy.</summary>
    public string PolicyName { get; init; } = string.Empty;

    /// <summary>Gets a human-readable description of the policy.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the owner of the policy.</summary>
    public string Owner { get; init; } = string.Empty;
}
