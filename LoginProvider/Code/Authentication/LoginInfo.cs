using System;
using System.Collections.Generic;

namespace LoginProvider.Code.Authentication
{
    public class LoginUserInfo
    {
        private readonly Dictionary<Guid, Guid> dicApplicationIdToLoginTokenMap
            = new Dictionary<Guid, Guid>();

        // ReSharper disable once InconsistentNaming
        public Dictionary<Guid, Guid> ApplicationId_To_LoginToken_Map
        {
            get { return this.dicApplicationIdToLoginTokenMap; }
        }

        public RecognizedLoginModes RecognizedLoginMode { get; set; }

        public string Name { get; set; }

        public Guid Id { get; set; }

        public bool AllowRedirectToHttp { get; set; }
    }
}