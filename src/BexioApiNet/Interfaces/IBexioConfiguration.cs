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

namespace BexioApiNet.Interfaces;

/// <summary>
/// Configuration for accessing bexio API
/// </summary>
public interface IBexioConfiguration
{
    /// <summary>
    /// Base URI for accessing the service. <see href="https://docs.bexio.com/#section/API-basics/API-routes">API-basics/API-routes</see>
    /// </summary>
    public string BaseUri { get; set; }

    /// <summary>
    /// Created JWT Token for accessing the API. <see href="https://docs.bexio.com/#section/Authentication/JWT-(JSON-Web-Tokens)">Authentication/JWT-(JSON-Web-Tokens)</see>
    /// </summary>
    public string JwtToken { get; set; }

    /// <summary>
    /// Requested format for the accept header for response. <see href="https://docs.bexio.com/#section/API-basics/HTTP-Headers">API-basics/HTTP-Headers</see>
    /// </summary>
    public string AcceptHeaderFormat { get; set; }
}
