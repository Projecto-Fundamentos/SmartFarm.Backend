using Microsoft.AspNetCore.Mvc;
using SmartFarm.Cattle.Api.Application.Services;
using SmartFarm.Cattle.Api.Interface.Resources;

namespace SmartFarm.Cattle.Api.Interface.Controllers;

[ApiController]
[Route("api/v1/ranches")]
public class RanchesController(CattleCatalogService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<RanchResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RanchResponse>> Create(
        CreateRanchRequest request, CancellationToken cancellationToken)
    {
        var ranch = await service.RegisterRanchAsync(request.Name, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ranch.Id }, RanchResponse.From(ranch));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RanchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RanchResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(RanchResponse.From(await service.GetRanchAsync(id, cancellationToken)));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RanchResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var ranches = await service.ListRanchesAsync(cancellationToken);
        return Ok(ranches.Select(RanchResponse.From).ToArray());
    }
}
