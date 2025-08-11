using EntryLog.Business.QueryFilters;

namespace EntryLog.Business.Pagination
{
    public class PaginatedResult<TModel> where TModel : class
    {
        public int Count { get; private set; }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int PageCount { get; private set; }
        public int ResultsCount { get; private set; }
        public IEnumerable<TModel>? Results { get; private set; }

        private PaginatedResult(IEnumerable<TModel> results, PaginationQuery query, int count)
        {
            Count = count;
            PageIndex = query.PageIndex ?? 1;
            PageSize = query.PageSize ?? 10;
            PageCount = (int)Math.Ceiling(count / (double)PageSize);
            ResultsCount = results?.Count() ?? 0;
            Results = results;
        }

        public static PaginatedResult<TModel> Create 
            (IEnumerable<TModel> results, PaginationQuery query, int count)
        {
            return new PaginatedResult<TModel>(results, query, count);
        }
    }
}
