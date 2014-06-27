using System;

namespace LoginProvider.Code
{
    [Serializable]
    public class JsonModelErrorData
    {
        public string Message { get; set; }
        public string[] Members { get; set; }
    }
}