using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AuthManual.Models
{
	public class TBLImage
	{
		[Key]
		public int Id { get; set; }

		[StringLength(255)]
		public string Title { get; set; }

		


		[StringLength(255)]
		public string ImageFileName { get; set; }

		//Veritabanı eşlemesinden hariç tut
		[NotMapped]
		public IFormFile ImageFile { get; set; }
	}
}
