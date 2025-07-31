using EntryLog.Data.SqlLegacy.Configs;
using EntryLog.Entities.POCOEntities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EntryLog.Data.SqlLegacy.Contexts
{
    internal class EmployeesDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
