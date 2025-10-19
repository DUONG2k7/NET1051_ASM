using ASM_1.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ASM_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AdminController(UserManager<AppUser> userManager,
                                 RoleManager<AppRole> roleInManager)
        {
            _userManager = userManager;
            _roleManager = roleInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            List<AppRole> roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        public async Task<IActionResult> ListUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var allRoles = await _roleManager.Roles.ToListAsync();
            var result = new List<UserRolesViewModel>();

            foreach (var u in users)
            {
                var userRoles = await _userManager.GetRolesAsync(u);

                var roleSelections = allRoles.Select(r => new RoleSelectionVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    IsSelected = userRoles.Contains(r.Name ?? string.Empty)
                }).ToList();

                result.Add(new UserRolesViewModel
                {
                    UserId = u.Id,
                    UserName = u.Email ?? u.UserName ?? "(no email)",
                    Roles = roleSelections
                });
            }

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(AppRole model)
        {
            if (ModelState.IsValid)
            {
                bool roleExist = await _roleManager.RoleExistsAsync(model.Name!);
                if (!roleExist)
                {
                    var result = await _roleManager.CreateAsync(new AppRole { Name = model.Name! });

                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"Role {model.Name} đã được tạo thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Role already exists");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var Role = await _roleManager.FindByIdAsync(id);
            if (Role == null)
            {
                return NotFound();
            }

            var model = new EditRoleViewModel
            {
                RoleId = Role.Id,
                RoleName = Role.Name ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var Role = await _roleManager.FindByIdAsync(model.RoleId);
                if (Role == null)
                {
                    return NotFound();
                }
                else
                {
                    Role.Name = model.RoleName;
                    var result = await _roleManager.UpdateAsync(Role);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"Role {model.RoleName} đã được sửa thành công!";
                        return RedirectToAction("ListRoles");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var Role = await _roleManager.FindByIdAsync(id);
            if (Role == null)
            {
                return NotFound();
            }
            else
            {
                var result = await _roleManager.DeleteAsync(Role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Role {Role.Name} đã được xóa thành công!";
                    return RedirectToAction("ListRoles");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("ListRoles", await _roleManager.Roles.ToListAsync());
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? user.Email ?? "(no email)"
            };

            var allRoles = await _roleManager.Roles.ToListAsync();

            foreach (var role in allRoles)
            {
                model.Roles.Add(new RoleSelectionVM
                {
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name!)
                });
            }


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserRoles(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Lấy role được chọn từ checkbox
            var selectedRoles = model.Roles
                                     .Where(r => r.IsSelected)
                                     .Select(r => r.RoleName)
                                     .ToList();

            // Xóa roles cũ
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa role cũ");
                return View(model);
            }

            // Thêm roles mới
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Không thể thêm role mới");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Cập nhật roles cho {user.UserName} thành công!";
            return RedirectToAction("ListUsers");
        }

    }
}
