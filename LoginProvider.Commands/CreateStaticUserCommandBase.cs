using System;
using JetBrains.Annotations;

namespace LoginProvider.Commands
{
    public class CreateStaticUserCommand :
        IActionCommand<string>
    {
        private readonly CreateUserCommand createUserCommand;
        private readonly string userLoginName;

        public CreateStaticUserCommand(
            [NotNull] CreateUserCommand createUserCommand,
            [NotNull] string userLoginName)
        {
            if (createUserCommand == null) throw new ArgumentNullException("createUserCommand");
            if (userLoginName == null) throw new ArgumentNullException("userLoginName");
            this.createUserCommand = createUserCommand;
            this.userLoginName = userLoginName;
        }

        public void Execute(string password)
        {
            this.createUserCommand.Execute(
                new CreateUserCommand.Data
                {
                    Login = this.userLoginName,
                    Password = password
                });
        }
    }
}