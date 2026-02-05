using SoftOne.Soe.Business;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Common.Util;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers
{
    [RoutePrefix("Scheduler")]
    public class SchedulerController : ApiBase
    {
        public SchedulerController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        [HttpGet]
        [Route("Run")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult Run(Guid guid, string key)
        {
            ActionResult result = new ActionResult();

            using (SysScheduledJobManager jm = new SysScheduledJobManager(null))
            {

                if (SoftOneIdConnector.ValidateSuperKey(guid, key))
                {
                    Thread.Sleep(new Random().Next(1000, 5000));
                    Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => jm.RunJobs()));
                }

                return result;
            }
        }

        [HttpGet]
        [Route("Run/OneJob")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult Run(Guid guid, string key, int sysScheduleJobId)
        {
            ActionResult result = new ActionResult();

            using (SysScheduledJobManager jm = new SysScheduledJobManager(null))
            {

                if (SoftOneIdConnector.ValidateSuperKey(guid, key))
                {
                    Thread.Sleep(new Random().Next(1, 1000));
                    Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => jm.RunJobs(sysScheduleJobId)));
                }

                return result;
            }
        }
    }
}