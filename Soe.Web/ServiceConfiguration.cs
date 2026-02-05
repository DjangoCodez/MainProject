using Ninject;
using Ninject.Web.Common;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.DI;
using SoftOne.Soe.Util.DI;
using SoftOne.Soe.Web.Security;
using SoftOne.Soe.Web.Services;
using SoftOne.Soe.Web.Util;
using System.Web;

namespace SoftOne.Soe.Web
{
    public class ServiceConfiguration
    {
        public void RegisterServices(IKernel kernel)
        {
            kernel.Bind<HttpContext>().ToMethod(x => HttpContext.Current);
            kernel.Bind<IConnectionStringCache>().ToConstant(new ConnectionStringCache().RegisterConnectionStringsInConfig());
            kernel.Bind<IParameterObjectProvider>().To<ParameterObjectProvider>().InRequestScope();
            kernel.Bind<ParameterObject>().ToMethod(x => x.Kernel.Get<IParameterObjectProvider>().CreateParameterObject());
            kernel.Bind<CompanyManager>().To<CompanyManager>().InRequestScope();
            kernel.Bind<GeneralManager>().To<GeneralManager>().InRequestScope();
            kernel.Bind<SettingManager>().To<SettingManager>().InRequestScope();
            kernel.Bind<LanguageManager>().To<LanguageManager>().InRequestScope();
            kernel.Bind<CommentManager>().To<CommentManager>().InRequestScope();
            kernel.Bind<EmployeeManager>().To<EmployeeManager>().InRequestScope();
            kernel.Bind<AccountManager>().To<AccountManager>().InRequestScope();
            kernel.Bind<LoginManager>().To<LoginManager>().InRequestScope();
            kernel.Bind<UserManager>().To<UserManager>().InRequestScope();
            kernel.Bind<IClaimsHelper>().ToConstant(new DefaultClaimsHelper("Cookies"));
            kernel.Bind<ITextService>().ToConstant(new TermCacheManagerTextService());
        }
    }
}