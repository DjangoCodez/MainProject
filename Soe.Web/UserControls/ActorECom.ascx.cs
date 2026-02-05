using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class ActorECom : ControlBase
    {
        #region Variables

        private bool initialized;
        private ContactManager ctm;

        #endregion

        public void InitControl(Controls.Form Form1)
        {
            ctm = new ContactManager(PageBase.ParameterObject);

            this.SoeForm = Form1;

            initialized = true;
        }

        public void Populate(bool repopulate, int actorId)
        {
            if (!initialized)
                return;

            ECom.Labels = PageBase.GetGrpText(TermGroup.SysContactEComType, addEmptyRow: true);

            if (repopulate && SoeForm.PreviousForm != null)
            {
                ECom.PreviousForm = SoeForm.PreviousForm;
            }
            else
            {
                if (actorId > 0)
                {
                    int pos = 0;
                    var contactEcoms = ctm.GetContactEComsFromActor(actorId, false);
                    foreach (var contactEcom in contactEcoms)
                    {
                        ECom.AddLabelValue(pos, contactEcom.SysContactEComTypeId.ToString());
                        ECom.AddValueFrom(pos, contactEcom.Text);

                        if (contactEcom.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)
                        {
                            ValidationItem validationItem = new ValidationItem()
                            {
                                Validation = TextEntryValidation.Email,
                                InvalidAlertTermID = 1523,
                                InvalidAlertDefaultTerm = "Du måste ange en korrekt epostadress",
                            };
                            ECom.AddValidationType(pos, validationItem);
                        }

                        pos++;
                        if (pos == ECom.NoOfIntervals)
                            break;
                    }
                }
            }
        }

        public bool Save(NameValueCollection F, int actorId, bool saveContact)
        {
            if (ctm == null)
                ctm = new ContactManager(PageBase.ParameterObject);

            if (saveContact)
            {
                if (!ctm.SaveContact(actorId).Success)
                    return false;
            }

            Collection<FormIntervalEntryItem> formIntervalEntryItems = ECom.GetData(F);
            return ctm.SaveContactECom(formIntervalEntryItems, actorId).Success;
        }

        public bool HasIntervals(NameValueCollection F)
        {
            return ECom.HasIntervals(F);
        }

        internal bool Validate(NameValueCollection F, out string message)
        {
            message = string.Empty;
            bool valid = false;
            Collection<FormIntervalEntryItem> formIntervalEntryItems = ECom.GetData(F);

            foreach (var item in formIntervalEntryItems)
	        {
                if (item.LabelType == (int)TermGroup_SysContactEComType.CompanyAdminEmail || item.LabelType == (int)TermGroup_SysContactEComType.Email)
                {
                    if (!IsValidEmailAddress(item.From))
                    {
                        message = PageBase.GetText(4591, "Ogiltig epostadress !");
                        valid = false;
                        return valid;
                    }
                }
                if (item.LabelType == (int)TermGroup_SysContactEComType.CompanyAdminEmail && !string.IsNullOrEmpty(item.From))
                {
                    valid = true;
                    break;
                }

            }

            if(!valid)
                message = PageBase.GetText(9103, "Du måste ange minst en epost systemadmin under telekomuppgifter.");

            return valid;
        }
        private bool IsValidEmailAddress(string eMail)
        {
            string strPattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";

            if (Regex.IsMatch(eMail, strPattern))
            {
                return true;
            }
            return false;

        }
    }
}


