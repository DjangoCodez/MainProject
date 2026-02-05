using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Common.Util;
using System.Net.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/EmailTemplates")]
    public class EmailTemplatesController : SoeApiController
    {
        #region Variables

        private readonly EmailManager emm;

        #endregion

        #region Constructor

        public EmailTemplatesController(EmailManager emm)
        {
            this.emm = emm;
        }

        #endregion


        #region Email

        [HttpGet]
        [Route("EmailTemplates/")]
        public IHttpActionResult GetEmailTemplates(int? id)
        {
            return Content(HttpStatusCode.OK, emm.GetEmailTemplates(base.ActorCompanyId, id).ToDTOs());
        }

        [HttpGet]
        [Route("EmailTemplates/ByType/{type:int}")]
        public IHttpActionResult GetEmailTemplatesByType(int type)
        {
            return Content(HttpStatusCode.OK, emm.GetEmailTemplatesByType(base.ActorCompanyId, type).ToDTOs());
        }

        [HttpGet]
        [Route("EmailTemplate/{emailTemplateId:int}")]
        public IHttpActionResult GetEmailTemplate(int emailTemplateId)
        {
            return Content(HttpStatusCode.OK, emm.GetEmailTemplate(emailTemplateId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("EmailTemplate")]
        public IHttpActionResult SaveEmailTemplate(EmailTemplateDTO emailTemplate)
        {
            return Content(HttpStatusCode.OK, emm.SaveEmailTemplate(emailTemplate, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EmailTemplate/{emailTemplateId:int}")]
        public IHttpActionResult DeleteEmailTemplate(int emailTemplateId)
        {
            return Content(HttpStatusCode.OK, emm.DeleteEmailTemplate(emailTemplateId, base.ActorCompanyId));
        }

        #endregion


    }
}