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

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetPayment([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }
}
