using Ninject;
using SoftOne.Soe.Business.Billing.Template.Managers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.Core.Mobile;
using SoftOne.Soe.Business.Core.Template;
using SoftOne.Soe.Business.Core.Template.Managers;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.Template.Managers;
using SoftOne.Soe.Data;
using System.Web;
using System.Web.Http;

namespace Soe.WebApi.App_Start
{
    public static class NinjectConfig
    {
        public static void Configure()
        {
            StandardKernel kernel = new StandardKernel();
            RegisterServices(kernel);
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);
        }

        public static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ParameterObject>().ToMethod(ctx => GetParameterObject());

            //TODO: First type could be replaced with interface to enable possibillity for unit-testing
            kernel.Bind<AccountBalanceManager>().To<AccountBalanceManager>()
                .WithConstructorArgument("actorCompanyId", x => GetParameterObject().ActorCompanyId);
            kernel.Bind<TimeEngineManager>().To<TimeEngineManager>()
                .WithConstructorArgument("actorCompanyId", x => GetParameterObject().ActorCompanyId)
                .WithConstructorArgument("userId", x => GetParameterObject().UserId)
                .WithConstructorArgument("roleId", x => GetParameterObject().RoleId);

            kernel.Bind<LoggerManager>().To<LoggerManager>();
            kernel.Bind<ApiManager>().To<ApiManager>();
            kernel.Bind<ApiDataManager>().To<ApiDataManager>();
            kernel.Bind<BridgeManager>().To<BridgeManager>();
            kernel.Bind<ExtraFieldManager>().To<ExtraFieldManager>();
            kernel.Bind<SieManager>().To<SieManager>();
            kernel.Bind<ImportExportManager>().To<ImportExportManager>();
            kernel.Bind<ImportExportCacheManager>().To<ImportExportCacheManager>();
            kernel.Bind<MobileManager>().To<MobileManager>();
            kernel.Bind<TimeTreeAttestManager>().To<TimeTreeAttestManager>();
            kernel.Bind<TimeTreePayrollManager>().To<TimeTreePayrollManager>();
            kernel.Bind<CompanyTemplateManager>().To<CompanyTemplateManager>();
            kernel.Bind<AttestTemplateManager>().To<AttestTemplateManager>();
            kernel.Bind<BillingTemplateManager>().To<BillingTemplateManager>();
            kernel.Bind<CoreTemplateManager>().To<CoreTemplateManager>();
            kernel.Bind<EconomyTemplateManager>().To<EconomyTemplateManager>();
            kernel.Bind<TimeTemplateManager>().To<TimeTemplateManager>();           
        }

        #region Help-methods

        private static ParameterObject GetParameterObject()
        {
            return (ParameterObject)HttpContext.Current.GetOwinContext().Environment["Soe.ParameterObject"];
        }

        #endregion
    }
}
