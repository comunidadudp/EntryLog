using EntryLog.Data.Interfaces;
using EntryLog.Data.SqlLegacy.Contexts;
using EntryLog.Entities.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace EntryLog.Data.SqlLegacy.Repositories
{
    internal class EmployeeRepository(EmployeesDbContext context) : IEmployeeRepository
    {
        private readonly EmployeesDbContext _context = context;

        public async Task<Employee?> GetByCodeAsync(int code)
        {
            return await _context.Employees
                .Where(x => x.Code == code)
                    .Include(e => e.Position)
                    .FirstOrDefaultAsync();
        }
    }
}
