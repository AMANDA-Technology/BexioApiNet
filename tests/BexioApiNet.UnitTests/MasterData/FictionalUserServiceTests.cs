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

using System.Text.Json;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.MasterData.FictionalUsers;
using BexioApiNet.Abstractions.Models.MasterData.FictionalUsers.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
///     Offline unit tests for <see cref="FictionalUserService" />. Each test asserts that the
///     service forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected verb,
///     path and payload, and returns the handler's <see cref="ApiResult{T}" /> unchanged. Verifies
///     that <see cref="FictionalUserService.Patch" /> routes to <c>PATCH</c> (not <c>PUT</c>) and
///     that <see cref="FictionalUserPatch" /> omits unset properties from serialization. No network,
///     no filesystem.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class FictionalUserServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="FictionalUserService" /> bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new FictionalUserService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "3.0/fictional_users";

    private FictionalUserService _sut = null!;

    private static FictionalUser NewFictionalUser(int id = 1)
    {
        return new FictionalUser(id, "male", "Rudolph", "Smith", "rudolph.smith@bexio.com", null);
    }

    /// <summary>
    ///     <c>Get</c> with no query parameter must hit <c>3.0/fictional_users</c> exactly once with
    ///     a <see langword="null" /> query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<FictionalUser>>
        {
            IsSuccess = true,
            Data = [NewFictionalUser()]
        };
        ConnectionHandler
            .GetAsync<List<FictionalUser>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<FictionalUser>>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     When a <see cref="QueryParameterFictionalUser" /> is supplied, the wrapped dictionary is
    ///     forwarded to the connection handler so pagination reaches the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToHandler()
    {
        var query = new QueryParameterFictionalUser(100, 50);
        ConnectionHandler
            .GetAsync<List<FictionalUser>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<FictionalUser>> { IsSuccess = true, Data = [] }));

        await _sut.Get(query);

        await ConnectionHandler.Received(1).GetAsync<List<FictionalUser>>(
            ExpectedEndpoint,
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>GetById</c> must build the request path with the fictional user id appended to the
    ///     endpoint root.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var expected = new ApiResult<FictionalUser>
        {
            IsSuccess = true,
            Data = NewFictionalUser(id)
        };
        ConnectionHandler
            .GetAsync<FictionalUser>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<FictionalUser>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>Create</c> forwards the payload and the <c>3.0/fictional_users</c> endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsyncWithExpectedPath()
    {
        var payload = new FictionalUserCreate("male", "Rudolph", "Smith", "rudolph.smith@bexio.com");
        var response = new ApiResult<FictionalUser>
        {
            IsSuccess = true,
            Data = NewFictionalUser()
        };
        ConnectionHandler
            .PostAsync<FictionalUser, FictionalUserCreate>(
                Arg.Any<FictionalUserCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).PostAsync<FictionalUser, FictionalUserCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>Patch</c> forwards the patch payload and the <c>3.0/fictional_users/{id}</c> endpoint
    ///     path to <see cref="IBexioConnectionHandler.PatchAsync{TResult,TPatch}" />. This asserts
    ///     <c>PATCH</c> semantics (not <c>PUT</c>) per the Bexio fictional-users contract.
    /// </summary>
    [Test]
    public async Task Patch_CallsPatchAsyncWithIdInPath()
    {
        const int id = 99;
        var payload = new FictionalUserPatch(Firstname: "Updated");
        var response = new ApiResult<FictionalUser>
        {
            IsSuccess = true,
            Data = NewFictionalUser(id)
        };
        ConnectionHandler
            .PatchAsync<FictionalUser, FictionalUserPatch>(
                Arg.Any<FictionalUserPatch>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Patch(id, payload);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).PatchAsync<FictionalUser, FictionalUserPatch>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     <c>Delete</c> forwards the call to <see cref="IBexioConnectionHandler.Delete" /> exactly
    ///     once, building the path with the fictional user id.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDeleteWithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(id);

        Assert.That(result, Is.SameAs(response));
        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).Delete(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Serializing a sparsely-populated <see cref="FictionalUserPatch" /> must omit every unset
    ///     (null) property from the JSON payload so Bexio only updates the fields the caller
    ///     supplied. A default-constructed patch produces the empty JSON object.
    /// </summary>
    [Test]
    public void FictionalUserPatch_SerializationOmitsNullFields()
    {
        var patch = new FictionalUserPatch(Firstname: "Updated");

        var json = JsonSerializer.Serialize(patch);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"firstname\":\"Updated\""));
            Assert.That(json, Does.Not.Contain("salutation_type"));
            Assert.That(json, Does.Not.Contain("lastname"));
            Assert.That(json, Does.Not.Contain("email"));
            Assert.That(json, Does.Not.Contain("title_id"));
        });
    }

    /// <summary>
    ///     A <see cref="FictionalUserPatch" /> with no arguments serializes to an empty object — every
    ///     nullable property is omitted because
    ///     <see cref="System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull" />
    ///     suppresses null output.
    /// </summary>
    [Test]
    public void FictionalUserPatch_EmptyPatch_SerializesToEmptyJsonObject()
    {
        var patch = new FictionalUserPatch();

        var json = JsonSerializer.Serialize(patch);

        Assert.That(json, Is.EqualTo("{}"));
    }
}
