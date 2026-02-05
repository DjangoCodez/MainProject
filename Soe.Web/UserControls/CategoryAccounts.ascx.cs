using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class CategoryAccounts : ControlBase
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

        public void Populate(bool repopulate, int actorCompanyId, int accountId, bool addEmptyRow, string legendHeader = "", string tableID = "")
        {
            if (!initialized)
                return;

            if (tableID != String.Empty)
                Category.ID = tableID;

            selected = new Dictionary<int, int>();

            Category.DataSourceFrom = cm.GetCategoriesDict(SoeCategoryType.Employee, actorCompanyId, addEmptyRow);
            Category.Labels.Add(4108, "Kategori");

            if (legendHeader != String.Empty)
                LegendHeader.InnerText = legendHeader;
            else
                LegendHeader.InnerText = PageBase.GetText(4101, "Kategorisering");

            if (repopulate && SoeForm.PreviousForm != null)
            {
                Category.PreviousForm = SoeForm.PreviousForm;
            }
            else
            {
                if (accountId > 0 && actorCompanyId > 0)
                {
                    int pos = 0;
                    var categoryAccounts = cm.GetCategoryAccountsByAccount(accountId, actorCompanyId, true);
                    foreach (var categoryAccount in categoryAccounts)
                    {
                        if (!categoryAccount.CategoryReference.IsLoaded)
                            categoryAccount.CategoryReference.Load();

                        Category.AddLabelValue(pos, String.Empty);
                        Category.AddValueFrom(pos, categoryAccount.Category.Name);
                        selected.Add(pos, categoryAccount.Category.CategoryId);

                        pos++;
                        if (pos == Category.NoOfIntervals)
                            break;
                    }
                }
            }
        }

        public bool Save(NameValueCollection F, int actorCompanyId, int accountId, string tableID = "")
        {
            if (cm == null)
                cm = new CategoryManager(PageBase.ParameterObject);

            if (tableID != String.Empty)
                Category.ID = tableID;

            Collection<FormIntervalEntryItem> formIntervalEntryItems = Category.GetData(F);
            return cm.SaveCategoryAccounts(formIntervalEntryItems, accountId, actorCompanyId).Success;
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

