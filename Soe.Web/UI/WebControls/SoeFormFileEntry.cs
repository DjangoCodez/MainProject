
namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFormFileEntry : SoeFormInputEntryBase
	{
		private const int WIDTH_MIN = 150;

		public override int? Width
		{
			get
			{
				if (!base.Width.HasValue || base.Width.Value < WIDTH_MIN)
					base.Width = WIDTH_MIN;
				return base.Width;
			}
			set
			{
				base.Width = value;
			}
		}

		public SoeFormFileEntry()
        {
            type = "file";
        }
	}
}
