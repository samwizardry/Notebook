namespace RottenRest.Contracts.Responses;

public class PagedResponse<TRespnose>
{
    public required IEnumerable<TRespnose> Items { get; init; } = [];

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int Total { get; init; }

    public bool HasNextPage => Total > (Page * PageSize);
}
