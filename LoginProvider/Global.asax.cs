using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Infra;
using LoginProvider.Commands;
using LoginProvider.Data;
using LoginProvider.Domain;
using Masb.Yai;
using Masb.Yai.Markers;

namespace LoginProvider
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var composerBuilder = new ExpressionComposerBuilder();

            composerBuilder.Add(
                new NewExpressionFilter<CreateMasterUserCommand>(
                    () => new CreateMasterUserCommand(
                        M.Get<CreateUserCommand>(), // component name = "createUserCommand" inferred from constructor parameter
                        M.Get(
                            //c.Composer.Compose<string>(
                            //    c.ParentContext,
                            //    c.ComponentName + "Xpto", // component name = "masterUserNameAppSetting" + "Xpto"
                            //    c.ReflectedDestinationInfo)
                            () => (Expression<Func<DateTime>>)(() => DateTime.Now)) + "xpto",
                        M.Get<string>("password")) // "password" will be used as the component name
                          {
                              TestProp = M.Get<int>(), // component name = "TestProp" inferred from property name
                              TestType = typeof(M.Parent<M.Parent<M.Current>>),
                              TestObj = new Logger<M.Current>(),
                              TestFunc = () => 1,
                              TestExpr = () => 1,
                          })
                    .AndThen(new MarkerReplacerExpressionFilter()));

            composerBuilder.Add(
                new DecoratorExpressionFilter<CreateMasterUserCommand>(
                    c => M.Get(() => StaticStore.GetExpression(c.Result)))
                .AndThen(new MarkerReplacerExpressionFilter()));

            //composer.Add(
            //    new CustomTypeChangerExpressionFilter(
            //        t => t == typeof(Logger<M.MarkerClass>),
            //        (c, t) => typeof(Logger<>).MakeGenericType(c.ComponentType)));

            //composer.Add(
            //    new RelativeTypeChangerExpressionFilter());

            composerBuilder.Add(
                new NewExpressionFilter<CreateUserCommand>(
                    () => new CreateUserCommand(
                        M.Get<IRepository<User>>(), // component name = "userRepository" inferred from constructor parameter
                        M.Get<HashPasswordCommand>())) // component name = "hashPasswordCommand" inferred from constructor parameter
                .AndThen(new MarkerReplacerExpressionFilter()));

            composerBuilder.Add(
                new NewExpressionFilter<IRepository<User>>(
                    () => new InMemoryRepository<User>()));

            composerBuilder.Add(
                new NewExpressionFilter<HashPasswordCommand>(
                    () => new HashPasswordCommand(
                        M.Get<Guid>())) // component name = "appGuidAppSetting" inferred from constructor parameter
                .AndThen(new MarkerReplacerExpressionFilter()));

            composerBuilder.Add(
                c =>
                {
                    if (c.Result == null)
                    {
                        if (c.IsComponent<Guid>("appGuidAppSetting"))
                        {
                            var value = Guid.Parse(WebConfigurationManager.AppSettings["AppGuid"]);
                            c.SetResult(() => value);
                        }
                        else if (c.IsComponent<string>("masterUserNameAppSetting" + "Xpto"))
                            c.SetResult(() => "Master.User.Name");
                        else if (c.IsComponent<string>("password"))
                            c.SetResult(() => "pwd123");
                    }
                });

            // creating a thread-safe container
            var containerCoreThreadSafe = new ThreadSafeComponentContainer(composerBuilder.ToImmutable());
            containerCoreThreadSafe.GetComponentEntry<CreateMasterUserCommand>();
            var serviceProvider1 = new ServiceProvider(containerCoreThreadSafe);
            var obj1 = (CreateMasterUserCommand)serviceProvider1.GetService(typeof(CreateMasterUserCommand));

            // creating a read-only container
            var components = new ComponentEntryCollectionBuilder(composerBuilder.ToImmutable());
            components.Add<CreateMasterUserCommand>();
            var containerCoreReadonly = new ImmutableComponentContainer(components.ToDictionary());
            var serviceProvider2 = new ServiceProvider(containerCoreReadonly);
            var obj2 = (CreateMasterUserCommand)serviceProvider2.GetService(typeof(CreateMasterUserCommand));


            //container.Register<CreateMasterUserCommand>()
            //    .WithExpression(
            //        () => new CreateMasterUserCommand(
            //            M.Get<CreateUserCommand>(/*"createUserCommand"*/),
            //            M.Get(c => c.Composer.Compose<string>()) + "xpto",
            //            M.Get<string>("password"))
            //        {
            //            TestProp = M.Get<int>(/*"TestProp"*/),
            //            TestType = typeof(M.Parent<M.Parent<M.Current>>),
            //            TestObj = new Logger<M.MarkerClass>(),
            //            TestFunc = () => 1,
            //            TestExpr = () => 1,
            //        })
            //    .DecorateWithExpression(c => M.Get(() => StaticStore.GetExpression(c.Result)))
            //    .ReplaceType<Logger<M.MarkerClass>>(c => typeof(Logger<>).MakeGenericType(c.ServiceType))
            //    ;



            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.DefaultBinder = new DefaultDictionaryBinder();

            var hashPasswordCommand = new HashPasswordCommand(Guid.Parse("C84F1CAF-AC19-4C66-A571-075D73831DFC"));
            IRepository<User> userRepository = new InMemoryRepository<User>();

            var masterUserCreator = new CreateMasterUserCommand(
                new CreateUserCommand(userRepository, hashPasswordCommand),
                "master",
                "1master2");

            masterUserCreator.Execute();
        }

        class Logger<TFor> { }
    }

    public static class RequestStore
    {
        private static readonly CustomKey itemsKey = new CustomKey("RequestStore");

        public static T GetOrCreate<T>(Func<T> func)
        {
            var key = new { itemsKey, t = typeof(T) };
            if (HttpContext.Current.Items.Contains(key))
            {
                var item = (T)HttpContext.Current.Items[key];
                return item;
            }
            else
            {
                var item = func();
                HttpContext.Current.Items[key] = item;
                return item;
            }
        }

        public static Expression<Func<T>> GetExpression<T>()
        {
            var key = new { itemsKey, t = typeof(T) };
            return () => (T)HttpContext.Current.Items[key];
        }
    }
}