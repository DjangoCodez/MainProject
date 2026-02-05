using System;
using System.Linq;
using System.Globalization;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Web.soe.billing.preferences.productsettings.productunit.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected ProductManager pm;
        private TermManager tm;

        protected ProductUnit unit;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_ProductSettings_ProductUnit_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            pm = new ProductManager(ParameterObject);
            tm = new TermManager(ParameterObject);
            var ccm = new CountryCurrencyManager(ParameterObject);
            //Mandatory parameters

            // Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            // Optional parameters
            int unitId;
            if (Int32.TryParse(QS["unit"], out unitId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    unit = pm.GetPrevNextProductUnit(unitId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (unit != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?unit=" + unit.ProductUnitId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?unit=" + unitId);
                }
                else
                {
                    unit = pm.GetProductUnit(unitId);
                    if (unit == null)
                    {
                        Form1.MessageWarning = GetText(3253, "Enhet hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(3230, "Redigera enhet");
            string registerModeTabHeaderText = GetText(3229, "Registrera enhet");
            PostOptionalParameterCheck(Form1, unit, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = unit != null ? unit.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Set data

            UnitTranslation.Labels = ccm.GetSysCountriesDict(false); // 

            if (unit != null)
            {
                Code.Value = unit.Code;
                Name.Value = unit.Name;

                int pos = 0;
                var data = tm.GetCompTermDTOs(CompTermsRecordType.ProductUnitName, this.unit.ProductUnitId, true);
                foreach (var item in data)
                {
                    this.UnitTranslation.AddLabelValue(pos, ((int)item.Lang).ToString());
                    this.UnitTranslation.AddValueFrom(pos, item.Name);
                    pos++;
                }
            }
            
            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3254, "Enhet sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(3255, "Enhet kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3256, "Enhet uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3257, "Enhet kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3258, "Enhet finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(3259, "Enhet kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1965, "Enhet borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(3260, "Enhet kunde inte tas bort");
                else if (MessageFromSelf == "TRANSLATIONFAILED")
                    Form1.MessageError = GetText(9159, "Översättningar kunde inte sparas");
                else if (MessageFromSelf == "ENTITYINUSE")
                    Form1.MessageError = GetText(9163, "Enheten går inte att ta bort eftersom den används");
            }

            #endregion

            #region Navigation

            if (unit != null)
            {
                Form1.SetRegLink(GetText(3229, "Registrera enhet"), "", 
                    Feature.Billing_Preferences_ProductSettings_ProductUnit_Edit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string code = F["Code"];
            string name = F["Name"];

            if (unit == null)
            {
                // Validation: Unit not already exist
                if (pm.ProductUnitExists(code, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Create Condition
                unit = new ProductUnit()
                {
                    Code = code,
                    Name = name,
                };

                var result = pm.AddProductUnit(unit, SoeCompany.ActorCompanyId);
                if (result.Success)
                {
                    result = this.SaveProductUnits(result.IntegerValue);
                    if (result.Success)
                        RedirectToSelf("SAVED");
                    else
                        RedirectToSelf("TRANSLATIONFAILED");
                }
                else
                {
                    RedirectToSelf("NOTSAVED", true);
                }
            }
            else
            {
                if (unit.Code != code)
                {
                    // Validation: Unit not already exist
                    if (pm.ProductUnitExists(code, SoeCompany.ActorCompanyId))
                        RedirectToSelf("EXIST", true);
                }

                // Update Condition
                unit.Code = code;
                unit.Name = name;

                var result = pm.UpdateProductUnit(unit);
                if (result.Success)
                {
                    result = this.SaveProductUnits(this.unit.ProductUnitId);
                    
                    if (result.Success)
                        RedirectToSelf("UPDATED");
                    else
                        RedirectToSelf("TRANSLATIONFAILED");
                }
                else
                {
                    RedirectToSelf("NOTUPDATED", true);
                }
            }

            RedirectToSelf("FAILED", true);
        }

        private ActionResult SaveProductUnits(int productUnitId)
        {
            ActionResult result;
            var terms = (from term in this.UnitTranslation.GetData(F)
                         where term.LabelType != 0
                         select new CompTermDTO { Lang = term.LabelType.ParseToEnum<TermGroup_Languages>(), Name = term.From, RecordId = productUnitId, RecordType = CompTermsRecordType.ProductUnitName });

            result = tm.DeleteCompTerms(CompTermsRecordType.ProductUnitName, productUnitId);
            result = tm.SaveCompTerms(terms, SoeCompany.ActorCompanyId);

            return result;
        }

        protected override void Delete()
        {
            var result = pm.DeleteProductUnit(unit);
            if (result.Success)
                RedirectToSelf("DELETED", false, true);
            else if(result.ErrorNumber == (int)ActionResultDelete.EntityInUse)
                RedirectToSelf("ENTITYINUSE");
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
