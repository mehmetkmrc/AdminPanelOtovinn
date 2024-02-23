using AuthManual.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthManual.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public RolesController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _db.Roles.ToList();
            return View(roles);
        }


        [HttpGet]
        public IActionResult Upsert(string id)
        {
            if (String.IsNullOrEmpty(id)) // if id is null this is create
            {
                return View();
            }
            else // if not it is update
            {
                var role = _db.Roles.FirstOrDefault(x => x.Id == id);
                return View(role);
            }
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Upsert(IdentityRole roleObj)
        {
            if (await _roleManager.RoleExistsAsync(roleObj.Name))
            {
                // error
                TempData["error"] = "Role already exists.";
                return RedirectToAction("Index", "Roles");

            }

            if (string.IsNullOrEmpty(roleObj.Id))
            {
                // create
                await _roleManager.CreateAsync(new IdentityRole() { Name = roleObj.Name });
                TempData["success"] = "Role created successfully.";
            }
            else
            {
                // update
                var dbRole = _db.Roles.FirstOrDefault(r => r.Id == roleObj.Id);
                dbRole!.Name = roleObj.Name;
                dbRole!.NormalizedName = roleObj.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(dbRole);
                TempData["success"] = "Role updated successfully.";
            }

            return RedirectToAction("Index", "Roles");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var dbRole = _db.Roles.FirstOrDefault(u => u.Id == id);
            if (dbRole == null)
            {
                TempData["error"] = "Role not found.";
                return RedirectToAction("Index", "Roles");
            }

            var userRoles = _db.UserRoles.Count(u => u.RoleId == id);
            if (userRoles > 0)
            {
                TempData["error"] = "Cannot delete role. There are users with this role.";
                return RedirectToAction("Index", "Roles");
            }

            await _roleManager.DeleteAsync(dbRole);
            TempData["success"] = "Role deleted successfully.";
            return RedirectToAction("Index", "Roles");
        }


    }
}
