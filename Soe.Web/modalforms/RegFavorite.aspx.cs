using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class RegFavorite : PageBase
    {
        #region Variables

        private CompanyManager cm = null;

        private int? companyId = null;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CompanyManager(ParameterObject);

            ((ModalFormMaster)Master).HeaderText = GetText(2000, "Ny favorit");
            ((ModalFormMaster)Master).Action = Url;

            string url = QS["url"];
            if (!string.IsNullOrEmpty(url))
            {
                url = UrlUtil.TrimUrl(Server.UrlDecode(url));
                url = AddClassficationToQueryString(url);
            }

            bool remote = StringUtility.GetBool(QS["remote"]);
            if (remote)
            {
                companyId = StringUtility.GetInt(QS["company"], 0);
                if (companyId == 0)
                    Redirect(url);
            }

            if (string.IsNullOrEmpty(url) && !remote)
                Redirect();

            #endregion

            if (F.Count == 0)
            {
                #region Setup

                if (remote && IsSupportLicense)
                {
                    if (companyId.HasValue && companyId.Value > 0)
                        RegFavoriteName.Value = cm.GetCompanyName(companyId.Value, true, true) + " " + TextService.GetSupportText();

                    RemoteFavorite.Visible = true;
                    RemoteFavorite.Value = Boolean.TrueString;
                    CompanyFavorite.Value = Boolean.TrueString;
                    UseAsDefaultPage.Value = Boolean.FalseString;
                }
                else
                {
                    RemoteFavorite.Visible = false;
                    RemoteFavorite.Value = Boolean.FalseString;
                    CompanyFavorite.Value = UrlUtil.UrlIsCompanySpecific(url).ToString();
                    UseAsDefaultPage.Value = Boolean.FalseString;
                }

                #endregion
            }
            else
            {
                #region Save

                string favoriteUrl = url;
                string name = F["RegFavoriteName"];
                bool remoteFavorite = StringUtility.GetBool(F["RemoteFavorite"]);
                bool companyFavorite = StringUtility.GetBool(F["CompanyFavorite"]);
                bool useAsDefaultPage = StringUtility.GetBool(F["UseAsDefaultPage"]);

                if (remoteFavorite)
                    favoriteUrl = $"/soe/manage/companies/edit/remote/?company={companyId}&login={1}";
                else if (companyFavorite || UrlUtil.UrlIsCompanySpecific(url))
                    companyId = SoeCompany.ActorCompanyId;

                ActionResult result = new ActionResult(true);
                if (useAsDefaultPage)
                    result = RemoveFavoriteDefaultPage();
                if (result.Success)
                    result = SaveFavorite(name, favoriteUrl, companyId, useAsDefaultPage);
                if (result.Success)
                    ClearPageCache(url);

                Redirect(url);

                #endregion
            }
        }

        private ActionResult SaveFavorite(string name, string url, int? actorCompanyId, bool useAsDefaultPage)
        {
            return SettingCacheManager.Instance.AddUserFavoriteTS(UserId, SoeCompany.ActorCompanyId, actorCompanyId, name, url, useAsDefaultPage);
        }

        private ActionResult RemoveFavoriteDefaultPage()
        {
            return SettingCacheManager.Instance.RemoveFavoriteDefaultPage(UserId);
        }

        private void ClearPageCache(string url)
        {
            RemoveAllOutputCacheItems(UrlUtil.GetAbsolutePath(url));
        }
    }
}
