using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vectra.Client.Internal;

/// <summary>
/// Shared <see cref="JsonSerializerOptions"/> used across all SDK HTTP operations.
/// </summary>
internal static class VectraJsonOptions
{
    internal static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
