using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class SettingsController : SoeApiController
    {
        #region Variables

        private readonly SettingManager sm;

        #endregion

        #region Constructor

        public SettingsController(SettingManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region UserCompanySetting

        [HttpGet]
        [Route("UserCompanySetting/User/{settingTypeIds}")]
        public IHttpActionResult GetUserSettings(string settingTypeIds)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.User, StringUtility.SplitNumericList(settingTypeIds), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/Company/{settingTypeIds}")]
        public IHttpActionResult GetCompanySettings(string settingTypeIds)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.Company, StringUtility.SplitNumericList(settingTypeIds), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/UserAndCompany/{settingTypeIds}")]
        public IHttpActionResult GetUserAndCompanySettings(string settingTypeIds)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.UserAndCompany, StringUtility.SplitNumericList(settingTypeIds), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/License/{settingTypeIds}")]
        public IHttpActionResult GetLicenseSettings(string settingTypeIds)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.License, StringUtility.SplitNumericList(settingTypeIds), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/License/ForEdit")]
        public IHttpActionResult GetLicenseSettingsForEdit()
        {
            return Content(HttpStatusCode.OK, sm.GetLicenseSettingsForEdit(base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting")]
        public IHttpActionResult SaveUserCompanySettings(List<UserCompanySettingEditDTO> settings)
        {
            return Content(HttpStatusCode.OK, sm.SaveUserCompanySettings(settings, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        #region Single setting

        [HttpGet]
        [Route("UserCompanySetting/Bool/{settingMainType:int}/{settingType:int}")]
        public IHttpActionResult GetBoolSetting(int settingMainType, int settingType)
        {
            return Content(HttpStatusCode.OK, sm.GetBoolSetting((SettingMainType)settingMainType, settingType, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/Int/{settingMainType:int}/{settingType:int}")]
        public IHttpActionResult GetIntSetting(int settingMainType, int settingType)
        {
            return Content(HttpStatusCode.OK, sm.GetIntSetting((SettingMainType)settingMainType, settingType, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/String/{settingMainType:int}/{settingType:int}")]
        public IHttpActionResult GetStringSetting(int settingMainType, int settingType)
        {
            return Content(HttpStatusCode.OK, sm.GetStringSetting((SettingMainType)settingMainType, settingType, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting/Bool")]
        public IHttpActionResult SaveBoolSetting(SaveUserCompanySettingModel model)
        {
            return Content(HttpStatusCode.OK, sm.UpdateInsertBoolSetting((SettingMainType)model.SettingMainType, model.SettingTypeId, model.BoolValue, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting/Int")]
        public IHttpActionResult SaveIntSetting(SaveUserCompanySettingModel model)
        {
            return Content(HttpStatusCode.OK, sm.UpdateInsertIntSetting((SettingMainType)model.SettingMainType, model.SettingTypeId, model.IntValue, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting/String")]
        public IHttpActionResult SaveStringSetting(SaveUserCompanySettingModel model)
        {
            return Content(HttpStatusCode.OK, sm.UpdateInsertStringSetting((SettingMainType)model.SettingMainType, model.SettingTypeId, model.StringValue, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        #endregion

        #endregion

    }
}