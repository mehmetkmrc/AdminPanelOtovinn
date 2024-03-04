using System.ComponentModel.DataAnnotations.Schema;

namespace AuthManual.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public byte[] ImageData { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}
