using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Sales/License")]
    public class InternalLicenseController : ApiBase
    {
        #region Constructor

        public InternalLicenseController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        /// Get all Licenses
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Licenses/")]
        [ResponseType(typeof(List<LicenseDTO>))]
        public IHttpActionResult GetLicenses()
        {
            var licenseManager = new LicenseManager(null);
            return Content(HttpStatusCode.OK, licenseManager.GetLicenses(true).ToDTOs());
        }

        /// <summary>
        /// Get all GoArticles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GoArticles/")]
        [ResponseType(typeof(List<SysXEArticleDTO>))]
        public IHttpActionResult GetGoArticles()
        {
            var featureManager = new FeatureManager(null);
            return Content(HttpStatusCode.OK, featureManager.GetSysXEArticles().ToDTOs());
        }

        /// <summary>
        /// Get all LicenseArticles for license
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("LicenseArticles/OrgNr")]
        [ResponseType(typeof(List<SysXEArticleDTO>))]
        public IHttpActionResult GetGoArticles(string orgNr)
        {
            var featureManager = new FeatureManager(null);
            var licenseManager = new LicenseManager(null);
            var license = licenseManager.GetLicenseByOrgNr(orgNr);
            List<LicenseArticleDTO> articles = new List<LicenseArticleDTO>();

            if (license != null)
                articles = licenseManager.GetLicenseArticles(license.LicenseId).ToDTOs();

            var XEArticles = featureManager.GetSysXEArticles().ToDTOs();
            XEArticles = XEArticles.Where(w => articles.Select(s => s.SysXEArticleId).Contains(w.SysXEArticleId)).ToList();

            foreach (var XEArticle in XEArticles)
            {
            
            }

            return Content(HttpStatusCode.OK, XEArticles);
        }

        /// <summary>
        /// Get all LicenseArticles for license
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("LicenseArticles/")]
        [ResponseType(typeof(List<LicenseArticleDTO>))]
        public IHttpActionResult GetGoArticles(int licenseId)
        {
            var licenseManager = new LicenseManager(null);
            return Content(HttpStatusCode.OK, licenseManager.GetLicenseArticles(licenseId).ToDTOs());
        }

        /// <summary>
        /// Get all LicenseArticles for license
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("LicenseArticles/OrgNr")]
        [ResponseType(typeof(List<LicenseArticleDTO>))]
        public IHttpActionResult GetGoArticlesFromOrgNr(string orgNr)
        {
            var licenseManager = new LicenseManager(null);
            var license = licenseManager.GetLicenseByOrgNr(orgNr);
            List<LicenseArticleDTO> articles = new List<LicenseArticleDTO>();

            if (license != null)
                articles = licenseManager.GetLicenseArticles(license.LicenseId).ToDTOs();

            return Content(HttpStatusCode.OK, articles);
        }

        #endregion
    }
}