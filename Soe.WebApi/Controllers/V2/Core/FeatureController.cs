using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class FeatureController : SoeApiController
    {
        #region Variables

        private readonly FeatureManager fm;

        #endregion

        #region Constructor

        public FeatureController(FeatureManager fm)
        {
            this.fm = fm;
        }

        #endregion

        #region Feature

        //        public IHttpActionResult HasReadOnlyPermissions([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] featureIds)
        [HttpGet]
        [Route("Feature/ReadOnlyPermission/{featureIds}")]
        public IHttpActionResult HasReadOnlyPermissions(string featureIds)
        {
            List<Feature> features = new List<Feature>();
            foreach (int featureId in StringUtility.SplitNumericList(featureIds))
            {
                features.Add((Feature)featureId);
            }
            if (base.ParameterObject != null)
                base.ParameterObject.SetThread("WebApi");
            return Content(HttpStatusCode.OK, fm.HasRolePermissions(features, Permission.Readonly, base.LicenseId, base.ActorCompanyId, base.RoleId));
        }

        //        public IHttpActionResult HasModifyPermissions([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] featureIds)
        [HttpGet]
        [Route("Feature/ModifyPermission/{featureIds}")]
        public IHttpActionResult HasModifyPermissions(string featureIds)
        {
            List<Feature> features = new List<Feature>();
            foreach (int featureId in StringUtility.SplitNumericList(featureIds))
            {
                features.Add((Feature)featureId);
            }
            if (base.ParameterObject != null)
                base.ParameterObject.SetThread("WebApi");
            return Content(HttpStatusCode.OK, fm.HasRolePermissions(features, Permission.Modify, base.LicenseId, base.ActorCompanyId, base.RoleId));
        }

        #endregion
    }
}