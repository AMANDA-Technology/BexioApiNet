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

using BexioApiNet.Abstractions.Enums.Api;
using Microsoft.Extensions.DependencyInjection;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Services;
using BexioApiNet.Services.Connectors.Accounting;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.AspNetCore;

/// <summary>
/// Bexio service collection extension for dependency injection
/// </summary>
public static class BexioServiceCollection
{
    /// <summary>
    /// Adds the configuration, handler and rest service to the services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="baseUri"></param>
    /// <param name="jwtToken"></param>
    /// <param name="acceptHeaderFormat"></param>
    /// <returns></returns>
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
    /// Adds the configuration, handler and rest service to the services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="bexioConfiguration"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection AddBexioServices(this IServiceCollection services, IBexioConfiguration bexioConfiguration)
    {
        services.AddSingleton(bexioConfiguration);
        services.AddSingleton<IBexioConnectionHandler, BexioConnectionHandler>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IManualEntryService, ManualEntryService>();
        services.AddScoped<IBexioApiClient, BexioApiClient>();

        return services;
    }
}
