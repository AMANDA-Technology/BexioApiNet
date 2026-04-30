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

using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
///     Integration tests for <see cref="PermissionService" /> against WireMock stubs. The
///     Bexio v3.0 permissions endpoint is a singleton (no id parameter) returning the access
///     descriptor for the signed-in user. Each test asserts HTTP verb, URL, and deserialization
///     of a fully-populated <c>v3PermissionsResponse</c> payload that mirrors the shape used
///     in the Bexio OpenAPI spec.
/// </summary>
public sealed class PermissionServiceIntegrationTests : IntegrationTestBase
{
    private const string PermissionsPath = "/3.0/permissions";

    private const string PermissionsResponse = """
                                                {
                                                    "components": [
                                                        "functionality1",
                                                        "functionality2"
                                                    ],
                                                    "permissions": {
                                                        "contact": {
                                                            "activation": "enabled",
                                                            "edit": "own",
                                                            "show": "all"
                                                        },
                                                        "kb_invoice": {
                                                            "activation": "enabled",
                                                            "edit": "all",
                                                            "show": "all"
                                                        },
                                                        "admin": {
                                                            "activation": "disabled"
                                                        }
                                                    }
                                                }
                                                """;

    /// <summary>
    ///     <c>Get()</c> issues a <c>GET</c> against <c>/3.0/permissions</c> (no id) and
    ///     deserializes the singleton response into the strongly-typed
    ///     <see cref="BexioApiNet.Abstractions.Models.MasterData.Permissions.Permission" /> /
    ///     <see cref="BexioApiNet.Abstractions.Models.MasterData.Permissions.PermissionAccess" />
    ///     records, including the <c>activation</c>-only permission entries.
    /// </summary>
    [Test]
    public async Task PermissionService_Get_SendsGetRequest_DeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(PermissionsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PermissionsResponse));

        var service = new PermissionService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PermissionsPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Components, Is.EquivalentTo(new[] { "functionality1", "functionality2" }));
            Assert.That(result.Data.Permissions, Is.Not.Null);
            Assert.That(result.Data.Permissions, Has.Count.EqualTo(3));
        });

        Assert.That(result.Data!.Permissions!.ContainsKey("contact"), Is.True);
        var contact = result.Data.Permissions["contact"];
        Assert.Multiple(() =>
        {
            Assert.That(contact.Activation, Is.EqualTo("enabled"));
            Assert.That(contact.Edit, Is.EqualTo("own"));
            Assert.That(contact.Show, Is.EqualTo("all"));
        });

        Assert.That(result.Data.Permissions.ContainsKey("admin"), Is.True);
        var admin = result.Data.Permissions["admin"];
        Assert.Multiple(() =>
        {
            Assert.That(admin.Activation, Is.EqualTo("disabled"));
            Assert.That(admin.Edit, Is.Null);
            Assert.That(admin.Show, Is.Null);
        });
    }
}
