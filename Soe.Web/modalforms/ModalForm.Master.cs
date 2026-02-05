using System;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class ModalFormMaster : MasterPageBase
    {
        public string Action { get; set; }
        public string ActionJs { get; set; }
        public string OnSubmit { get; set; }
        public string FormMethod { get; set; }
        public string OnSubmitButtonClick { get; set; }
        public string SubmitButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public string ActionButtonText { get; set; }
        public string HeaderText { get; set; }
        public string InfoText {get;set;}
        public string ContentStyle { get; set; }
        public Boolean showSubmitButton { get; set; }
        public Boolean showCancelButton { get; set; }
        public Boolean showActionButton { get; set; }
        public Boolean showActionJsButton { get; set; }

        protected void Page_Init(object sender, EventArgs e)
        {
            FormMethod = "post";

            SubmitButtonText = PageBase.GetText(41, "OK");
            CancelButtonText = PageBase.GetText(42, "Avbryt");
            ActionButtonText = "";
            showSubmitButton = true;
            showCancelButton = true;
            showActionButton = false;
            showActionJsButton = false;
            ContentStyle = string.Empty;
        }
    }
}
