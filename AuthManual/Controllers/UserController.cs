using System.Security.Claims;
using AuthManual.Data;
using AuthManual.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthManual.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userLisT = _db.ApplicationUser.ToList();
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach (var user in userLisT)
            {
                var role = userRoles.FirstOrDefault(u => u.UserId == user.Id);
                if (role == null)
                {
                    user.Role = "None";
                }
                else
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == role.RoleId)!.Name;
                }
            }

            return View(userLisT);
        }

        [HttpGet]
        public IActionResult Edit(string userId)
        {
            var dbUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == userId);
            if (dbUser == null)
            {
                return NotFound();
            }

            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            var role = userRole.FirstOrDefault(u => u.UserId == dbUser.Id);
            if (role != null)
            {
                dbUser.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId)!.Id;
            }

            dbUser.RoleList = _db.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id,
            });

            return View(dbUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                var dbUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == user.Id);
                if (dbUser == null)
                {
                    return NotFound();
                }

                var userRole = _db.UserRoles.FirstOrDefault(u => u.UserId == dbUser.Id);
                if (userRole != null) // user already has a role
                {
                    var previousRoleName = _db.Roles
                        .Where(u => u.Id == userRole.RoleId)
                        .Select(e => e.Name)
                        .FirstOrDefault();
                    // Removing old role from user
                    await _userManager.RemoveFromRoleAsync(dbUser, previousRoleName);
                }

                // Adding new role
                await _userManager.AddToRoleAsync(dbUser, _db.Roles.FirstOrDefault(u => u.Id == user.RoleId)!.Name);

                // Updating the name
                dbUser.Name = user.Name;

                await _db.SaveChangesAsync();

                TempData["success"] = "User Edited Successfully.";

                return RedirectToAction("Index", "User");
            }

            user.RoleList = _db.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id,
            });

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LockUnlock(string userId)
        {
            var dbUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == userId);
            if (dbUser == null)
            {
                return NotFound();
            }

            if (dbUser.LockoutEnd != null && dbUser.LockoutEnd > DateTime.Now)
            {
                // User is locked and we have to unlock
                dbUser.LockoutEnd = DateTime.Now;
                TempData["success"] = "User unlocked successfully";
            }
            else
            {
                // User is not locked 
                dbUser.LockoutEnd = DateTimeOffset.Now.AddYears(50);
                TempData["success"] = "User locked successfully";
            }

            _db.SaveChanges();

            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string userId)
        {
            var dbUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == userId);
            if (dbUser == null)
            {
                return NotFound();
            }

            _db.ApplicationUser.Remove(dbUser);
            _db.SaveChanges();
            TempData["success"] = "User deleted successfully.";
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel()
            {
                UserId = userId
            };
            foreach (var claim in ClaimStore.ClaimsList)
            {
                UserClaim userClaim = new UserClaim()
                {
                    ClaimType = claim.Type
                };
                if (existingUserClaims.Any(u => u.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }

                model.UserClaims.Add(userClaim);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel userClaimsViewModel)
        {
            var user = await _userManager.FindByIdAsync(userClaimsViewModel.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var delResult =
                await _userManager.RemoveClaimsAsync(user, claims); // removing all claims before adding/re-adding them

            if (!delResult.Succeeded)
            {
                TempData["error"] = "Error while removing claims.";
                return View(userClaimsViewModel);
            }

            var result = await _userManager.AddClaimsAsync(user, userClaimsViewModel.UserClaims
                .Where(u => u.IsSelected)
                .Select(u => new Claim(u.ClaimType, u.IsSelected.ToString())));

            if (!result.Succeeded)
            {
                TempData["error"] = "Error while removing claims.";
                return View(userClaimsViewModel);
            }

            TempData["success"] = "Claims updated successfully.";
            return RedirectToAction("Index", "User");
        }
    }
}