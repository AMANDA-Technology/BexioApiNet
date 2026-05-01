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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.MasterData.Languages;

/// <summary>
///     Live E2E coverage for the Bexio v2 <c>/language</c> endpoint. The endpoint is read-only
///     (no Create/Update/Delete in the OpenAPI spec) so the lifecycle is List + Search only.
/// </summary>
public sealed class TestLanguages : BexioE2eTestBase
{
    /// <summary>
    ///     Lists languages with a small page and asserts that the schema-required fields
    ///     (<c>name</c>, <c>iso_639_1</c>) are populated for at least one entry.
    /// </summary>
    [Test]
    public async Task List_ReturnsLanguagesWithRequiredFields()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Languages.Get(new QueryParameterLanguage(10, 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        Assert.That(result.Data, Is.Not.Empty);
        var first = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(first.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(first.Iso6391, Is.Not.Null.And.Not.Empty);
        });
    }

    /// <summary>
    ///     Searches languages by ISO 639-1 code. The Bexio API supports <c>name</c> and
    ///     <c>iso_639_1</c> as search fields; this exercise filters on the latter and
    ///     expects the result to either be empty (no language present in the tenant) or
    ///     to contain only entries matching the requested ISO code.
    /// </summary>
    [Test]
    public async Task Search_ByIso6391_FiltersResults()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Languages.Search(
        [
            new SearchCriteria { Field = "iso_639_1", Value = "de", Criteria = "=" }
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        foreach (var language in result.Data!)
            Assert.That(language.Iso6391, Is.EqualTo("de"));
    }
}
