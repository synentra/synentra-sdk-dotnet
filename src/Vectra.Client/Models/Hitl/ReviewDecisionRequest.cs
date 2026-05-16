namespace Vectra.Client.Models.Hitl;

/// <summary>
/// The request body sent when approving or denying a HITL review.
/// </summary>
public sealed class ReviewDecisionRequest
{
    /// <summary>Gets or sets an optional comment from the reviewer explaining the decision.</summary>
    public string? Comment { get; set; }
}
