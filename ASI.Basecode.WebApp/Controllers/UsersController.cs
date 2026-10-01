using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(
        Roles = DomainValues.Roles.Administrator,
        Policy = DomainValues.Permissions.UserRoleManage)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UsersController : Controller
    {
        private readonly IUserAdministrationService _userAdministration;

        public UsersController(IUserAdministrationService userAdministration)
        {
            _userAdministration = userAdministration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string search,
            string status,
            string role,
            int page = 1)
        {
            if (!await _userAdministration.CanManageAsync(UserId))
            {
                return Forbid();
            }

            var accounts = await _userAdministration.GetAccountsAsync(
                UserId, search, status, role, page);
            var permissions = await _userAdministration.GetRolePermissionsAsync(UserId);
            if (accounts == null || permissions == null)
            {
                return Forbid();
            }

            ViewData["Title"] = "Users & roles";
            ViewData["Eyebrow"] = "Administration";
            ViewData["PageDate"] = DateTime.Now.ToString("MMM d, yyyy");
            ViewData["PermissionMatrix"] = permissions;
            ViewData["SuccessMessage"] = TempData["SuccessMessage"];
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            return View(accounts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!await _userAdministration.CanManageAsync(UserId))
            {
                return Forbid();
            }

            ViewData["Title"] = "Create account";
            ViewData["Eyebrow"] = "Administration";
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!await _userAdministration.CanManageAsync(UserId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Create account";
                ViewData["Eyebrow"] = "Administration";
                return View(model);
            }

            var result = await _userAdministration.CreateAsync(UserId, model);
            if (result.Forbidden)
            {
                return Forbid();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                ViewData["Title"] = "Create account";
                ViewData["Eyebrow"] = "Administration";
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!await _userAdministration.CanManageAsync(UserId))
            {
                return Forbid();
            }

            var model = await _userAdministration.GetUserAsync(UserId, id);
            if (model == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit account";
            ViewData["Eyebrow"] = "Administration";
            ViewData["SuccessMessage"] = TempData["SuccessMessage"];
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!await _userAdministration.CanManageAsync(UserId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await GetRoleOptionsAsync();
                ViewData["Title"] = "Edit account";
                ViewData["Eyebrow"] = "Administration";
                return View(model);
            }

            var result = await _userAdministration.UpdateAsync(UserId, model);
            if (result.Forbidden)
            {
                return Forbid();
            }

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(SetAccountActiveViewModel model)
        {
            if (!await _userAdministration.CanManageAsync(UserId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "The account form is incomplete. Reload and try again.";
                return RedirectToAction(nameof(Edit), new { id = model.UserId });
            }

            var result = await _userAdministration.SetActiveAsync(UserId, model);
            if (result.Forbidden)
            {
                return Forbid();
            }

            if (result.NotFound)
            {
                return NotFound();
            }

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Edit), new { id = model.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(UpdateRolePermissionsViewModel model)
        {
            if (!await _userAdministration.CanManageAsync(UserId))
            {
                return Forbid();
            }

            var result = await _userAdministration.UpdatePermissionsAsync(UserId, model);
            if (result.Forbidden)
            {
                return Forbid();
            }

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private string UserId => User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        private async Task<System.Collections.Generic.IReadOnlyList<RoleOptionViewModel>>
            GetRoleOptionsAsync()
        {
            var model = await _userAdministration.GetRolePermissionsAsync(UserId);
            return model?.Roles ?? Array.Empty<RoleOptionViewModel>();
        }
    }
}
