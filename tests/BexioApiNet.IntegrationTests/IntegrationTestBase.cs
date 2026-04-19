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

using System.Net.Http.Headers;
using BexioApiNet.Abstractions.Enums.Api;

namespace BexioApiNet.IntegrationTests;

/// <summary>
/// Base class for offline integration tests that exercise the real
/// <see cref="BexioConnectionHandler"/> against a local <see cref="WireMockServer"/>.
/// The server is started once per fixture and reset between tests so stubs do not
/// leak across test cases.
/// </summary>
[Category("Integration")]
public abstract class IntegrationTestBase
{
    /// <summary>
    /// Fake JWT used for the <c>Authorization</c> header in integration tests.
    /// Does not need to be a valid token — WireMock does not verify it.
    /// </summary>
    protected const string FakeJwtToken = "fake-integration-jwt";

    /// <summary>
    /// WireMock server receiving all HTTP requests from the real connection handler.
    /// </summary>
    protected WireMockServer Server { get; private set; } = null!;

    /// <summary>
    /// HTTP client pointed at <see cref="Server"/> with the Bexio authorization and
    /// accept headers pre-configured. Shared by all tests within a fixture.
    /// </summary>
    protected HttpClient HttpClient { get; private set; } = null!;

    /// <summary>
    /// Real <see cref="BexioConnectionHandler"/> wired to <see cref="HttpClient"/>,
    /// used to verify end-to-end serialization, URL construction and header handling
    /// without hitting the live Bexio API.
    /// </summary>
    protected BexioConnectionHandler ConnectionHandler { get; private set; } = null!;

    /// <summary>
    /// Starts a <see cref="WireMockServer"/> and creates the <see cref="HttpClient"/>
    /// and <see cref="ConnectionHandler"/> pointing at it. Runs once per fixture.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUpServer()
    {
        Server = WireMockServer.Start();

        HttpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(Server.Url! + "/"),
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue(ApiAcceptHeaders.JsonFormatted) },
                Authorization = new AuthenticationHeaderValue("Bearer", FakeJwtToken)
            }
        };

        var configuration = new BexioConfiguration
        {
            BaseUri = Server.Url! + "/",
            JwtToken = FakeJwtToken,
            AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
        };

        ConnectionHandler = new BexioConnectionHandler(HttpClient, configuration);
    }

    /// <summary>
    /// Clears all WireMock stubs and request logs between tests so each test starts
    /// from a clean server state.
    /// </summary>
    [SetUp]
    public void ResetServer()
    {
        Server.Reset();
    }

    /// <summary>
    /// Disposes the connection handler and HTTP client and stops the WireMock server.
    /// Runs once per fixture after all tests complete.
    /// </summary>
    [OneTimeTearDown]
    public void OneTimeTearDownServer()
    {
        ConnectionHandler.Dispose();
        HttpClient.Dispose();
        Server.Stop();
        Server.Dispose();
    }
}
