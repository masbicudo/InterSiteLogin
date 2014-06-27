using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace LoginProvider.Code
{
    public class Dependencies
    {
        private static readonly object BundleHttpContextItemsKey = new object();

        public static void RequireFile([PathReference]string path)
        {
            var bundleInfo = GetBundleInfo();
            var fullPath = HttpContext.Current.Server.MapPath(path);
            if (!File.Exists(fullPath))
                throw new Exception(string.Format("The requested file was not found on server.\n{0}", path));
            bundleInfo.Add(path);
        }

        public static MvcHtmlString RenderStyles()
        {
            var bundleInfo = GetBundleInfo();
            var cssFiles = bundleInfo.GetAllFiles()
                .Where(fn => fn.EndsWith(".css"))
                .Select(UrlFromVirtualOrAbsoluteOrRelativePath);

            var tags = string.Join(
                "\n",
                cssFiles.Select(fn => string.Format(@"<link rel=""stylesheet"" type=""text/css"" href=""{0}"">", fn)));

            return new MvcHtmlString(tags);
        }

        public static object RenderScripts()
        {
            var bundleInfo = GetBundleInfo();

            var cssFiles = bundleInfo.GetAllFiles().Where(fn => fn.EndsWith(".js"))
                .Select(UrlFromVirtualOrAbsoluteOrRelativePath);

            var tags = string.Join(
                "\n",
                cssFiles.Select(fn => string.Format(@"<script type=""text/javascript"" src=""{0}""></script>", fn)));

            return new MvcHtmlString(tags);
        }

        private static string UrlFromVirtualOrAbsoluteOrRelativePath(string virtualOrAbsoluteOrRelativePath)
        {
            var appPath = HttpContext.Current.Request.ApplicationPath;
            var result = virtualOrAbsoluteOrRelativePath == "~" || virtualOrAbsoluteOrRelativePath.StartsWith("~/")
                ? appPath.Trim('/') + '/' + virtualOrAbsoluteOrRelativePath.Substring(1).TrimStart('/')
                : virtualOrAbsoluteOrRelativePath;
            return result;
        }

        private static DependencyInfo GetBundleInfo()
        {
            var items = HttpContext.Current.Items;
            DependencyInfo bundleInfo;
            if (!items.Contains(BundleHttpContextItemsKey))
                items[BundleHttpContextItemsKey] = bundleInfo = new DependencyInfo();
            else
                bundleInfo = (DependencyInfo)items[BundleHttpContextItemsKey];
            return bundleInfo;
        }
    }
}