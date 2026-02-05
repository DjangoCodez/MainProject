using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Controls;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class SoeFormFooter : ControlBase
    {
        public event EventHandler Save;
        public event EventHandler Delete;

        protected string Message;

        protected List<Link> Links;

        public string ButtonSaveText { get; set; }
        public string ButtonDeleteText { get; set; }
        public bool EnableDelete { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ButtonSave.Text = ButtonSaveText;
            ButtonDelete.Text = ButtonDeleteText;

            if (EnableDelete)
                ButtonDelete.Visible = true;
        }

        public void ClearMessage()
        {
            this.Message = "";
        }

        public void SetMessage(string message, SoeTabView.SoeMessageType messageType)
        {
            this.Message = message;
        }

        public void SetButtonSaveAccessabillity(bool enabled)
        {
            ButtonSave.Enabled = enabled;
        }

        public void SetButtonSaveVisbillity(bool visible)
        {
            ButtonSave.Visible = visible;
        }

        public void ButtonSave_Click(object sender, EventArgs e)
        {
            if (Save != null)
                Save(sender, e);
        }

        public void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (EnableDelete && Delete != null)
                Delete(sender, e);
        }

        /// Add a link to the Form.
        /// </summary>
        /// <param name="text">The text for the button</param>
        /// <param name="href">The link for the button to link to</param>
        /// <param name="feature">The Feature to verify permission for</param>
        /// <param name="permission">The minimum Permission to be allowed</param>
        public void AddLink(string label, string href, Feature feature, Permission permission)
        {
            AddLink(label, href, feature, permission, false);
        }

        /// <summary>
        /// Add a link to the Form.
        /// </summary>
        /// <param name="text">The text for the button</param>
        /// <param name="href">The link for the button to link to</param>
        /// <param name="feature">The Feature to verify permission for</param>
        /// <param name="permission">The minimum Permission to be allowed</param>
        /// <param name="permission">True if the link should open as a modal window/param>
        public void AddLink(string label, string href, Feature feature, Permission permission, bool popLink)
        {
            if (PageBase.HasRolePermission(feature, permission))
            {
                Link link = new Link()
                {
                    Href = href,
                    Value = label,
                    Alt = label,
                };

                if (popLink)
                    link.CssClass = "PopLink";

                if (Links == null)
                    Links = new List<Link>();
                Links.Add(link);
            }
        }
    }
}