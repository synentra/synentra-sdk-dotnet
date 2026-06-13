namespace Synentra.Client.Models.Policies;

/// <summary>
/// The default effect applied by a policy when no rule matches.
/// </summary>
public enum PolicyType
{
    /// <summary>Allow the request to pass through.</summary>
    Allow,

    /// <summary>Deny the request.</summary>
    Deny,

    /// <summary>Route the request to a human reviewer.</summary>
    Hitl
}
