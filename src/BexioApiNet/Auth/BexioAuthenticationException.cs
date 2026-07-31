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

using System.Net;

namespace BexioApiNet.Auth;

/// <summary>
/// Raised when an access token cannot be obtained from the bexio identity provider.
/// </summary>
/// <remarks>
/// Connector methods never throw for non-2xx bexio API responses — they return an
/// <c>ApiResult</c>. This exception is different: it reports a failure of the token endpoint,
/// which happens before any API request exists and therefore has no <c>ApiResult</c> to carry it.
/// </remarks>
public sealed class BexioAuthenticationException : ApplicationException
{
    /// <summary>
    /// OAuth <c>error</c> code returned by the token endpoint, e.g. <c>invalid_grant</c> or
    /// <c>unauthorized_client</c>. Null when the failure was not an OAuth error response.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// OAuth <c>error_description</c> returned by the token endpoint, when present.
    /// </summary>
    public string? ErrorDescription { get; }

    /// <summary>
    /// Status code returned by the token endpoint. Null when no response was received.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioAuthenticationException" /> class.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    public BexioAuthenticationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioAuthenticationException" /> class for a
    /// failed token endpoint response.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    /// <param name="statusCode">Status code returned by the token endpoint.</param>
    /// <param name="error">OAuth <c>error</c> code.</param>
    /// <param name="errorDescription">OAuth <c>error_description</c>.</param>
    public BexioAuthenticationException(string message, HttpStatusCode statusCode, string? error, string? errorDescription)
        : base(message)
    {
        StatusCode = statusCode;
        Error = error;
        ErrorDescription = errorDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioAuthenticationException" /> class.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    /// <param name="inner">Underlying exception.</param>
    public BexioAuthenticationException(string message, Exception inner) : base(message, inner)
    {
    }
}
