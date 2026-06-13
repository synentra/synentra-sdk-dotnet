using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Synentra.Client.Exceptions;
using Synentra.Client.Models.Common;

namespace Synentra.Client.Internal;

/// <summary>
/// Extension methods for handling <see cref="HttpResponseMessage"/> objects.
/// </summary>
internal static class HttpResponseExtensions
{
    /// <summary>
    /// Reads the response body as <typeparamref name="T"/>, throwing a
    /// <see cref="SynentraApiException"/> on non-success status codes.
    /// </summary>
    internal static async Task<T> ReadAsAsync<T>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<T>(
            SynentraJsonOptions.Default,
            cancellationToken);

        if (result is null)
            throw new SynentraApiException(
                (int)response.StatusCode,
                $"The server returned an empty response for a {typeof(T).Name} request.");

        return result;
    }

    /// <summary>
    /// Ensures the response is successful, throwing appropriate SDK exceptions otherwise.
    /// </summary>
    internal static async Task EnsureSuccessAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
            return;

        var statusCode = (int)response.StatusCode;

        // Attempt to parse a structured error body first.
        SynentraApiError? apiError = null;
        try
        {
            apiError = await response.Content.ReadFromJsonAsync<SynentraApiError>(
                SynentraJsonOptions.Default,
                cancellationToken);
        }
        catch (JsonException) { /* fall through to raw message */ }

        if (statusCode is StatusCodes.Status401Unauthorized
                       or StatusCodes.Status403Forbidden)
        {
            var msg = apiError?.Message
                ?? (statusCode == StatusCodes.Status401Unauthorized
                    ? "Authentication failed. Verify the agent ID and client secret."
                    : "Access denied. The authenticated agent does not have permission to perform this action.");

            throw new SynentraAuthenticationException(statusCode, msg);
        }

        if (apiError is not null)
        {
            var errorWithStatus = new SynentraApiError
            {
                Message = apiError.Message,
                Code = apiError.Code,
                StatusCode = statusCode,
                Details = apiError.Details
            };
            throw new SynentraApiException(errorWithStatus);
        }

        // Fallback: read raw text.
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new SynentraApiException(statusCode, string.IsNullOrWhiteSpace(raw)
            ? $"The server returned HTTP {statusCode}."
            : raw);
    }

    private static class StatusCodes
    {
        internal const int Status401Unauthorized = 401;
        internal const int Status403Forbidden = 403;
    }
}
