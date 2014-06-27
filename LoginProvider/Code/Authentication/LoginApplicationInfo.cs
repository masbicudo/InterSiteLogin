using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoginProvider.Code.Authentication
{
    public class LoginApplicationInfo
    {
        public string ReferrerStartsWith { get; set; }

        public Uri CallbackLoginUrl { get; set; }

        public string CallbackServiceUrl { get; set; }
    }
}
