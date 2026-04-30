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
using BexioApiNet.Abstractions.Models.Files.Files.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Files;

/// <summary>
///     Live end-to-end tests for the file connector exposed via
///     <see cref="IBexioApiClient.Files" />. Tests are skipped when the required environment
///     variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are not present.
///     The mutating lifecycle (Upload → Get → Patch → Download → Delete) is exercised against
///     a fresh PDF every run so the tenant is left in a clean state.
/// </summary>
[Category("E2E")]
public sealed class FileServiceE2eTests : BexioE2eTestBase
{
    private const string AssetPath = "Assets/letter.pdf";

    /// <summary>
    ///     Lists the first page of files and asserts the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task Get_ReturnsFiles()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Files.Get(new QueryParameterFile(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Searches files via an empty criteria list (Bexio treats this as "match everything")
    ///     and asserts the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task Search_ReturnsFiles()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.Files.Search(new List<SearchCriteria>());

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Verifies the full Upload → GetById → Patch → Download → Delete lifecycle on a single
    ///     PDF asset. The asset is uploaded, fetched back to confirm metadata, renamed via
    ///     <c>Patch</c>, downloaded to confirm content round-trips, and finally deleted to keep
    ///     the tenant clean.
    /// </summary>
    [Test]
    public async Task Lifecycle_UploadGetPatchDownloadDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var uploaded = await BexioApiClient!.Files.Upload(new List<FileInfo> { new(AssetPath) });

        Assert.That(uploaded.IsSuccess, Is.True);
        Assert.That(uploaded.Data, Is.Not.Null);
        Assert.That(uploaded.Data, Has.Count.EqualTo(1));

        var created = uploaded.Data![0];
        Assert.That(created.MimeType, Is.EqualTo("application/pdf"));

        try
        {
            var fetched = await BexioApiClient.Files.GetById(created.Id);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(created.Id));
                Assert.That(fetched.Data!.MimeType, Is.EqualTo("application/pdf"));
            });

            var renamed = $"e2e-letter-{Guid.NewGuid():N}.pdf";
            var patched = await BexioApiClient.Files.Patch(created.Id, new FilePatch(Name: renamed));
            Assert.Multiple(() =>
            {
                Assert.That(patched.IsSuccess, Is.True);
                Assert.That(patched.Data, Is.Not.Null);
                Assert.That(patched.Data!.Name, Is.EqualTo(renamed));
            });

            var downloaded = await BexioApiClient.Files.Download(created.Id);
            Assert.Multiple(() =>
            {
                Assert.That(downloaded.IsSuccess, Is.True);
                Assert.That(downloaded.Data, Is.Not.Null);
                Assert.That(downloaded.Data!.Length, Is.GreaterThan(0));
            });
        }
        finally
        {
            await BexioApiClient.Files.Delete(created.Id);
        }
    }

    /// <summary>
    ///     Verifies that uploading via the <see cref="MemoryStream" /> overload returns the
    ///     same metadata shape as the <see cref="FileInfo" /> overload. Cleans up the uploaded
    ///     file at the end of the test.
    /// </summary>
    [Test]
    public async Task Upload_FromStream_ReturnsFileMetadata()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var bytes = await File.ReadAllBytesAsync(AssetPath);
        var stream = new MemoryStream(bytes);
        var name = $"e2e-stream-{Guid.NewGuid():N}.pdf";

        var uploaded = await BexioApiClient!.Files.Upload(
            new List<Tuple<MemoryStream, string>> { Tuple.Create(stream, name) });

        Assert.That(uploaded.IsSuccess, Is.True);
        Assert.That(uploaded.Data, Is.Not.Null);
        Assert.That(uploaded.Data, Has.Count.EqualTo(1));

        try
        {
            Assert.That(uploaded.Data![0].MimeType, Is.EqualTo("application/pdf"));
        }
        finally
        {
            await BexioApiClient.Files.Delete(uploaded.Data![0].Id);
        }
    }
}
