using Microsoft.AspNetCore.Mvc;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PurchaseResponse>> Create(
    [FromBody] CreatePurchaseRequest request,
    CancellationToken ct)
    {
        var response = await _purchaseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseResponse>> GetById(Guid id, CancellationToken ct)
    {
        var response = await _purchaseService.GetByIdAsync(id, ct);

        return response is null
            ? NotFound()
            : Ok(response);
    }

}