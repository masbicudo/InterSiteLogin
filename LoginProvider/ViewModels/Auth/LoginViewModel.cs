using System.ComponentModel.DataAnnotations;
using LoginProvider.Code.Authentication;

namespace LoginProvider.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        public string Name { get; set; }

        public RecognizedLoginModes RecognizedLoginMode { get; set; }
    }
}