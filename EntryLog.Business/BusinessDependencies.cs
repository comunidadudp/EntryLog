using EntryLog.Business.Constants;
using EntryLog.Business.Cryptography;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mailtrap;
using EntryLog.Business.Mailtrap.Models;
using EntryLog.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace EntryLog.Business
{
    public static class BusinessDependencies
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            //configuraciones (options)
            services.Configure<Argon2PasswordHashOptions>(configuration.GetSection(nameof(Argon2PasswordHashOptions)));
            services.Configure<EncryptionKeyValues>(configuration.GetSection(nameof(EncryptionKeyValues)));
            services.Configure<MailtrapApiOptions>(configuration.GetSection(nameof(MailtrapApiOptions)));

            // HttpClientFactory
            services.AddHttpClient(ApiNames.MailtrapIO, (sp, client) =>
            {
                MailtrapApiOptions options = sp.GetRequiredService<IOptions<MailtrapApiOptions>>().Value;

                client.BaseAddress = new Uri(options.ApiUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
            });

            // servicios de infraestructura
            services.AddScoped<IEmailSenderService, MailtrapApiService>();
            services.AddSingleton<IEncryptionService, RSAAsymmetricEncryptionService>();
            services.AddSingleton<IPasswordHasherService, Argon2PasswordHasherService>();

            // servicios de aplicación
            services.AddScoped<IAppUserServices, AppUserServices>();
            services.AddScoped<IWorkSessionServices, WorkSessionServices>();

            return services;
        }
    }
}
