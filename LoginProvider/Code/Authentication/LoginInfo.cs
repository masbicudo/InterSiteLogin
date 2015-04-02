using System;
using System.Collections.Generic;

namespace LoginProvider.Code.Authentication
{
    public class LoginUserInfo
    {
        private readonly Dictionary<Guid, Guid> dicApplicationGuidToLoginTokenGuidMap
            = new Dictionary<Guid, Guid>();

        // ReSharper disable once InconsistentNaming
        public Dictionary<Guid, Guid> ApplicationGuid_To_LoginTokenGuid_Map
        {
            get { return this.dicApplicationGuidToLoginTokenGuidMap; }
        }

        public RecognizedLoginModes RecognizedLoginMode { get; set; }

        public string Name { get; set; }

        public Guid Guid { get; set; }

        public bool AllowRedirectToHttp { get; set; }
    }
}