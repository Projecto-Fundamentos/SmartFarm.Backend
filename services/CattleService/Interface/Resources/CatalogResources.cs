using System.ComponentModel.DataAnnotations;
using SmartFarm.Cattle.Api.Domain.Entities;

namespace SmartFarm.Cattle.Api.Interface.Resources;

public class CreateRanchRequest
{
    [Required]
    [StringLength(120)]
    public string Name { get; init; } = string.Empty;
}

public class CreateCattleRequest
{
    public Guid RanchId { get; init; }

    [Required]
    [StringLength(50)]
    public string EarTag { get; init; } = string.Empty;

    public Guid? LotId { get; init; }
}

public record RanchResponse(Guid Id, string Name, string Status)
{
    public static RanchResponse From(Ranch ranch) => new(ranch.Id, ranch.Name, ranch.Status);
}

public record CattleResponse(Guid Id, Guid RanchId, Guid? LotId, string EarTag, string Status, long Version)
{
    public static CattleResponse From(CattleAnimal animal) => new(
        animal.Id, animal.RanchId, animal.LotId, animal.EarTag, animal.Status, animal.Version);
}
