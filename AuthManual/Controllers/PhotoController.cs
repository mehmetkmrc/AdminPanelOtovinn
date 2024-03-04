using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AuthManual.Data;
using AuthManual.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class PhotoController : Controller
{
    private readonly ApplicationDbContext _context;

    public PhotoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Photo/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Photo/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,ImageFile")] Photo photo)
    {
        if (!ModelState.IsValid)
        {
            if (photo.ImageFile != null && photo.ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await photo.ImageFile.CopyToAsync(memoryStream);
                    photo.ImageData = memoryStream.ToArray();
                }
            }
            _context.Add(photo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(photo);
    }

    // GET: Photo/Index
    public async Task<IActionResult> Index(int id)
    {
        var photos = await _context.Photos.Where(x=>x.Id == id).ToListAsync();
        return View(photos);
    }
}
