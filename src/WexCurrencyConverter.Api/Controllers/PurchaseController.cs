using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ICurrencyConversionService _currencyConversionService;

    public PurchaseController(IPurchaseService purchaseService, ICurrencyConversionService currencyConversionService)
    {
        _purchaseService = purchaseService;
        _currencyConversionService = currencyConversionService;
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
    [ProducesResponseType(typeof(ConvertedPurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetById(  // ← name matches CreatedAtAction
        Guid id,
        [FromQuery, Required] string currency,
        CancellationToken ct)
    {
        var result = await _currencyConversionService.ConvertAsync(id, currency, ct);
        return Ok(result);
    }

}