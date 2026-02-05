using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using System;
using System.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SoftOne.Soe.Web.App_Start.DependencyInjectionSetUp), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(SoftOne.Soe.Web.App_Start.DependencyInjectionSetUp), "Stop")]

namespace SoftOne.Soe.Web.App_Start
{
    public static class DependencyInjectionSetUp
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();
        public static IKernel Kernel => bootstrapper.Kernel;

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        public static void PerformDependencyInjection(object page)
        {
            bootstrapper.Kernel.Inject(page);
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new NinjectSettings { AllowNullInjection = true });
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                new ServiceConfiguration().RegisterServices(kernel);

                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }
    }
}