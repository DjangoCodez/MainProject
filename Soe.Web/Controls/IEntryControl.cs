
namespace SoftOne.Soe.Web.Controls
{
    /// <summary>
    /// Interface for all Entry controls that should implement Setting navigation
    /// Has Extension Methods in ControlExtensions.
    /// </summary>
    public interface IEntryControl
	{
		string ID { get; set; }
		bool Visible { get; set; }

		int TermID { get; set; }
		string DefaultTerm { get; set; }
		int? InvalidAlertTermID { get; set; }
		string InvalidAlertDefaultTerm { get; set; }
		string LabelSetting { get; set; }
		int FormId { get; set; }
		int FieldId { get; set; }
		bool DisableSettings { get; set; }
		bool SkipTabStop { get; set; }
		bool ReadOnly { get; set; }
		bool BoldLabel { get; set; }
        bool FitInTable { get; set; }
	}
}
