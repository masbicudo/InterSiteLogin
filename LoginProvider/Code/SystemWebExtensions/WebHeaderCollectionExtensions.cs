using System.Linq;
using System.Net;

namespace LoginProvider.Code.SystemWebExtensions
{
    public static class WebHeaderCollectionExtensions
    {
        public static string GetHeadersAsString(this WebHeaderCollection collection)
        {
            var result = string.Join(
                "\r\n",
                collection.AllKeys.Select(
                    key => string.Format(
                        "{0}: {1}",
                        key,
                        collection[key])));

            return result;
        }
    }
}