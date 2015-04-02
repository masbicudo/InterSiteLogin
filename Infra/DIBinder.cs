using System.Web.Mvc;

namespace Infra
{
    /// <summary>
    /// Binds a model to the default MVC dependency injection resolver.
    /// </summary>
    public class DIBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = DependencyResolver.Current.GetService(bindingContext.ModelType);
            return result;
        }
    }
}