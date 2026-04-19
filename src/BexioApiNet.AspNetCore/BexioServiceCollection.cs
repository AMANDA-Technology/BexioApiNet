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
using Microsoft.Extensions.DependencyInjection;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Services;
using BexioApiNet.Services.Connectors.Accounting;
using BexioApiNet.Services.Connectors.Banking;
using BexioApiNet.Services.Connectors.Contacts;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.AspNetCore;

/// <summary>
/// Bexio service collection extension for dependency injection.
/// </summary>
public static class BexioServiceCollection
{
    /// <summary>
    /// Adds the configuration, handler and rest service to the services.
    /// </summary>
    /// <param name="services">Service collection to register Bexio services into.</param>
    /// <param name="baseUri">Bexio API base URI (see <see href="https://docs.bexio.com/#section/API-basics/API-routes"/>).</param>
    /// <param name="jwtToken">JWT token used for authentication (see <see href="https://docs.bexio.com/#section/Authentication/JWT-(JSON-Web-Tokens)"/>).</param>
    /// <param name="acceptHeaderFormat">Accept header format. Defaults to <see cref="ApiAcceptHeaders.JsonFormatted"/>.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddBexioServices(this IServiceCollection services, string baseUri, string jwtToken, string acceptHeaderFormat = ApiAcceptHeaders.JsonFormatted)
    {
        return services.AddBexioServices(new BexioConfiguration
        {
            BaseUri = baseUri,
            JwtToken = jwtToken,
            AcceptHeaderFormat = acceptHeaderFormat
        });
    }

    /// <summary>
    /// Adds the configuration, handler and rest service to the services. The <see cref="IBexioConnectionHandler"/>
    /// is registered as a typed <see cref="HttpClient"/> backed by <see cref="IHttpClientFactory"/> to avoid
    /// socket exhaustion under load.
    /// </summary>
    /// <param name="services">Service collection to register Bexio services into.</param>
    /// <param name="bexioConfiguration">Bexio configuration (base URI, JWT token, accept header format).</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection AddBexioServices(this IServiceCollection services, IBexioConfiguration bexioConfiguration)
    {
        services.AddSingleton(bexioConfiguration);

        services.AddHttpClient<IBexioConnectionHandler, BexioConnectionHandler>((provider, client) =>
            {
                var configuration = provider.GetRequiredService<IBexioConfiguration>();

                if (!configuration.BaseUri.EndsWith('/'))
                    configuration.BaseUri += '/';

                client.BaseAddress = new Uri(configuration.BaseUri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(configuration.AcceptHeaderFormat));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.JwtToken);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IManualEntryService, ManualEntryService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IAccountGroupService, AccountGroupService>();
        services.AddScoped<IBusinessYearService, BusinessYearService>();
        services.AddScoped<ICalendarYearService, CalendarYearService>();
        services.AddScoped<IVatPeriodService, VatPeriodService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IPaymentTypeService, PaymentTypeService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOutgoingPaymentService, OutgoingPaymentService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IContactGroupService, ContactGroupService>();
        services.AddScoped<IContactRelationService, ContactRelationService>();
        services.AddScoped<IContactSectorService, ContactSectorService>();
        services.AddScoped<IAdditionalAddressService, AdditionalAddressService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IInvoiceReminderService, InvoiceReminderService>();
        services.AddScoped<IBexioApiClient, BexioApiClient>();

        return services;
    }
}
