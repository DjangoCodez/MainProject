using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class removeFavorite : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                int userFavoriteId;
                if (Int32.TryParse(QS["userFavoriteId"], out userFavoriteId))
                {
                    ActionResult result = SettingCacheManager.Instance.DeleteUserFavoriteTS(UserId, userFavoriteId);
                    if (result.Success)
                    {
                        RemoveAllOutputCacheItems(Request.UrlReferrer.AbsolutePath);
                        ResponseObject = new { Success = result.Success };
                    }
                    else
                        ResponseObject = new { Success = false };
                }
            }
            catch (Exception)
            {
                ResponseObject = new { Success = false };
            }
        }
    }
}