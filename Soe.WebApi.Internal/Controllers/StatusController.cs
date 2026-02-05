using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers
{
    [RoutePrefix("SoftOneStatus")]
    public class StatusController : ApiBase
    {
        public StatusController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }
        #region SoftOne Status

        [HttpGet]
        [Route("GetSoftOneStatus")]
        [ResponseType(typeof(SoftOneStatusDTO))]
        public SoftOneStatusDTO Status(Guid guid, string key)
        {
            StatusManager statusManager = new StatusManager();

            if (SoftOneIdConnector.ValidateSuperKey(guid, key))
            {
                return statusManager.GetSoftOneStatusDTO(ServiceType.WebApiInternal);
            }

            return null;
        }

        #endregion

        #region Licenses

        [HttpGet]
        [Route("GetLicenceSysServers")]
        [ResponseType(typeof(List<LicenseSysServer>))]
        public List<LicenseSysServer> GetLicenceSysServers(Guid guid, string key)
        {
            if (SoftOneIdConnector.ValidateSuperKey(guid, key))
            {
                LicenseManager licenseManager = new LicenseManager(null);
                List<LicenseSysServer> list = new List<LicenseSysServer>();

                foreach (var item in licenseManager.GetLicenses())
                    list.Add(new LicenseSysServer() { licenseId = item.LicenseId, sysServerId = item.SysServerId });

                return list;
            }

            return null;
        }

        [HttpGet]
        [Route("ChangeSysServIdOnLicense")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult ChangeSysServIdOnLicense(Guid guid, string key, int licenseId, int? toSysServerId)
        {
            if (SoftOneIdConnector.ValidateSuperKey(guid, key))
            {
                LicenseManager licenseManager = new LicenseManager(null);

                return licenseManager.ChangeSysServIdOnLicense(licenseId, toSysServerId);
            }

            return null;
        }

        [HttpGet]
        [Route("ChangeSysServerId")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult ChangeSysServerId(Guid guid, string key, int licenseI, int? fromSysServerId, int? toSysServerId)
        {
            if (SoftOneIdConnector.ValidateSuperKey(guid, key))
            {
                LicenseManager licenseManager = new LicenseManager(null);

                return licenseManager.ChangeSysServerId(fromSysServerId, toSysServerId);
            }

            return null;
        }

        #endregion
    }

    public class LicenseSysServer
    {
        public int licenseId { get; set; }
        public int? sysServerId { get; set; }
    }

}