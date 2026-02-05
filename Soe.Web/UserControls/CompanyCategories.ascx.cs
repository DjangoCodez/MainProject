using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class CompanyCategories : ControlBase
    {
        #region Variables

        private bool initialized;
        private CategoryManager cm;

        public Dictionary<int, int> selected;

        #endregion

        #region Properties

        public string CategoryID
        {
            get { return Category.ID; }
        }

        #endregion

        public void InitControl(Controls.Form Form1, string tableID = "")
        {
            cm = new CategoryManager(PageBase.ParameterObject);

            if (tableID != String.Empty)
                Category.ID = tableID;

            this.SoeForm = Form1;

            initialized = true;
        }

        public void Populate(bool repopulate, int actorCompanyId, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int recordId, bool addEmptyRow, string legendHeader = "", string tableID = "")
        {
            if (!initialized)
                return;

            if (tableID != String.Empty)
                Category.ID = tableID;

            selected = new Dictionary<int, int>();
            bool checkbox = false;

            Category.DataSourceFrom = cm.GetCategoriesDict(categoryType, actorCompanyId, addEmptyRow);
            Category.Labels.Add(4108, "Kategori");
            Category.CheckDescription = PageBase.GetText(8562, "Meddelandeavisering");
            
            if (legendHeader != String.Empty)
                LegendHeader.InnerText = legendHeader;
            else
                LegendHeader.InnerText = PageBase.GetText(4101, "Kategorisering");

            if (categoryRecordEntity == SoeCategoryRecordEntity.AttestRole)
                checkbox = true;

            Category.EnableCheck = checkbox;

            if (repopulate && SoeForm.PreviousForm != null)
            {
                Category.PreviousForm = SoeForm.PreviousForm;
            }
            else
            {
                if (recordId > 0 && actorCompanyId > 0)
                {
                    int pos = 0;
                    List<CompanyCategoryRecord> records = cm.GetCompanyCategoryRecords(categoryType, categoryRecordEntity, recordId, actorCompanyId);
                    foreach (CompanyCategoryRecord record in records)
                    {
                        if (!record.CategoryReference.IsLoaded)
                            record.CategoryReference.Load();

                        Category.AddLabelValue(pos, String.Empty);
                        Category.AddValueFrom(pos, record.Category.Name);
                        Category.AddValueCheck(pos, record.IsExecutive);
                        Category.AddValueHidden(pos, record.IsExecutive.ToString());
                        Category.EnableCheck = checkbox;
                        selected.Add(pos, record.Category.CategoryId);

                        pos++;
                        if (pos == Category.NoOfIntervals)
                            break;
                    }
                }
            }
        }

        public bool Save(NameValueCollection F, int actorCompanyId, SoeCategoryType type, SoeCategoryRecordEntity categoryRecordEntity, int recordId, string tableID = "")
        {
            if (cm == null)
                cm = new CategoryManager(PageBase.ParameterObject);

            if (tableID != String.Empty)
                Category.ID = tableID;

            Collection<FormIntervalEntryItem> formIntervalEntryItems = Category.GetData(F);
            return cm.SaveCompanyCategoryRecords(formIntervalEntryItems, actorCompanyId, type, categoryRecordEntity, recordId).Success;
        }

        public bool HasIntervals(NameValueCollection F, string tableID = "")
        {
            if (tableID != String.Empty)
                Category.ID = tableID;

            return Category.HasIntervals(F);
        }

        public Collection<FormIntervalEntryItem> GetData(NameValueCollection F, string tableID = "")
        {
            if (tableID != String.Empty)
                Category.ID = tableID;

            return Category.GetData(F);
        }
    }
}

