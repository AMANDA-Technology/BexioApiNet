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

using System.Runtime.InteropServices;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Positions;
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Sales.Positions;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Sales.Positions;

/// <inheritdoc cref="ISubtotalPositionService" />
public sealed class SubtotalPositionService : PositionService, ISubtotalPositionService
{
    /// <inheritdoc />
    protected override string PositionType => "kb_position_subtotal";

    /// <summary>
    ///     Initializes a new instance of the <see cref="SubtotalPositionService" /> class.
    /// </summary>
    /// <param name="bexioConnectionHandler">The Bexio connection handler.</param>
    public SubtotalPositionService(IBexioConnectionHandler bexioConnectionHandler)
        : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public Task<ApiResult<List<PositionSubtotal>>> GetAll(string documentType, int documentId,
        [Optional] CancellationToken cancellationToken)
        => GetAllPositionsAsync<PositionSubtotal>(documentType, documentId, cancellationToken);

    /// <inheritdoc />
    public Task<ApiResult<PositionSubtotal>> GetById(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken)
        => GetPositionAsync<PositionSubtotal>(documentType, documentId, positionId, cancellationToken);

    /// <inheritdoc />
    public Task<ApiResult<PositionSubtotal>> Create(string documentType, int documentId,
        PositionSubtotalCreate payload, [Optional] CancellationToken cancellationToken)
        => CreatePositionAsync<PositionSubtotal, PositionSubtotalCreate>(documentType, documentId, payload,
            cancellationToken);

    /// <inheritdoc />
    public Task<ApiResult<PositionSubtotal>> Update(string documentType, int documentId, int positionId,
        PositionSubtotalCreate payload, [Optional] CancellationToken cancellationToken)
        => UpdatePositionAsync<PositionSubtotal, PositionSubtotalCreate>(documentType, documentId, positionId,
            payload, cancellationToken);

    /// <inheritdoc />
    public Task<ApiResult<object>> Delete(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken)
        => DeletePositionAsync(documentType, documentId, positionId, cancellationToken);
}
