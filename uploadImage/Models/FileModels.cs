using Microsoft.AspNetCore.Http;

namespace uploadImage.Models {
    public class FileModels {
        public string FileName { get; set; }
        public IFormFile file { get; set; }
    }
}
