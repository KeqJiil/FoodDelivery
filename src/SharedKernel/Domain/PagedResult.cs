namespace SharedKernel.Domain;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, bool HasNextPage, bool HasPreviousPage);