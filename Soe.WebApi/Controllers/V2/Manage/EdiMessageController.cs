using System.Net;
using System.Web.Http;
using Banker.Shared.Types;
using Soe.Edi.Common.DTO;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Banker;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using static Soe.Edi.Common.Enumerations;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/System")]
    public class EdiMessageController : SoeApiController
    {
        #region Variables

        private readonly SysServiceManager ssm;

        #endregion

        #region Constructor

        public EdiMessageController(SysServiceManager ssm)
        {
            this.ssm = ssm;
        }

        #endregion

        #region SysEdiMessageHead

        [HttpGet]
        [Route("Edi/SysEdiMessageHead")]
        public IHttpActionResult GetSysEdiMessageHeads()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageHeads());
        }

        [HttpGet]
        [Route("Edi/SysEdiMessageHeadMsg/{sysEdiMessageHeadId}")]
        public IHttpActionResult GetSysEdiMessageHeadMsg(int sysEdiMessageHeadId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageHeadMessage(sysEdiMessageHeadId));
        }

        [HttpGet]
        [Route("Edi/SysEdiMessageGridHead/{status}/{take}/{missingSysCompanyId}/{ediMessageHeadId:int?}")]
        public IHttpActionResult SysEdiMessageGridHead(SysEdiMessageHeadStatus status, int take, bool missingSysCompanyId, int? ediMessageHeadId = null)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageGridHeads(status, take, missingSysCompanyId, ediMessageHeadId));
        }

        [HttpGet]
        [Route("Edi/SysEdiMessagesGrid/{open}/{closed}/{raw}/{missingSysCompanyId}")]
        public IHttpActionResult SysEdiMessagesGrid(bool open, bool closed, bool raw, bool missingSysCompanyId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessagesGridHeads(open, closed, raw, missingSysCompanyId));
        }

        [HttpGet]
        [Route("Edi/SysEdiMessageHead/{sysEdiMessageHead}")]
        public IHttpActionResult GetSysEdiMessageHead(int sysEdiMessageHead)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageHead(sysEdiMessageHead));
        }

        [HttpPost]
        [Route("Edi/SysEdiMessageHead")]
        public IHttpActionResult SysEdiMessageHead(SysEdiMessageHeadDTO model)
        {
            var result = ssm.SaveSysEdiMessageHead(model);
            if(result.Success){
                result.IntegerValue = model.SysEdiMessageHeadId;
            }
            return Content(HttpStatusCode.OK,result);
        }

        #endregion
    }
}