using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartFarm.Cattle.Api.Domain.Entities;
using SmartFarm.Cattle.Api.Domain.Exceptions;
using SmartFarm.Cattle.Api.Domain.Repositories;
using SmartFarm.Cattle.Api.Infrastructure.Persistence;

namespace SmartFarm.Cattle.Api.Infrastructure.Repositories;

public class CattleCatalogRepository(CattleDbContext db) : ICattleCatalogRepository
{
    public Task<Ranch?> FindRanchAsync(Guid id, CancellationToken cancellationToken) =>
        db.Ranches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Lot?> FindLotAsync(Guid id, CancellationToken cancellationToken) =>
        db.Lots.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<CattleAnimal?> FindCattleAsync(Guid id, Guid ranchId, CancellationToken cancellationToken) =>
        db.Cattle.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == id && x.RanchId == ranchId, cancellationToken);

    public async Task<IReadOnlyList<Ranch>> ListRanchesAsync(CancellationToken cancellationToken) =>
        await db.Ranches.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.Id)
            .Take(100).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CattleAnimal>> ListCattleAsync(
        Guid ranchId, int limit, CancellationToken cancellationToken) =>
        await db.Cattle.AsNoTracking().Where(x => x.RanchId == ranchId)
            .OrderBy(x => x.EarTag).ThenBy(x => x.Id).Take(limit)
            .ToListAsync(cancellationToken);

    public async Task AddRanchAsync(Ranch ranch, CancellationToken cancellationToken)
    {
        db.Ranches.Add(ranch);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddCattleAsync(CattleAnimal animal, CancellationToken cancellationToken)
    {
        db.Cattle.Add(animal);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgres &&
            postgres.SqlState == PostgresErrorCodes.UniqueViolation &&
            postgres.ConstraintName == "IX_cattle_ranch_id_ear_tag")
        {
            throw new CatalogConflictException("DUPLICATE_EAR_TAG",
                "An animal with this ear tag already exists in this ranch.");
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgres &&
            postgres.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new CatalogConflictException("REFERENCE_CHANGED",
                "A referenced ranch or lot is no longer available.");
        }
    }
}
