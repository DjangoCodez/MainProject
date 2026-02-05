using System;
using System.Collections;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFormNumericSelectEntry : SoeFormEntryBase
    {
        #region Numeric Variables - copied from SoeFormNumericEntry

        public bool AllowNegative { get; set; }
        public bool AllowDecimals { get; set; }

        #endregion

        #region Numeric Variables - extended

        /// <summary>
        /// MaxLength for the numeric entry
        /// </summary>
        public int? NumericMaxLength { get; set; }

        /// <summary>
        /// Width for the numeric entry
        /// </summary>
        public int? NumericWidth { get; set; }

        #endregion

        #region Select Variables - Copied from SoeFormSelectEntry

        public string DataTextField { get; set; }
        public string DataValueField { get; set; }

        private object dataSource;

        public virtual object DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                // make sure we're working with a string, XmlReader, or TextReader
                if (value == null || value is IEnumerable)
                    dataSource = value;
                else
                    throw new ArgumentException("DataSource must be assigned an IEnumerable");
            }
        }

        protected override string DefaultCssClass
        {
            get
            {
                StringBuilder css = new StringBuilder(base.DefaultCssClass);
                if (css.Length > 0)
                    css.Append(" ");
                css.Append("numeric");
                if (AllowNegative)
                    css.Append(" negative");
                if (AllowDecimals)
                    css.Append(" decimals");
                return css.ToString();
            }
        }

        #endregion

        #region Select Variables - extended

        /// <summary>
        /// Width for the select entry
        /// </summary>
        public int? SelectWidth { get; set; }

        public string SelectID
        {
            get { return ID + "-select"; }
        }

        #endregion

        #region Numeric methods - copied from SoeFormNumericEntry

        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            decimal value;
            bool valid = false;
            if (Decimal.TryParse(Value, out value))
            {
                if ((AllowNegative || value >= 0) && (AllowDecimals || value == Math.Truncate(value)))
                    valid = true;
            }
            return valid;
        }

        #endregion

        #region Select methods - copied from SoeFormSelectEntry

        protected virtual IEnumerable GetDataSource()
        {
            if (dataSource == null)
                return null;

            IEnumerable resolvedDataSource;
            resolvedDataSource = dataSource as IEnumerable;

            return resolvedDataSource;
        }

        protected virtual void CreateMyControlHeirarchy()
        {
            IEnumerable resolvedDataSource = GetDataSource();

            if (resolvedDataSource != null)
            {
                foreach (object dataItem in resolvedDataSource)
                {
                    string text = null;
                    if (!String.IsNullOrEmpty(DataTextField))
                        text = DataBinder.Eval(dataItem, DataTextField).ToString();
                    else
                        text = dataItem.ToString();

                    string value = null;
                    if (!String.IsNullOrEmpty(DataValueField))
                        value = DataBinder.Eval(dataItem, DataValueField).ToString();
                    else
                        value = dataItem.ToString();

                    var option = new HtmlGenericControl("option");
                    if (value != null)
                    {
                        option.Attributes["value"] = value;
                        if (value.Equals(Value))
                            option.Attributes["selected"] = "selected";
                    }
                    option.Controls.Add(new LiteralControl(text));

                    Controls.Add(option);
                }
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            CreateMyControlHeirarchy();
            ChildControlsCreated = true;
        }

        #endregion

        #region Events

        protected override void Render(HtmlTextWriter writer)
        {
            CreateChildControls();	

            RenderPrefix(writer);

            //Numeric
            writer.Write("<input");
            writer.WriteAttribute("id", ID);
            writer.WriteAttribute("type", "text");
            writer.WriteAttribute("name", ID);

            RenderEntrySettings(writer);

            writer.WriteAttribute("onchange", "SynchSelect('" + ID + "','" + SelectID + "')");

            RenderCssClassAttribute(writer);

            if (NumericMaxLength.HasValue)
                writer.WriteAttribute("maxlength", NumericMaxLength.ToString());
            if (NumericWidth.HasValue)
                writer.WriteAttribute("style", "width:" + NumericWidth.ToString() + "px");

            int value;
            if (Int32.TryParse(Value, out value) && value > 0)
                writer.WriteAttribute("value", Value);
            else
                writer.WriteAttribute("value", String.Empty);

            writer.Write(">");

            //Select
            writer.Write("<select");
            writer.WriteAttribute("id", SelectID);
            writer.WriteAttribute("name", SelectID);
            writer.WriteAttribute("title", Label);
            if (SelectWidth.HasValue)
                writer.WriteAttribute("style", "width:" + SelectWidth.Value.ToString() + "px");

            RenderEntrySettings(writer);

            writer.WriteAttribute("onchange", "SynchNumeric('" + ID + "','" + SelectID + "')");

            writer.Write(">");

            RenderChildren(writer);

            writer.Write("</select>");

            RenderPostfix(writer);
        }

        #endregion
    }
}