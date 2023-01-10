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
using BexioApiNet.Abstractions.Enums.Api;

namespace BexioApiNet.Abstractions.Models.Api;

/// <summary>
/// An API result with meta information received from bexio
/// </summary>
public record ApiResult
{
    /// <summary>
    /// Indicates if the API call was successfully or not
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Error with details if any.
    /// <see href="https://docs.bexio.com/#section/API-basics/Errors">Errors</see>
    /// </summary>
    public ApiError? ApiError { get; init; }

    /// <summary>
    /// Dictionary with extracted response headers. <see cref="ApiHeaderNames"/>
    /// </summary>
    public Dictionary<string, int?>? ResponseHeaders { get; set; }

    /// <summary>
    /// HTTP status code received from api <see href="https://docs.bexio.com/#section/API-basics/HTTP-Headers"/>
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
}

/// <summary>
/// An API result with meta information and data received from bexio
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record ApiResult<T> : ApiResult
{
    /// <summary>
    /// Data received from the API. Check <see cref="ApiResult.IsSuccess"/> and <see cref="ApiResult.ApiError"/>, especially when Data is null.
    /// </summary>
    public T? Data { get; init; }
}
