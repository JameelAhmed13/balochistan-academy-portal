namespace BalochiAcademy.Application.Common;

public class PagedResult<T>
{
    public List<T> Items      { get; set; } = [];
    public int     TotalCount { get; set; }
    public int     Page       { get; set; }
    public int     PageSize   { get; set; }
    public int     TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool    HasNext    => Page < TotalPages;
    public bool    HasPrev    => Page > 1;
}

public record PageQuery(int Page = 1, int PageSize = 20, string? Search = null, string? SortBy = null, bool Desc = false);
