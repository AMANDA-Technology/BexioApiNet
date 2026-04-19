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

namespace BexioApiNet.Services.Connectors.Contacts;

/// <summary>
/// Additional address endpoint configuration. Routes are nested under
/// <c>2.0/contact/{contactId}/additional_address</c>; the parent contact id is
/// supplied per-call by the service so only the version and trailing segment
/// live here.
/// </summary>
public static class AdditionalAddressConfiguration
{
    /// <summary>
    /// Current api version of the endpoint
    /// </summary>
    public const string ApiVersion = "2.0";

    /// <summary>
    /// Parent resource segment (contacts) under which additional addresses are nested.
    /// </summary>
    public const string ParentEndpoint = "contact";

    /// <summary>
    /// The trailing request path segment below the parent contact id.
    /// </summary>
    public const string EndpointSegment = "additional_address";
}
