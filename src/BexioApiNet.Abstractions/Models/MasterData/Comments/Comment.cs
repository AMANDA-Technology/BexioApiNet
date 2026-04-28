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

namespace BexioApiNet.Abstractions.Models.MasterData.Comments;

/// <summary>
/// Comment attached to a Bexio document (offer, order, or invoice). The discriminator
/// (<c>kb_document_type</c>) is part of the URL only — it is not present in the response body.
/// <see href="https://docs.bexio.com/#tag/Comments">Comments</see>
/// </summary>
public sealed record Comment
{
    /// <summary>Unique comment identifier (assigned by Bexio).</summary>
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    /// <summary>Free-form comment text.</summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    /// <summary>Identifier of the user who authored the comment. References a user object.</summary>
    [JsonPropertyName("user_id")]
    public required int? UserId { get; init; }

    /// <summary>Email address of the user who authored the comment, if known.</summary>
    [JsonPropertyName("user_email")]
    public string? UserEmail { get; init; }

    /// <summary>Display name of the user who authored the comment.</summary>
    [JsonPropertyName("user_name")]
    public required string? UserName { get; init; }

    /// <summary>Server-assigned creation timestamp formatted as <c>yyyy-MM-dd HH:mm:ss</c>.</summary>
    [JsonPropertyName("date")]
    public string? Date { get; init; }

    /// <summary>Whether the comment is publicly visible (<c>true</c>) or internal-only (<c>false</c>).</summary>
    [JsonPropertyName("is_public")]
    public bool? IsPublic { get; init; }

    /// <summary>Base64-encoded profile image of the comment author, when available.</summary>
    [JsonPropertyName("image")]
    public string? Image { get; init; }

    /// <summary>URL to the profile image of the comment author, when available.</summary>
    [JsonPropertyName("image_path")]
    public string? ImagePath { get; init; }
}
