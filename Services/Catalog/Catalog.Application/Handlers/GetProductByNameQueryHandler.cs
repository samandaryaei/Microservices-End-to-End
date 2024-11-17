using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetProductByNameQueryHandler(IProductRepository productRepository) 
    : IRequestHandler<GetProductByNameQuery, IList<ProductResponse>>
{
    public async Task<IList<ProductResponse>> Handle(GetProductByNameQuery request, CancellationToken cancellationToken)
    {
        var productsList = await productRepository.GetProductByName(request.ProductName);
        var productResponseList = ProductMapper.Mapper.Map<IList<ProductResponse>>(productsList);
        
        return productResponseList;
    }
}