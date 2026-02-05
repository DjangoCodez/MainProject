using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/FieldSetting")]
    public class FieldSettingController : SoeApiController
    {
        #region Variables

        private readonly SettingManager sm;
        private readonly FieldSettingManager fsm;

        #endregion

        #region Constructor

        public FieldSettingController(SettingManager sm, FieldSettingManager fsm)
        {
            this.sm = sm;
            this.fsm = fsm;
        }

        #endregion

        #region FieldSettings

        [HttpGet]
        [Route("Grid/{type:int}/{formId:int?}")]
        public IHttpActionResult GetFieldSettings(int type, int? formId = null)
        {
            return Content(HttpStatusCode.OK, fsm.GetFieldsAndSettings((SoeFieldSettingType)type, base.ActorCompanyId, formId));
        }


        [HttpPost]
        [Route("FieldSettings/")]
        public IHttpActionResult SaveFieldSetting(FieldSettingDTO fieldSetting)
        {
            return Content(HttpStatusCode.OK, fsm.SaveFieldSettings(fieldSetting, base.ActorCompanyId));
        }


        #endregion
    }
}