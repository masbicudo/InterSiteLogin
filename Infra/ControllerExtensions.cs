using System.Linq;
using System.Web.Mvc;

namespace Infra
{
    public static class ControllerExtensions
    {
        public static JsonResult JsonWithModelErrors(this Controller controller)
        {
            var data = new JsonResponseData();

            var errors = controller.ModelState.SelectMany(
                kv => kv.Value.Errors.Select(
                    e => new
                    {
                        fld = kv.Key,
                        msg = string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? string.Join(
                                "\n",
                                e.Exception.GetPath(x => x.InnerException)
                                    .Select(ex => string.Format("{0}: {1}", ex.GetType().Name, ex.Message)))
                            : e.ErrorMessage
                    }));

            var errorsByMessage = errors
                .GroupBy(e => e.msg)
                .Select(g => new JsonModelErrorData
                {
                    Message = g.Key,
                    Members = g.Select(m => m.fld).ToArray()
                })
                .ToArray();

            data.ModelErrors = errorsByMessage;

            return new JsonResult
            {
                Data = data,
                ContentType = null,
                ContentEncoding = null,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}