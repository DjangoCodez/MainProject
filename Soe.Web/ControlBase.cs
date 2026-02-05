using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Controls;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Web.UI;

namespace SoftOne.Soe.Web
{
    /// <summary>
    /// Base class providing base functionality for Controls
    /// </summary>	
    public abstract class ControlBase : UserControl
    {
        #region PageBase

        private PageBase pageBase = null;
        public PageBase PageBase
        {
            get
            {
                if (pageBase == null)
                    pageBase = Page as PageBase;
                if (pageBase == null)
                    pageBase = (PageBase)System.Web.HttpContext.Current.Handler;
                return pageBase;
            }
        }

        #endregion

        #region Url

        public SoeModule SoeModule
        {
            get
            {
                return PageBase.SoeModule;
            }
        }

        public string Module
        {
            get
            {
                string module = PageBase.Module;

                //No module is available for Form and Field setting pages
                if (String.IsNullOrEmpty(module) && IsFieldOrFormSettingPage)
                    module = LastModule;

                //Update last Module
                LastModule = module;

                return module;
            }
        }

        public string LastModule
        {
            get
            {
                return PageBase.LastModule;
            }
            set
            {
                PageBase.LastModule = value;
            }
        }

        public string Section
        {
            get
            {
                return PageBase.Section;
            }
        }

        public bool IsFieldOrFormSettingPage
        {
            get
            {
                return PageBase.IsFieldOrFormSettingPage;
            }
        }

        #endregion

        #region Form

        /// <summary>
        /// The SoeForm control the control exist in
        /// </summary>
        public Form SoeForm { get; set; }

        #endregion

        #region Action-methods

        /// <summary>
        /// Appply settings for any SoeFormEntryBase that was added dynamically in Render method of a UserControl.
        /// </summary>
        /// <param name="entry"></param>
        protected void ApplySettings(SoeFormEntryBase entry)
        {
            if (SoeForm != null)
            {
                //Enable FieldSetting
                IEntryControl control = entry as IEntryControl;
                if (control != null)
                    control.CheckFieldProperties(SoeForm.fsm, SoeForm.form, SoeForm.Name, SoeForm.StopSettings, SoeForm.settings);

                //Enable CopyPost
                if (SoeForm.PreviousForm != null)
                    entry.Value = SoeForm.PreviousForm[entry.ID];
            }
        }

        #endregion

        #region webforms

        public string SelectedItemGridId
        {
            get { return ViewState["GridId"] as string; }
            set { ViewState["GridId"] = value; }
        }


        public Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id) return root;
            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, id);
                if (t != null) return t;
            }
            return null;
        }

        
        #endregion
    }
}
