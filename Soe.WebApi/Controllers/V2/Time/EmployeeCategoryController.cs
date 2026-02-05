using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.FlexForceService;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/EmployeeCategory")]
    public class EmployeeCategoryController : SoeApiController
    {
        #region Variables

        private readonly CategoryManager cm;

        #endregion

        #region Constructor

        public EmployeeCategoryController(CategoryManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region Category

        [HttpGet]
        [Route("SmallGenericTypes/{soeCategoryTypeId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult CategorySmallGenericTypes(int soeCategoryTypeId, bool addEmptyRow)
        {
            SoeCategoryType categoryType = (SoeCategoryType)soeCategoryTypeId;
            return Content(HttpStatusCode.OK, cm.GetCategoriesDict(categoryType, base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("GetCategoriesDict/{soeCategoryTypeId:int}/{categoryId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCategoriesDict(int soeCategoryTypeId, int categoryId, bool addEmptyRow)
        {
            SoeCategoryType categoryType = (SoeCategoryType)soeCategoryTypeId;
            return Content(HttpStatusCode.OK, cm.GetCategoriesDict(categoryType, base.ActorCompanyId, addEmptyRow, categoryId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("GetCategoryGroupsDict/{soeCategoryTypeId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCategoryGroupsDict(int soeCategoryTypeId, bool addEmptyRow)
        {
            SoeCategoryType categoryType = (SoeCategoryType)soeCategoryTypeId;
            return Content(HttpStatusCode.OK, cm.GetCategoryGroupsDict(categoryType, base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Grid/{soeCategoryTypeId:int}/{loadCompanyCategoryRecord:bool}/{loadChildren:bool}/{loadCategoryGroups:bool}")]
        public IHttpActionResult GetCategoriesGrid(int soeCategoryTypeId, bool loadCompanyCategoryRecord, bool loadChildren, bool loadCategoryGroups)
        {
            SoeCategoryType categoryType = (SoeCategoryType)soeCategoryTypeId;            
            return Content(HttpStatusCode.OK,  cm.GetCategoryGridDTOs(categoryType, base.ActorCompanyId, loadCompanyCategoryRecord, loadChildren, loadCategoryGroups));
        }
        
        [HttpGet]
        [Route("Category/{categoryId:int}")]
        public IHttpActionResult GetCategory(int categoryId)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoryDTO(categoryId, base.ActorCompanyId, true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Save(CategoryDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, cm.SaveCategory(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{categoryId:int}")]
        public IHttpActionResult DeleteCategory(int categoryId)
        {
            var model = cm.GetCategory(categoryId, base.ActorCompanyId, false);
            return Content(HttpStatusCode.OK, cm.DeleteCategory(model, base.ActorCompanyId));
        }


        [HttpGet]
        [Route("ForRoleFromType/{employeeId:int}/{categoryType:int}/{isAdmin:bool}/{includeSecondary:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCategories(int employeeId, int categoryType, bool isAdmin, bool includeSecondary, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoriesForRoleFromTypeDict(base.ActorCompanyId, base.UserId, employeeId, (SoeCategoryType)categoryType, isAdmin, includeSecondary, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("AccountsByAccount/{accountId:int}/{loadCategory:bool}")]
        public IHttpActionResult GetCategoryAccounts(int accountId, bool loadCategory)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoryAccountsByAccount(accountId, base.ActorCompanyId, loadCategory).ToDTOs());
        }

        #endregion

    }
}