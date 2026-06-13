using System.ComponentModel.DataAnnotations;

namespace Synentra.Client.Models.Agents;

/// <summary>
/// The request body used to assign a policy to an existing agent.
/// </summary>
public sealed class AssignPolicyRequest
{
    /// <summary>Gets or sets the name of the policy to assign to the agent.</summary>
    [Required]
    public required string PolicyName { get; set; }
}
