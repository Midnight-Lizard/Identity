using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MidnightLizard.Web.Identity.Models;
using MidnightLizard.Web.Identity.Models.ManageViewModels;
using MidnightLizard.Web.Identity.Security.Claims;
using MidnightLizard.Web.Identity.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._emailSender = emailSender;
            this._logger = logger;
            this._urlEncoder = urlEncoder;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var model = new IndexViewModel
            {
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = StatusMessage
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await this._userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var phoneNumber = user.PhoneNumber;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await this._userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            var displayName = user.DisplayName;
            if (model.DisplayName != displayName)
            {
                user.DisplayName = model.DisplayName;
                var updateResult = await this._userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting display name of user with ID '{user.Id}'.");
                }
            }

            this.StatusMessage = "Your profile has been updated";
            return this.RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(IndexViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            await this.SendVerificationEmailToTheUserAsync(user);

            this.StatusMessage = "Verification email sent. Please check your email. (Including spam folder)";
            return this.RedirectToAction(nameof(Index));
        }

        private async Task SendVerificationEmailToTheUserAsync(ApplicationUser user)
        {
            var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = this.Url.EmailConfirmationLink(user.Id, code, this.Request.Scheme);
            var email = user.Email;
            await this._emailSender.SendEmailConfirmationRequestAsync(user.DisplayName, email, callbackUrl);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var hasPassword = await this._userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return this.RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var changePasswordResult = await this._userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                this.AddErrors(changePasswordResult);
                return this.View(model);
            }

            await this._signInManager.SignInAsync(user, isPersistent: false);
            this._logger.LogInformation("User changed their password successfully.");
            this.StatusMessage = "Your password has been changed.";

            return this.RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var hasPassword = await this._userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return this.RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var addPasswordResult = await this._userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                this.AddErrors(addPasswordResult);
                return this.View(model);
            }

            await this._signInManager.SignInAsync(user, isPersistent: false);
            this.StatusMessage = "Your password has been set.";

            return this.RedirectToAction(nameof(SetPassword));
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var model = new ExternalLoginsViewModel { CurrentLogins = await this._userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await this._signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = await this._userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            model.StatusMessage = this.StatusMessage;

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await this.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = this.Url.Action(nameof(LinkLoginCallback));
            var properties = this._signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, this._userManager.GetUserId(this.User));
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var info = await this._signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info == null)
            {
                throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await this._userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await this.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            this.StatusMessage = "The external login was added.";
            return this.RedirectToAction(nameof(ExternalLogins));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var result = await this._userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
            }

            await this._signInManager.SignInAsync(user, isPersistent: false);
            this.StatusMessage = "The external login was removed.";
            return this.RedirectToAction(nameof(ExternalLogins));
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await this._userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await this._userManager.CountRecoveryCodesAsync(user),
            };

            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Disable2faWarning()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            return this.View(nameof(Disable2fa));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var disable2faResult = await this._userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            this._logger.LogInformation("User with ID {UserId} has disabled 2fa.", user.Id);
            return this.RedirectToAction(nameof(TwoFactorAuthentication));
        }

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var unformattedKey = await this._userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await this._userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await this._userManager.GetAuthenticatorKeyAsync(user);
            }

            var model = new EnableAuthenticatorViewModel
            {
                SharedKey = this.FormatKey(unformattedKey),
                AuthenticatorUri = this.GenerateQrCodeUri(user.Email, unformattedKey)
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await this._userManager.VerifyTwoFactorTokenAsync(
                user, this._userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                this.ModelState.AddModelError("model.Code", "Verification code is invalid.");
                return this.View(model);
            }

            await this._userManager.SetTwoFactorEnabledAsync(user, true);
            this._logger.LogInformation("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
            return this.RedirectToAction(nameof(GenerateRecoveryCodes));
        }

        [HttpGet]
        public IActionResult ResetAuthenticatorWarning()
        {
            return this.View(nameof(ResetAuthenticator));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            await this._userManager.SetTwoFactorEnabledAsync(user, false);
            await this._userManager.ResetAuthenticatorKeyAsync(user);
            this._logger.LogInformation("User with id '{UserId}' has reset their authentication app key.", user.Id);

            return this.RedirectToAction(nameof(EnableAuthenticator));
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await this._userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            var model = new GenerateRecoveryCodesViewModel { RecoveryCodes = recoveryCodes.ToArray() };

            this._logger.LogInformation("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PersonalData()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadPersonalData()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            this._logger.LogInformation("User with ID '{UserId}' asked for their personal data.", this._userManager.GetUserId(this.User));

            // Only include personal data for download
            var personalData = new Dictionary<string, string>
            {
                { "UserId", await this._userManager.GetUserIdAsync(user) },
                { "UserName", await this._userManager.GetUserNameAsync(user) },
                { "DisplayName", user.DisplayName },
                { "Email", await this._userManager.GetEmailAsync(user) },
                { "EmailConfirmed", (await this._userManager.IsEmailConfirmedAsync(user)).ToString() },
                { "PhoneNumber", await this._userManager.GetPhoneNumberAsync(user) },
                { "PhoneNumberConfirmed", (await this._userManager.IsEmailConfirmedAsync(user)).ToString() }
            };

            this.Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(personalData)), "text/json");
        }

        public async Task<IActionResult> DeletePersonalData()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var model = new DeletePersonalDataModel
            {
                RequirePassword = await this._userManager.HasPasswordAsync(user)
            };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePersonalData(DeletePersonalDataModel model)
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            if (await this._userManager.HasPasswordAsync(user))
            {
                if (!await this._userManager.CheckPasswordAsync(user, model.Password))
                {
                    this.ModelState.AddModelError(string.Empty, "Password not correct.");
                    return this.View();
                }
            }

            foreach (var login in await this._userManager.GetLoginsAsync(user))
            {
                var loginResult = await this._userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
                if (!loginResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleteing user {login.LoginProvider} login with ID '{user.Id}'.");
                }
            }

            user.UserName = user.Id;
            user.NormalizedUserName = user.Id.ToUpper();
            user.DisplayName = "Deleted";
            user.Email = null;
            user.NormalizedEmail = null;
            user.EmailConfirmed = false;
            user.PhoneNumber = null;
            user.PhoneNumberConfirmed = false;
            user.PasswordHash = null;
            var updateResult = await this._userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleteing user data with ID '{user.Id}'.");
            }

            await this._signInManager.SignOutAsync();

            this._logger.LogInformation("User with ID '{UserId}' deleted themselves.", this._userManager.GetUserId(this.User));

            return this.Redirect("~/");
        }

        [Authorize(Roles = nameof(AppRole.Owner))]
        public async Task<IActionResult> SendVerificationEmailToAllUsers()
        {
            var result = "The following users have been notified:";
            foreach (var user in this._userManager.Users
                .Where(x => x.EmailConfirmed == false && x.Email != null))
            {
                result += "/n" + user.Email;
                await this.SendVerificationEmailToTheUserAsync(user);
            }
            return this.Content(result);
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            var currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenicatorUriFormat,
                this._urlEncoder.Encode("MidnightLizard.Web.Identity"),
                this._urlEncoder.Encode(email),
                unformattedKey);
        }

        #endregion
    }
}
