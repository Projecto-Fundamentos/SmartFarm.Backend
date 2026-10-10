namespace SmartFarm.Cattle.Api.Domain.Exceptions;

public class CatalogNotFoundException(string message) : Exception(message)
{
}

public class CatalogConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
