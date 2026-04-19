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

namespace BexioApiNet.Abstractions.Models.Sales.Positions;

/// <summary>
/// String literals for the Bexio <c>type</c> discriminator that identifies each concrete
/// <see cref="Position" /> variant on the wire. Kept in one place so the converter and each
/// concrete record share a single source of truth.
/// </summary>
internal static class PositionTypes
{
    /// <summary>Discriminator for <see cref="PositionArticle" />.</summary>
    public const string Article = "KbPositionArticle";

    /// <summary>Discriminator for <see cref="PositionCustom" />.</summary>
    public const string Custom = "KbPositionCustom";

    /// <summary>Discriminator for <see cref="PositionText" />.</summary>
    public const string Text = "KbPositionText";

    /// <summary>Discriminator for <see cref="PositionSubposition" />.</summary>
    public const string Subposition = "KbPositionSubposition";

    /// <summary>Discriminator for <see cref="PositionSubtotal" />.</summary>
    public const string Subtotal = "KbPositionSubtotal";

    /// <summary>Discriminator for <see cref="PositionPagebreak" />.</summary>
    public const string Pagebreak = "KbPositionPagebreak";

    /// <summary>Discriminator for <see cref="PositionDiscount" />.</summary>
    public const string Discount = "KbPositionDiscount";
}
