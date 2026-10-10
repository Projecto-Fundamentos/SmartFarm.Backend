namespace SmartFarm.Cattle.Api.Domain.Entities;

public class CattleAnimal
{
    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public Guid? LotId { get; private set; }
    public string EarTag { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Active";
    public long Version { get; private set; } = 1;

    private CattleAnimal() { }

    public CattleAnimal(Guid ranchId, string earTag, Guid? lotId = null)
    {
        if (ranchId == Guid.Empty)
            throw new ArgumentException("Ranch ID is required.");

        if (lotId == Guid.Empty)
            throw new ArgumentException("Lot ID must be valid.");

        if (string.IsNullOrWhiteSpace(earTag) ||
            earTag.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Ear tag must contain between 1 and 50 characters.");
        }

        Id = Guid.NewGuid();
        RanchId = ranchId;
        LotId = lotId;
        EarTag = earTag.Trim().ToUpperInvariant();
    }
}