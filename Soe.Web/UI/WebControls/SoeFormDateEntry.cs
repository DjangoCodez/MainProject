using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.UI.WebControls
{
    /// <summary>
    /// An input type text element that goes into a SoeForm.
    /// </summary>
	public class SoeFormDateEntry : SoeFormTextEntry
    {
		protected override string DefaultCssClass
		{
			get
			{
				string css = base.DefaultCssClass;
				if (css.Length > 0)
					css += " ";
				css += "date";
				return css;
			}
		}

		public override bool Validate()
		{
			if (!base.Validate())
				return false;

			return Validator.ValidateDate(Value);
		}
    }
}
