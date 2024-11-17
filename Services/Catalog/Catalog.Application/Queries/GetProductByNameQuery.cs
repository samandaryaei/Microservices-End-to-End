using Catalog.Application.Responses;
using Catalog.Core.Entities;
using MediatR;

namespace Catalog.Application.Queries;

public class GetProductByNameQuery(string productName) : IRequest<IList<ProductResponse>>
{
    public string ProductName { get; set; } = productName;
}