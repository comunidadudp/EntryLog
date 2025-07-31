using EntryLog.Data.Interfaces;
using EntryLog.Data.MongoDB.Config;
using EntryLog.Data.MongoDB.Repositories;
using EntryLog.Data.SqlLegacy.Contexts;
using EntryLog.Data.SqlLegacy.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EntryLog.Data
{
    public static class DataDependencies
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            //SQL
            services.AddDbContext<EmployeesDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("EmployeesDB")));

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            //MongoDB
            services.Configure<EntryLogDbOptions>(configuration.GetSection(nameof(EntryLogDbOptions)));

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<EntryLogDbOptions>>().Value;
                var client = new MongoClient(options.ConnectionUri);
                return client.GetDatabase(options.DatabaseName);
            });

            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();

            return services;
        }
    }
}
