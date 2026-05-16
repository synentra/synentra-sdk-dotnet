namespace Vectra.Client.Models.Policies;

/// <summary>
/// Represents a single condition within a policy rule.
/// </summary>
public sealed class PolicyRuleCondition
{
    /// <summary>
    /// Gets the field to evaluate (e.g. <c>method</c>, <c>path</c>, <c>user.role</c>).
    /// </summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Gets the comparison operator.
    /// Supported values: <c>eq</c>, <c>ne</c>, <c>in</c>, <c>notIn</c>,
    /// <c>startsWith</c>, <c>endsWith</c>, <c>contains</c>, <c>gt</c>, <c>lt</c>, <c>regex</c>.
    /// </summary>
    public string Operator { get; init; } = string.Empty;

    /// <summary>Gets the value to compare against.</summary>
    public object Value { get; init; } = new();
}
