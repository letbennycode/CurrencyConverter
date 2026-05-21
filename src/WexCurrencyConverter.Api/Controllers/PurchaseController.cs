using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WexCurrencyConverter.Application.Abstractions.Interfaces;
using WexCurrencyConverter.Application.Models;

namespace WexCurrencyConverter.Api.Controllers;

/// <summary>
/// Handles purchase creation and currency-converted retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ICurrencyConversionService _currencyConversionService;

    /// <inheritdoc />
    public PurchaseController(IPurchaseService purchaseService, ICurrencyConversionService currencyConversionService)
    {
        _purchaseService = purchaseService;
        _currencyConversionService = currencyConversionService;
    }

    /// <summary>
    /// Records a new purchase transaction in USD.
    /// </summary>
    /// <param name="request">The purchase details including description, date, and amount.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created purchase with its assigned ID.</returns>
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

    /// <summary>
    /// Retrieves a purchase by ID and converts the amount to the specified currency.
    /// </summary>
    /// <remarks>
    /// Exchange rates are sourced from the US Treasury Fiscal Data API. The rate used
    /// is the most recent one published within six months of the transaction date.
    /// </remarks>
    /// <param name="id">The unique identifier of the purchase.</param>
    /// <param name="currency">
    /// The target currency label as it appears in the Treasury exchange rate data
    /// (e.g., "Canada-Dollar", "Euro Zone-Euro").
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The purchase with the converted amount and the exchange rate that was applied.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConvertedPurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery, Required] string currency,
        CancellationToken ct)
    {
        var result = await _currencyConversionService.ConvertAsync(id, currency, ct);
        return Ok(result);
    }
}
