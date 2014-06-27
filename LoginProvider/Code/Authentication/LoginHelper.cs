using LoginProvider.Code.Authentication;
using System;
using System.Collections.Generic;
using System.Net;

namespace LoginProvider.Code
{
    public static class LoginHelper
    {
        private static readonly Dictionary<Guid, LoginInfo> dicLoginInfos
            = new Dictionary<Guid, LoginInfo>();

        private static readonly Dictionary<Guid, LoginApplicationInfo> dicApplicationInfos
            = new Dictionary<Guid, LoginApplicationInfo>();

        public static LoginApplicationInfo GetApplication(string apiKey)
        {
            throw new NotImplementedException();
        }

        public static Guid? CreateUserToken(Guid apiKey, Guid userId)
        {
            LoginInfo loginInfo;
            if (!dicLoginInfos.TryGetValue(userId, out loginInfo))
                return null;

            Guid token;
            loginInfo.ApiKeyToTokenMap[apiKey] = token = Guid.NewGuid();

            return token;
        }

        public static void LogoutAll(Guid userId)
        {
            LoginInfo loginInfo;
            if (!dicLoginInfos.TryGetValue(userId, out loginInfo))
                return;

            var callbackUrls = new List<Uri>(loginInfo.ApiKeyToTokenMap.Count);
            foreach (var kv in loginInfo.ApiKeyToTokenMap)
            {
                LoginApplicationInfo app;
                if (dicApplicationInfos.TryGetValue(kv.Key, out app) && app != null)
                {
                    var uri = new UriBuilder(app.CallbackServiceUrl);
                    uri.Query += (uri.Query.Length > 0 ? "&" : "?") + "logout=" + kv.Value.ToString("N");

                var request = WebRequest.Create(uri.Uri);
                request.GetResponseAsync()
                    .ContinueWith(
                        t =>
                        {
                            if (t.IsFaulted)
                            {
                                app.Exceptions.Add(t.Exception);
                            }
                            else
                            {
                                t.Result.Headers.
                            }
                        });
                }
            }
        }
    }
}