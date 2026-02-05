using System;
namespace SoftOne.Soe.Web.ajax
{
    public partial class getSysTerm : JsonBase
	{
		protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["sysTermId"], out int sysTermId))
            {
                string name;
                if (Int32.TryParse(QS["sysTermGroupId"], out int sysTermGroupId))
                    name = PageBase.GetText(sysTermId, sysTermGroupId);
                else
                    name = PageBase.TextService.GetText(sysTermId);

                if (!String.IsNullOrEmpty(name))
                {
                    ResponseObject = new
                    {
                        Found = true,
                        Name = name,
                    };
                }
            }

            if (ResponseObject == null)
			{
				ResponseObject = new
				{
					Found = false
				};
			}
		}
	}
}
