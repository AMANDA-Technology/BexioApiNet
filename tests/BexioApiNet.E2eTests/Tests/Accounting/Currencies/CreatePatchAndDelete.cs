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

using BexioApiNet.Abstractions.Models.Accounting.Currencies.Views;

namespace BexioApiNet.E2eTests.Tests.Accounting.Currencies;

/// <summary>
/// E2E coverage for the full Create → Read → Update → Delete cycle of <c>CurrencyService</c>
/// against the live Bexio API. The test cleans up after itself by deleting the
/// currency it created so it can be re-run safely.
/// </summary>
public class TestCreatePatchAndDelete : BexioE2eTestBase
{
    /// <summary>
    /// Creates a unique throwaway currency code (<c>X??</c>), reads it back via
    /// <c>GetById</c>, patches its round factor and finally deletes it. Each step asserts
    /// a successful response and a schema-compliant payload, and the test guarantees the
    /// created currency is removed via the <c>finally</c> block.
    /// </summary>
    [Test]
    public async Task CreatePatchAndDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var name = $"X{Random.Shared.Next(10, 99)}";

        var created = await BexioApiClient!.Currencies.Create(new CurrencyCreate(name, 0.05));

        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess, Is.True);
            Assert.That(created.ApiError, Is.Null);
            Assert.That(created.Data, Is.Not.Null);
            Assert.That(created.Data!.Id, Is.GreaterThan(0));
            Assert.That(created.Data!.Name, Is.EqualTo(name));
            Assert.That(created.Data!.RoundFactor, Is.EqualTo(0.05));
        });

        try
        {
            var read = await BexioApiClient!.Currencies.GetById(created.Data!.Id);
            Assert.Multiple(() =>
            {
                Assert.That(read.IsSuccess, Is.True);
                Assert.That(read.Data, Is.Not.Null);
                Assert.That(read.Data!.Id, Is.EqualTo(created.Data!.Id));
                Assert.That(read.Data!.Name, Is.EqualTo(name));
            });

            var patched = await BexioApiClient!.Currencies.Patch(created.Data!.Id, new CurrencyPatch(0.10));

            Assert.That(patched, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(patched.IsSuccess, Is.True);
                Assert.That(patched.ApiError, Is.Null);
                Assert.That(patched.Data, Is.Not.Null);
                Assert.That(patched.Data!.Id, Is.EqualTo(created.Data!.Id));
                Assert.That(patched.Data!.RoundFactor, Is.EqualTo(0.10));
            });
        }
        finally
        {
            var deleted = await BexioApiClient!.Currencies.Delete(created.Data!.Id);

            Assert.That(deleted, Is.Not.Null);
            Assert.That(deleted.IsSuccess, Is.True);
        }
    }
}
