using System.ComponentModel.DataAnnotations.Schema;

namespace AuthManual.Models
{
    public class Screen
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public byte[] ImageData { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }

        // Tab_Screen ile ilişkilendirilecek özellik
        public Guid? Tab_ScreenId { get; set; }
        [ForeignKey("Tab_ScreenId")]
        public Tab_Screen Tab_Screen { get; set; }
    }
}
