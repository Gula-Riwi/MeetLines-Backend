using System.IO;
using System.Threading.Tasks;

namespace MeetLines.Application.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadPhotoAsync(Stream fileStream, string fileName);
        Task DeletePhotoAsync(string publicId);
    }
}
