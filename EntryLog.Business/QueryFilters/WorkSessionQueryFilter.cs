namespace EntryLog.Business.QueryFilters
{
    public class WorkSessionQueryFilter : PaginationQuery
    {
        public int? EmployeeId { get; set; }
    }
}
