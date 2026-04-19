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

using BexioApiNet.Abstractions.Models.Sales.Positions;

namespace BexioApiNet.Abstractions.Json;

/// <summary>
///     <see cref="System.Text.Json.Serialization.JsonConverter{T}" /> that maps Bexio's
///     <c>anyOf</c> position payload onto the strongly-typed <see cref="Position" /> hierarchy.
///     Uses the <c>type</c> discriminator (<c>KbPositionArticle</c>, <c>KbPositionCustom</c>,
///     <c>KbPositionText</c>, <c>KbPositionSubposition</c>, <c>KbPositionSubtotal</c>,
///     <c>KbPositionPagebreak</c>, <c>KbPositionDiscount</c>) emitted on every position element.
/// </summary>
public sealed class PositionJsonConverter : DiscriminatedJsonConverter<Position>
{
    /// <inheritdoc />
    protected override string DiscriminatorPropertyName => "type";

    /// <inheritdoc />
    protected override Type? ResolveType(string discriminator) => discriminator switch
    {
        PositionTypes.Article => typeof(PositionArticle),
        PositionTypes.Custom => typeof(PositionCustom),
        PositionTypes.Text => typeof(PositionText),
        PositionTypes.Subposition => typeof(PositionSubposition),
        PositionTypes.Subtotal => typeof(PositionSubtotal),
        PositionTypes.Pagebreak => typeof(PositionPagebreak),
        PositionTypes.Discount => typeof(PositionDiscount),
        _ => null
    };
}
