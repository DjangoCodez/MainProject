using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.UI.WebControls;
using System;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class SoeFormPrefix : ControlBase
    {
        public string Title { get; set; }
        public SoeTabViewType TabType { get; set; }
        protected string TabImageSrc = "";
        protected string StatusImageSrc = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            switch (TabType)
            {
                case SoeTabViewType.Edit:
                    TabImageSrc = "/img/edit.png";
                    break;
                case SoeTabViewType.Setting:
                    TabImageSrc = "/img/gear_view.png";
                    break;
                case SoeTabViewType.View:
                    TabImageSrc = "/img/view.png";
                    break;
                case SoeTabViewType.Admin:
                    TabImageSrc = "/img/worker.png";
                    break;
                default:
                    TabImageSrc = String.Empty;
                    break;
            }
        }

        public void SetStatus(SoeTabView.SoeMessageType messageType)
        {
            switch (messageType)
            {
                case SoeTabView.SoeMessageType.Information:
                    StatusImageSrc = "/img/information.png";
                    break;
                case SoeTabView.SoeMessageType.Success:
                    StatusImageSrc = "/img/check.png";
                    break;
                case SoeTabView.SoeMessageType.Warning:
                    StatusImageSrc = "/img/exclamation.png";
                    break;
                case SoeTabView.SoeMessageType.Error:
                    StatusImageSrc = "/img/error.png";
                    break;
                default:
                    StatusImageSrc = String.Empty;
                    break;
            }
        }
    }
}