using MediatR;

namespace Catalog.Application.Commands;

public class DeleteProductByIdCommand(string id) : IRequest<bool>, IRequest<Unit>
{
    public string Id { get; set; } = id;
}