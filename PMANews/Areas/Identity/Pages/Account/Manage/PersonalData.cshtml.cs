using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PMANews.Areas.Identity.Data;
using PMANews.Data;

namespace PMANews.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly PMANewsContext _context;
        private readonly ILogger<PersonalDataModel> _logger;

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            PMANewsContext context,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {

            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Username")]
            public string Username { get; set; }

        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;
            FirstName = user.FirstName;
            LastName = user.LastName;

            Input = new InputModel
            {
                FirstName = FirstName,
                LastName = LastName,
                Username = Username
            };
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(appUser);
                return Page();
            }


            if (Input.Username != appUser.UserName | Input.FirstName != appUser.FirstName | Input.LastName != appUser.LastName)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(appUser, Input.Username);
                if (!setUsernameResult.Succeeded)
                {
                    StatusMessage = "Unexpected error.";
                    return RedirectToPage();
                }
                appUser.UserName = Input.Username;
                appUser.FirstName = Input.FirstName;
                appUser.LastName = Input.LastName;
                _context.User.Update(appUser);
                _context.SaveChanges();

            }

            await _signInManager.RefreshSignInAsync(appUser);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}