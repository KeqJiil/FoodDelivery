using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Ordering;

public sealed record AddOrderLineRequest(
    [Required] Guid MenuId
);