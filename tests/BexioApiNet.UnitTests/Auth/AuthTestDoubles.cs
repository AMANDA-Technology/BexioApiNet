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
using System.Text;

namespace BexioApiNet.UnitTests.Auth;

/// <summary>
/// A request as observed by <see cref="QueuedResponseHandler" />. The values are copied at send
/// time, because an <see cref="HttpRequestMessage" /> and its content are disposed afterwards.
/// </summary>
/// <param name="Method">HTTP method used.</param>
/// <param name="RequestUri">Absolute request URI.</param>
/// <param name="Authorization">Value of the <c>Authorization</c> header, without the scheme.</param>
/// <param name="Body">Request body, or null when the request had no content.</param>
/// <param name="Headers">All request headers, excluding content headers.</param>
internal sealed record RecordedRequest(HttpMethod Method, Uri? RequestUri, string? Authorization, string? Body,
    IReadOnlyDictionary<string, string[]> Headers);

/// <summary>
/// Test double for <see cref="HttpMessageHandler" /> that records every request and answers with
/// a queued response. The last queued response is repeated once the queue runs dry.
/// </summary>
internal sealed class QueuedResponseHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _responses = new();
    private readonly Lock _gate = new();
    private readonly List<RecordedRequest> _requests = [];
    private Func<HttpResponseMessage> _fallback = () => new HttpResponseMessage(HttpStatusCode.OK);

    /// <summary>
    /// All requests seen so far, in order. Safe to read after the requests under test completed.
    /// </summary>
    public IReadOnlyList<RecordedRequest> Requests => _requests;

    /// <summary>
    /// Answers based on the request rather than a queue. Takes precedence over
    /// <see cref="Enqueue" /> and is the deterministic option for concurrent tests, where the
    /// order requests reach the handler is undefined.
    /// </summary>
    public Func<RecordedRequest, HttpResponseMessage>? Responder { get; init; }

    /// <summary>
    /// Queues a response to be returned for the next request. The last queued response repeats once
    /// the queue is drained.
    /// </summary>
    /// <param name="statusCode">Status code to answer with.</param>
    /// <param name="body">Response body.</param>
    /// <param name="contentType">Content type of the body.</param>
    public QueuedResponseHandler Enqueue(HttpStatusCode statusCode, string body = "", string contentType = "application/json")
    {
        _fallback = () => Create(statusCode, body, contentType);
        _responses.Enqueue(_fallback);
        return this;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var recorded = new RecordedRequest(
            request.Method,
            request.RequestUri,
            request.Headers.Authorization?.Parameter,
            request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken),
            request.Headers.ToDictionary(header => header.Key, header => header.Value.ToArray()));

        // Concurrency tests drive several requests through one handler instance.
        lock (_gate)
        {
            _requests.Add(recorded);

            if (Responder is not null)
                return Responder(recorded);

            return _responses.Count > 0 ? _responses.Dequeue()() : _fallback();
        }
    }

    /// <summary>
    /// Builds a response message with the given body.
    /// </summary>
    private static HttpResponseMessage Create(HttpStatusCode statusCode, string body, string contentType)
        => new(statusCode) { Content = new StringContent(body, Encoding.UTF8, contentType) };
}

/// <summary>
/// A <see cref="TimeProvider" /> whose clock only moves when the test advances it, so token expiry
/// and clock skew can be asserted without waiting.
/// </summary>
/// <param name="utcNow">Initial time.</param>
internal sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    private DateTimeOffset _utcNow = utcNow;

    /// <inheritdoc />
    public override DateTimeOffset GetUtcNow() => _utcNow;

    /// <summary>
    /// Moves the clock forward.
    /// </summary>
    /// <param name="delta">Amount of time to advance.</param>
    public void Advance(TimeSpan delta) => _utcNow = _utcNow.Add(delta);
}

/// <summary>
/// Token provider that hands out a fixed sequence of tokens: invalidating the current one moves to
/// the next, modelling a cache that re-mints on demand. Mirrors the real compare-and-invalidate
/// semantics, so a call naming an already-replaced token is ignored.
/// </summary>
internal sealed class SequencedTokenProvider : IBexioTokenProvider
{
    private readonly Queue<string> _pending;
    private string _current;

    /// <summary>
    /// Number of times a token was actually discarded.
    /// </summary>
    public int InvalidateCount { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequencedTokenProvider" /> class.
    /// </summary>
    /// <param name="tokens">Tokens to hand out, starting with the first.</param>
    public SequencedTokenProvider(params string[] tokens)
    {
        _pending = new Queue<string>(tokens);
        _current = _pending.Dequeue();
    }

    /// <inheritdoc />
    public bool CanRenew => true;

    /// <inheritdoc />
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) => Task.FromResult(_current);

    /// <inheritdoc />
    public void Invalidate(string accessToken)
    {
        if (!string.Equals(_current, accessToken, StringComparison.Ordinal))
            return;

        InvalidateCount++;

        if (_pending.Count > 0)
            _current = _pending.Dequeue();
    }
}
