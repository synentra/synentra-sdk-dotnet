namespace Synentra.Client.Models.Policies;

/// <summary>
/// Represents a single rule within a <see cref="PolicyDetails"/>.
/// </summary>
public sealed class PolicyRule
{
    /// <summary>Gets the name of the rule.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets an optional human-readable reason describing the rule's intent.</summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Gets the priority of this rule. Higher values take precedence over lower values.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>Gets the effect applied when all conditions match.</summary>
    public PolicyType Effect { get; init; }

    /// <summary>Gets the conditions that must all be satisfied for this rule to apply.</summary>
    public IReadOnlyList<PolicyRuleCondition> Conditions { get; init; } = [];
}
