using MediatR;
using TattooShop.Api.Models;
using TattooShop.Api.Repositories;

namespace TattooShop.Api.Features;

public record SaveTattooDesignCommand(TattooDesign Design) : IRequest<TattooDesign>;

public class SaveTattooDesignHandler(ITattooDesignRepository repository) : IRequestHandler<SaveTattooDesignCommand, TattooDesign>
{
    private readonly ITattooDesignRepository _repository = repository;

    public async Task<TattooDesign> Handle(SaveTattooDesignCommand request, CancellationToken cancellationToken)
    {
        await _repository.AddDesignAsync(request.Design);
        return request.Design;
    }
}