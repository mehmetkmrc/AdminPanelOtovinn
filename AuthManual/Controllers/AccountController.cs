using AuthManual.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace AuthManual.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;


        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            UrlEncoder urlEncoder, RoleManager<IdentityRole> roleManager    
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _roleManager = roleManager;

        }




        [AllowAnonymous]
        [HttpGet] // Display all the properties the user has to enter
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            // If admin role doesn't exist
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                // create role
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "Admin",
                Text = "Admin"
            });
            listItems.Add(new SelectListItem()
            {
                Value = "User",
                Text = "User"
            });

            ViewData["ReturnUrl"] = returnUrl;
            var registerViewModel = new RegisterViewModel()
            {
                RoleList = listItems
            };
            return View(registerViewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Doğrulama e-postasını gönder
                await SendVerificationEmail(user);

                // Kayıt başarılı, kullanıcıyı giriş yapması için yönlendir
                // Ancak, burada kullanıcının e-posta doğrulanmış olup olmadığını kontrol etmeliyiz
                // Eğer doğrulanmamışsa, kullanıcıyı giriş yapmaktan alıkoymalıyız.
                if (!user.EmailConfirmed)
                {
                    TempData["Message"] = "Email adresinizi doğrulamadan giriş yapamazsınız. Lütfen e-postanızı kontrol edin ve doğrulama linkine tıklayın.";
                    return RedirectToAction(nameof(Login));
                }

                // Eğer kullanıcı doğrulanmışsa normal şekilde giriş yapılabilir.
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(returnUrl);

                if (model.RoleSelected == "Admin")
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
            }
            else
            {
                AddErrors(result);
            }

            // Eğer kayıt işlemi başarısız olursa, yeniden kayıt sayfasını gösterirken modeli ve rol listesini doldurun
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "Admin",
                Text = "Admin"
            });
            listItems.Add(new SelectListItem()
            {
                Value = "User",
                Text = "User"
            });

            model.RoleList = listItems;
            return View(model);
        }



        private async Task SendVerificationEmail(ApplicationUser user)
        {
            var email = new Email
            {
                FromAddress = "mehmet.komurcu@ogr.iuc.edu.tr",
                FromName = "Mehmet",
                ToAddress = user.Email,
                Subject = "Please verify your email address",
                Body = $"Dear {user.Name},<br><br>" +
               $"Thank you for registering. Please click the following link to verify your email address:<br><br>" +
               $"<a href='{Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = await _userManager.GenerateEmailConfirmationTokenAsync(user) }, protocol: HttpContext.Request.Scheme)}'>Confirm Email</a><br><br>" +
               $"Regards,<br>Your Application Team",
                SmtpConnection = "smtp.gmail.com",
                SmtpPort = 587, // Gmail SMTP port for TLS
                SmtpUser = "mehmet.komurcu@ogr.iuc.edu.tr",
                SmtpPassword = "77MTR8K1" // Gmail uygulama şifresi
            };

            email.SendMail();
        }



        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            // Retrieve user details based on userId
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                // E-posta doğrulaması başarılı oldu, kullanıcının giriş yapmasına izin ver
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("ConfirmEmail", "Account"); // veya istediğiniz bir yere yönlendirin
            }
            else
            {
                // Hata durumunu ele alın
                // Örneğin, kullanıcıya bir hata sayfası gösterin veya uygun bir işlem yapın
                return View("Error");
            }
        }


        [AllowAnonymous]
        [HttpGet] // Display all the properties the user has to enter
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                lockoutOnFailure: true);
            if (result.Succeeded)
            {
                // Kullanıcı giriş yaptı, ancak e-postası doğrulanmamışsa, giriş yapmaktan alıkoy
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (!user.EmailConfirmed)
                {
                    await _signInManager.SignOutAsync();
                    TempData["Message"] = "Email adresinizi doğrulamadan giriş yapamazsınız. Lütfen e-postanızı kontrol edin ve doğrulama linkine tıklayın.";
                    return RedirectToAction(nameof(Login));
                }

                // Eğer kullanıcı doğrulanmışsa normal şekilde giriş yapılabilir.
                return RedirectToAction("Index", "TestImage");
            }
            if (result.IsLockedOut)
            {
                return View("LockedOut");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
    

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "Home");
        }

        // Forget Password
        [AllowAnonymous]
        [HttpGet] // Display all the properties the user has to enter
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        // Configure and send a token
        {

            if (ModelState.IsValid)
            {

                // Kullanıcıyı e-posta adresine göre bul
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Kullanıcı bulunamazsa hata mesajı gönder
                    return Content("User Not found.");
                }

                // Kullanıcı bulunduğunda şifre sıfırlama kodu oluştur
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Şifre sıfırlama bağlantısı oluştur
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                
                // Şifre sıfırlama bağlantısını e-posta ile gönder
                await SendForgotEmail(user, callbackUrl);

                // Şifre sıfırlama işleminin başarılı olduğuna dair onay sayfasını göster
                //return View("ForgotPasswordConfirmation", new ResetPasswordLinkViewModel { Link = callbackUrl });
                return RedirectToAction("ResetLinkSent");

            }

            // Geçersiz model durumunda formu tekrar göster
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet] // Display all the properties the user has to enter
        public IActionResult ResetLinkSent()
        {
            return View();
        }


        private async Task SendForgotEmail(IdentityUser user, string callbackUrl)
        {
            var email = new Email
            {
                FromAddress = "mehmet.komurcu@ogr.iuc.edu.tr",
                FromName = "Mehmet",
                ToAddress = user.Email,
                Subject = "Please Reset Your Password",
                Body = $"Dear user,<br><br>" +
               $"I heard you forgot your password. Please click the following link to reset your password:<br><br>" +
               $"<a href='{callbackUrl}'>Reset Password</a><br><br>" +
               $"Regards,<br>Your Application Team",
                SmtpConnection = "smtp.gmail.com",
                SmtpPort = 587, // Change this according to your SMTP server configuration
                SmtpUser = "mehmet.komurcu@ogr.iuc.edu.tr",
                SmtpPassword = "77MTR8K1"
            };

            email.SendMail();
        }




        // Reset password
        [AllowAnonymous]
        [HttpGet] // Display all the properties the user has to enter
        public IActionResult ResetPassword(string? code = null)
        {
            return code == null ? View("Error") : View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        // Configure and send a token
        {
            if (ModelState.IsValid)
            {
                // Checking if user exists
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Content("User Not found.");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

                if (result.Succeeded)
                {
                    return View("ResetPasswordConfirmation");
                }
                AddErrors(result);
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        // the method will be used anytime the user wants to enable authentication
        {
            string AuthenticationUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            // this will find the user that is logged in
            var user = await _userManager.GetUserAsync(User);

            // If there are any previous authenticators we will reset those
            await _userManager.ResetAuthenticatorKeyAsync(user);

            // Generating a new token
            var token = await _userManager.GetAuthenticatorKeyAsync(user);
            string AuthenticatorUri = string.Format(AuthenticationUriFormat, _urlEncoder.Encode("ManualAuth"),
                _urlEncoder.Encode(user.Email), token);

            var model = new TwoFactorAuthenticationViewModel { Token = token, QRCodeUrl = AuthenticatorUri };
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> EnableAuthenticator(TwoFactorAuthenticationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var succeeded = await _userManager.VerifyTwoFactorTokenAsync(user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);
                if (succeeded)
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, true);
                }
                else
                {
                    ModelState.AddModelError("Verify", "Your two factor auth code could not be validated.");
                    return View(model);
                }
            }
            return View("AuthenticationConfirmation");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnurl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            ViewData["ReturnUrl"] = returnurl;
            return View(new VerifyAuthenticatorViewModel { ReturnUrl = returnurl, RememberMe = rememberMe });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorViewModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, rememberClient: false);

            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                return View("LockedOut");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid Code.");
                return View(model);
            }

        }

        private void AddErrors(IdentityResult result) // Helper method 
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
        }
    }
}