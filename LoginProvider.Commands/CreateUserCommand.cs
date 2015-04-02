using System;
using System.Linq;
using JetBrains.Annotations;
using LoginProvider.Domain;

namespace LoginProvider.Commands
{
    public class CreateUserCommand :
        IActionCommand<CreateUserCommand.Data>
    {
        public class Data
        {
            [NotNull]
            public string Login { get; set; }

            [NotNull]
            public string Password { get; set; }
        }

        private readonly IRepository<User> userRepository;
        private readonly HashPasswordCommand hashPasswordCommand;

        public CreateUserCommand([NotNull] IRepository<User> userRepository,
            [NotNull] HashPasswordCommand hashPasswordCommand)
        {
            if (userRepository == null) throw new ArgumentNullException("userRepository");
            if (hashPasswordCommand == null) throw new ArgumentNullException("hashPasswordCommand");
            this.userRepository = userRepository;
            this.hashPasswordCommand = hashPasswordCommand;
        }

        public void Execute(Data data)
        {
            if (!this.userRepository.Queryable.Any(x => x.Login == data.Login))
            {
                // creating master user
                var user = this.userRepository.CreateNew();
                user.Login = data.Login;

                // creating random password salt
                var bits1 = new byte[16];
                var random = new Random();
                random.NextBytes(bits1);
                var bits2 = Guid.NewGuid().ToByteArray();
                user.PasswordSalt = bits1.Concat(bits2).ToArray();

                // hasing password
                user.Password = this.hashPasswordCommand.Execute(
                    new HashPasswordCommand.Data
                    {
                        PartialSalt = user.PasswordSalt,
                        Password = data.Password
                    });

                this.userRepository.Save(user);
            }
        }
    }
}