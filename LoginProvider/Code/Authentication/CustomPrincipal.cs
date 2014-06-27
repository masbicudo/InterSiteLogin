using System.Security.Claims;
using System.Security.Principal;

namespace LoginProvider.Code
{
    public class CustomPrincipal : ClaimsPrincipal
    {
        public CustomPrincipal(IIdentity mainIdentity, CustomTicketData ticketData)
            : base(mainIdentity)
        {
            this.Ticket = ticketData;
        }

        public CustomTicketData Ticket { get; private set; }
    }
}