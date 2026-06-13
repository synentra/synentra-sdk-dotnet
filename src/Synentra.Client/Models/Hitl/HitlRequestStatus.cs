namespace Synentra.Client.Models.Hitl;

/// <summary>
/// The current lifecycle status of a Human-in-the-Loop (HITL) request.
/// </summary>
public enum HitlRequestStatus
{
    /// <summary>The request is awaiting a reviewer decision.</summary>
    Pending,

    /// <summary>The request was approved by a reviewer and replayed upstream.</summary>
    Approved,

    /// <summary>The request was denied by a reviewer.</summary>
    Denied,

    /// <summary>The review window elapsed before a decision was made.</summary>
    Expired,

    /// <summary>The request ID does not exist.</summary>
    NotFound
}
