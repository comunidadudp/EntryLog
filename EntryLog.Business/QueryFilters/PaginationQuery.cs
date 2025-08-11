using EntryLog.Business.Enums;

namespace EntryLog.Business.QueryFilters
{
    public abstract class PaginationQuery
    {
        public SortType? Sort { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
