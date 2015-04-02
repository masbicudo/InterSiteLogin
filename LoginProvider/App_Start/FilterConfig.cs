using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace LoginProvider
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}