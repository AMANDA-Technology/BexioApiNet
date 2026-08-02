/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace BexioApiNet.Configuration;

/// <summary>
/// Extensions applying caller-supplied configuration to an <see cref="HttpClient" /> that talks to bexio.
/// </summary>
public static class HttpClientExtension
{
    /// <summary>
    /// Header names the library sets itself. A caller-supplied entry for one of these is skipped, because
    /// the header collections append on a duplicate name instead of replacing — two <c>Authorization</c>
    /// values would break authentication rather than fail loudly.
    /// </summary>
    private static readonly HashSet<string> ReservedHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Accept",
        "Host"
    };

    /// <summary>
    /// Characters that are legal in a header name on top of ALPHA and DIGIT (RFC 9110 token).
    /// </summary>
    private const string HeaderNameSpecialCharacters = "!#$%&'*+-.^_`|~";

    /// <summary>
    /// Applies static default request headers to <paramref name="client" />, on top of the headers the
    /// library sets itself. Intended for caller identification and diagnostics — an outbound tag naming the
    /// calling application, environment and build, for example.
    /// </summary>
    /// <remarks>
    /// Semantics:
    /// <list type="bullet">
    /// <item>
    /// Applied once per <see cref="HttpClient" />, onto <see cref="HttpClient.DefaultRequestHeaders" />, so
    /// only static values belong here. A per-request value on a shared client would be a data race.
    /// </item>
    /// <item>
    /// Applied after the library's own headers, removing before adding, so re-applying replaces rather than
    /// appends.
    /// </item>
    /// <item>
    /// Reserved names — <c>Authorization</c>, <c>Accept</c> and <c>Host</c>, compared case-insensitively —
    /// are silently skipped. This is load-bearing: the header collections append on a duplicate name rather
    /// than throwing, so an unguarded caller entry would send two values and break authentication.
    /// </item>
    /// <item>
    /// Never throws. A <c>null</c> or empty <paramref name="headers" /> is a no-op; an entry with a blank
    /// name or value, a name carrying a character that is not a legal header-name token, or a value carrying
    /// a control character is skipped. The control-character check is not cosmetic: the value is added
    /// without validation, and the connection writer then rejects a value containing CR or LF at send time —
    /// turning a diagnostic header into a failed request.
    /// </item>
    /// </list>
    /// </remarks>
    /// <param name="client">Client to configure.</param>
    /// <param name="headers">Static headers to add, or <c>null</c> to add none.</param>
    /// <returns>The same client, to allow chaining.</returns>
    public static HttpClient ApplyDefaultRequestHeaders(this HttpClient client, IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null || headers.Count == 0)
            return client;

        foreach (var (name, value) in headers)
        {
            if (!IsApplicable(name, value))
                continue;

            client.DefaultRequestHeaders.Remove(name);
            client.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        }

        return client;
    }

    /// <summary>
    /// Decides whether a caller-supplied header can be put on the wire safely.
    /// </summary>
    /// <param name="name">Header name.</param>
    /// <param name="value">Header value.</param>
    /// <returns>True when the entry is well-formed and does not collide with a header the library owns.</returns>
    private static bool IsApplicable(string? name, string? value)
        => !string.IsNullOrWhiteSpace(name)
           && !string.IsNullOrWhiteSpace(value)
           && !ReservedHeaderNames.Contains(name)
           && name.All(IsHeaderNameToken)
           && !value.Any(IsControlCharacter);

    /// <summary>
    /// Checks a single character against the RFC 9110 header-name token set.
    /// </summary>
    /// <param name="character">Character to check.</param>
    /// <returns>True when the character may appear in a header name.</returns>
    private static bool IsHeaderNameToken(char character)
        => char.IsAsciiLetterOrDigit(character) || HeaderNameSpecialCharacters.Contains(character);

    /// <summary>
    /// Checks a single character for a control code, CR and LF above all.
    /// </summary>
    /// <param name="character">Character to check.</param>
    /// <returns>True when the character must not appear in a header value.</returns>
    private static bool IsControlCharacter(char character)
        => character < 0x20 || character == 0x7F;
}
