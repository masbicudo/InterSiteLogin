﻿using System;

namespace Infra
{
    [Serializable]
    public class JsonResponseData
    {
        public string RedirectUrl { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Obj { get; set; }
        public string ErrorType { get; set; }
        public int Status { get; set; }

        public JsonModelErrorData[] ModelErrors { get; set; }
    }
}