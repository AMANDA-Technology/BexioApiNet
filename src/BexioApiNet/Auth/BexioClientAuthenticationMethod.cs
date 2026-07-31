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

namespace BexioApiNet.Auth;

/// <summary>
/// How the client authenticates itself at the bexio token endpoint.
/// </summary>
/// <remarks>
/// The bexio realm advertises five methods and the documentation does not state which one a
/// portal-registered app is configured for. Picking the wrong one yields a <c>401</c> with an
/// unhelpful body, so the choice is configurable. Only the two secret-based methods are
/// implemented; <c>private_key_jwt</c>, <c>client_secret_jwt</c> and <c>tls_client_auth</c> are not.
/// </remarks>
public enum BexioClientAuthenticationMethod
{
    /// <summary>
    /// <c>client_secret_post</c> — client id and secret are sent as form fields in the request body.
    /// </summary>
    ClientSecretPost = 0,

    /// <summary>
    /// <c>client_secret_basic</c> — client id and secret are sent in the HTTP <c>Basic</c>
    /// authorization header, per RFC 6749 § 2.3.1.
    /// </summary>
    ClientSecretBasic = 1
}
