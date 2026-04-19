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

namespace BexioApiNet.Abstractions.Models.Contacts.ContactRelations;

/// <summary>
/// Contact relation linking two contacts. <see href="https://docs.bexio.com/#tag/Contact-Relations/operation/v2ListContactRelations"/>
/// </summary>
/// <param name="Id">Unique identifier of the contact relation.</param>
/// <param name="ContactId">Identifier of the primary contact.</param>
/// <param name="ContactSubId">Identifier of the related contact.</param>
/// <param name="Description">Free text description of the relation.</param>
/// <param name="UpdatedAt">Timestamp of the last update (read-only, supplied by Bexio).</param>
public sealed record ContactRelation(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("contact_id")] int? ContactId,
    [property: JsonPropertyName("contact_sub_id")] int? ContactSubId,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("updated_at")] string? UpdatedAt
);
