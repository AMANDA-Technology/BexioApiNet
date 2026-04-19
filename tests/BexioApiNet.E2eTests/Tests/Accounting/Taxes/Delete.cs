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

namespace BexioApiNet.E2eTests.Tests.Accounting.Taxes;

/// <summary>
/// Live end-to-end coverage of <c>DELETE /3.0/taxes/{id}</c>. The Bexio API
/// rejects deletion of taxes that are referenced or assigned to digit 000
/// with a 409 Conflict — see <see href="https://docs.bexio.com/#tag/Taxes/operation/DeleteTax"/>.
/// This stub probes the call against a clearly non-existent id so it does not
/// destroy production data; both 404 and 409 are acceptable terminal states.
/// </summary>
public class TestDelete : BexioE2eTestBase
{
    /// <summary>
    /// Calls <c>Delete</c> with a deliberately invalid id. Asserts the call
    /// produced a populated <see cref="ApiResult{T}"/> — either success (if the
    /// id by coincidence matched a deletable tax) or a non-success with a
    /// populated <see cref="ApiError"/>.
    /// </summary>
    [Test]
    public async Task DeleteNonExistentReturnsApiError()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        const int nonExistentId = int.MaxValue;
        var res = await BexioApiClient!.Taxes.Delete(nonExistentId);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.ApiError, Is.Not.Null);
    }
}
