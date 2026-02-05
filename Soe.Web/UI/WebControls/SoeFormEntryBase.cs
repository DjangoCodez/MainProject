using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    /// <summary>
    /// Base class for form elements that goes into SoeForms.
    /// </summary>
	public abstract class SoeFormEntryBase : Control
    {
        /// <summary>
        /// The class attribute of the form element.
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// Override this to add default class attributes besides the ones specified in the
        /// CssClass property.
        /// </summary>
        protected virtual string DefaultCssClass { get { return null; } }

        /// <summary>
        /// The maxlength attribute of the element.
        /// </summary>
        public virtual int? MaxLength { get; set; }

        /// <summary>
        /// The width attribute of the element.
        /// </summary>
        public virtual int? Width { get; set; }

        /// <summary>
        /// The width attribute of the label element.
        /// </summary>
        public virtual int? LabelWidth { get; set; }

        /// <summary>
        /// The content of the label element that is rendered and associated with the form element.
        /// </summary>
        public virtual string Label { get; set; }

        /// <summary>
        /// A info text that is rendered after the element
        /// </summary>
        public virtual string InfoText { get; set; }

        /// <summary>
        /// Hide the Label of the control
        /// Default: Show Label
        /// </summary>
        public virtual bool HideLabel { get; set; }

        /// <summary>
        /// Hide the  InfoText of the control
        /// Default: Show Label
        /// </summary>
        public virtual bool HideInfoText { get; set; }

        /// <summary>
        /// Sets the Label to bold style
        /// Default: Label not bold
        /// </summary>
        public virtual bool BoldLabel { get; set; }

        /// <summary>
        /// Format the Control to fit in a tablecell
        /// Default: Do not fit in tablecell
        /// </summary>
        public virtual bool FitInTable { get; set; }

        /// <summary>
        /// The border attribute of the element.
        /// </summary>
        public virtual int? Border { get; set; }

        /// <summary>
        /// Text that discribes the a valid value for the field, appears if an invalid value has
        /// been entered.
        /// </summary>
        public virtual string InvalidText { get; set; }

        /// <summary>
        /// True if the control should skip TabStop
        /// </summary>
        public virtual bool SkipTabStop { get; set; }

        /// <summary>
        /// True if the control should be ReadOnly (Disabled)
        /// </summary>
        public virtual bool ReadOnly { get; set; }

        /// <summary>
        /// Content alignment.
        /// </summary>
        public virtual string Align { get; set; }

        /// <summary>
        /// The onchange event of the element.
        /// </summary>
        public virtual string OnFocus { get; set; }

        /// <summary>
        /// The onchange event of the element.
        /// </summary>
        public virtual string OnChange { get; set; }

        /// <summary>
        /// The onclick event of the element.
        /// </summary>
        public virtual string OnClick { get; set; }

        /// <summary>
        /// The onkeydown event of the element.
        /// </summary>
        public virtual string OnKeyDown { get; set; }
        /// <summary>
        /// The onkeyup event of the element.
        /// </summary>
        public virtual string OnKeyUp { get; set; }
        /// <summary>
        /// True if no HTML should be rendered to fit element in "standard container".
        /// </summary>
        public bool NoContainer { get; set; }
        /// <summary>
        /// Indents the label.
        /// </summary>
        public bool Indent { get; set; }

        /// <summary>
        /// The name attribute of the element. Will by default be same as ID.
        /// Override property to change value.
        /// </summary>
        public string Name
        {
            get
            {
                return ID;
            }
        }

        private string value;

        /// <summary>
        /// The value attribute of the form element.
        /// </summary>
        public virtual string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                isValidated = false;
            }
        }

        public virtual bool AutoComplete { get; set; }

        private bool isValid;
        private bool isValidated = false;

        public bool IsValid
        {
            get
            {
                if (!isValidated)
                    Validate();
                return isValid;
            }
        }

        public virtual bool Validate()
        {
            isValidated = true;
            isValid = true;
            return true;
        }

        #region Render methods

        protected override void Render(HtmlTextWriter writer)
        {
            RenderPrefix(writer);
            RenderPostfix(writer);
        }

        /// <summary>
        /// Render the HTML that should appear before the element HTML.
        /// </summary>
        /// <param name="writer"></param>
        protected void RenderPrefix(HtmlTextWriter writer)
        {
            if (!NoContainer)
            {
                if (!FitInTable)
                {
                    writer.Write("<tr");
                    writer.WriteAttribute("valign", "middle");
                    writer.Write("><th");
                    if (LabelWidth.HasValue)
                        writer.WriteAttribute("style", "width:" + LabelWidth.Value.ToString() + "px");
                    writer.Write(">");
                }

                if (!HideLabel)
                {
                    writer.Write("<label");
                    writer.WriteAttribute("for", ID);
                    writer.WriteAttribute("class", "LabelText");
                    if (Indent)
                        writer.WriteAttribute("style", "margin-left: 16px;");
                    writer.Write(">");
                    writer.WriteEncodedText(Label);
                    writer.Write("</label>");

                    #region Depcreated
                    /*
                    writer.Write("<label");
                    writer.WriteAttribute("for", ID);
                    writer.WriteAttribute("class", "LabelDataText");
                    writer.WriteAttribute("style", "display:none");
                    RenderLabelSettings(writer);
                    writer.Write(">");
                    writer.WriteEncodedText("[" + ID + "]");
                    writer.Write("</label>");
                    */
                    #endregion
                }

                if (!FitInTable)
                    writer.Write("</th><td>");
            }
        }

        /// <summary>
        /// Render the HTML that should appear after the element HTML.
        /// </summary>
        /// <param name="writer"></param>
        protected void RenderPostfix(HtmlTextWriter writer)
        {
            /*if (!actionsAreRendered)
            {
                RenderActions(writer);
            }*/
            if (!NoContainer)
            {
                string invalidText = InvalidText;
                if (!String.IsNullOrEmpty(invalidText))
                {
                    writer.Write("<span");
                    writer.WriteAttribute("class", "invalid");
                    writer.WriteAttribute("id", "invalid-" + ID);
                    writer.Write(">&nbsp;<span>");
                    writer.WriteEncodedText(invalidText);
                    writer.Write("</span></span>");
                }
                RenderPostEntryContent(writer);
                RenderInfoText(writer);
                if (!FitInTable)
                    writer.Write("</td></tr>");
            }
        }

        private void RenderInfoText(HtmlTextWriter writer)
        {
            writer.Write("<label");
            writer.WriteAttribute("id", ID + "-infotext");
            if (HideInfoText)
                writer.WriteAttribute("style", "display: none;");

            if (this.GetType().Name == "CheckBoxEntry")
                writer.WriteAttribute("class", "infoLabel checkboxInfoLabel");
            else
                writer.WriteAttribute("class", "infoLabel");
            writer.Write(">");
            writer.WriteEncodedText(InfoText != null ? InfoText : String.Empty);
            writer.Write("</label>");
        }

        protected virtual void RenderPostEntryContent(HtmlTextWriter writer)
        {
            //Overrided by IEntryControl controls
        }

        protected void RenderCssClassAttribute(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(CssClass) || !String.IsNullOrEmpty(DefaultCssClass))
            {
                writer.Write(" class=\"");
                if (!String.IsNullOrEmpty(CssClass))
                {
                    writer.Write(CssClass);
                    if (!String.IsNullOrEmpty(DefaultCssClass))
                    {
                        writer.Write(" ");
                        writer.Write(DefaultCssClass);
                    }
                }
                else if (!String.IsNullOrEmpty(DefaultCssClass))
                {
                    writer.Write(DefaultCssClass);
                }
                writer.Write("\"");
            }
        }

        protected void RenderEntrySettings(HtmlTextWriter writer)
        {
            if (ReadOnly)
                writer.WriteAttribute("disabled", "disabled");
            if (SkipTabStop)
                writer.WriteAttribute("tabindex", "-1");
            if (MaxLength.HasValue)
                writer.WriteAttribute("maxlength", MaxLength.ToString());
            if (Width.HasValue)
                writer.WriteAttribute("style", "width:" + Width.ToString() + "px");
            if (Border.HasValue)
                writer.WriteAttribute("style", "border:" + Border.ToString() + "px");
            if (!String.IsNullOrEmpty(Align))
                writer.WriteAttribute("style", "text-align:" + Align);
        }

        protected void RenderEntryActions(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(OnClick))
                writer.WriteAttribute("onclick", OnClick);
            if (!String.IsNullOrEmpty(OnChange))
                writer.WriteAttribute("onchange", OnChange);
            if (!String.IsNullOrEmpty(OnKeyDown))
                writer.WriteAttribute("onkeydown", OnKeyDown);
            if (!String.IsNullOrEmpty(OnKeyUp))
                writer.WriteAttribute("onkeyup", OnKeyUp);
            if (!String.IsNullOrEmpty(OnFocus))
                writer.WriteAttribute("onfocus", OnFocus);
        }

        #endregion
    }
}
