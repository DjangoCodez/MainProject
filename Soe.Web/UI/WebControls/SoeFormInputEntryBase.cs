using SoftOne.Soe.Common.Util;
using System;
using System.Text;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    /// <summary>
    /// Base class for input elements that goes into SoeForms.
    /// </summary>
    public abstract class SoeFormInputEntryBase : SoeFormEntryBase
    {
        protected string type;

		public TextEntryValidation Validation { get; set; }

        protected override string DefaultCssClass
		{
			get
			{
				StringBuilder css = new StringBuilder("validate");
				if ((Validation & TextEntryValidation.Required) != 0)
					css.Append("-required");
				if ((Validation & TextEntryValidation.Email) != 0)
					css.Append("-email");
				if ((Validation & TextEntryValidation.Luhn) != 0)
					css.Append("-luhn");
				return css.ToString();
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			RenderPrefix(writer);
			writer.Write("<input");
			writer.WriteAttribute("id", ID);
			writer.WriteAttribute("type", type);
			writer.WriteAttribute("name", Name);
            writer.WriteAttribute("title", Label);
            if (!AutoComplete)
                writer.WriteAttribute("autocomplete", "off");
			RenderEntrySettings(writer);
			RenderEntryActions(writer);
			RenderCssClassAttribute(writer);

			if (Value != null)
				writer.WriteAttribute("value", Value);
			writer.Write(">");
			RenderPostfix(writer);
		}

		public override bool Validate()
		{
			if (!base.Validate())
				return false;

			if (MaxLength.HasValue && Value.Length > MaxLength.Value)
				return false;

			if ((Validation & TextEntryValidation.Required) != 0 && String.IsNullOrEmpty(Value))
				return false;
			if ((Validation & TextEntryValidation.Email) != 0 && !Validator.ValidateEmail(Value))
				return false;
			if ((Validation & TextEntryValidation.Luhn) != 0 && !Validator.ValidateLuhn(Value))
				return false;

			return true;
		}

		[Flags]
		public enum TextEntryValidation : byte
		{
			//Enumeration also in SoftOne.Soe.Business.Util)

			Required = 1,
			//RequiredGroup = 2,
			Email = 4,
			//MinLength = 8,
			//Match = 16,
			Luhn = 32
		}	
	}
}
