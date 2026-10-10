using Microsoft.AspNetCore.Mvc;
using SmartFarm.Cattle.Api.Application.Services;
using SmartFarm.Cattle.Api.Interface.Resources;

namespace SmartFarm.Cattle.Api.Interface.Controllers;

[ApiController]
[Route("api/v1/cattle")]
public class CattleController(CattleCatalogService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CattleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CattleResponse>> Create(
        CreateCattleRequest request, CancellationToken cancellationToken)
    {
        var animal = await service.RegisterCattleAsync(
            request.RanchId, request.EarTag, request.LotId, cancellationToken);
        return CreatedAtAction(nameof(GetById),
            new { id = animal.Id, ranchId = animal.RanchId }, CattleResponse.From(animal));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CattleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CattleResponse>> GetById(
        Guid id, [FromQuery] Guid ranchId, CancellationToken cancellationToken) =>
        Ok(CattleResponse.From(await service.GetCattleAsync(id, ranchId, cancellationToken)));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CattleResponse>>> GetAll(
        [FromQuery] Guid ranchId, CancellationToken cancellationToken, [FromQuery] int limit = 100)
    {
        var animals = await service.ListCattleAsync(ranchId, limit, cancellationToken);
        return Ok(animals.Select(CattleResponse.From).ToArray());
    }
}
