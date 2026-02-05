using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class ActorContactPerson : ControlBase
    {
        #region Variables

        private ContactManager ctm;
        public TermGroup_SysContactType Type { get; set; }

        #endregion

        #region Constructor

        public ActorContactPerson()
        {
            if (Type == TermGroup_SysContactType.Undefined) //default initializer
            {
                Type = TermGroup_SysContactType.Company;
            }
        }

        #endregion

        public void InitControl(Controls.Form Form1)
        {
            //Add scripts and style sheets
            PageBase.Scripts.Add("/UserControls/ActorContactAddressList.js");

            this.SoeForm = Form1;
            ActorECom.InitControl(SoeForm);
            ActorContactAddressList.InitControl(Form1);
        }

        public void Populate(ContactPerson contactPerson, bool repopulate)
        {
            ctm = new ContactManager(PageBase.ParameterObject);

            #region Populate

            int actorId = contactPerson != null && contactPerson.Actor != null ? contactPerson.Actor.ActorId : 0;

            //Set UserControl parameters
            this.ActorECom.Populate(repopulate, actorId);
            ActorContactAddressList.ActorId = actorId;

            Position.ConnectDataSource(PageBase.GetGrpText(TermGroup.ContactPersonPosition, addEmptyRow: true));
            Sex.ConnectDataSource(PageBase.GetGrpText(TermGroup.Sex));

            #endregion

            #region Set data

            if (contactPerson != null && contactPerson.Actor != null)
            {
                FirstName.Value = contactPerson.FirstName;
                LastName.Value = contactPerson.LastName;
                Position.Value = contactPerson.Position.ToString();
                SocialSec.Value = contactPerson.SocialSec;
                Sex.Value = contactPerson.Sex.ToString();
            }

            #endregion
        }

        public ActionResult Save(NameValueCollection F, ContactPerson contactPerson, int actorCompanyId, int userId)
        {
            ActionResult result = new ActionResult(false);

            if (ctm == null)
                ctm = new ContactManager(PageBase.ParameterObject);

            string firstName = F["FirstName"];
            string lastName = F["LastName"];
            int position = Convert.ToInt32(F["Position"]);
            string socialSec = F["SocialSec"];
            int sex = 0;
            Int32.TryParse(F["Sex"], out sex);

            if (contactPerson == null)
            {
                //Create ContactPerson
                contactPerson = new ContactPerson()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Position = position,
                    SocialSec = socialSec,
                    Sex = sex
                };

                if (ctm.AddContactPerson(contactPerson, actorCompanyId).Success)
                {
                    result = SaveContact(F, contactPerson, false);
                }
                else
                {
                    result.Success = false;
                    result.ErrorNumber = (int)ActionResultSave.ContactPersonNotSaved;
                }
            }
            else
            {
                contactPerson.FirstName = firstName;
                contactPerson.LastName = lastName;
                contactPerson.Position = position;
                contactPerson.SocialSec = socialSec;
                contactPerson.Sex = sex;

                if (ctm.UpdateContactPerson(contactPerson).Success)
                {
                    result = SaveContact(F, contactPerson, true);
                }
                else
                {
                    result.Success = false;
                    result.SuccessNumber = (int)ActionResultSave.ContactPersonNotUpdated;
                }
            }

            return result;
        }

        private ActionResult SaveContact(NameValueCollection F, ContactPerson contactPerson, bool update)
        {
            ActionResult result = new ActionResult(false);
            if (contactPerson == null || contactPerson.Actor == null)
                return result;

            if (ctm.SaveContact(contactPerson.Actor.ActorId).Success)
            {
                bool actorEcomSuccess = this.ActorECom.Save(F, contactPerson.Actor.ActorId, false);
                bool actorContactAddressSuccess = ActorContactAddressList.Save(F, contactPerson.Actor.ActorId, false);
                if (actorEcomSuccess && actorContactAddressSuccess)
                {
                    result.Success = true;
                    result.SuccessNumber = update ? (int)ActionResultSave.ContactPersonUpdated : (int)ActionResultSave.ContactPersonSaved;
                    result.Value = contactPerson;
                }
                else
                {
                    result.Success = true;
                    result.SuccessNumber = update ? (int)ActionResultSave.ContactPersonUpdatedWithErrors : (int)ActionResultSave.ContactPersonSavedWithErrors;
                    result.Value = contactPerson;
                }
            }
            else
            {
                result.Success = false;
                result.ErrorNumber = (int)ActionResultSave.ContactPersonSavedWithErrors;
            }

            return result;
        }

        public ActionResult Delete(ContactPerson contactPerson, int actorId, int userId)
        {
            ActionResult result = new ActionResult(false);

            if (ctm == null)
                ctm = new ContactManager(PageBase.ParameterObject);

            if (ctm.DeleteContactPerson(contactPerson, actorId).Success)
            {
                result.Success = true;
                result.SuccessNumber = (int)ActionResultDelete.ContactPersonDeleted;
            }
            else
            {
                result.Success = false;
                result.SuccessNumber = (int)ActionResultDelete.ContactPersonNotDeleted;
            }

            return result;
        }
    }
}