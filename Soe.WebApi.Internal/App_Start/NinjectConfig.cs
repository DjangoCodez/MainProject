using Ninject;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Soe.Api.Internal
{
    public class NinjectConfig
    {
        public static void Configure()
        {
            StandardKernel kernel = new StandardKernel();
            RegisterServices(kernel);
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);
        }

        public static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<WebApiInternalParamObject>().ToMethod(ctx => GetInternalParameterObject());
            kernel.Bind<ParameterObject>().ToMethod(ctx => GetParameterObject());
        }

        #region Help-methods

        private static WebApiInternalParamObject GetInternalParameterObject()
        {
            return (WebApiInternalParamObject)HttpContext.Current.GetOwinContext().Environment["Soe.WebApiInternalParamObject"];
        }

        private static ParameterObject GetParameterObject()
        {
            return ParameterObject.Empty();
        }

        #endregion
    }
}