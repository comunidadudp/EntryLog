using EntryLog.Business.Constants;
using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EntryLog.Business.ImageBB
{
    internal class ImageBBService(
        IHttpClientFactory clientFactory,
        IOptions<ImageBBOptions> options) : ILoadImagesService
    {
        private readonly IHttpClientFactory _clientFactory = clientFactory;
        private readonly ImageBBOptions _options = options.Value;

        public async Task<ImageBBResponseDTO> UploadAsync(Stream image, string type, string filename, string extension)
        {

            using var client = _clientFactory.CreateClient(ApiNames.ImageBB);
            using var streamContent = new StreamContent(image);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(type);

            using var form = new MultipartFormDataContent();
            form.Add(streamContent, "image", filename);

            //https://api.imgbb.com/1/upload?expiration=600&key=YOUR_CLIENT_API_KEY
            var response = await client.PostAsync($"/1/upload?expiration={_options.ExpirationSeconds}&key={_options.ApiToken}", form);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ImageBBResponseDTO imageBBResponse = JsonSerializer.Deserialize<ImageBBResponseDTO>(responseBody)!;
                return imageBBResponse;
            }
            else
            {
                return null;
            }
        }
    }
}
