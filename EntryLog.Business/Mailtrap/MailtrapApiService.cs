using EntryLog.Business.Interfaces;
using EntryLog.Business.Mailtrap.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntryLog.Business.Mailtrap
{
    internal class MailtrapApiService(
        IHttpClientFactory clientFactory,
        IOptions<MailtrapApiOptions> options) : IEmailSenderService
    {
        private readonly IHttpClientFactory _clientFactory = clientFactory;
        private readonly MailtrapApiOptions _options = options.Value;

        public Task<bool> SendEmailWithTemplateAsync(string templateName, string to, object? data = null)
        {
            throw new NotImplementedException();
        }
    }
}
