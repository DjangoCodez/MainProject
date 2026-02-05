using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Web.Http;
using System.Web.Http.Description;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/ExtraField/ExtraField")]
    public class ExtraFieldController : ApiBase
    {
        #region Constructor

        public ExtraFieldController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all ExtraFields on entity
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ExtraFieldInformation")]
        [ResponseType(typeof(List<ExtraFieldInformation>))]
        public IHttpActionResult GetExtraFieldInformation(Guid companyApiKey, Guid connectApiKey, string token, ExtraFieldEntityType fieldEntityType)
        {
            #region Validation   

            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }
            List<ExtraFieldInformation> extraFieldInformations = new List<ExtraFieldInformation>();
            #endregion
            var param = apiManager.GetParameterObject();
            var extraFieldManager = new ExtraFieldManager(param);
            var featureManager = new FeatureManager(param);

            if (featureManager.HasRolePermission(Feature.Common_ExtraFields, Permission.Modify, param.RoleId, param.ActorCompanyId))
            {
                var fields = extraFieldManager.GetExtraFields(param.ActorCompanyId);
                List<ExtraField> extraFields = new List<ExtraField>();
                if (fields.Any())
                {
                    if (fieldEntityType == ExtraFieldEntityType.Account && featureManager.HasRolePermission(Feature.Common_ExtraFields_Account, Permission.Modify, param.RoleId, param.ActorCompanyId))
                        extraFields.AddRange(fields.Where(w => w.Entity == (int)SoeEntityType.Account));

                    if (fieldEntityType == ExtraFieldEntityType.Employee && featureManager.HasRolePermission(Feature.Common_ExtraFields_Employee, Permission.Modify, param.RoleId, param.ActorCompanyId))
                        extraFields.AddRange(fields.Where(w => w.Entity == (int)SoeEntityType.Employee));

                    if (fieldEntityType == ExtraFieldEntityType.Supplier && featureManager.HasRolePermission(Feature.Common_ExtraFields_Supplier, Permission.Modify, param.RoleId, param.ActorCompanyId))
                        extraFields.AddRange(fields.Where(w => w.Entity == (int)SoeEntityType.Supplier));

                    if (fieldEntityType == ExtraFieldEntityType.Customer && featureManager.HasRolePermission(Feature.Common_ExtraFields_Customer, Permission.Modify, param.RoleId, param.ActorCompanyId))
                        extraFields.AddRange(fields.Where(w => w.Entity == (int)SoeEntityType.Customer));
                }

                foreach (var extrafield in extraFields)
                {
                    if (extrafield.Entity == (int)SoeEntityType.Account)
                        extraFieldInformations.Add(new ExtraFieldInformation()
                        {
                            ExtraFieldEntityType = ExtraFieldEntityType.Account,
                            Key = $"AEF{extrafield.ExtraFieldId}",
                            Name = extrafield.Text
                        });
                    else if (extrafield.Entity == (int)SoeEntityType.Employee)
                        extraFieldInformations.Add(new ExtraFieldInformation()
                        {
                            ExtraFieldEntityType = ExtraFieldEntityType.Employee,
                            Key = $"EEF{extrafield.ExtraFieldId}",
                            Name = extrafield.Text
                        });
                    else if (extrafield.Entity == (int)SoeEntityType.Customer)
                        extraFieldInformations.Add(new ExtraFieldInformation()
                        {
                            ExtraFieldEntityType = ExtraFieldEntityType.Customer,
                            Key = $"CEF{extrafield.ExtraFieldId}",
                            Name = extrafield.Text
                        });
                    else if (extrafield.Entity == (int)SoeEntityType.Supplier)
                        extraFieldInformations.Add(new ExtraFieldInformation()
                        {
                            ExtraFieldEntityType = ExtraFieldEntityType.Supplier,
                            Key = $"SEF{extrafield.ExtraFieldId}",
                            Name = extrafield.Text
                        });
                }
            }
            else
                return Content(HttpStatusCode.Unauthorized, "Permission missing");

            return Content(HttpStatusCode.OK, extraFieldInformations);
        }

        /// <summary>
        /// Get all ExtraFields on entity
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ExtraFieldInformation")]
        [ResponseType(typeof(List<ExtraFieldInformation>))]
        public IHttpActionResult GetExtraFieldRecords(Guid companyApiKey, Guid connectApiKey, string token, ExtraFieldRequest extraFieldRequest)
        {
            var apiManager = new ApiManager(null);
            string validatationResult;
            if (!apiManager.ValidateToken(companyApiKey, connectApiKey, token, out validatationResult))
            {
                return Content(HttpStatusCode.Unauthorized, validatationResult);
            }
            List<ExtraFieldInformation> extraFieldInformations = new List<ExtraFieldInformation>();
            #endregion
            var param = apiManager.GetParameterObject();
            var extraFieldManager = new ExtraFieldManager(param);
            var featureManager = new FeatureManager(param);

            if (featureManager.HasRolePermission(Feature.Common_ExtraFields, Permission.Modify, param.RoleId, param.ActorCompanyId))
            {
                var fields = extraFieldManager.GetExtraFields(param.ActorCompanyId);
                if (extraFieldRequest.ExtraFieldEntityType == ExtraFieldEntityType.Employee)
                {
                    EmployeeManager employeeManager = new EmployeeManager(param);
                    List<Employee> employees = employeeManager.GetEmployeesForUsersAttestRoles(out _, param.ActorCompanyId, param.UserId, param.RoleId);

                    if (employees.Any())
                    {
                        employees = employees.Where(w => extraFieldRequest.Codes.Contains(w.EmployeeNr)).ToList();
                        List<int> employeeIds = employees.Select(s => s.EmployeeId).ToList();
                        extraFieldManager.GetExtraFieldRecords(employeeIds, (int)SoeEntityType.Employee, param.ActorCompanyId);

                        foreach (var employee in employees)
                        {

                        }

                    }
                }
            }

            return Content(HttpStatusCode.OK, new ActionResult()); //TODO
        }


        /// <summary>
        /// Get all GoArticles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Account/")]
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

    }
}