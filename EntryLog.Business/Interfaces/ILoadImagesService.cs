using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces
{
    internal interface ILoadImagesService
    {
        Task<ImageBBResponseDTO> UploadAsync(Stream image, string type,string filename, string extension);
    }
}
