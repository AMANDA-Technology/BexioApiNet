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

namespace BexioApiNet.Abstractions.Enums.Api;

/// <summary>
/// Possible API response header codes. <see href="https://docs.bexio.com/#section/API-basics/HTTP-Headers">https://docs.bexio.com/#section/API-basics/HTTP-Headers</see>
/// </summary>
public static class ResponseCodes
{
    /// <summary>
    /// Request OK
    /// </summary>
    public const int RequestOk = 200;

    /// <summary>
    /// New resource created
    /// </summary>
    public const int NewResourceCreated = 201;

    /// <summary>
    /// The resource has not been changed
    /// </summary>
    public const int ResourceHasNotBeenChanged = 403;

    /// <summary>
    /// The request parameters are invalid
    /// </summary>
    public const int RequestParameterAreInvalid = 40;

    /// <summary>
    /// The bearer token or the provided api key is invalid
    /// </summary>
    public const int BearerTokenIsInvalid = 401;

    /// <summary>
    /// You do not possess the required rights to access this resource
    /// </summary>
    public const int NoRightsForAccessingResource = 403;

    /// <summary>
    /// The resource could not be found / is unknown
    /// </summary>
    public const int ResourceNotFound = 404;

    /// <summary>
    /// Length Required
    /// </summary>
    public const int LengthRequired = 411;

    /// <summary>
    /// The data could not be processed or the accept header is invalid
    /// </summary>
    public const int DataNotProcessedOrInvalidAcceptHeader = 415;

    /// <summary>
    /// Could not save the entity
    /// </summary>
    public const int EntityNotSaved = 422;

    /// <summary>
    /// Too many requests
    /// </summary>
    public const int TooManyRequest = 429;

    /// <summary>
    /// An unexpected condition was encountered
    /// </summary>
    public const int UnexpectedCondition = 500;

    /// <summary>
    /// The server is not available (maintenance work)
    /// </summary>
    public const int ApiMaintenance = 503;
}
