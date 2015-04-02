using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace LoginProvider.Commands
{
    public class CreateMasterUserCommand : IActionCommand
    {
        private readonly CreateUserCommand createUserCommand;
        private readonly string masterUserNameAppSetting;
        private readonly string masterUserPasswordAppSetting;

        public int TestProp { get; set; }
        public Type TestType { get; set; }
        public object TestObj { get; set; }
        public Func<int> TestFunc { get; set; }
        public Expression<Func<int>> TestExpr { get; set; }

        public CreateMasterUserCommand(
            [NotNull] CreateUserCommand createUserCommand,
            [NotNull] string masterUserNameAppSetting,
            [NotNull] string masterUserPasswordAppSetting)
        {
            this.createUserCommand = createUserCommand;
            this.masterUserNameAppSetting = masterUserNameAppSetting;
            this.masterUserPasswordAppSetting = masterUserPasswordAppSetting;
        }

        public void Execute()
        {
            this.createUserCommand.Execute(
                new CreateUserCommand.Data
                {
                    Login = this.masterUserNameAppSetting,
                    Password = this.masterUserPasswordAppSetting
                });
        }
    }
}