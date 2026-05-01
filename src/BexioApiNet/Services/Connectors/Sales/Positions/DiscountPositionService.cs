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

/// <inheritdoc cref="IDiscountPositionService" />
public sealed class DiscountPositionService : PositionService, IDiscountPositionService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscountPositionService" /> class.
    /// </summary>
    /// <param name="bexioConnectionHandler">The Bexio connection handler.</param>
    public DiscountPositionService(IBexioConnectionHandler bexioConnectionHandler)
        : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    protected override string PositionType => "kb_position_discount";

    /// <inheritdoc />
    public Task<ApiResult<List<PositionDiscount>>> GetAll(string documentType, int documentId,
        [Optional] CancellationToken cancellationToken)
    {
        return GetAllPositionsAsync<PositionDiscount>(documentType, documentId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<PositionDiscount>> GetById(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken)
    {
        return GetPositionAsync<PositionDiscount>(documentType, documentId, positionId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<PositionDiscount>> Create(string documentType, int documentId, PositionDiscountCreate position,
        [Optional] CancellationToken cancellationToken)
    {
        return CreatePositionAsync<PositionDiscount, PositionDiscountCreate>(documentType, documentId, position,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<PositionDiscount>> Update(string documentType, int documentId, int positionId,
        PositionDiscountCreate position, [Optional] CancellationToken cancellationToken)
    {
        return UpdatePositionAsync<PositionDiscount, PositionDiscountCreate>(documentType, documentId, positionId, position,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResult<object>> Delete(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken)
    {
        return DeletePositionAsync(documentType, documentId, positionId, cancellationToken);
    }
}
