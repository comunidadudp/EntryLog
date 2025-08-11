using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.Infrastructure
{
    internal class UriService : IUriService
    {
        public UriService(IHttpContextAccessor httpContextAccessor)
        {
            var request = (httpContextAccessor.HttpContext?.Request)
                ?? throw new Exception("Ha ocurrido un error al generar el contexto http de la aplicación");

            ApplicationURL = $"{request.Scheme}://{request.Host}{request.PathBase}";
            UserAgent = $"{request.Headers["Sec-Ch-Ua"]}";
            Platform = $"{request.Headers["Sec-Ch-Ua-Platform"]}";
            RemoteIpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown RemoteIpAddress";
        }

        public string ApplicationURL {  get; private set; }
        public string UserAgent { get; private set; }
        public string Platform { get; private set; }
        public string RemoteIpAddress { get; private set; }
    }
}
