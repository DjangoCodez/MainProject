using System;
using System.Text;

namespace SoftOne.Soe.Web.UI.WebControls
{
    /// <summary>
    /// An input type text element that goes into a SoeForm.
    /// </summary>
	public class SoeFormNumericEntry : SoeFormTextEntry
    {
		public bool AllowNegative { get; set; }
		public bool AllowDecimals { get; set; }

        public SoeFormNumericEntry()
        {

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
    }
}
