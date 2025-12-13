using Microsoft.AspNetCore.Http;

namespace MeetLines.API.DTOs
{
    public class UploadProjectPhotoRequest
    {
        public IFormFile File { get; set; } = default!;
    }
}
