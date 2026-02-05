using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Web;
using System.Web.UI;

namespace SoftOne.Soe.Web.Controls
{
    [ParseChildren(true)]
    public class TabView : SoeTabView, IFormControl
    {
    }

    public class Tab : SoeTab, IFormControl
    {
        public int TermID { get; set; }
        public string DefaultTerm { get; set; }
        public string HeaderTextSetting { get; set; }

        public SoeTabViewType Type { get; set; }
        public override string ImgAlt { get; set; }
        public override string ImgSrc
        {
            get
            {
                switch (Type)
                {
                    case SoeTabViewType.Edit:
                        return "/img/edit.png";
                    case SoeTabViewType.Setting:
                        return "/img/gear_edit.png";
                    case SoeTabViewType.View:
                        return "/img/view.png";
                    case SoeTabViewType.Admin:
                        return "/img/worker.png";
                    case SoeTabViewType.Import:
                        return "/img/import.png";
                    case SoeTabViewType.Export:
                        return "/img/export.png";
                    default:
                        //Ensure that all SoeTab's has a valid Type
                        throw new SoeGeneralException("Invalid SoeTabViewType", this.ToString());
                }
            }
        }

        /// <summary>Cache the previous HeaderText. Used to repopulate</summary>
        private string previousHeaderText;
        protected string PreviousHeaderText
        {
            get
            {
                try
                {
                    if (String.IsNullOrEmpty(previousHeaderText))
                        previousHeaderText = ((Page)HttpContext.Current.Handler).Session[Constants.SESSION_SOETAB_PREVIOUS_HEADERTEXT] as string;
                    return previousHeaderText;
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                    return "";
                }
            }
            set
            {
                try
                {
                    previousHeaderText = value;
                    ((Page)HttpContext.Current.Handler).Session[Constants.SESSION_SOETAB_PREVIOUS_HEADERTEXT] = previousHeaderText;
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }
            }
        }

        public override string HeaderText
        {
            get
            {
                string headerText = "";

                //HeaderTextSetting and PreviousHeaderText only available for Tab 1
                if (this.TabNo == 1)
                {
                    if (!String.IsNullOrEmpty(HeaderTextSetting))
                    {
                        headerText = HeaderTextSetting;
                    }
                    else
                    {
                        if (TermID > 0 && !String.IsNullOrEmpty(DefaultTerm))
                            headerText = this.GetText(TermID, DefaultTerm);
                    }

                    if (String.IsNullOrEmpty(headerText))
                        headerText = PreviousHeaderText;

                    PreviousHeaderText = headerText;
                }
                else
                {
                    if (TermID > 0 && !String.IsNullOrEmpty(DefaultTerm))
                        headerText = this.GetText(TermID, DefaultTerm);
                }

                if (String.IsNullOrEmpty(headerText))
                {
                    switch (Type)
                    {
                        case SoeTabViewType.Edit:
                            headerText = this.GetText(2214, "Redigera");
                            break;
                        case SoeTabViewType.Setting:
                            headerText = this.GetText(26, "Inställningar");
                            break;
                        case SoeTabViewType.View:
                            headerText = "";
                            break;
                        case SoeTabViewType.Admin:
                            headerText = "";
                            break;
                        case SoeTabViewType.Import:
                            headerText = this.GetText(1174, "Importera");
                            break;
                        case SoeTabViewType.Export:
                            headerText = this.GetText(1292, "Exportera");
                            break;
                    }
                }

                return headerText;
            }
        }
    }
}