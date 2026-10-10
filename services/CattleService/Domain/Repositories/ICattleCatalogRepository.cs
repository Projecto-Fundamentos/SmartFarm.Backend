using SmartFarm.Cattle.Api.Domain.Entities;

namespace SmartFarm.Cattle.Api.Domain.Repositories;

public interface ICattleCatalogRepository
{
    Task<Ranch?> FindRanchAsync(Guid id, CancellationToken cancellationToken);
    Task<Lot?> FindLotAsync(Guid id, CancellationToken cancellationToken);
    Task<CattleAnimal?> FindCattleAsync(Guid id, Guid ranchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Ranch>> ListRanchesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CattleAnimal>> ListCattleAsync(Guid ranchId, int limit, CancellationToken cancellationToken);
    Task AddRanchAsync(Ranch ranch, CancellationToken cancellationToken);
    Task AddCattleAsync(CattleAnimal animal, CancellationToken cancellationToken);
}
