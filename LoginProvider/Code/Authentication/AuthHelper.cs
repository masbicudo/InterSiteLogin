using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Web;
using System.Web.Security;
using LoginProvider.Domain;
using LoginProvider.Models;
using Newtonsoft.Json;

namespace LoginProvider.Code
{
    public static class AuthHelper
    {
        public static void AuthenticateRequest(HttpRequest request)
        {
            var authCookie = request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                Debug.Assert(authTicket != null, "authTicket != null");
                var ticketData = JsonConvert.DeserializeObject<CustomTicketData>(authTicket.UserData);
                Debug.Assert(ticketData.Login == authTicket.Name, "ticketData.Login == authTicket.Name");

                if (authTicket.Version != CustomTicketData.StaticTicketVersion)
                {
                    FormsAuthentication.SignOut();
                    HttpContext.Current.User = null;
                    return;
                }

                var newUser = new CustomPrincipal(
                    new ClaimsIdentity("", ticketData.Login, null),
                    ticketData);
                HttpContext.Current.User = newUser;
            }
        }

        public static CustomTicketData CreateTicket(User user, CustomTicketData ticketDoPersonificador = null)
        {
            var userInfoInTicket = new CustomTicketData
            {
                UserGuid = user.Guid,
                Login = user.Login,
                Name = user.Name,
            };
            return userInfoInTicket;
        }

        public static void SignIn(HttpResponseBase response, CustomTicketData ticketData, TimeSpan? lembrarDeMimDurante = null, bool lembrarDeMimAposFecharBrowser = false)
        {
            var userData = JsonConvert.SerializeObject(ticketData);

            var authTicket = new FormsAuthenticationTicket(
                version: CustomTicketData.StaticTicketVersion,
                name: ticketData.Login,
                issueDate: DateTime.Now,
                expiration: DateTime.Now + (lembrarDeMimDurante ?? TimeSpan.MaxValue),
                isPersistent: lembrarDeMimAposFecharBrowser,
                userData: userData,
                cookiePath: FormsAuthentication.FormsCookiePath);

            var ticket = FormsAuthentication.Encrypt(authTicket);

            response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, ticket));
        }

        public static void SignOut(HttpResponseBase response)
        {
            FormsAuthentication.SignOut();
            response.Redirect("~/");
        }
    }
}