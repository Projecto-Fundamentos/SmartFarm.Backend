namespace SmartFarm.Cattle.Api.Domain.Entities;

public class Ranch
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Active";

    private Ranch() { }

    public Ranch(string name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            name.Trim().Length > 120)
        {
            throw new ArgumentException(
                "Ranch name must contain between 1 and 120 characters.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }
}