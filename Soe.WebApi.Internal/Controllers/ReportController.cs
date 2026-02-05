using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers
{
    namespace Soe.Api.Internal.Controllers
    {
        [RoutePrefix("Report")]
        public class ReportController : ApiBase
        {
            #region Variables

            private string runningPrintReport = "RunningPrintReportKey";

            #endregion

            #region Constructor

            public ReportController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
            {
            }

            #endregion

            [HttpPost]
            [Route("PrintReportPackageData")]
            [ResponseType(typeof(int))]
            public int PrintReportPackageData(List<EvaluatedSelection> evaluatedSelections, int actorCompanyId, int userId, string culture)
            {
                SetLanguage(culture);

                ReportDataManager rdm = new ReportDataManager(GetParameterObject(actorCompanyId, userId));
                return rdm.PrintReportPackageId(evaluatedSelections);
            }

            [HttpPost]
            [Route("PrintReport")]
            [ResponseType(typeof(int))]
            public int PrintReport(EvaluatedSelection es, int actorCompanyId, int userId, string culture)
            {
                int running = 0;
                try
                {
                    SetLanguage(culture);

                    ReportDataManager rdm = new ReportDataManager(GetParameterObject(actorCompanyId, userId));
                    running = BusinessMemoryCache<int>.Get(runningPrintReport);
                    running++;
                    BusinessMemoryCache<int>.Set(runningPrintReport, running, 120);
                    var result = rdm.PrintReportId(es);
                    running = BusinessMemoryCache<int>.Get(runningPrintReport);
                    return result;
                }
                catch (Exception ex)
                {
                    SysLogConnector.LogErrorString("PrintReport controller " + ex.ToString());
                }
                finally
                {
                    if (running > 0)
                    {
                        running--;
                        BusinessMemoryCache<int>.Set(runningPrintReport, running, 120);
                    }
                }
                return 0;
            }

            [HttpPost]
            [Route("PrintReport/Queue/FromSaved")]
            public ReportPrintoutDTO CreateReportGenerationJob(int reportPrintoutId, int actorCompanyId, int userId, int roleId)
            {
                ReportDataManager rdm = new ReportDataManager(GetParameterObject(actorCompanyId, userId, roleId));
                var result = rdm.PrintMigratedReportDTO(reportPrintoutId, actorCompanyId, userId, roleId);

                if (result == null)
                    LogCollector.LogError(Environment.MachineName + " Printed from ApiInternal failed " + reportPrintoutId.ToString());

                return result;
            }

            [HttpPost]
            [Route("PrintReport/Queue")]
            public ReportPrintoutDTO CreateReportGenerationJob([FromBody]ReportJobDefinitionDTO job, int actorCompanyId, int userId, int roleId, bool forcePrint)
            {
                ReportDataManager rdm = new ReportDataManager(GetParameterObject(actorCompanyId, userId, roleId));

                var result = rdm.PrintMigratedReportDTO(job, actorCompanyId, userId, roleId, forcePrint, skipApiInternal: true);
                if (result == null)
                    LogCollector.LogError(Environment.MachineName + " Printed from ApiInternal failed ");

                return result;
            }

            [HttpGet]
            [Route("RunningPrintReport")]
            [ResponseType(typeof(int))]
            public int PrintReport()
            {
                return BusinessMemoryCache<int>.Get(runningPrintReport);
            }

            [HttpPost]
            [Route("PrintReportGetData")]
            [ResponseType(typeof(ReportPrintoutDTO))]
            public ReportPrintoutDTO PrintReportGetData(EvaluatedSelection es, int actorCompanyId, int userId, string culture)
            {
                SetLanguage(culture);

                ReportDataManager rdm = new ReportDataManager(GetParameterObject(actorCompanyId, userId));
                return rdm.PrintReportDTO(es);
            }

            [HttpPost]
            [Route("GenerateReportForEdi")]
            [ResponseType(typeof(byte[]))]
            public int GenerateReportForEdi(List<int> ediEntryIds, int actorCompanyId, int userId, string culture)
            {
                SetLanguage(culture);

                ReportDataManager rdm = new ReportDataManager(GetParameterObject(actorCompanyId, userId));
                rdm.GenerateReportForEdi(ediEntryIds, actorCompanyId);
                return 0;
            }

            [HttpGet]
            [Route("GetNrOfLoads")]
            [ResponseType(typeof(int))]
            public int GetNrOfLoads()
            {
                return 0;
            }

            [HttpGet]
            [Route("GetNrOfDispose")]
            [ResponseType(typeof(int))]
            public int GetNrOfDispose()
            {
                return 0;
            }
        }
    }
}