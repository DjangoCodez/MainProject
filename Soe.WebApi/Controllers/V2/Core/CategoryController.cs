using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Category")]
    public class CategoryController : SoeApiController
    {
        #region Variables

        private readonly CategoryManager cm;

        #endregion

        #region Constructor

        public CategoryController(CategoryManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region Category

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetCategoryTypesByPermission()
        {
            return Content(HttpStatusCode.OK, cm.GetCategoryTypeByPermission());
        }

        [HttpGet]
        [Route("Grid/{soeCategoryTypeId:int}/{categoryId:int?}")]
        public IHttpActionResult GetCategoryGrid(int soeCategoryTypeId, int? categoryId = null)
        {
            SoeCategoryType categoryType = (SoeCategoryType)soeCategoryTypeId;
            return Content(HttpStatusCode.OK, cm.GetCategoriesForGrid(categoryType, base.ActorCompanyId, categoryId));
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetCategoriesGrid([FromUri] int soeCategoryTypeId, [FromUri] bool loadCompanyCategoryRecord, [FromUri] bool loadChildren, [FromUri] bool loadCategoryGroups)
        {
            SoeCategoryType categoryType = (SoeCategoryType)soeCategoryTypeId;
            return Content(HttpStatusCode.OK, cm.GetCategoryDTOs(categoryType, base.ActorCompanyId, loadCompanyCategoryRecord, loadChildren, loadCategoryGroups));
        }

        [HttpGet]
        [Route("Dict")]
        public IHttpActionResult GetCategoriesDict([FromUri]SoeCategoryType categoryType, [FromUri] bool addEmptyRow, [FromUri] int? excludeCategoryId = null)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoriesDict(categoryType, base.ActorCompanyId, addEmptyRow, excludeCategoryId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ForRoleFromType/{employeeId:int}/{categoryType:int}/{isAdmin:bool}/{includeSecondary:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCategories(int employeeId, int categoryType, bool isAdmin, bool includeSecondary, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoriesForRoleFromTypeDict(base.ActorCompanyId, base.UserId, employeeId, (SoeCategoryType)categoryType, isAdmin, includeSecondary, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Category/CompCategoryRecords/{soeCategoryTypeId:int}/{categoryRecordEntity:int}/{recordId:int}")]
        public IHttpActionResult GetCompCategoryRecords(int soeCategoryTypeId, int categoryRecordEntity, int recordId)
        {
            return Content(HttpStatusCode.OK, cm.GetCompanyCategoryRecords((SoeCategoryType)soeCategoryTypeId, (SoeCategoryRecordEntity)categoryRecordEntity, recordId, base.ActorCompanyId).ToDTOs(false));
        }

        [HttpGet]
        [Route("AccountsByAccount/{accountId:int}/{loadCategory:bool}")]
        public IHttpActionResult GetCategoryAccounts(int accountId, bool loadCategory)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoryAccountsByAccount(accountId, base.ActorCompanyId, loadCategory).ToDTOs());
        }

        [HttpGet]
        [Route("{categoryId:int}")]
        public IHttpActionResult GetCategory(int categoryId)
        {
            return Content(HttpStatusCode.OK, cm.GetCategory(categoryId, base.ActorCompanyId).ToDTO(true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveCategory(CategoryDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, cm.SaveCategory(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{categoryId:int}")]
        public IHttpActionResult DeleteCategory(int categoryId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteCategory(categoryId, base.ActorCompanyId));
        }

        #endregion
    }
}