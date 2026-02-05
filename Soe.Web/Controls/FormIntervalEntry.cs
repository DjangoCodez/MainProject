using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.Controls
{
    public class FormIntervalEntry : SoeFormIntervalEntry, IEntryControl, IFormControl
    {
        internal enum LabelAutoCompleteTypeEnum
        {
            Customer = 1,
            Supplier = 2,
            Project = 3,
            Employee = 4,
            Account = 5,
        }

        #region Identic content in all Controls

        public int TermID { get; set; }
        public string DefaultTerm { get; set; }
        public int? InvalidAlertTermID { get; set; }
        public string InvalidAlertDefaultTerm { get; set; }
        public int FormId { get; set; }
        public int FieldId { get; set; }
        public string LabelSetting { get; set; }
        public bool DisableSettings { get; set; }
        public int? FromWidth { get; set; }
        public int? ToWidth { get; set; }
        public override bool SkipTabStop { get; set; }
        public override bool ReadOnly { get; set; }
        public override bool HideLabel { get; set; }
        public override bool FitInTable { get; set; }

        public override string Label
        {
            get
            {
                if (!String.IsNullOrEmpty(LabelSetting))
                    return LabelSetting;
                if (TermID > 0)
                    return this.GetText(TermID, DefaultTerm);
                return String.Empty;
            }
        }

        public override string InvalidText
        {
            get
            {
                if (InvalidAlertTermID.HasValue)
                    return this.GetText(InvalidAlertTermID.Value, InvalidAlertDefaultTerm);
                return null;
            }
        }

        protected override void RenderPostEntryContent(HtmlTextWriter writer)
        {
            base.RenderPostEntryContent(writer);

            this.RenderFieldSettingIcon(writer, FormId, FieldId, Validation == TextEntryValidation.Required);
        }

        #endregion

        #region Variables

        /// <summary>
        /// Values of the previous Form. Use to repopulate.
        /// </summary>
        public NameValueCollection PreviousForm { get; set; }

        /// <summary>
        /// Disable the Header (From - to)
        /// </summary>
        public bool DisableHeader { get; set; }

        /// <summary>
        /// Enable checkbox after each row
        /// </summary>
        private bool enableCheck;
        public bool EnableCheck
        {
            get
            {
                //Check only supported for LabelType Select
                if (noOfIntervals > 1)
                    return enableCheck;
                return false;
            }
            set
            {
                enableCheck = value;
            }
        }

        /// <summary>
        /// Enable garbage icon after each row
        /// </summary>
        public bool EnableDelete { get; set; }

        /// <summary>
        /// Only shows from value (i.e. no interval)
        /// </summary>
        public bool OnlyFrom { get; set; }

        /// <summary>
        /// Type of control for label (Text or Select). Use SoeFormIntervalEntryType
        /// </summary>
        public int LabelType { get; set; }

        /// <summary>
        /// Type for AutoCompleate/Search
        /// </summary>
        public int LabelAutoCompleteType { get; set; }

        /// <summary>
        /// Description for checkbox if EnableCheck is true.
        /// Overrides DisableHeader flag.
        /// </summary>
        public string LabelHeader { get; set; }

        /// <summary>
        /// Type of control for content (Text, Numeric, Date or Select). Use SoeFormIntervalEntryType
        /// </summary>
        public int ContentType { get; set; }

        public bool AllowNegative { get; set; }
        public bool AllowDecimal { get; set; }

        public string DefaultValue { get; set; }

        /// <summary>
        /// Max no of intervals
        /// </summary>
        private const int maxIntervals = 100;

        /// <summary>
        /// Description for checkbox if EnableCheck is true
        /// </summary>
        private string checkDescription;
        public string CheckDescription
        {
            get
            {
                if (EnableCheck)
                    return checkDescription;
                return "";
            }
            set
            {
                checkDescription = value;
            }
        }

        /// <summary>
        /// No of intervals
        /// </summary>
        private int noOfIntervals;
        public int NoOfIntervals
        {
            get
            {
                return noOfIntervals;
            }
            set
            {
                noOfIntervals = value;
                valuesLabel = new string[noOfIntervals];
                valuesFrom = new string[noOfIntervals];
                valuesTo = new string[noOfIntervals];
                valuesHidden = new string[noOfIntervals];
                valuesCheck = new bool[noOfIntervals];
            }
        }

        /// <summary>
        /// Labels for FormIntervalEntry's with LabelType SelectEntry
        /// </summary>
        private IDictionary<int, string> labels;
        public IDictionary<int, string> Labels
        {
            get
            {
                if (labels == null)
                    labels = new Dictionary<int, string>();
                return labels;
            }
            set
            {
                labels = value;
            }
        }

        /// <summary>
        /// Labels for From and To labels, if DisableHeader is false
        /// </summary>
        public string LabelFrom { get; set; }
        public string LabelTo { get; set; }

        /// <summary>
        /// DataSource for From in FormIntervalEntry's with ContentType SelectEntry
        /// </summary>
        private IDictionary<int, string> dataSourceFrom;
        public IDictionary<int, string> DataSourceFrom
        {
            get
            {
                if (dataSourceFrom == null)
                    dataSourceFrom = new Dictionary<int, string>();
                return dataSourceFrom;
            }
            set
            {
                dataSourceFrom = value;
            }
        }

        /// <summary>
        /// DataSource for To in FormIntervalEntry's with ContentType SelectEntry
        /// </summary>
        private IDictionary<int, string> dataSourceTo;
        public IDictionary<int, string> DataSourceTo
        {
            get
            {
                if (dataSourceTo == null)
                    dataSourceTo = new Dictionary<int, string>();
                return dataSourceTo;
            }
            set
            {
                dataSourceTo = value;
            }
        }

        /// <summary>
        /// Label value if NoOfIntervals is 1
        /// </summary>
        public string ValueLabel { get; set; }

        /// <summary>
        /// From value if NoOfIntervals is 1
        /// </summary>
        public string ValueFrom { get; set; }

        /// <summary>
        /// To value if NoOfIntervals is 1
        /// </summary>
        public string ValueTo { get; set; }

        /// <summary>
        /// Hidden value if NoOfIntervals is 1
        /// </summary>
        public string ValueHidden { get; set; }

        /// <summary>
        /// Label values if NoOfIntervals is > 1
        /// </summary>
        private string[] valuesLabel;
        public string[] ValuesLabel
        {
            get
            {
                return valuesLabel;
            }
            set
            {
                valuesLabel = value;
            }
        }
        public void AddLabelValue(int pos, string value)
        {
            if (pos < NoOfIntervals)
            {
                valuesLabel[pos] = value;
            }
        }

        /// <summary>
        /// From values if NoOfIntervals is > 1
        /// </summary>
        private string[] valuesFrom;
        public string[] ValuesFrom
        {
            get
            {
                return valuesFrom;
            }
            set
            {
                valuesFrom = value;
            }
        }
        public void AddValueFrom(int pos, string value)
        {
            if (pos < NoOfIntervals)
            {
                valuesFrom[pos] = value;
            }
        }

        /// <summary>
        /// Hidden values if NoOfIntervals is > 1
        /// </summary>
        private string[] valuesHidden;
        public string[] ValuesHidden
        {
            get
            {
                return valuesHidden;
            }
            set
            {
                valuesHidden = value;
            }
        }
        public void AddValueHidden(int pos, string value)
        {
            if (pos < NoOfIntervals)
            {
                valuesHidden[pos] = value;
            }
        }

        /// <summary>
        /// To values if NoOfIntervals is > 1
        /// </summary>
        private string[] valuesTo;
        public string[] ValuesTo
        {
            get
            {
                return valuesTo;
            }
            set
            {
                valuesTo = value;
            }
        }
        public void AddValueTo(int pos, string value)
        {
            if (pos < NoOfIntervals)
            {
                valuesTo[pos] = value;
            }
        }

        /// <summary>
        /// Check values if EnableCheck is true
        /// </summary>
        private bool[] valuesCheck;
        public bool[] ValuesCheck
        {
            get
            {
                return valuesCheck;
            }
            set
            {
                valuesCheck = value;
            }
        }
        public void AddValueCheck(int pos, bool isChecked)
        {
            if (pos < NoOfIntervals)
            {
                ValuesCheck[pos] = isChecked;
            }
        }

        /// <summary>
        /// Validation types for the content controls
        /// </summary>
        private Dictionary<int, ValidationItem> validationTypes;
        public Dictionary<int, ValidationItem> ValidationTypes
        {
            get
            {
                if (validationTypes == null)
                    validationTypes = new Dictionary<int, ValidationItem>();
                return validationTypes;
            }
            set
            {
                validationTypes = value;
            }
        }
        public void AddValidationType(int pos, ValidationItem validationItem)
        {
            if (pos < NoOfIntervals)
            {
                if (validationTypes == null)
                    validationTypes = new Dictionary<int, ValidationItem>();
                validationTypes[pos] = validationItem;
            }
        }

        #endregion

        #region Cache

        private Collection<FormIntervalEntryItem> formIntervalEntryItems;

        #endregion

        #region Render

        protected override void Render(HtmlTextWriter writer)
        {
            if (NoOfIntervals > 0 && maxIntervals > 0)
            {
                string tableId = GetTableId();
                int intervalCounterValue = FindIntervalCounterValue();
                int renderedVisibleIntervals = 0;

                RenderHead(writer, intervalCounterValue);

                for (int intervalNo = 1; intervalNo <= NoOfIntervals; intervalNo++)
                {
                    if (intervalNo > maxIntervals)
                        break;

                    bool visible = true;
                    if (renderedVisibleIntervals == intervalCounterValue)
                        visible = false;
                    if (IsIntervalDeleted(intervalNo))
                        visible = false;

                    RenderRowPrefix(writer, tableId + "-" + intervalNo, visible);
                    RenderRow(writer, intervalNo, tableId);
                    RenderRowPostfix(writer);

                    if (visible)
                        renderedVisibleIntervals++;
                }

                RenderFooter(writer);
            }
        }

        private void RenderHead(HtmlTextWriter writer, int intervalCounterValue)
        {
            string intervalCounterId = GetIntervalId();
            string noOfIntervalsId = GetNoOfIntervalsId();

            bool hasLabelHeader = !String.IsNullOrEmpty(LabelHeader);

            writer.Write("<tr");
            if (!hasLabelHeader)
                writer.WriteAttribute("style", "display:none");
            writer.Write("><th");
            if (hasLabelHeader)
                writer.WriteAttribute("width", LabelWidth.Value.ToString() + "px");
            writer.Write(">");
            if (!String.IsNullOrEmpty(LabelHeader))
                writer.Write(LabelHeader);
            writer.Write("<input type=\"hidden\" id=\"" + intervalCounterId + "\" name=\"" + intervalCounterId + "\" value=\"" + intervalCounterValue + "\"/>");
            writer.Write("</th>");
            writer.Write("<th>");
            writer.Write("<input type=\"hidden\" id=\"" + noOfIntervalsId + "\" name=\"" + noOfIntervalsId + "\" value=\"" + this.noOfIntervals + "\"/>");
            if (!DisableHeader)
            {
                writer.Write("<label");
                writer.WriteAttribute("style", "padding-left:5px");
                writer.Write(">");
                if (!String.IsNullOrEmpty(LabelFrom))
                    writer.WriteEncodedText(LabelFrom);
                else
                    writer.WriteEncodedText(this.GetText(1315, "Från"));
                writer.Write("</label>");
            }
            writer.Write("</th><th>");
            if (!DisableHeader && !OnlyFrom)
            {
                writer.Write("<label");
                writer.WriteAttribute("style", "padding-left:5px");
                writer.Write(">");
                if (!String.IsNullOrEmpty(LabelTo))
                    writer.WriteEncodedText(LabelTo);
                else
                    writer.WriteEncodedText(this.GetText(1316, "Till"));
                writer.Write("</label>");
            }
            writer.Write("</th><th>");
            writer.Write("</th></tr>");
        }

        private void RenderFooter(HtmlTextWriter writer)
        {
            if (EnableCheck)
            {
                writer.Write("<tr><td");
                writer.WriteAttribute("colspan", "4");
                writer.Write(">");

                if (!String.IsNullOrEmpty((CheckDescription)))
                {
                    InstructionList description = new InstructionList();
                    description.DefaultIdentifier = "*";
                    description.DisableFieldset = true;
                    description.Numeric = false;
                    description.Instructions = new List<string>()
				    {
					    this.GetText(1729, "Kryssruta") + " = " + CheckDescription,
				    };
                    description.RenderControl(writer);
                }

                writer.Write("</td></tr>");
            }
        }

        public void RenderRowPrefix(HtmlTextWriter writer, string id, bool visible)
        {
            writer.Write("<tr");
            writer.WriteAttribute("class", "interval-row");
            writer.WriteAttribute("id", id);
            if (visible)
                writer.WriteAttribute("style", "");
            else
                writer.WriteAttribute("style", "display:none");
            writer.Write(">");
        }

        public void RenderRowPostfix(HtmlTextWriter writer)
        {
            writer.Write("</tr>");
        }

        private void RenderRow(HtmlTextWriter writer, int intervalNo, string tableId)
        {
            writer.Write("<th");
            if (LabelWidth.HasValue)
                writer.WriteAttribute("width", LabelWidth.Value.ToString() + "px");
            writer.Write(">");

            switch (LabelType)
            {
                case (int)SoeFormIntervalEntryType.Text:
                    CreateTextLabel(writer, intervalNo);
                    break;
                case (int)SoeFormIntervalEntryType.Select:
                    CreateSelectLabel(writer, intervalNo);
                    break;
                case (int)SoeFormIntervalEntryType.Numeric:
                    CreateNumericLabel(writer, intervalNo);
                    break;
                default:
                    throw new SoeGeneralException("Unknown LabelType for FormIntervalEntry " + LabelType, this.ToString());
            }

            writer.Write("</th><td>");

            switch (ContentType)
            {
                case (int)SoeFormIntervalEntryType.Text:
                    CreateTextEntry(writer, intervalNo);
                    break;
                case (int)SoeFormIntervalEntryType.Select:
                    CreateSelectEntry(writer, intervalNo);
                    break;
                case (int)SoeFormIntervalEntryType.Date:
                    CreateDateEntry(writer, intervalNo);
                    break;
                case (int)SoeFormIntervalEntryType.Numeric:
                    CreateNumericEntry(writer, intervalNo, AllowDecimal, AllowNegative, DefaultValue);
                    break;
                default:
                    break;
            }

            if (EnableCheck)
            {
                CreateCheckEntry(writer, intervalNo);
            }

            writer.Write("</td><td>");

            if (EnableDelete)
            {
                RenderDeleteIntervalIcon(writer, intervalNo, tableId);
            }

            if (intervalNo == 1)
            {
                RenderPostEntryContent(writer);
                if (NoOfIntervals > 1)
                {
                    RenderIncreaseIntervalIcon(writer, tableId);
                }
            }

            writer.Write("</td>");
        }

        public void RenderIncreaseIntervalIcon(HtmlTextWriter writer, string tableId)
        {
            string intervalCounterId = GetIntervalId();
            string imgId = tableId + "-plus";

            writer.Write("<a");
            writer.WriteAttribute("href", "javascript:;");
            writer.WriteAttribute("onclick", "increaseInterval('" + tableId + "','" + intervalCounterId + "','" + NoOfIntervals + "');");
            writer.WriteAttribute("tabindex", "-1");
            writer.WriteAttribute("title", this.GetText(1320, "Öka urval"));
            writer.Write(">");
            writer.Write("<span");
            writer.WriteAttribute("id", imgId);
            writer.WriteAttribute("class", "fal fa-plus");
            writer.Write(">");
            writer.Write("</span>");
            writer.Write("</a>");
        }

        public void RenderDeleteIntervalIcon(HtmlTextWriter writer, int intervalNo, string tableId)
        {
            string intervalCounterId = GetIntervalId();
            string labelId = GetLabelId(intervalNo);
            string fromId = GetFromId(intervalNo);
            string toId = GetToId(intervalNo);
            string imgId = tableId + "-delete";

            writer.Write("<a");
            writer.WriteAttribute("href", "javascript:;");
            writer.WriteAttribute("onclick", "deleteInterval('" + labelId + "','" + fromId + "','" + toId + "','" + tableId + "','" + intervalCounterId + "','" + intervalNo + "');");
            writer.WriteAttribute("tabindex", "-1");
            writer.WriteAttribute("title", this.GetText(1565, "Ta bort rad"));
            writer.Write(">");
            writer.Write("<span");
            writer.WriteAttribute("id", imgId);
            writer.WriteAttribute("class", "fal fa-times errorColor");
            writer.Write(">");
            writer.Write("</span>");
            writer.Write("</a>");
        }

        #endregion

        #region Create label

        private void CreateTextLabel(HtmlTextWriter writer, int intervalNo)
        {
            if (!HideLabel)
            {
                string labelId = GetLabelId(intervalNo);

                writer.Write("<label");
                writer.WriteAttribute("for", labelId);
                writer.WriteAttribute("class", "LabelText");
                writer.Write(">");
                writer.WriteEncodedText(Label);
                writer.Write("</label>");

                writer.Write("<label");
                writer.WriteAttribute("class", "LabelDataText");
                writer.WriteAttribute("style", "display:none");
                writer.Write(">");
                writer.WriteEncodedText("[" + ID + "]");
                writer.Write("</label>");
            }
        }

        private void CreateSelectLabel(HtmlTextWriter writer, int intervalNo)
        {
            string labelId = GetLabelId(intervalNo);
            CheckPreviousForm(intervalNo);

            SelectEntry from = new SelectEntry();
            from.ID = labelId;
            from.HideLabel = true;
            from.FitInTable = true;
            from.Width = LabelWidth;
            from.DisableSettings = true;
            from.ConnectDataSource(labels);
            SetValueLabel(from, intervalNo);
            from.RenderControl(writer);
        }

        private void CreateNumericLabel(HtmlTextWriter writer, int intervalNo)
        {
            string labelId = GetLabelId(intervalNo);
            CheckPreviousForm(intervalNo);

            NumericEntry label = new NumericEntry();
            label.ID = labelId;
            label.AllowDecimals = false;
            label.AllowNegative = false;
            label.MaxLength = LabelWidth;
            label.Width = LabelWidth;
            label.HideLabel = true;
            label.FitInTable = true;
            label.DisableSettings = true;
            SetValueLabel(label, intervalNo);
            SetNumericValidation(label, intervalNo);
            label.RenderControl(writer);
        }

        #endregion

        #region Create content

        private void CreateTextEntry(HtmlTextWriter writer, int intervalNo)
        {
            string fromId = GetFromId(intervalNo);
            string toId = GetToId(intervalNo);
            CheckPreviousForm(intervalNo);

            //From
            TextEntry from = new TextEntry();
            from.ID = fromId;
            from.MaxLength = 50;
            from.HideLabel = true;
            from.HideInfoText = LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Account;
            from.FitInTable = true;
            from.DisableSettings = true;
            from.OnChange = "copyValue('" + fromId + "','" + toId + "','true')";
            SetValueFrom(from, intervalNo);
            SetTextValidation(from, intervalNo);

            if(LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Customer)
            {
                from.OnChange = "CustomerByNumberSearch.searchField('" + fromId + "');";
                from.OnKeyUp = "CustomerByNumberSearch.keydown('" + fromId + "');";
            }
            if (LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Supplier)
            {
                from.OnChange = "SupplierByNumberSearch.searchField('" + fromId + "');";
                from.OnKeyUp = "SupplierByNumberSearch.keydown('" + fromId + "');";
            }
            if (LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Project)
            {
                from.OnChange = "ProjectByNumberSearch.searchField('" + fromId + "');";
                from.OnKeyUp = "ProjectByNumberSearch.keydown('" + fromId + "');";
            }
            if (LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Employee)
            {
                from.OnChange = "EmployeeByNumberSearch.searchField('" + fromId + "');";
                from.OnKeyUp = "EmployeeByNumberSearch.keydown('" + fromId + "');";
            }
            if (LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Account)
            {
                from.OnChange = "accountSearch.searchField('" + fromId + "');";
                from.OnKeyUp = "accountSearch.keydown('" + fromId + "');";
            }

            from.RenderControl(writer);

            CreateHiddenEntry(writer, intervalNo);

            writer.Write("</td><td>");

            if (OnlyFrom)
                return;

            //To
            TextEntry to = new TextEntry();
            to.ID = toId;
            to.MaxLength = 50;
            to.HideLabel = true;
            to.HideInfoText = LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Account;
            to.FitInTable = true;
            to.DisableSettings = true;
            SetValueTo(to, intervalNo);
            SetTextValidation(from, intervalNo);

            if (LabelAutoCompleteType == (int)LabelAutoCompleteTypeEnum.Account)
            {
                to.OnChange = "accountSearch.searchField('" + toId + "');";
                to.OnKeyUp = "accountSearch.keydown('" + toId + "');";
            }

            to.RenderControl(writer);
        }

        private void CreateSelectEntry(HtmlTextWriter writer, int intervalNo)
        {
            string fromId = GetFromId(intervalNo);
            string toId = GetToId(intervalNo);
            CheckPreviousForm(intervalNo);

            //From
            SelectEntry from = new SelectEntry();
            from.ID = fromId;
            from.HideLabel = true;
            from.FitInTable = true;
            from.DisableSettings = true;
            from.OnChange = "copyValue('" + fromId + "','" + toId + "')";
            from.ConnectDataSource(DataSourceFrom);
            if (FromWidth.HasValue)
                from.Width = FromWidth.Value;
            SetValueFrom(from, intervalNo);
            from.RenderControl(writer);

            CreateHiddenEntry(writer, intervalNo);

            writer.Write("</td><td>");

            if (OnlyFrom)
                return;

            //To
            SelectEntry to = new SelectEntry();
            to.ID = toId;
            to.HideLabel = true;
            to.FitInTable = true;
            to.DisableSettings = true;
            to.ConnectDataSource(DataSourceTo);
            if (ToWidth.HasValue)
                to.Width = ToWidth.Value;
            SetValueTo(to, intervalNo);
            to.RenderControl(writer);
        }

        private void CreateDateEntry(HtmlTextWriter writer, int intervalNo)
        {
            string fromId = GetFromId(intervalNo);
            string toId = GetToId(intervalNo);
            CheckPreviousForm(intervalNo);

            //From
            DateEntry from = new DateEntry();
            from.ID = fromId;
            from.HideLabel = true;
            from.FitInTable = true;
            from.DisableSettings = true;
            SetValueFrom(from, intervalNo);
            from.RenderControl(writer);

            CreateHiddenEntry(writer, intervalNo);

            writer.Write("</td><td>");

            if (OnlyFrom)
                return;

            //To
            DateEntry to = new DateEntry();
            to.ID = toId;
            to.HideLabel = true;
            to.FitInTable = true;
            to.DisableSettings = true;
            SetValueTo(to, intervalNo);
            to.RenderControl(writer);
        }

        private void CreateNumericEntry(HtmlTextWriter writer, int intervalNo, bool allowDecimals, bool allowNegative, string defaultValue)
        {
            string fromId = GetFromId(intervalNo);
            string toId = GetToId(intervalNo);
            CheckPreviousForm(intervalNo);

            //From
            NumericEntry from = new NumericEntry();
            from.ID = fromId;
            from.AllowDecimals = allowDecimals;
            from.AllowNegative = allowNegative;
            from.MaxLength = 10;
            from.HideLabel = true;
            from.FitInTable = true;
            from.DisableSettings = true;
            if (defaultValue != null)
                from.Value = defaultValue;
            from.OnChange = "copyValue('" + fromId + "','" + toId + "','true')";
            SetValueFrom(from, intervalNo);
            SetNumericValidation(from, intervalNo);
            from.RenderControl(writer);

            CreateHiddenEntry(writer, intervalNo);

            writer.Write("</td><td>");

            if (OnlyFrom)
                return;

            //To
            NumericEntry to = new NumericEntry();
            to.ID = toId;
            from.AllowDecimals = allowDecimals;
            to.MaxLength = 10;
            to.HideLabel = true;
            to.FitInTable = true;
            to.DisableSettings = true;
            SetValueTo(to, intervalNo);
            SetNumericValidation(from, intervalNo);
            to.RenderControl(writer);
        }

        #endregion

        #region HiddenEntry

        public void CreateHiddenEntry(HtmlTextWriter writer, int intervalNo)
        {
            string hiddenId = GetHiddenValueId(intervalNo);

            string value = GetValueHidden(intervalNo);
            writer.Write("<input type=\"hidden\" runat=\"server\" id=\"" + hiddenId + "\" name=\"" + hiddenId + "\" value=\"" + value + "\"/>");
        }

        #endregion

        #region Check

        public void CreateCheckEntry(HtmlTextWriter writer, int intervalNo)
        {
            string checkId = GetCheckId(intervalNo);

            CheckBoxEntry check = new CheckBoxEntry();
            check.ID = checkId;
            check.FitInTable = true;
            check.DisableSettings = true;
            check.LabelSetting = CheckDescription;
            check.HideLabel = true; //LabelSetting and HideLabel combination results in only Alt text (set by LabelSetting)
            SetChecked(check, intervalNo);    

            //Diffrent javascript behavior for different LabelTypes
            switch (LabelType)
            {
                case (int)SoeFormIntervalEntryType.Text:
                    //TODO: Not implemented
                    break;
                case (int)SoeFormIntervalEntryType.Select:
                    //TODO: Not implemented
                    break;
                case (int)SoeFormIntervalEntryType.Date:
                    //TODO: Not implemented
                    break;
                case (int)SoeFormIntervalEntryType.Numeric:
                    //TODO: Not implemented
                    break;
            }

            check.RenderControl(writer);
            
        }

        #endregion

        #region Help methods

        private string GetTableId()
        {
            return ID + "-FormIntervalEntry";
        }

        private string GetIntervalId()
        {
            return ID + "-intervalcounter";
        }

        private string GetNoOfIntervalsId()
        {
            return ID + "-noofintervals";
        }

        private string GetLabelId(int intervalNo)
        {
            return ID + "-label-" + intervalNo;
        }

        private string GetFromId(int intervalNo)
        {
            return ID + "-from-" + intervalNo;
        }

        private string GetToId(int intervalNo)
        {
            return ID + "-to-" + intervalNo;
        }

        private string GetHiddenValueId(int intervalNo)
        {
            return ID + "-hiddenvalue-" + intervalNo;
        }

        private string GetCheckId(int intervalNo)
        {
            return ID + "-check-" + intervalNo;
        }

        private bool IsIntervalDeleted(int intervalNo)
        {
            bool deleted = false;
            if (NoOfIntervals == 1 || intervalNo == 1)
                return deleted;

            if (PreviousForm != null)
            {
                string label = PreviousForm[GetLabelId(intervalNo)];
                string from = PreviousForm[GetFromId(intervalNo)];
                deleted = (String.IsNullOrEmpty(label) || label == "0") && String.IsNullOrEmpty(from);

                if (deleted && !OnlyFrom)
                {
                    string to = PreviousForm[GetToId(intervalNo)];
                    deleted = String.IsNullOrEmpty(to);
                }
            }

            return deleted;
        }

        private void CheckPreviousForm(int intervalNo)
        {
            if (PreviousForm != null)
            {
                if (NoOfIntervals == 1)
                {
                    ValueFrom = PreviousForm[GetFromId(intervalNo)];
                    ValueTo = PreviousForm[GetToId(intervalNo)];
                }
                else
                {
                    ValuesFrom[intervalNo - 1] = PreviousForm[GetFromId(intervalNo)];
                    ValuesTo[intervalNo - 1] = PreviousForm[GetToId(intervalNo)];
                    ValuesLabel[intervalNo - 1] = PreviousForm[GetLabelId(intervalNo)];
                    ValuesCheck[intervalNo - 1] = StringUtility.GetBool(PreviousForm[GetCheckId(intervalNo)]);
                }
            }
        }

        private void SetTextValidation(SoeFormInputEntryBase control, int intervalNo)
        {
            intervalNo--;
            if (ValidationTypes.ContainsKey(intervalNo))
            {
                ValidationItem validationItem = ValidationTypes[intervalNo];
                if (validationItem != null)
                {
                    int validation = (int)validationItem.Validation;
                    control.Validation = (TextEntryValidation)validation;

                    TextEntry textEntry = control as TextEntry;
                    if (textEntry != null)
                    {
                        textEntry.InvalidAlertTermID = validationItem.InvalidAlertTermID;
                        textEntry.InvalidAlertDefaultTerm = validationItem.InvalidAlertDefaultTerm;
                    }
                }
            }
        }

        private void SetNumericValidation(SoeFormInputEntryBase control, int intervalNo)
        {
            intervalNo--;
            if (ValidationTypes.ContainsKey(intervalNo))
            {
                ValidationItem validationItem = ValidationTypes[intervalNo];
                if (validationItem != null)
                {
                    int validation = (int)validationItem.Validation;
                    control.Validation = (TextEntryValidation)validation;

                    NumericEntry numericEntry = control as NumericEntry;
                    if (numericEntry != null)
                    {
                        numericEntry.InvalidAlertTermID = validationItem.InvalidAlertTermID;
                        numericEntry.InvalidAlertDefaultTerm = validationItem.InvalidAlertDefaultTerm;
                    }
                }
            }
        }

        private void SetValueLabel(SoeFormEntryBase control, int intervalNo)
        {
            if (NoOfIntervals == 1)
            {
                if (!String.IsNullOrEmpty(ValueLabel))
                    control.Value = ValueLabel;
            }
            else
            {
                intervalNo--;
                if (ValuesLabel.Length >= intervalNo && !String.IsNullOrEmpty(ValuesLabel[intervalNo]))
                    control.Value = ValuesLabel[intervalNo];
            }
        }

        private void SetValueFrom(SoeFormEntryBase control, int intervalNo)
        {
            if (NoOfIntervals == 1)
            {
                if (!String.IsNullOrEmpty(ValueFrom))
                    control.Value = ValueFrom;
            }
            else
            {
                intervalNo--;
                if (ValuesFrom.Length >= intervalNo && !String.IsNullOrEmpty(ValuesFrom[intervalNo]))
                    control.Value = ValuesFrom[intervalNo];
            }
        }

        private void SetValueTo(SoeFormEntryBase control, int intervalNo)
        {
            if (NoOfIntervals == 1)
            {
                if (!String.IsNullOrEmpty(ValueTo))
                    control.Value = ValueTo;
            }
            else
            {
                intervalNo--;
                if (ValuesTo.Length >= intervalNo && !String.IsNullOrEmpty(ValuesTo[intervalNo]))
                    control.Value = ValuesTo[intervalNo];
            }
        }

        private string GetValueHidden(int intervalNo)
        {
            string value = "";

            if (NoOfIntervals == 1)
            {
                value = ValueHidden;
            }
            else
            {
                intervalNo--;
                if (ValuesHidden.Length >= intervalNo)
                {
                    value = ValuesHidden[intervalNo];
                }
            }

            return value;
        }

        private void SetChecked(SoeFormEntryBase control, int intervalNo)
        {
            intervalNo--;
            if (ValuesCheck.Length >= intervalNo)
            {
                control.Value = ValuesCheck[intervalNo] ? Boolean.TrueString : Boolean.FalseString;
            }
        }

        private int FindIntervalCounterValue()
        {
            if (PreviousForm != null)
            {
                int intervalId;
                if (Int32.TryParse(PreviousForm[GetIntervalId()], out intervalId))
                    return intervalId;
            }

            if (NoOfIntervals != 1 || ValuesFrom != null || ValuesTo != null)
            {
                for (int i = NoOfIntervals - 1; i >= 1; i--)
                {
                    if (OnlyFrom)
                    {
                        if (ValuesFrom != null && !String.IsNullOrEmpty(ValuesFrom[i]))
                        {
                            return i + 1;
                        }
                    }
                    else
                    {
                        if ((ValuesFrom != null && !String.IsNullOrEmpty(ValuesFrom[i])) &&
                            (ValuesTo != null && !String.IsNullOrEmpty(ValuesTo[i])))
                        {
                            return i + 1;
                        }
                    }
                }
            }

            return 1;
        }

        #endregion

        #region Validation

        public bool ValidateCheck(NameValueCollection F)
        {
            Dictionary<int, bool> dict = new Dictionary<int, bool>();

            if (formIntervalEntryItems == null)
                formIntervalEntryItems = GetData(F);

            foreach (FormIntervalEntryItem formIntervalEntryItem in formIntervalEntryItems)
            {
                if (formIntervalEntryItem.Checked)
                {
                    if (dict.ContainsKey(formIntervalEntryItem.LabelType))
                        return false;

                    dict.Add(formIntervalEntryItem.LabelType, true);
                }
            }

            return true;
        }

        public bool ContainsLabelType(NameValueCollection F, int labelType)
        {
            if (formIntervalEntryItems == null)
                formIntervalEntryItems = GetData(F);

            if (HideLabel || LabelType != (int)SoeFormIntervalEntryType.Select)
                return false;

            foreach (FormIntervalEntryItem formIntervalEntryItem in formIntervalEntryItems)
            {
                if (formIntervalEntryItem.LabelType == labelType)
                    return true;
            }

            return false;
        }

        #endregion

        #region GetData

        public Collection<FormIntervalEntryItem> GetData(NameValueCollection F)
        {
            Collection<FormIntervalEntryItem> items = new Collection<FormIntervalEntryItem>();

            int intervalCounter = GetNoOfIntervals(F);
            if (intervalCounter > 0)
            {
                for (int interval = 1; interval <= NoOfIntervals; interval++)
                {
                    if (intervalCounter == items.Count)
                        break;

                    string from = F[GetFromId(interval)];
                    string to = F[GetToId(interval)];
                    string hiddenValue = F[GetHiddenValueId(interval)];
                    bool isChecked = StringUtility.GetBool(F[GetCheckId(interval)]);

                    //Check from and to values (only from is checked if OnlyFrom is true)
                    if ((!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to)) ||
                        (!String.IsNullOrEmpty(from) && OnlyFrom) || (EnableCheck && ContentType == 0))
                    {
                        //Check labels (not if HideLabel is true)
                        int labelType;
                        if ((Int32.TryParse(F[GetLabelId(interval)], out labelType) && labelType > 0) || HideLabel)
                        {
                            items.Add(new FormIntervalEntryItem()
                            {
                                LabelType = labelType,
                                From = from,
                                To = to,
                                HiddenValue = hiddenValue,
                                Checked = isChecked,
                            });
                        }
                    }
                }
            }

            return items;
        }

        public int GetNoOfIntervals(NameValueCollection F)
        {
            int intervalCounter;
            Int32.TryParse(F[GetIntervalId()], out intervalCounter);
            return intervalCounter;
        }

        public bool HasIntervals(NameValueCollection F)
        {
            return GetNoOfIntervals(F) > 0;
        }

        #endregion
    }
}