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
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Web;
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Interfaces;
using BexioApiNet.Models;

namespace BexioApiNet.Services;

/// <inheritdoc />
public sealed class BexioConnectionHandler : IBexioConnectionHandler
{
    /// <summary>
    /// Holds the http client with some basic settings, to be used for all connectors
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioConnectionHandler"/> class.
    /// </summary>
    /// <param name="configuration"></param>
    public BexioConnectionHandler(IBexioConfiguration configuration)
    {
        if (!configuration.BaseUri.EndsWith("/"))
            configuration.BaseUri += "/";

        _client = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new(configuration.BaseUri),
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue(configuration.AcceptHeaderFormat) },
                Authorization = new("Bearer", configuration.JwtToken)
            }
        };
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> GetAsync<TResult>(string requestPath, [Optional] QueryParameter queryParameter, [Optional] CancellationToken cancellationToken)
    {
        return await GetApiResult<TResult>(await _client.SendAsync(CreateHttpRequestMessage(HttpMethod.Get, requestPath, queryParameter), cancellationToken));
    }
    /// <inheritdoc />
    public async Task<ApiResult<TResult>> PostAsync<TResult, TCreate>(TCreate payload, string requestPath, [Optional] CancellationToken cancellationToken)
    {
        return await GetApiResult<TResult>(await _client.SendAsync(CreateHttpRequestMessageWithBody(HttpMethod.Post, requestPath, payload), cancellationToken));
    }

    /// <inheritdoc />
    public async Task<List<TResult>> FetchAll<TResult>(int fetchedObjects, int maxObjects, string requestPath, QueryParameter queryParameter, [Optional] CancellationToken cancellationToken)
    {
        var res = new List<TResult>();
        var initialOffset = (int)queryParameter.Parameters["offset"];
        maxObjects -= initialOffset;

        while (fetchedObjects < maxObjects) //TODO: Possible bug when initial offset from user is != 0
        {
            var res2 = await GetAsync<List<TResult>>(requestPath, queryParameter, cancellationToken);
            if (res2.Data == null || !res2.IsSuccess) throw new("Paging failed");

           res.AddRange(res2.Data);
           fetchedObjects += res2.Data.Count;

           queryParameter.Parameters["offset"] = fetchedObjects + initialOffset;
        }

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> PostMultiPartFileAsync<TResult>(List<FileInfo> files, string requestPath, [Optional] CancellationToken cancellationToken)
    {
        var form = new MultipartFormDataContent();

        foreach (var file in files)
        {
            form.Add(new ByteArrayContent(await File.ReadAllBytesAsync(file.FullName, cancellationToken)), file.Name, file.Name);
        }

        return await GetApiResult<TResult>(await _client.SendAsync(CreateHttpRequestMessageWithContent(HttpMethod.Post, requestPath, form), cancellationToken));
    }

    /// <summary>
    /// Create http request message to send to the API (without body)
    /// </summary>
    /// <param name="httpMethod">Http method to use</param>
    /// <param name="requestPath">Request path to use</param>
    /// <param name="queryParameter">Query parameter to append</param>
    /// <returns></returns>
    private HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, string requestPath, [Optional] QueryParameter? queryParameter)
    {
        var httpRequestMessage = new HttpRequestMessage { Method = httpMethod };

        var uriBuilder = new UriBuilder(new Uri(_client.BaseAddress!, requestPath));
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        if(queryParameter is not null)
            foreach (var (key, value) in queryParameter.Parameters)
                query[key] = value.ToString();

        uriBuilder.Query = query.ToString();
        httpRequestMessage.RequestUri = uriBuilder.Uri;

        return httpRequestMessage;
    }

    /// <summary>
    /// Create http request message to send to the API with body, normally a serialized create view
    /// </summary>
    /// <param name="httpMethod">Http method to use</param>
    /// <param name="requestPath">Request path to use</param>
    /// <param name="payload">The payload as object to serialize</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private HttpRequestMessage CreateHttpRequestMessageWithBody<T>(HttpMethod httpMethod, string requestPath, T? payload)
    {
        var httpRequestMessage = CreateHttpRequestMessage(httpMethod, requestPath);

        httpRequestMessage.Content = payload is null
            ? null
            : new StringContent(JsonSerializer.Serialize(payload), new MediaTypeHeaderValue("application/json"));

        return httpRequestMessage;
    }

    /// <summary>
    /// Create http request message to send to the API with content (non serialized), normally a binary file
    /// </summary>
    /// <param name="httpMethod">Http method to use</param>
    /// <param name="requestPath">Request path to use</param>
    /// <param name="payload">The payload as HttpContent</param>
    /// <returns></returns>
    private HttpRequestMessage CreateHttpRequestMessageWithContent(HttpMethod httpMethod, string requestPath, HttpContent payload)
    {
        var httpRequestMessage = CreateHttpRequestMessage(httpMethod, requestPath);
        httpRequestMessage.Content = payload;

        return httpRequestMessage;
    }

    /// <summary>
    /// Get API result from response
    /// </summary>
    /// <param name="httpResponseMessage">Received api response to process</param>
    /// <typeparam name="T">Type to deserialized to if request was successfully</typeparam>
    /// <returns></returns>
    private static async Task<ApiResult<T>> GetApiResult<T>(HttpResponseMessage httpResponseMessage)
    {
        var isSuccess = httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode is HttpStatusCode.Found;
        var headers = GetResponseHeaders(httpResponseMessage);
        var content = await httpResponseMessage.Content.ReadAsStringAsync();

        return new()
        {
            IsSuccess = isSuccess,
            ApiError = isSuccess ? null : JsonSerializer.Deserialize<ApiError>(content),
            Data = httpResponseMessage.IsSuccessStatusCode ? JsonSerializer.Deserialize<T>(content) : default,
            ResponseHeaders = headers,
            StatusCode = httpResponseMessage.StatusCode
        };
    }

    /// <summary>
    /// Get API headers from response
    /// </summary>
    /// <param name="httpResponseMessage">Received api response to process</param>
    /// <returns></returns>
    private static Dictionary<string, int?> GetResponseHeaders(HttpResponseMessage httpResponseMessage)
    {
        return new()
        {
            [ApiHeaderNames.RequestLimit] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RequestLimit, out var values) && int.TryParse(values.First(), out var requestLimit)
                ? requestLimit : 0,

            [ApiHeaderNames.AppliedOffset] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.AppliedOffset, out values) && int.TryParse(values.First(), out var appliedOffset)
                ? appliedOffset : 0,

            [ApiHeaderNames.TotalResults] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.TotalResults, out values) && int.TryParse(values.First(), out var totalResults)
                ? totalResults : 0,
        };
    }
}
