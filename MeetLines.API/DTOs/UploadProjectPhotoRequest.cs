using Microsoft.AspNetCore.Http;

namespace MeetLines.API.DTOs
{
    public class UploadProjectPhotoRequest
    {
        [Microsoft.AspNetCore.Mvc.FromForm(Name = "file")]
        public IFormFile File { get; set; } = default!;
    }
}
