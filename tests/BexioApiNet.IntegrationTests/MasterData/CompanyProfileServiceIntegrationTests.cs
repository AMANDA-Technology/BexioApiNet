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

using BexioApiNet.Abstractions.Models.MasterData.CompanyProfiles.Enums;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
/// Integration tests covering the read entry points of <see cref="CompanyProfileService" /> against
/// WireMock stubs. The Bexio company profile is read-only via the v2 API; tests verify that the
/// path composed from <see cref="CompanyProfileConfiguration" /> reaches the handler correctly,
/// and that the JSON response deserialises into the
/// <see cref="BexioApiNet.Abstractions.Models.MasterData.CompanyProfiles.CompanyProfile"/>
/// record covering every field documented in the OpenAPI spec — including the
/// <see cref="CompanyLegalForm"/> enum and <c>logo_base64</c> bytes.
/// </summary>
public sealed class CompanyProfileServiceIntegrationTests : IntegrationTestBase
{
    private const string CompanyProfilePath = "/2.0/company_profile";

    private const string CompanyProfileResponse = """
                                                  {
                                                      "id": 1,
                                                      "name": "bexio AG",
                                                      "address": "Alte Jonastrasse 24",
                                                      "address_nr": "",
                                                      "postcode": "8640",
                                                      "city": "Rapperswil",
                                                      "country_id": 1,
                                                      "legal_form": "association",
                                                      "country_name": "Switzerland",
                                                      "mail": "info@bexio.com",
                                                      "phone_fixed": "+41 (0)71 552 00 60",
                                                      "phone_mobile": "+41 (0)79 123 45 67",
                                                      "fax": "",
                                                      "url": "https://www.bexio.com",
                                                      "skype_name": "",
                                                      "facebook_name": "",
                                                      "twitter_name": "",
                                                      "description": "",
                                                      "ust_id_nr": "CHE-322.646.985",
                                                      "mwst_nr": "CHE-322.646.985 MWST",
                                                      "trade_register_nr": "",
                                                      "has_own_logo": true,
                                                      "is_public_profile": false,
                                                      "is_logo_public": false,
                                                      "is_address_public": false,
                                                      "is_phone_public": false,
                                                      "is_mobile_public": false,
                                                      "is_fax_public": false,
                                                      "is_mail_public": false,
                                                      "is_url_public": false,
                                                      "is_skype_public": false,
                                                      "logo_base64": "R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs="
                                                  }
                                                  """;

    /// <summary>
    /// <c>CompanyProfileService.Get</c> must issue a <c>GET</c> request against
    /// <c>/2.0/company_profile</c> and populate every documented field on the resulting
    /// <c>CompanyProfile</c> record from the JSON body.
    /// </summary>
    [Test]
    public async Task CompanyProfileService_Get_SendsGetRequest_AndDeserialisesEveryField()
    {
        Server
            .Given(Request.Create().WithPath(CompanyProfilePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CompanyProfileResponse}]"));

        var service = new CompanyProfileService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CompanyProfilePath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));

            var profile = result.Data![0];
            Assert.That(profile.Id, Is.EqualTo(1));
            Assert.That(profile.Name, Is.EqualTo("bexio AG"));
            Assert.That(profile.Address, Is.EqualTo("Alte Jonastrasse 24"));
            Assert.That(profile.AddressNr, Is.EqualTo(string.Empty));
            Assert.That(profile.Postcode, Is.EqualTo("8640"));
            Assert.That(profile.City, Is.EqualTo("Rapperswil"));
            Assert.That(profile.CountryId, Is.EqualTo(1));
            Assert.That(profile.LegalForm, Is.EqualTo(CompanyLegalForm.association));
            Assert.That(profile.CountryName, Is.EqualTo("Switzerland"));
            Assert.That(profile.Mail, Is.EqualTo("info@bexio.com"));
            Assert.That(profile.PhoneFixed, Is.EqualTo("+41 (0)71 552 00 60"));
            Assert.That(profile.PhoneMobile, Is.EqualTo("+41 (0)79 123 45 67"));
            Assert.That(profile.Fax, Is.EqualTo(string.Empty));
            Assert.That(profile.Url, Is.EqualTo("https://www.bexio.com"));
            Assert.That(profile.SkypeName, Is.EqualTo(string.Empty));
            Assert.That(profile.FacebookName, Is.EqualTo(string.Empty));
            Assert.That(profile.TwitterName, Is.EqualTo(string.Empty));
            Assert.That(profile.Description, Is.EqualTo(string.Empty));
            Assert.That(profile.UstIdNr, Is.EqualTo("CHE-322.646.985"));
            Assert.That(profile.MwstNr, Is.EqualTo("CHE-322.646.985 MWST"));
            Assert.That(profile.TradeRegisterNr, Is.EqualTo(string.Empty));
            Assert.That(profile.HasOwnLogo, Is.True);
            Assert.That(profile.IsPublicProfile, Is.False);
            Assert.That(profile.IsLogoPublic, Is.False);
            Assert.That(profile.IsAddressPublic, Is.False);
            Assert.That(profile.IsPhonePublic, Is.False);
            Assert.That(profile.IsMobilePublic, Is.False);
            Assert.That(profile.IsFaxPublic, Is.False);
            Assert.That(profile.IsMailPublic, Is.False);
            Assert.That(profile.IsUrlPublic, Is.False);
            Assert.That(profile.IsSkypePublic, Is.False);
            Assert.That(profile.LogoBase64, Is.EqualTo("R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs="));
        });
    }

    /// <summary>
    /// <c>CompanyProfileService.GetById</c> must issue a <c>GET</c> request that includes the
    /// target id in the URL path and surface the returned profile on success with all fields
    /// populated.
    /// </summary>
    [Test]
    public async Task CompanyProfileService_GetById_SendsGetRequest_AndDeserialisesEveryField()
    {
        const int id = 1;
        var expectedPath = $"{CompanyProfilePath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CompanyProfileResponse));

        var service = new CompanyProfileService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Name, Is.EqualTo("bexio AG"));
            Assert.That(result.Data.LegalForm, Is.EqualTo(CompanyLegalForm.association));
            Assert.That(result.Data.LogoBase64, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
