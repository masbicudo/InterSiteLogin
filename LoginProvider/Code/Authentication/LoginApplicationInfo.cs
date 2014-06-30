using System;
using System.Collections.Generic;

namespace LoginProvider.Code.Authentication
{
    public class LoginApplicationInfo
    {
        public LoginApplicationInfo()
        {
            this.WebRequestLogs = new List<WebRequestLog>();
        }

        public string ReferrerStartsWith { get; set; }

        public string LoginRedirectUrl { get; set; }

        public string CallbackLogoutUrl { get; set; }

        public List<WebRequestLog> WebRequestLogs { get; private set; }

        public Guid Id { get; set; }

        public RecognizedLoginModes RecognizedLoginMode { get; set; }

        public string CallbackAuthenticateUrl { get; set; }

        public bool NotifyUserLogin { get; set; }

        public bool AllowRedirectToHttp { get; set; }

        public string[] RootUrls { get; set; }
    }
}