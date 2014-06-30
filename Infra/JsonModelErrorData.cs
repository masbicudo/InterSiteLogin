using System;

namespace Infra
{
    [Serializable]
    public class JsonModelErrorData
    {
        public string Message { get; set; }
        public string[] Members { get; set; }
    }
}