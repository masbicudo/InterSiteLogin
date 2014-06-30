using System;

namespace LoginProvider.Code
{
    public class WebRequestLog
    {
        public long Id { get; set; }

        public Guid ApplicationId { get; set; }

        public Guid UserId { get; set; }

        public AggregateException Exception { get; set; }

        public string ResponseHeaders { get; set; }

        public string ResponseBody { get; set; }

        public DateTime StartDate { get; set; }

        public int HeadResponseTime { get; set; }

        public int BodyResponseTime { get; set; }

        public AppCallbackType CallbackType { get; set; }
    }
}