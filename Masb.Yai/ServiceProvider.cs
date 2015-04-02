using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    /// <summary>
    /// Service provider implementation that uses a <see cref="ComponentContainer"/>
    /// to obtain the required services.
    /// </summary>
    public class ServiceProvider :
        IServiceProviderEx
    {
        private readonly ComponentContainer componentContainer;

        public ServiceProvider(ComponentContainer componentContainer)
        {
            this.componentContainer = componentContainer;
        }

        public virtual object GetService(Type serviceType)
        {
            var entry = this.componentContainer.GetComponentEntry(serviceType);
            if (entry == null)
                throw new Exception(string.Format("Service value not found for type {0}", serviceType.Name));

            var result = entry.GetValueAsObject();
            return result;
        }

        public virtual Func<object> GetServiceFunc(Type serviceType)
        {
            var entry = this.componentContainer.GetComponentEntry(serviceType);
            if (entry == null)
                throw new Exception(string.Format("Service value not found for type {0}", serviceType.Name));

            var result = entry.GetObjectGetter();
            return result;
        }

        public virtual Expression GetServiceExpression(Type serviceType)
        {
            var entry = this.componentContainer.GetComponentEntry(serviceType);
            if (entry == null)
                throw new Exception(string.Format("Service value not found for type {0}", serviceType.Name));

            var result = entry.GetExpression();
            return result;
        }

        public virtual bool TryGetService<T>(out T outService)
        {
            var entryT = this.componentContainer.GetComponentEntry<T>();
            if (entryT == null)
            {
                outService = default(T);
                return false;
            }

            outService = entryT.GetValue().GetValueOrDefault();
            return true;
        }

        public virtual bool TryGetServiceFunc<T>(out Func<T> outGetter)
        {
            var entryT = this.componentContainer.GetComponentEntry<T>();
            if (entryT == null)
            {
                outGetter = null;
                return false;
            }

            outGetter = entryT.GetGetter();
            return true;
        }

        public virtual bool TryGetServiceExpression<T>(out Expression<Func<T>> outExpression)
        {
            var entryT = this.componentContainer.GetComponentEntry<T>();
            if (entryT == null)
            {
                outExpression = null;
                return false;
            }

            outExpression = entryT.GetLambdaExpression();
            return true;
        }

        public virtual T GetService<T>()
        {
            var entryT = this.componentContainer.GetComponentEntry<T>();
            if (entryT == null)
                throw new Exception(string.Format("Service value not found for type {0}", typeof(T).Name));

            var result = entryT.GetValue();
            if (!result.HasValue)
                return default(T);

            return result.Value;
        }

        public virtual Func<T> GetServiceFunc<T>()
        {
            var entryT = this.componentContainer.GetComponentEntry<T>();
            if (entryT == null)
                throw new Exception(string.Format("Service value not found for type {0}", typeof(T).Name));

            var result = entryT.GetGetter();
            return result;
        }

        public virtual Expression<Func<T>> GetServiceExpression<T>()
        {
            var entryT = this.componentContainer.GetComponentEntry<T>();
            if (entryT == null)
                throw new Exception(string.Format("Service value not found for type {0}", typeof(T).Name));

            var result = entryT.GetLambdaExpression();
            return result;
        }
    }
}