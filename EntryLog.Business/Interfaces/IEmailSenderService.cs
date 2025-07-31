namespace EntryLog.Business.Interfaces
{
    internal interface IEmailSenderService
    {
        Task<bool> SendEmailWithTemplateAsync(string templateName, string to, object? data = null);
    }
}
