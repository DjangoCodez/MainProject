using System;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Drawing;
using SoftOne.Soe.Common.Util;
using System.IO;
using SoftOne.Soe.Web.UI.WebControls;
using SoftOne.Soe.Business.Core.Reporting;

namespace SoftOne.Soe.Web.soe.manage.system.admin.news.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private FeatureManager fm;
        private ReportGenManager rgm;
        private SysNewsManager snm;

        private SysNews sysNews;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rgm = new ReportGenManager(ParameterObject);
            fm = new FeatureManager(ParameterObject);
            snm = new SysNewsManager(ParameterObject);

            //Optional parameters
            int newsId;
            if (Int32.TryParse(QS["newsId"], out newsId))
            {
                sysNews = snm.GetSysNews(newsId);
                if (sysNews == null)
                {
                    string message = GetText(5182, "Nyhet hittades inte");
                    SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Warning);
                    SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Warning);
                }
            }

            SetupEventHandlers();
            SetupLanguage();

            #endregion

            if (!IsPostBack)
            {
                #region Populate

                SysXEArticles.DataSource = fm.GetSysXEArticles();
                SysXEArticles.DataValueField = "SysXEArticleId";
                SysXEArticles.DataTextField = "Name";
                SysXEArticles.DataBind();

                SysNewsDisplayType.DataSource = GetGrpText(TermGroup.SysNewsDisplayType);
                SysNewsDisplayType.DataValueField = "Key";
                SysNewsDisplayType.DataTextField = "Value";
                SysNewsDisplayType.DataBind();

                SysLang.DataSource = GetGrpText(TermGroup.Language);
                SysLang.DataValueField = "Key";
                SysLang.DataTextField = "Value";
                SysLang.DataBind();

                #endregion

                #region Set data

                if (sysNews != null)
                {
                    Author.Text = sysNews.Author;
                    NewsTitle.Text = sysNews.Title;
                    Preview.Text = sysNews.Preview;
                    Description.Text = sysNews.Description;
                    IsPublic.Checked = sysNews.IsPublic;
                    SysXEArticles.SelectedValue = sysNews.SysXEArticleId.ToString();
                    SysNewsDisplayType.SelectedValue = sysNews.DisplayType.ToString();
                    SysLang.SelectedValue = sysNews.SysLanguageId.ToString();
                    AttachmentFileName.Text = sysNews.AttachmentFileName;

                    if (sysNews.IsPublic)
                        SysXEArticles.Enabled = false;
                }
                else
                {
                    IsPublic.Checked = true;
                    SysXEArticles.Enabled = false;
                    SysXEArticles.SelectedIndex = 0;
                    SysNewsDisplayType.SelectedIndex = 0;
                    SysLang.SelectedValue = ((int)TermGroup_Languages.Swedish).ToString();
                }

                #endregion
            }

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                {
                    string message = GetText(5183, "Nyheten sparad");
                    SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Success);
                    SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Success);
                }
                else if (MessageFromSelf == "NOTSAVED")
                {
                    string message = GetText(5184, "Nyheten kunde inte sparas");
                    SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Error);
                    SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Error);
                }
                else if (MessageFromSelf == "DELETED")
                {
                    string message = GetText(5299, "Nyheten borttagen");
                    SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Success);
                    SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Success);
                }
                else if (MessageFromSelf == "NOTDELETED")
                {
                    string message = GetText(5300, "Nyheten kunde inte tas bort");
                    SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Error);
                    SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Error);
                }
            }

            #endregion

            #region Navigation

            SoeFormFooter.AddLink(GetText(5187, "Registrera nyhet"), "",
                Feature.Manage_System, Permission.Modify);

            #endregion
        }

        #region Setup

        private void SetupEventHandlers()
        {
            SoeFormFooter.Save += new EventHandler(SoeFormFooter_ButtonClick);
            SoeFormFooter.Delete += new EventHandler(SoeFormFooter_Delete);
            IsPublic.CheckedChanged += new EventHandler(IsPublic_CheckedChanged);
        }

        private void SetupLanguage()
        {
            SoeFormPrefix.Title = GetText(5270, "Publik nyhet");
            SoeFormPrefix.TabType = SoeTabViewType.Admin;

            SoeFormFooter.ButtonSaveText = GetText(30, "Spara");
            SoeFormFooter.ButtonDeleteText = GetText(2185, "Ta bort");
        }

        #endregion

        #region Events

        protected void SoeFormFooter_ButtonClick(object sender, EventArgs e)
        {
            Save();
        }

        protected void SoeFormFooter_Delete(object sender, EventArgs e)
        {
            Delete();
        }

        protected void IsPublic_CheckedChanged(object sender, EventArgs e)
        {
            if (IsPublic.Checked)
                SysXEArticles.Enabled = false;
            else
                SysXEArticles.Enabled = true;
        }

        #endregion

        #region Action-methods

        protected override void Save()
        {
            string title = NewsTitle.Text;
            string author = Author.Text;
            string preview = Preview.Text;
            string description = Description.Text;
            bool isPublic = IsPublic.Checked;
            int sysXEArticleId = Convert.ToInt32(SysXEArticles.SelectedItem.Value);
            int sysNewsDisplayType = Convert.ToInt32(SysNewsDisplayType.SelectedItem.Value);
            int sysLanguageId = Convert.ToInt32(SysLang.SelectedItem.Value);

            #region Attachment

            byte[] attachment = null;
            string attachmentFileName = "";
            string attachmentFileType = "";
            string attachmentImageSrc = "/img/mimetypes/blank.gif";
            string attachmentContentType = "";
            int attachmentExportType = 0;

            if (Attachment.HasFile)
            {
                int idx = Attachment.PostedFile.FileName.LastIndexOf(".");
                attachment = Attachment.FileBytes;
                attachmentFileName = Attachment.FileName;
                if (idx > 0)
                {
                    attachmentFileType = Attachment.PostedFile.FileName.Substring(idx + 1);
                    SoeExportFormat exportFormat = rgm.ConvertToSoeExportFormat(attachmentFileType);
                    attachmentExportType = (int)exportFormat;
                    rgm.GetResponseContentType(exportFormat, out attachmentContentType, out attachmentFileType, out attachmentImageSrc);
                }
                AttachmentFileName.Text = attachmentFileName;
            }
            else
            {
                AttachmentFileName.Text = "";
            }

            #endregion

            if (sysNews == null)
            {
                #region Add

                sysNews = new SysNews()
                {
                    Title = title,
                    Preview = preview,
                    Description = description,
                    Author = author,
                    IsPublic = isPublic,
                    SysXEArticleId = sysXEArticleId,
                    DisplayType = sysNewsDisplayType,
                    SysLanguageId = sysLanguageId,
                    Attachment = attachment,
                    AttachmentFileName = attachmentFileName,
                    AttachmentImageSrc = attachmentImageSrc,
                    AttachmentExportType = attachmentExportType,
                    PubDate = DateTime.Now,
                };

                if (snm.AddSysNews(sysNews).Success)
                    RedirectToSelf("SAVED", "&newsId=" + sysNews.SysNewsId);
                else
                    RedirectToSelf("NOTSAVED", true);

                #endregion
            }
            else
            {
                #region Update

                sysNews.Author = author;
                sysNews.Title = title;
                sysNews.Preview = preview;
                sysNews.Description = description;
                sysNews.IsPublic = isPublic;
                sysNews.SysXEArticleId = sysXEArticleId;
                sysNews.DisplayType = sysNewsDisplayType;
                sysNews.SysLanguageId = sysLanguageId;
                sysNews.Attachment = attachment;
                sysNews.AttachmentFileName = attachmentFileName;
                sysNews.AttachmentImageSrc = attachmentImageSrc;
                sysNews.AttachmentExportType = attachmentExportType;

                if (snm.UpdateSysNews(sysNews).Success)
                    RedirectToSelf("SAVED", "&newsId=" + sysNews.SysNewsId);
                else
                    RedirectToSelf("NOTSAVED", true);

                #endregion
            }
        }

        protected override void Delete()
        {
            if (sysNews == null)
                return;

            if (snm.DeleteSysNews(sysNews.SysNewsId, SoeUser).Success)
                RedirectToSelf("DELETED", false, true);
            else
            {
                string message = GetText(5300, "Nyheten kunde inte tas bort");
                SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Error);
                SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Error);
            }
        }

        private void Clear()
        {
            Author.Text = "";
            NewsTitle.Text = "";
            Preview.Text = "";
            Description.Text = "";
            AttachmentFileName.Text = "";
        }

        #endregion
    }
}
