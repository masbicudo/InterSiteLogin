using System;
using System.Diagnostics;

namespace LoginProvider.Models
{
    [DebuggerDisplay("{Id} - {Name}")]
    public class Usuario
    {
        public string Password { get; set; }

        public string Name { get; set; }

        public string Login { get; set; }

        public Guid Id { get; set; }
    }
}