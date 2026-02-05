using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class RemoveFavorite : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((ModalFormMaster)Master).HeaderText = GetText(2192, "Ta bort favorit");
            ((ModalFormMaster)Master).Action = Url;

            SettingManager sm = new SettingManager(ParameterObject);

            Favorites.ConnectDataSource(sm.GetUserFavoriteItems(UserId), "FavoriteName", "FavoriteId");

            if (F.Count > 0)
            {
                int userFavoriteId = Convert.ToInt32(F["Favorites"]);
                if (userFavoriteId > 0)
                {
                    ActionResult result = SettingCacheManager.Instance.DeleteUserFavoriteTS(UserId, userFavoriteId);
                    if (result.Success)
                    {
                        //Clear page cache
                        RemoveAllOutputCacheItems(Request.UrlReferrer.AbsolutePath);
                    }
                }
                Response.Redirect(Request.UrlReferrer.ToString());
            }
        }
    }
}
