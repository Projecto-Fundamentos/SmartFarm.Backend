namespace SmartFarm.Cattle.Api.Domain.Entities;

public class Lot
{
    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Active";

    private Lot() { }

    public Lot(Guid ranchId, string name)
    {
        if (ranchId == Guid.Empty)
            throw new ArgumentException("Ranch ID is required.");

        if (string.IsNullOrWhiteSpace(name) ||
            name.Trim().Length > 120)
        {
            throw new ArgumentException(
                "Lot name must contain between 1 and 120 characters.");
        }

        Id = Guid.NewGuid();
        RanchId = ranchId;
        Name = name.Trim();
    }
}