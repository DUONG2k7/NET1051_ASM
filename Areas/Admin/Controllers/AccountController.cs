using ASM_1.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASM_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Tên người dùng đã tồn tại");
                    return View(model);
                }

                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("", "Email đã tồn tại");
                    return View(model);
                }

                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser? user;

                if (model.UsernameOrEmail.Contains("@"))
                {
                    user = await _userManager.FindByEmailAsync(model.UsernameOrEmail);
                }
                else
                {
                    user = await _userManager.FindByNameAsync(model.UsernameOrEmail);
                }

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            }
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var checkOld = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!checkOld)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng.");
                return View(model);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
