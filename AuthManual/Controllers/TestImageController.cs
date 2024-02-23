using AuthManual.Data;
using AuthManual.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AuthManual.Controllers
{
    [Authorize]
	public class TestImageController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly Random _random;

		public TestImageController(ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment)
		{
			_dbContext = dbContext;
			_hostingEnvironment = hostingEnvironment;
            _random = new Random();
		}

		[HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
		{
			return View();
		}

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(ResimViewModel vm)
        {
            if (vm != null)
            {


                var tblimage = new TBLImage(); // Her dosya için yeni bir TBLImage örneği oluştur

                var fileName = Path.GetFileName(vm.ImageFile.FileName);
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "img", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    vm.ImageFile.CopyTo(stream);
                }

                tblimage.Title = vm.Title;
                tblimage.ImageFileName = fileName; // Dosya adını ImageFileName özelliğine atayın

                _dbContext.TBLImages.Add(tblimage);
                await _dbContext.SaveChangesAsync();

            }
            else
            {
                ModelState.AddModelError("", "Lütfen bir veya daha fazla dosya seçin.");
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Index(int id)
		{
           
            var data = await _dbContext.TBLImages.Where(x => x.Id == id).ToListAsync();
            return View(data);
        }

	}
}
