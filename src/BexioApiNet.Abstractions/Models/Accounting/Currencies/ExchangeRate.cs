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

namespace BexioApiNet.Abstractions.Models.Accounting.Currencies;

/// <summary>
/// Exchange rate for a currency.
/// <see href="https://docs.bexio.com/#tag/Currencies/operation/ListExchangeRatesForCurrency">
/// List Exchange Rates For
/// Currency
/// </see>
/// </summary>
/// <param name="FactorNr">
/// The exchange rate of the currency in comparison with the currency listed in
/// <paramref name="ExchangeCurrency" />.
/// </param>
/// <param name="ExchangeCurrency">The currency that the exchange rate is compared against.</param>
/// <param name="Ratio">The ratio representing how much of the base currency equals one unit of the quote currency.</param>
/// <param name="ExchangeRateToRatio">The exchange rate of the currency multiplied by the ratio.</param>
/// <param name="Source">The source where the exchange rate is fetched from (<c>custom</c>, <c>monthly_average</c>).</param>
/// <param name="SourceReason">
/// The reason why the source of the exchange rate was chosen (<c>monthly_average_provided</c>,
/// <c>monthly_average_unavailable</c>, <c>monthly_average_unreachable</c>, <c>source_custom</c>).
/// </param>
/// <param name="ExchangeRateDate">The validity date of the exchange rate.</param>
public sealed record ExchangeRate(
    [property: JsonPropertyName("factor_nr")]
    decimal FactorNr,
    [property: JsonPropertyName("exchange_currency")]
    Currency ExchangeCurrency,
    [property: JsonPropertyName("ratio")] decimal? Ratio,
    [property: JsonPropertyName("exchange_rate_to_ratio")]
    decimal? ExchangeRateToRatio,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("source_reason")]
    string? SourceReason,
    [property: JsonPropertyName("exchange_rate_date")]
    string? ExchangeRateDate
);
