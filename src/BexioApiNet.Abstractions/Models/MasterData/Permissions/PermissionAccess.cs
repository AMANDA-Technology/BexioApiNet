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

namespace BexioApiNet.Abstractions.Models.MasterData.Permissions;

/// <summary>
/// Access descriptor for a single Bexio permission resource. Each property corresponds to an
/// attribute returned by the Bexio <c>/3.0/permissions</c> endpoint. Not every resource returns
/// every attribute — only <see cref="Activation"/> is present for every resource.
/// <see href="https://docs.bexio.com/#tag/Permissions"/>
/// </summary>
public sealed record PermissionAccess
{
    /// <summary>
    /// Activation state of the resource (e.g. <c>enabled</c>, <c>disabled</c>). When this is
    /// <c>disabled</c>, the other attributes have no effect.
    /// </summary>
    [JsonPropertyName("activation")]
    public string? Activation { get; init; }

    /// <summary>
    /// Edit access level (e.g. <c>all</c>, <c>own</c>, <c>none</c>).
    /// </summary>
    [JsonPropertyName("edit")]
    public string? Edit { get; init; }

    /// <summary>
    /// View / show access level (e.g. <c>all</c>, <c>own</c>).
    /// </summary>
    [JsonPropertyName("show")]
    public string? Show { get; init; }
}
