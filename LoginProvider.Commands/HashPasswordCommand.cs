using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace LoginProvider.Commands
{
    public class HashPasswordCommand :
        IFuncCommand<HashPasswordCommand.Data, byte[]>
    {
        private readonly Guid appGuid;

        public HashPasswordCommand(Guid appGuidAppSetting)
        {
            this.appGuid = appGuidAppSetting;
        }

        public class Data
        {
            [NotNull]
            public string Password { get; set; }

            [NotNull]
            public byte[] PartialSalt { get; set; }
        }

        public byte[] Execute(Data data)
        {
            var salt = data.PartialSalt.Concat(this.appGuid.ToByteArray()).ToArray();
            var pwdbits = Encoding.UTF8.GetBytes(data.Password);
            var pwdsaltbits = pwdbits.Concat(salt).ToArray();
            var hashbits = MD5.Create().ComputeHash(pwdsaltbits);
            return hashbits;
        }
    }
}