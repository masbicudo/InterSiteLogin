using System;

namespace LoginProvider.Code
{
    public class CustomTicketData
    {
        public static readonly int StaticTicketVersion = 1;

        public CustomTicketData()
        {
            this.Version = StaticTicketVersion;
        }

        public Guid Id { get; set; }

        public string Login { get; set; }

        public string Name { get; set; }

        public int Version { get; set; }
    }
}