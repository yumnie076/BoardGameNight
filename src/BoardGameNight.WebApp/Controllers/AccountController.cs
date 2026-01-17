using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BoardGameNight.Infrastructure.Identity;
using BoardGameNight.Application.Interfaces;
using BoardGameNight.Application.DTOs;
using BoardGameNight.WebApp.Models;
using BoardGameNight.Domain.Exceptions;

namespace BoardGameNight.WebApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPersonService _personService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPersonService personService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _personService = personService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Create person first
            var personDto = new CreatePersonDto
            {
                Name = model.Name,
                Email = model.Email,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Street = model.Street,
                HouseNumber = model.HouseNumber,
                City = model.City,
                DietaryPreferences = model.DietaryPreferences
            };

            var person = await _personService.CreateAsync(personDto);

            // Create identity user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PersonId = person.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Link person to identity user
                await _personService.LinkToIdentityUserAsync(person.Id, user.Id);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        catch (DomainValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Ongeldige inloggegevens.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
