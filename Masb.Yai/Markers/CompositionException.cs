using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Masb.Yai.Markers
{
#if FULL_NET
    [System.Runtime.Serialization.Serializable]
#endif
    [DataContract]
    public class CompositionException : Exception
    {
        public CompositionException() { }
        public CompositionException(string message) : base(message) { }
        public CompositionException(string message, Exception inner) : base(message, inner) { }

#if FULL_NET
        protected CompositionException(
            System.Runtime.Serialization.SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
#endif

        [DataMember]
        public override string Message { get { return base.Message; } }

        [DataMember]
        public override string StackTrace { get { return base.StackTrace; } }

        [DataMember]
        public override IDictionary Data { get { return base.Data; } }
    }
}