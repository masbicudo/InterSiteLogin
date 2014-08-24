using System;
using System.Linq;
using System.Web.Mvc;
using Infra;
using LoginProvider.Code;
using LoginProvider.Code.Authentication;
using LoginProvider.Models;
using LoginProvider.ViewModels.Auth;

namespace LoginProvider.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string apiKey, string actionToken, string returnUrl)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                // == NO API-KEY - DIRECT LOGIN ==
                // 
                // This means that the user is loggin in to the provider site directly.
                // In this case, by logging in, the user will only be recognized by the login provider.
                //
                // Later, when the USER tryes to login to any APPLICATION, one of these may happen:
                //
                //  1) AUTOMATIC LOGIN: allow the user to login,
                //                      without bothering him/her with additional steps
                //
                //  2) CONFIRM LOGIN:   require the user to confirm he/she wishes to login,
                //                      but without having to enter credentials
                //
                //  3) VERIFIED LOGIN:  require the user to enter credentials again...
                //                      even if the user already have the provider authentication cookie
                //
                // Both the USER and the APPLICATION can control this behavior.
                // When they diverge, the most restrictive behavior will be used.
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
                // Let the user login, and get the authentication cookie.
                this.ViewBag.ReturnUrl = application.LoginRedirectUrl;
                return this.View();
            }

            var authTicket = ((CustomPrincipal)this.HttpContext.User).Ticket;
            var user = LoginHelper.GetUser(authTicket.UserId);

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
                //      - notification goes with the login-token and the action-token
                //      - app must respond with "RETURN USER TO <url>" so that we know where to redirect the user to
                //  - the application has the option "Notify User Login" set:
                //      - notification goes with the login-key and the default-login-action-token if it exists
                //      - if LoginRedirectUrl is empty: we must wait for the notification response with "RETURN USER TO <url>"
                //      - if LoginRedirectUrl is filled: we don't wait for any response, and redirect the user immediately sending the login-token through the URL
                //
                // Redirect user to the application,
                // based on the response from the notification,
                // or to the default login return url.
                var loginResult = LoginHelper.LogUserInAngGetReturnUrl(application, user, actionToken, returnUrl, this.Request);

                return this.Redirect(returnUrl);
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

            var usuario = (new[] { new Usuario { Login = "user", Password = "123" } })
                .SingleOrDefault(u => u.Login == viewModel.Login && u.Password == viewModel.Password);

            if (usuario == null)
            {
                this.ModelState.AddModelError(
                    "User name or password are incorrect.",
                    () => viewModel.Login,
                    () => viewModel.Password);
            }

            if (!this.ModelState.IsValid)
                return this.JsonWithModelErrors();

            AuthHelper.SignIn(
                this.Response,
                AuthHelper.CreateTicket(usuario),
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
            LoginHelper.LogoutAll(authTicket.UserId);
            AuthHelper.SignOut(this.Response);
        }
    }
}