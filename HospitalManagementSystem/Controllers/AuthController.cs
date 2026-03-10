using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser> _userMgr;

        public AuthController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> userMgr)
        {
            _signIn = signIn;
            _userMgr = userMgr;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        public IActionResult Profile()
        {
            var user = _userMgr.GetUserAsync(User).Result;
            return View(user);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
