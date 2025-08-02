using EntryLog.Business.Constants;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mailtrap.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace EntryLog.Business.Mailtrap
{
    internal class MailtrapApiService(
        IHttpClientFactory clientFactory,
        IOptions<MailtrapApiOptions> options) : IEmailSenderService
    {
        private readonly IHttpClientFactory _clientFactory = clientFactory;
        private readonly MailtrapApiOptions _options = options.Value;

        public async Task<bool> SendEmailWithTemplateAsync(string templateName, string to, object? data = null)
        {
            string templateUuid = _options.Templates.First(x => x.Name == templateName).Uuid;

            using (var client = _clientFactory.CreateClient(ApiNames.MailtrapIO))
            {
                var body = new MailtrapRequestBody
                {
                    From = new From
                    {
                        Email = _options.FromEmail,
                        Name = _options.FromName,
                    },
                    To = [new To { Email = to }],
                    TemplateUuid = templateUuid,
                    TemplateVariables = data,
                };

                var response = await client.PostAsJsonAsync<MailtrapRequestBody>("/api/send", body);

                return response.IsSuccessStatusCode;
            }
        }
    }
}
