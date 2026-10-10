using SmartFarm.Cattle.Api.Domain.Entities;
using SmartFarm.Cattle.Api.Domain.Exceptions;
using SmartFarm.Cattle.Api.Domain.Repositories;

namespace SmartFarm.Cattle.Api.Application.Services;

public class CattleCatalogService(ICattleCatalogRepository repository)
{
    public async Task<Ranch> RegisterRanchAsync(string name, CancellationToken cancellationToken)
    {
        var ranch = new Ranch(name);
        await repository.AddRanchAsync(ranch, cancellationToken);
        return ranch;
    }

    public Task<IReadOnlyList<Ranch>> ListRanchesAsync(CancellationToken cancellationToken) =>
        repository.ListRanchesAsync(cancellationToken);

    public async Task<Ranch> GetRanchAsync(Guid id, CancellationToken cancellationToken)
    {
        RequireId(id, "Ranch");
        return await repository.FindRanchAsync(id, cancellationToken)
            ?? throw new CatalogNotFoundException("Ranch was not found.");
    }

    public async Task<CattleAnimal> RegisterCattleAsync(
        Guid ranchId, string earTag, Guid? lotId, CancellationToken cancellationToken)
    {
        var animal = new CattleAnimal(ranchId, earTag, lotId);
        var ranch = await GetRanchAsync(ranchId, cancellationToken);
        if (ranch.Status != "Active")
            throw new CatalogConflictException("INACTIVE_RANCH", "Ranch must be active.");

        if (lotId.HasValue)
        {
            var lot = await repository.FindLotAsync(lotId.Value, cancellationToken)
                ?? throw new CatalogNotFoundException("Lot was not found.");
            if (lot.RanchId != ranchId)
                throw new ArgumentException("Lot must belong to the same ranch as the animal.");
            if (lot.Status != "Active")
                throw new CatalogConflictException("INACTIVE_LOT", "Lot must be active.");
        }

        await repository.AddCattleAsync(animal, cancellationToken);
        return animal;
    }

    public async Task<CattleAnimal> GetCattleAsync(
        Guid id, Guid ranchId, CancellationToken cancellationToken)
    {
        RequireId(id, "Cattle");
        RequireId(ranchId, "Ranch");
        return await repository.FindCattleAsync(id, ranchId, cancellationToken)
            ?? throw new CatalogNotFoundException("Animal was not found in this ranch.");
    }

    public async Task<IReadOnlyList<CattleAnimal>> ListCattleAsync(
        Guid ranchId, int limit, CancellationToken cancellationToken)
    {
        if (limit < 1 || limit > 100)
            throw new ArgumentException("Limit must be between 1 and 100.");
        await GetRanchAsync(ranchId, cancellationToken);
        return await repository.ListCattleAsync(ranchId, limit, cancellationToken);
    }

    private static void RequireId(Guid id, string resource)
    {
        if (id == Guid.Empty)
            throw new ArgumentException($"{resource} ID is required.");
    }
}
