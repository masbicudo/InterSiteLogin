using LoginProvider.Code;
using LoginProvider.Models;
using LoginProvider.ViewModels.Auth;
using System;
using System.Linq;
using System.Web.Mvc;

namespace LoginProvider.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string apiKey, string showLoginPage)
        {
            var application = LoginHelper.GetApplication(apiKey);

            var guidApiKey = new Guid(apiKey);

            if (!string.IsNullOrWhiteSpace(application.ReferrerStartsWith))
            {
                if (this.Request.UrlReferrer == null || !this.Request.UrlReferrer.ToString().StartsWith(application.ReferrerStartsWith))
                {
                    this.ViewBag.NotSafe = true;
                    return this.View();
                }
            }

            if (this.HttpContext.User.Identity.IsAuthenticated && showLoginPage != "always")
            {
                var authTicket = ((CustomPrincipal)this.HttpContext.User).Ticket;
                var userToken = LoginHelper.CreateUserToken(guidApiKey, authTicket.Id);
                var url = new UriBuilder(application.CallbackLoginUrl);
                if (!userToken.HasValue)
                {
                    this.ViewBag.NotSafe = true;
                    return this.View();
                }

                url.Query += (url.Query.Length > 0 ? "&" : "?") + "userToken=" + userToken.Value.ToString("N");
                return this.Redirect(url.Uri.ToString());
            }

            this.ViewBag.ReturnUrl = application.CallbackLoginUrl;
            return this.View();
        }

        // Post: /Auth/Login
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

            return this.Json(
                string.IsNullOrWhiteSpace(returnUrl)
                    ? this.Url.Action("Index", "Home")
                    : returnUrl);
        }

        public void Logout()
        {
            var authTicket = ((CustomPrincipal)this.HttpContext.User).Ticket;
            LoginHelper.LogoutAll(authTicket.Id);
            AuthHelper.SignOut(this.Response);
        }
    }
}
