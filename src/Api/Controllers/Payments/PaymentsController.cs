using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.GetPaymentById;

namespace Api.Controllers.Payments;

[ApiController]
[Route("v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _mediator;

    public PaymentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gets a payment by id.</summary>
    /// <param name="id">Payment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetPayment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
