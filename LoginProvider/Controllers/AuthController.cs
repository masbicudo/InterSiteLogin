using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Infra;
using LoginProvider.Code;
using LoginProvider.Code.Authentication;
using LoginProvider.Commands;
using LoginProvider.Domain;
using LoginProvider.Models;
using LoginProvider.ViewModels.Auth;

namespace LoginProvider.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        protected readonly IRepository<User> UserRepository;
        protected readonly HashPasswordCommand passwordHasher;

        public AuthController(IRepository<User> userRepository)
        {
            this.UserRepository = userRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string apiKey, string actionToken, string returnUrl)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                // == NO API-KEY - DIRECT LOGIN ==
                // 
                // This means that the user is loggin in to the provider site directly.
                // In this case, by logging in, the user will be recognized by the login provider itself.
                // The user may or may not be given access to the login provider as an application.
                //
                // Later, when the USER tryes to login to any APPLICATION, one of these may happen:
                //
                //  1) AUTOMATIC LOGIN: allow the user to login,
                //                      without bothering him/her with additional steps
                //                      (very dangerous and leaky)
                //
                //  2) CONFIRM LOGIN:   require the user to confirm he/she wishes to login,
                //                      but without having to enter credentials
                //
                //  3) VERIFIED LOGIN:  require the user to enter credentials again...
                //                      even if the user already have the provider authentication cookie
                //
                // Both the USER and the APPLICATION can control this behavior.
                // When they diverge, the most restrictive behavior will be respected.
                return this.View();
            }

            var application = LoginHelper.ValidateRequestAndGetApplication(apiKey, this.Request);
            if (application == null)
            {
                // == INVALID REQUEST ==
                //
                // There is something wrong with this request.
                // Maybe someone is trying to forge a user login,
                // or induce the user to do something without proper knowledge.
                this.ViewBag.NotSafe = true;
                return this.View();
            }

            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                // == USER NOT YET RECOGNIZED (VISITOR) ==
                //
                // Let the user enter his/her login information:
                // - user name
                // - password
                this.ViewBag.ReturnUrl = application.LoginRedirectUrl;
                return this.View();
            }

            var authTicket = ((CustomPrincipal)this.HttpContext.User).Ticket;
            var user = LoginHelper.GetUser(authTicket.UserGuid);

            if (user == null)
            {
                // == INVALID USER ==
                // 
                // There is something wrong with this request.
                // Maybe someone is trying to forge a user login.
                this.ViewBag.NotSafe = true;
                return this.View();
            }

            if (application.RecognizedLoginMode == RecognizedLoginModes.RequireCredentials
                || user.RecognizedLoginMode == RecognizedLoginModes.RequireCredentials)
            {
                // == USER RECOGNIZED - SETTING IS: MUST ENTER CREDENTIALS ==
                //
                // Let the user enter his/her credentials again.
                return this.View(
                    new LoginViewModel
                    {
                        Name = user.Name,
                        RecognizedLoginMode = RecognizedLoginModes.RequireCredentials,
                    });
            }

            if (application.RecognizedLoginMode == RecognizedLoginModes.RequireConfirmation
                || user.RecognizedLoginMode == RecognizedLoginModes.RequireConfirmation)
            {
                // == USER RECOGNIZED - OPTION: MUST CONFIRM ==
                //
                // Let the user enter his/her credentials again.
                return this.View(
                    new LoginViewModel
                    {
                        Name = user.Name,
                        RecognizedLoginMode = RecognizedLoginModes.RequireConfirmation,
                    });
            }

            if (application.RecognizedLoginMode == RecognizedLoginModes.DontDisturb
                || user.RecognizedLoginMode == RecognizedLoginModes.DontDisturb)
            {
                // == USER RECOGNIZED - OPTION: DONT DISTURB USER ==
                //
                // Send notification to the application, that the user logged in, if:
                //  - there is an actionToken:
                //      - data to send: login-token; action-token; login-mode (auto/confirmed/verified)
                //      - app may respond with "RETURN USER TO <url>" so that we know where to redirect the user to
                //      - app may respond with "SWITCH MODE <mode>" where mode is confirmed or verified
                //  - the application has the option "Notify User Login" set:
                //      - data to send: login-key; default-login-action-token (if it exists); login-mode (auto/confirmed/verified)
                //      - if LoginRedirectUrl is empty:
                //          - we must wait for the notification response with "RETURN USER TO <url>"
                //          - or for a "SWITCH MODE <mode>" response
                //      - if LoginRedirectUrl is filled: we don't wait for any response,
                //          and redirect the user immediately sending the login-token through the URL
                //
                // Redirect user to the application:
                //  - based on the response from the notification,
                //  - or the default login return url
                //  - or the passed return url (only when there is no action-token)
                var loginResult = LoginHelper.LogUserInAngGetReturnUrl(application, user, actionToken, returnUrl, this.Request);

                return this.Redirect(loginResult.ReturnUrl.ToString());
            }

            throw new Exception("Unrecognized option.");
        }

        /// <summary>
        /// Post: /Auth/Login
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public JsonResult Login(LoginViewModel viewModel, bool lembrarMinhaSenha = false, string returnUrl = "")
        {
            if (!this.ModelState.IsValid)
                return this.JsonWithModelErrors();

            // getting user from login info
            var dbuser = this.UserRepository.Queryable
                .SingleOrDefault(u => u.Login == viewModel.Login);

            if (dbuser != null)
            {
                // checking user password
                var hashbits = this.passwordHasher.Execute(
                    new HashPasswordCommand.Data
                    {
                        PartialSalt = dbuser.PasswordSalt,
                        Password = viewModel.Password,
                    });

                if (hashbits.SequenceEqual(dbuser.Password))
                    dbuser = null;
            }

            if (dbuser == null)
            {
                this.ModelState.AddModelError(
                    "User login name or password are incorrect.",
                    () => viewModel.Login,
                    () => viewModel.Password);
            }

            if (!this.ModelState.IsValid)
                return this.JsonWithModelErrors();

            AuthHelper.SignIn(
                this.Response,
                AuthHelper.CreateTicket(dbuser),
                TimeSpan.FromHours(8),
                lembrarMinhaSenha);

            if (this.Request.AcceptTypes.Contains(""))
            {
            }

            return this.Json(
                string.IsNullOrWhiteSpace(returnUrl)
                    ? this.Url.Action("Index", "Home")
                    : returnUrl);
        }

        public void Logout()
        {
            var authTicket = ((CustomPrincipal)this.HttpContext.User).Ticket;
            LoginHelper.LogoutAll(authTicket.UserGuid);
            AuthHelper.SignOut(this.Response);
        }
    }
}