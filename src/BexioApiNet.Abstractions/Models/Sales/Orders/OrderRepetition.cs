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

namespace BexioApiNet.Abstractions.Models.Sales.Orders;

/// <summary>
/// Order repetition payload returned by (and accepted by) the Bexio
/// <c>/2.0/kb_order/{order_id}/repetition</c> endpoints. <see cref="Repetition" /> is the
/// polymorphic <see cref="OrderRepetitionSchedule" /> union deserialized into the concrete subtype
/// identified by its <c>type</c> discriminator.
/// <see href="https://docs.bexio.com/#tag/Orders/operation/v2ShowOrderRepetition" />
/// </summary>
/// <param name="Start">Repetition start date in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="End">
/// Repetition end date in Bexio's <c>yyyy-MM-dd</c> format; <see langword="null" /> for indefinite
/// repetitions.
/// </param>
/// <param name="Repetition">
/// Polymorphic repetition descriptor. The <c>type</c> discriminator (<c>daily</c>, <c>weekly</c>,
/// <c>monthly</c>, <c>yearly</c>) selects the concrete <see cref="OrderRepetitionSchedule" /> subtype.
/// </param>
public sealed record OrderRepetition(
    [property: JsonPropertyName("start")] string? Start,
    [property: JsonPropertyName("end")] string? End,
    [property: JsonPropertyName("repetition")]
    OrderRepetitionSchedule? Repetition
);
