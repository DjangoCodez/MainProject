using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class Translations : ControlBase
    {
        #region Constants

        private const string ADD_TRANSLATION = "AddTranslation";
        private const string REMOVE_TRANSLATION = "RemoveTranslation";

        #endregion

        #region Variables

        private CompTermsRecordType recordType;
        private int recordId;
        private bool initialized;

        private TermManager tm;

        #endregion

        public void InitControl(CompTermsRecordType recordType, int recordId)
        {
            this.initialized = true;
            this.recordType = recordType;
            this.recordId = recordId;
            this.tm = new TermManager(PageBase.ParameterObject);

            var languages = new Dictionary<int, string>();
            var terms = TermCacheManager.Instance.GetTermGroupContent(TermGroup.Language);
            foreach (var item in terms)
            {
                languages.Add(item.Id, item.Name);
            }
            TranslationCountry.ConnectDataSource(languages);

            RenderTranslations();
        }

        public ActionResult SaveTranslations()
        {
            if (!this.initialized)
                return new ActionResult(false);

            var result = new ActionResult(true);

            string name = PageBase.F["TranslationName"];
            string country = PageBase.F["TranslationCountry"];
            TermGroup_Languages lang = (TermGroup_Languages)Convert.ToInt32(country);

            foreach (string key in PageBase.F.AllKeys)
            {
                if (key.Contains(ADD_TRANSLATION))
                {
                    result = tm.SaveCompTerm(name, this.recordType, this.recordId, (int)lang, PageBase.ParameterObject.ActorCompanyId);
                    if (!result.Success)
                        return result;
                }
                else if (key.Contains(REMOVE_TRANSLATION))
                {
                    string compTermId = key.Substring(key.IndexOf("_") + 1);
                    int deleteCompTermId = Convert.ToInt32(compTermId);
                    if (deleteCompTermId != 0)
                    {
                        result = tm.DeleteCompTerm(deleteCompTermId);
                        if (!result.Success)
                            return result;
                    }                        
                }

            }

            return result;
        }

        private void RenderTranslations()
        {
            bool printHead = true;

            HtmlTableRow tRow;
            HtmlInputButton tButton;
            HtmlTableCell tCell;
            Text label;

            if (this.recordId > 0)
            {
                var compTermTranslations = tm.GetCompTermDTOs(this.recordType, this.recordId, true);
                foreach (var compTermTranslation in compTermTranslations)
                {
                    #region Header

                    if (printHead)
                    {
                        tRow = new HtmlTableRow();

                        //Country
                        tCell = new HtmlTableCell();
                        tCell.Style["WIDTH"] = "150px";
                        label = new Text()
                        {
                            TermID = 4725,
                            DefaultTerm = "Land",
                            FitInTable = true,
                        };
                        tCell.Controls.Add(label);
                        tRow.Cells.Add(tCell);

                        //Name
                        tCell = new HtmlTableCell();
                        tCell.Style["WIDTH"] = "250px";
                        label = new Text()
                        {
                            TermID = 4729,
                            DefaultTerm = "Translation",
                            FitInTable = true,
                        };
                        tCell.Controls.Add(label);
                        tRow.Cells.Add(tCell);

                        ExistingTranslations.Rows.Add(tRow);
                        printHead = false;
                    }

                    #endregion

                    #region Rows

                    tRow = new HtmlTableRow();

                    //Lang
                    tCell = new HtmlTableCell();
                    tCell.Controls.Add(new LiteralControl(compTermTranslation.LangName));
                    tRow.Cells.Add(tCell);

                    //Name
                    tCell = new HtmlTableCell();
                    tCell.Controls.Add(new LiteralControl(compTermTranslation.Name));
                    tRow.Cells.Add(tCell);

                    // Remove button
                    tButton = new HtmlInputButton("submit");
                    tButton.Value = PageBase.GetText(2185, "Ta bort");
                    tButton.ID = String.Format("{0}_{1}", REMOVE_TRANSLATION, compTermTranslation.CompTermId.ToString());

                    tCell = new HtmlTableCell();
                    tCell.Controls.Add(tButton);
                    tRow.Cells.Add(tCell);

                    ExistingTranslations.Rows.Add(tRow);

                    #endregion
                }
            }
        }
    }
}