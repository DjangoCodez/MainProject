using System;
namespace SoftOne.Soe.Web.ajax
{
    public partial class getLanguage : JsonBase
	{
		protected void Page_Load(object sender, EventArgs e)
        {
            ResponseObject = new
			{
                Language = PageBase.Language,
                LanguageId = PageBase.GetLanguageId(),
                IsSwedish = PageBase.IsLanguageSwedish(),
                IsEnglish = PageBase.IsLanguageEnglish(),
                IsFinnish = PageBase.IsLanguageFinnish(),
                IsNorwegian = PageBase.IsLangugeNorwegian(),
                Found = true,
			};
		}
	}
}
