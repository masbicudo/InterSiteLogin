using System;
using System.Web;

namespace Infra
{
    public static class UriBuilderExtensions
    {
        public static void AddQueryParameter(this UriBuilder uriBuilder, string name, object value)
        {
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
            queryParams.Add(name, (value ?? "").ToString());
            uriBuilder.Query = queryParams.ToString();
        }
    }
}