using System;
using System.Diagnostics;

namespace LoginProvider.Models
{
    [DebuggerDisplay("{Id} - {Name}")]
    public class Usuario
    {
        public virtual int Id { get; set; }

        public virtual Guid Guid { get; set; }

        public virtual string Password { get; set; }

        public virtual string Name { get; set; }

        public virtual string Login { get; set; }
    }
}