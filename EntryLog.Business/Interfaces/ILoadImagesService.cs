using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces
{
    internal interface ILoadImagesService
    {
        Task<string> UploadAsync(Stream image, string type,string filename);
    }
}
