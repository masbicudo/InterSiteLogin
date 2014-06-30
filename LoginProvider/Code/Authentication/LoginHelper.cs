using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Infra;
using LoginProvider.Code.SystemWebExtensions;

namespace LoginProvider.Code.Authentication
{
    public static class LoginHelper
    {
        private static readonly Dictionary<Guid, LoginUserInfo> dicLoginInfos
            = new Dictionary<Guid, LoginUserInfo>();

        private static readonly Dictionary<Guid, LoginApplicationInfo> dicApplicationInfos
            = new Dictionary<Guid, LoginApplicationInfo>();

        private static readonly Dictionary<long, WebRequestLog> dicRequestsLog
            = new Dictionary<long, WebRequestLog>();

        private static LoginApplicationInfo GetApplication(Guid applicationId)
        {
            LoginApplicationInfo result;
            lock (dicApplicationInfos)
                dicApplicationInfos.TryGetValue(applicationId, out result);
            return result;
        }

        public static LoginApplicationInfo ValidateRequestAndGetApplication(
            string apiKey,
            HttpRequestBase httpRequestBase)
        {
            Guid applicationId;
            if (!Guid.TryParse(apiKey, out applicationId))
                return null;

            var app = GetApplication(applicationId);
            if (app == null)
                return null;

            if (!string.IsNullOrWhiteSpace(app.ReferrerStartsWith))
            {
                if (httpRequestBase.UrlReferrer == null)
                    return null;

                if (!httpRequestBase.UrlReferrer.ToString().StartsWith(app.ReferrerStartsWith))
                    return null;
            }

            return app;
        }

        /// <summary>
        /// Logs an user into an application.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="user"></param>
        /// <param name="actionToken"></param>
        /// <param name="returnUrlFromRequest"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static LoginResult LogUserInAngGetReturnUrl(
            LoginApplicationInfo application,
            LoginUserInfo user,
            string actionToken,
            string returnUrlFromRequest,
            HttpRequestBase request)
        {
            Guid loginToken;
            user.ApplicationId_To_LoginToken_Map[application.Id] = loginToken = Guid.NewGuid();

            // Send notification to the application, that the user logged in, if:
            //  - there is an actionToken:
            //      - notification goes with the login-token and the action-token
            //      - app must respond with "RETURN USER TO <url>" so that we know where to redirect the user to
            //  - the application has the option "NotifyUserLogin" set:
            //      - notification goes with the login-key and the default-login-action-token if it exists
            //      - if LoginRedirectUrl is empty: we must wait for the notification response with "RETURN USER TO <url>"
            //      - if LoginRedirectUrl is filled: we don't wait for any response, and redirect the user immediately sending the login-token through the URL

            // We will now send a signal to the application indicating that the user is authentic.
            UriBuilder uriBuilderNotify = null;
            bool mustWaitResponse = false;
            var callbackType = AppCallbackType.Authenticate;

            if (!string.IsNullOrWhiteSpace(actionToken))
            {
                uriBuilderNotify = new UriBuilder(application.CallbackAuthenticateUrl);
                uriBuilderNotify.AddQueryParameter("actionToken", actionToken);
                mustWaitResponse = true;
                callbackType = AppCallbackType.AuthenticateAction;
            }

            if (application.NotifyUserLogin)
            {
                uriBuilderNotify = new UriBuilder(application.CallbackAuthenticateUrl);
            }

            if (uriBuilderNotify != null)
            {
                uriBuilderNotify.AddQueryParameter("loginToken", loginToken);
                var webrequest = WebRequest.Create(uriBuilderNotify.Uri);
                var startDate = DateTime.Now;
                var task = webrequest.GetResponseAsync()
                    .ContinueWith(
                        t => WebRequestLog(application, user.Id, t.Result, callbackType, t.Exception, startDate));

                if (mustWaitResponse || string.IsNullOrWhiteSpace(application.LoginRedirectUrl))
                {
                    task.Wait();
                    var response = task.Result;
                    if (response.Exception == null)
                    {
                        var match = Regex.Match(response.ResponseBody, @"^RETURN USER TO (?<URL>.*)");
                        if (match.Success)
                        {
                            var returnUrlFromResponse = new Uri(match.Groups["URL"].Value);

                            // We trust all HTTPS redirects, except URLs that are not inside the RootUrl.
                            if (returnUrlFromResponse.Scheme == "https")
                                return GetAndValidateReturnUrl(returnUrlFromResponse, returnUrlFromRequest, application.RootUrls);

                            // We don't redirect back to HTTP when:
                            //  - the user states that he/she don't trust HTTP sites
                            //  - the application does not accept HTTP
                            //  - the referrer is HTTPS and the redirect is HTTP
                            if (returnUrlFromResponse.Scheme == "http"
                                && user.AllowRedirectToHttp
                                && application.AllowRedirectToHttp
                                && (request.UrlReferrer == null || request.UrlReferrer.Scheme == "http"))
                            {
                                return GetAndValidateReturnUrl(returnUrlFromResponse, returnUrlFromRequest, application.RootUrls);
                            }
                        }
                    }

                    // Application call resulted in incorrect response... we should abort user authentication.
                    return new LoginResult(new Exception("Application failed to respond to authentication notification."));
                }
            }

            var uriBuilderRedirect = new UriBuilder(application.LoginRedirectUrl);
            uriBuilderNotify.AddQueryParameter("loginToken", loginToken);

            return GetAndValidateReturnUrl(uriBuilderRedirect.Uri, returnUrlFromRequest, application.RootUrls);
        }

        private static LoginResult GetAndValidateReturnUrl(Uri returnUrl, string relativeUrl, string[] rootUrls)
        {
            rootUrls = rootUrls == null || rootUrls.Length == 0 ? new[] { returnUrl.GetLeftPart(UriPartial.Authority) } : rootUrls;
            var result = new Uri(returnUrl, relativeUrl);

            return rootUrls.Any(rootUrl => result.ToString().StartsWith(rootUrl))
                ? new LoginResult(result)
                : new LoginResult(new Exception("Invalid redirection URL."));
        }

        /// <summary>
        /// Logs the user out of all applications that he/she has logged into.
        /// </summary>
        /// <param name="userId">The id of the user that wants to log out.</param>
        /// <returns>
        /// A list of logout call result to every application the user was logged.
        /// </returns>
        public static IEnumerable<WebRequestLog> LogoutAll(Guid userId)
        {
            LoginUserInfo loginInfo;
            if (!dicLoginInfos.TryGetValue(userId, out loginInfo))
                return Enumerable.Empty<WebRequestLog>();

            var tasks = new List<Task<WebRequestLog>>(loginInfo.ApplicationId_To_LoginToken_Map.Count);

            // When an user logs in an application it is created a token that represents this action.
            // Both user and application are associated with this token.
            // When the user wants to logout, we must destroy all login tokens associated with this user.
            foreach (var kv in loginInfo.ApplicationId_To_LoginToken_Map)
            {
                // Getting the application object, for the current token.
                LoginApplicationInfo app;
                if (!dicApplicationInfos.TryGetValue(kv.Key, out app) || app == null)
                    continue;

                // We will now send a signal to the application indicating that the user wants to logout.
                var uri = new UriBuilder(app.CallbackLogoutUrl);
                uri.Query += (uri.Query.Length > 0 ? "&" : "?") + "user=" + kv.Value.ToString("N");

                var request = WebRequest.Create(uri.Uri);
                var startDate = DateTime.Now;
                var task = request.GetResponseAsync()
                    .ContinueWith(
                        t => WebRequestLog(app, userId, t.Result, AppCallbackType.Logout, t.Exception, startDate));

                tasks.Add(task);
            }

            // ReSharper disable once CoVariantArrayConversion
            // Waiting for all tasks to end, and then saving the logout results.
            Task.WaitAll(tasks.ToArray());

            var logoutResults = tasks.Select(t => t.Result).ToArray();

            lock (dicRequestsLog)
            {
                var nextId = dicRequestsLog.Count + 1;
                foreach (var webRequestLog in logoutResults)
                {
                    webRequestLog.Id = nextId;
                    dicRequestsLog.Add(nextId++, webRequestLog);
                }
            }

            return logoutResults;
        }

        public static LoginUserInfo GetUser(Guid userId)
        {
            LoginUserInfo result;
            lock (dicLoginInfos)
                dicLoginInfos.TryGetValue(userId, out result);
            return result;
        }

        private static WebRequestLog WebRequestLog(
            LoginApplicationInfo application,
            Guid userId,
            WebResponse webResponse,
            AppCallbackType callbackType,
            AggregateException exception,
            DateTime startDate)
        {
            // Here, the request was made, and we need to log the respose, so that we know what happened.
            // An application that fails too often is undesirable, so we can warn the user against using such apps.
            // We also get the initial request date and time-intervals, to measure performance of the app.
            using (webResponse)
            {
                var stream = webResponse.GetResponseStream() ?? new MemoryStream();
                var headResponseDate = DateTime.Now;
                using (var reader = new StreamReader(stream))
                {
                    var body = reader.ReadToEnd();
                    var bodyResponseDate = DateTime.Now;

                    return
                        new WebRequestLog
                        {
                            ApplicationId = application.Id,
                            UserId = userId,
                            CallbackType = callbackType,
                            Exception = exception,
                            ResponseHeaders = webResponse.Headers.GetHeadersAsString(),
                            ResponseBody = body,
                            StartDate = startDate,
                            HeadResponseTime = (headResponseDate - startDate).Milliseconds,
                            BodyResponseTime = (bodyResponseDate - startDate).Milliseconds,
                        };
                }
            }
        }
    }

    public class LoginResult
    {
        public LoginResult(Uri returnUrl)
        {
            this.ReturnUrl = returnUrl;
        }

        public LoginResult(Exception ex)
        {
            this.Exception = ex;
        }

        public Uri ReturnUrl { get; private set; }

        public Exception Exception { get; private set; }
    }
}