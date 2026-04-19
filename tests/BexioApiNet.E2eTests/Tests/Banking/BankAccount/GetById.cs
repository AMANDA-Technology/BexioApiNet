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

namespace BexioApiNet.E2eTests.Tests.Banking.BankAccount;

/// <summary>
/// Live E2E coverage for <c>GET /3.0/banking/accounts/{id}</c>. The test fetches
/// the first bank account via the list endpoint and then re-fetches it by id,
/// asserting the round-trip returns the same identifier.
/// </summary>
public class TestGetById : BexioE2eTestBase
{
    /// <summary>
    /// Retrieves the first bank account from the list endpoint, then verifies
    /// that <c>GetById</c> returns the same record against the live API.
    /// </summary>
    [Test]
    public async Task GetById()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var list = await BexioApiClient!.BankingBankAccounts.Get();
        Assert.Multiple(() =>
        {
            Assert.That(list, Is.Not.Null);
            Assert.That(list.IsSuccess, Is.True);
            Assert.That(list.Data, Is.Not.Null.And.Not.Empty);
        });

        var firstId = list.Data!.First().Id;
        Assert.That(firstId, Is.Not.Null);

        var res = await BexioApiClient!.BankingBankAccounts.GetById(firstId!.Value);
        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Id, Is.EqualTo(firstId));
        });
    }
}
