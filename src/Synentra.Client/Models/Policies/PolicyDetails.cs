namespace Synentra.Client.Models.Policies;

/// <summary>
/// Represents the full details of a policy, including its rules.
/// </summary>
public sealed class PolicyDetails
{
    /// <summary>Gets the name of the policy.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the optional human-readable description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the owner of the policy.</summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>Gets when the policy was created.</summary>
    public DateTime? CreatedOn { get; init; }

    /// <summary>
    /// Gets the default effect applied when no rule in <see cref="Rules"/> matches.
    /// </summary>
    public PolicyType Default { get; init; }

    /// <summary>Gets the ordered list of rules evaluated against each request.</summary>
    public IReadOnlyList<PolicyRule> Rules { get; init; } = [];
}
