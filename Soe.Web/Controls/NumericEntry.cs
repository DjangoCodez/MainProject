using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Web.UI;

namespace SoftOne.Soe.Web.Controls
{
    public class NumericEntry : SoeFormNumericEntry, IEntryControl, IFormControl
	{
		#region Identic content in all Controls

		public int TermID { get; set; }
		public string DefaultTerm { get; set; }
		public int? InvalidAlertTermID { get; set; }
		public string InvalidAlertDefaultTerm { get; set; }
		public int FormId { get; set; }
		public int FieldId { get; set; }
		public string LabelSetting { get; set; }
		public bool DisableSettings { get; set; }
		public override bool SkipTabStop { get; set; }
		public override bool ReadOnly { get; set; }
		public override bool HideLabel { get; set; }
		public override bool BoldLabel { get; set; }
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

		#endregion, FileEntry
	}
}
