using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Model.Api.Upload
{
    public class UploadImageModel
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
