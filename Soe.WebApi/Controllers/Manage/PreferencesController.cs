using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/Preferences")]
    public class PreferencesController : SoeApiController
    {
        #region Variables

        private readonly SettingManager sm;
        private readonly FieldSettingManager fsm;

        #endregion

        #region Constructor

        public PreferencesController(SettingManager sm, FieldSettingManager fsm)
        {
            this.sm = sm;
            this.fsm = fsm;
        }

        #endregion

        #region FieldSettings

        [HttpGet]
        [Route("FieldSettings/{type:int}")]
        public IHttpActionResult GetFieldsAndSettingsForGrid(int type)
        {
            return Content(HttpStatusCode.OK, fsm.GetFieldsAndSettings((SoeFieldSettingType)type, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("FieldSettings/")]
        public IHttpActionResult GetAttestRolesForMeeting(FieldSettingDTO fieldSetting)
        {
            return Content(HttpStatusCode.OK, fsm.SaveFieldSettings(fieldSetting, base.ActorCompanyId));
        }

        #endregion

        #region Check Settings

        [HttpGet]
        [Route("Settings/Areas/")]
        public IHttpActionResult GetAreas()
        {
            return Content(HttpStatusCode.OK, sm.GetCheckSettingAreas(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Settings/Check/")]
        public IHttpActionResult CheckSettings(List<int> areas)
        {
            return Content(HttpStatusCode.OK, sm.CheckSettings(areas));
        }

        #endregion
    }
}