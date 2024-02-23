using System.ComponentModel.DataAnnotations;

namespace AuthManual.Models
{
    public class ResimViewModel
    {
        [StringLength(255)]
        public string Title { get; set; }

        //Bunun altındakiler null geliyor
        [StringLength(255)]
        public string ImageFileName { get; set; }

        //Veritabanı eşlemesinden hariç tut

        [Required(ErrorMessage = "Please select an image file.")]
        public IFormFile ImageFile { get; set; }
    }
}
