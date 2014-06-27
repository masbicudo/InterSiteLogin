using System;
using System.Collections.Generic;

namespace LoginProvider.Code
{
    public class LoginInfo
    {
        private readonly Dictionary<Guid, Guid> dicApiKeyToTokenMap
            = new Dictionary<Guid, Guid>();

        public Dictionary<Guid, Guid> ApiKeyToTokenMap
        {
            get { return dicApiKeyToTokenMap; }
        }
    }
}