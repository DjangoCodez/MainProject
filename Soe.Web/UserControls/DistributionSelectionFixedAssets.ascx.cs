using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class DistributionSelectionFixedAssets : ControlBase
    {
        #region Variables

        public NameValueCollection F { get; set; }
        public ReportSelection ReportSelection { get; set; }

        private InventoryManager im;
        private CategoryManager cm;

        #endregion

        public void Populate(bool repopulate)
        {
            #region Init

            im = new InventoryManager(PageBase.ParameterObject);
            cm = new CategoryManager(PageBase.ParameterObject);

            #endregion

            #region Populate

            //Inventories
            Dictionary<int, string> inventories = new Dictionary<int, string>();
            inventories.Add(0, " ");
            var listOfInventories = im.GetInventories(PageBase.SoeCompany.ActorCompanyId);
            foreach (var inventory in listOfInventories)
            {
                inventories.Add(inventory.InventoryId, inventory.InventoryNr + " - " + inventory.Name);
            }
            Inventories.DataSourceFrom = inventories;
            Inventories.DataSourceTo = inventories;

            //Categories
            Dictionary<int, string> categories = new Dictionary<int, string>();
            categories.Add(0, " ");
            var listOfCategories = cm.GetCategories(SoeCategoryType.Inventory, PageBase.SoeCompany.ActorCompanyId);
            foreach (var category in listOfCategories)
            {
                categories.Add(category.CategoryId, category.Code + " - " + category.Name);
            }
            Categories.DataSourceFrom = categories;
            Categories.DataSourceTo = categories;

            //prognosTypes
            var prognosTypes = PageBase.GetGrpText(TermGroup.PrognosTypes).Sort(true, false);
            PrognosType.DataSource = prognosTypes;
            PrognosType.DataTextField = "value";
            PrognosType.DataValueField = "key";
            PrognosType.DataBind();

            #endregion

            #region Set data

            if (repopulate && SoeForm != null && SoeForm.PreviousForm != null)
            {
                Inventories.PreviousForm = SoeForm.PreviousForm;
                Categories.PreviousForm = SoeForm.PreviousForm;
            }
            else
            {
                //if (ReportSelection != null)
                //{
                //    #region ReportSelection

                //    bool foundVoucherSeriesTypeNr = false;
                //    bool foundVoucherNr = false;
                //    IEnumerable<ReportSelectionInt> reportSelectionInts = rm.GetReportSelectionInts(ReportSelection.ReportSelectionId);
                //    foreach (ReportSelectionInt reportSelectionInt in reportSelectionInts)
                //    {
                //        switch (reportSelectionInt.ReportSelectionType)
                //        {
                //            case (int)SoeSelectionData.Int_Voucher_VoucherSeriesId:
                //                Inventories.ValueFrom = reportSelectionInt.SelectFrom.ToString();
                //                Inventories.ValueTo = reportSelectionInt.SelectTo.ToString();
                //                foundVoucherSeriesTypeNr = true;
                //                break;
                //            case (int)SoeSelectionData.Int_Voucher_VoucherNr:
                //                VoucherNr.ValueFrom = reportSelectionInt.SelectFrom.ToString();
                //                VoucherNr.ValueTo = reportSelectionInt.SelectTo.ToString();
                //                foundVoucherNr = true;
                //                break;
                //        }

                //        if (foundVoucherSeriesTypeNr && foundVoucherNr)
                //            break;
                //    }

                //    #endregion
                //}
            }

            #endregion
        }

        public bool Evaluate(SelectionFixedAssets s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return false;

            #region Init

            im = new InventoryManager(PageBase.ParameterObject);
            cm = new CategoryManager(PageBase.ParameterObject);

            if (F == null)
                return false;

            #endregion

            #region Validate input and read interval into SelectionVoucher

            #region Read from Form

            string inventoryNrFrom = F["Inventories-from-1"];
            string inventoryNrTo = F["Inventories-to-1"];
            string categoryNrFrom = F["Categories-from-1"];
            string categoryNrTo = F["Categories-to-1"];
            int prognoseType = Convert.ToInt32(F["PrognosType"]);

            #endregion

            #region Validate interval

            //Validate VoucherSeries and VoucherNr
            if (!Validator.ValidateTextInterval(inventoryNrFrom, inventoryNrTo) || !Validator.ValidateTextInterval(categoryNrFrom, categoryNrTo))
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                return false;
            }

            #endregion

            #region Inventories

            int invIdFrom = 0;
            int invIdTo = 0;


            using (var entites = new CompEntities())
            {
                //From
                if (Int32.TryParse(inventoryNrFrom, out invIdFrom) && invIdFrom > 0)
                {
                    s.InventoryFrom = im.GetInventoryNumber(entites, invIdFrom);                 
                }

                //To
                if (Int32.TryParse(inventoryNrTo, out invIdTo) && invIdTo > 0)
                {
                    s.InventoryTo = im.GetInventoryNumber(entites, invIdTo);
                }

                #endregion

                #region Categories

                int catIdFrom = 0;
                int catIdTo = 0;

                //From            
                if (Int32.TryParse(categoryNrFrom, out catIdFrom) && catIdFrom > 0)
                    s.CategoryFrom = cm.GetCategoryCode(entites, catIdFrom, es.ActorCompanyId);
                

                //To
                if (Int32.TryParse(categoryNrTo, out catIdTo) && catIdTo > 0)
                    s.CategoryTo = cm.GetCategoryCode(entites, catIdTo, es.ActorCompanyId);
            }

            //prognoseType ;
            s.PrognoseType = prognoseType;

            #endregion

            #endregion

            #region Set EvaluatedSelection from SelectionVoucher

            SetEvaluated(s, es);

            #endregion

            return true;
        }

        public void SetEvaluated(SelectionFixedAssets s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return;

            if ((!String.IsNullOrEmpty(s.InventoryFrom) && s.InventoryFrom != "0") || (!String.IsNullOrEmpty(s.InventoryTo) && s.InventoryTo != "0"))
            {
                es.SFA_HasInventoryInterval = true;
                es.SFA_InventoryFrom = s.InventoryFrom;
                es.SFA_InventoryTo = s.InventoryTo;
            }
            else
                es.SFA_HasInventoryInterval = false;

            if ((!String.IsNullOrEmpty(s.CategoryFrom) && s.CategoryFrom != "0") || (!String.IsNullOrEmpty(s.CategoryTo) && s.CategoryTo != "0"))
            {
                es.SFA_HasCategoryInterval = true;
                es.SFA_CategoryFrom = s.CategoryFrom;
                es.SFA_CategoryTo = s.CategoryTo;
            }
            else
                es.SFA_HasCategoryInterval = false;

            es.SFA_PrognoseType = s.PrognoseType;

            //Set as evaluated
            es.SFA_IsEvaluated = true;
        }
    }
}