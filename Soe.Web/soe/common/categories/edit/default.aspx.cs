using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Web.soe.common.categories.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected CategoryManager cm;
        protected Category category;
        protected int actorCompanyId;
        protected int type;

        //Module specifics
        private Feature FeatureEdit = Feature.None;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            // Add scripts and style sheets
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Common_Categories_Product:
                        FeatureEdit = Feature.Common_Categories_Product_Edit;
                        break;
                    case Feature.Common_Categories_Customer:
                        FeatureEdit = Feature.Common_Categories_Customer_Edit;
                        break;
                    case Feature.Common_Categories_Supplier:
                        FeatureEdit = Feature.Common_Categories_Supplier_Edit;
                        break;
                    case Feature.Common_Categories_ContactPersons:
                        FeatureEdit = Feature.Common_Categories_ContactPersons_Edit;
                        break;
                    case Feature.Common_Categories_AttestRole:
                        FeatureEdit = Feature.Common_Categories_AttestRole_Edit;
                        break;
                    case Feature.Common_Categories_Employee:
                        FeatureEdit = Feature.Common_Categories_Employee_Edit;
                        break;
                    case Feature.Common_Categories_Project:
                        FeatureEdit = Feature.Common_Categories_Project_Edit;
                        break;
                    case Feature.Common_Categories_Contract:
                        FeatureEdit = Feature.Common_Categories_Contract_Edit;
                        break;
                    case Feature.Common_Categories_Inventory:
                        FeatureEdit = Feature.Common_Categories_Inventory_Edit;
                        break;
                    case Feature.Common_Categories_Order:
                        FeatureEdit = Feature.Common_Categories_Order_Edit;
                        break;
                    case Feature.Common_Categories_PayrollProduct:
                        FeatureEdit = Feature.Common_Categories_PayrollProduct_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CategoryManager(ParameterObject);

            //Mandatory parameters
            if (!Int32.TryParse(QS["type"], out type))
                throw new SoeQuerystringException("type", this.ToString());

            //Mode             
            string absolutePathAndMandatoryQS = Request.Url.AbsolutePath;
            string pathAndQuery = Request.Url.PathAndQuery;
            
            if (Mode == SoeFormMode.Copy)
            {
                Mode = SoeFormMode.RegisterFromCopy;                
                Response.Redirect(absolutePathAndMandatoryQS + "?type=" + type);
            }
            else if (Mode == SoeFormMode.StopSettings)
            {
                Mode = SoeFormMode.NoSettingsApplied;
                Response.Redirect(pathAndQuery);
            }
            else if (Mode == SoeFormMode.RunSettings)
            {
                Mode = SoeFormMode.WithSettingsApplied;
                Response.Redirect(pathAndQuery);
            }

            //Optional parameters
            int categoryId;
            if (Int32.TryParse(QS["category"], out categoryId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    category = cm.GetPrevNextCategory(categoryId, (SoeCategoryType)type, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (category != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?category=" + category.CategoryId + "&type=" + type);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?category=" + categoryId + "&type=" + type);
                }
                else
                {
                    category = cm.GetCategory(categoryId, SoeCompany.ActorCompanyId);
                    if (category == null)
                    {
                        Form1.MessageWarning = GetText(4109, "Kategori hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }            

            //Mode
            string editModeTabHeaderText = GetText(4110, "Redigera kategori");
            string registerModeTabHeaderText = GetText(4111, "Registrera kategori");
            PostOptionalParameterCheck(Form1, category, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = category != null ? category.Name : "";

            #endregion

            #region Actions

            bool back = Mode == SoeFormMode.Back;

            if (Form1.IsPosted)
            {
                Save();
            }
            else if (back)
            {
                ClearSoeFormObject();
                Back();
            }

            #endregion

            #region Populate

            ParentCategory.ConnectDataSource(cm.GetCategoriesDict((SoeCategoryType)type, SoeCompany.ActorCompanyId, true, excludeCategoryId: (category != null ? category.CategoryId : (int?)null)));
            CategoryGroup.ConnectDataSource(cm.GetCategoryGroupsDict((SoeCategoryType)type, SoeCompany.ActorCompanyId, true));

            #endregion

            #region Set data

            if (category != null)
            {
                Code.Value = category.Code;
                Name.Value = category.Name;
                if (category.ParentId.HasValue)
                    ParentCategory.Value = category.ParentId.Value.ToString();
                if (category.CategoryGroupId.HasValue)
                    CategoryGroup.Value = category.CategoryGroupId.Value.ToString();
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(4113, "Kategori sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(5183, "Kategori kunde inte sparas");
                else if (MessageFromSelf == "NOTSAVED_EXISTS")
                    Form1.MessageError = GetText(3364, "Kategori kunde inte sparas, kod och/eller namn finns redan på annan kategori");
                else if (MessageFromSelf == "NOTSAVED_INVALIDCHAIN")
                    Form1.MessageError = GetText(8720, "Kategori kunde inte sparas, vald underkategori skapar oändlig kedja av kategorier");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(4112, "Kategori uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(4117, "Kategori kunde inte uppdateras");
                else if (MessageFromSelf == "NOTUPDATED_EXISTS")
                    Form1.MessageError = GetText(3365, "Kategori kunde inte uppdateras, kod och/eller namn finns redan på annan kategori");
                else if (MessageFromSelf == "NOTUPDATED_INVALIDCHAIN")
                    Form1.MessageError = GetText(8720, "Kategori kunde inte sparas, vald underkategori skapar oändlig kedja av kategorier");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(4116, "Kategori finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(4115, "Kategori kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1967, "Kategori borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(4114, "Kategori kunde inte tas bort");
                else if (MessageFromSelf == "NOTDELETED_CATEGORYEXISTS")
                    Form1.MessageError = GetText(8758, "Kategori kunde inte tas bort, den används");
                else Form1.MessageError = MessageFromSelf;
            }

            #endregion

            #region Navigation

            if (category != null)
            {
                Form1.SetRegLink(GetText(4119, "Registrera kategori"), "?type=" + type,
                    FeatureEdit, Permission.Modify);
            }

            #endregion
        }       

        #region Action-methods

        protected override void Save()
        {
            string categoryCode = F["Code"];
            string categoryName = F["Name"];
            int? parentId = StringUtility.GetNullableInt(F["ParentCategory"]);
            int? groupId = StringUtility.GetNullableInt(F["CategoryGroup"]);

            //Validation
            if (String.IsNullOrEmpty(categoryName))
                RedirectToSelf("NOTSAVED", true);

            if (category == null)
            {
                category = new Category()
                {
                    Code = categoryCode,
                    Name = categoryName,
                    Type = type
                };

                //Set Parent
                if (parentId.HasValue && parentId.Value > 0)
                    category.ParentId = parentId.Value;
                //Group
                if (groupId.HasValue && groupId.Value > 0)
                    category.CategoryGroupId = groupId.Value;

                ActionResult result = cm.AddCategory(category, SoeCompany.ActorCompanyId);
                if (result.Success)
                    RedirectToSelf("SAVED");
                else
                {
                    if (result.ErrorNumber == (int)ActionResultSave.CategoryExists)
                        RedirectToSelf("NOTSAVED_EXISTS", true);
                    else if (result.ErrorNumber == (int)ActionResultSave.CategoryInvalidChain)
                        RedirectToSelf("NOTSAVED_INVALIDCHAIN", true);
                    else
                        RedirectToSelf("NOTSAVED", true);
                }
            }
            else
            {
                category.Code = categoryCode;
                category.Name = categoryName;

                //Set Parent
                if (parentId.HasValue && parentId.Value > 0)
                    category.ParentId = parentId.Value;
                else
                    category.ParentId = null;

                //Set group
                if (groupId.HasValue && groupId.Value > 0)
                    category.CategoryGroupId = groupId.Value;
                else
                    category.CategoryGroupId = null;

                ActionResult result = cm.UpdateCategory(category, SoeCompany.ActorCompanyId);
                if (result.Success)
                    RedirectToSelf("UPDATED");
                else
                {
                    if (result.ErrorNumber == (int)ActionResultSave.CategoryExists)
                        RedirectToSelf("NOTUPDATED_EXISTS", true);
                    else if (result.ErrorNumber == (int)ActionResultSave.CategoryInvalidChain)
                        RedirectToSelf("NOTUPDATED_INVALIDCHAIN", true);
                    else
                        RedirectToSelf("NOTUPDATED", true);
                }
            }
            RedirectToSelf("FAILED", true);
        }        

        protected override void Delete()
        {
            var result = cm.DeleteCategory(category, SoeCompany.ActorCompanyId);
            if (result.Success)
            {
                string postBackUrlQs = "&type=" + type;
                RedirectToSelf("DELETED", postBackUrlQs);
            }
            else
            {
                if (result.ErrorNumber == (int)ActionResultDelete.CategoryHasCompanyCategoryRecords)
                    RedirectToSelf(result.ErrorMessage, true);
                else
                    RedirectToSelf("NOTDELETED", true);
            }                
        }

        protected void Back()
        {
            string sectionUrl = "";

            switch (type)
            {
                case (int)SoeCategoryType.AttestRole:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_ATTEST);
                    break;
                case (int)SoeCategoryType.ContactPerson:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_CONTACTPERSONS);
                    break;
                case (int)SoeCategoryType.Contract:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_CONTRACT);
                    break;
                case (int)SoeCategoryType.Customer:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_CUSTOMER);
                    break;
                case (int)SoeCategoryType.Employee:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_EMPLOYEE);
                    break;
                case (int)SoeCategoryType.Inventory:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_INVENTORY);
                    break;
                case (int)SoeCategoryType.Order:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_ORDER);
                    break;
                case (int)SoeCategoryType.PayrollProduct:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_PAYROLL);
                    break;
                case (int)SoeCategoryType.Product:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_PRODUCT);
                    break;
                case (int)SoeCategoryType.Project:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_PROJECT);
                    break;
                case (int)SoeCategoryType.Supplier:
                    sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_SUPPLIER);
                    break;
            }

            
            string reportsUrl = sectionUrl + "categories/";
            Response.Redirect(reportsUrl + "?type=" + type);
        }

        #endregion
    }
}
