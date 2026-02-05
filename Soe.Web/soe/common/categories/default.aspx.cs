using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.common.categories
{
    public partial class _default : PageBase
    {
        protected Feature FeatureEdit = Feature.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);
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
                    case Common.Util.Feature.Common_Categories_Order:
                        FeatureEdit = Feature.Common_Categories_Order_Edit;
                        break;
                    case Common.Util.Feature.Common_Categories_PayrollProduct:
                        FeatureEdit = Feature.Common_Categories_PayrollProduct_Edit;
                        break;
                }
            }
        }
    }
}
