using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.UserControls;

namespace SoftOne.Soe.Web.soe.manage.contactpersons.edit
{
	public partial class _default : PageBase
    {
        #region Variables

        protected ActorManager am;
        protected ContactManager ctm;

        protected ContactPerson contactPerson;
		protected int actorId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_ContactPersons_Edit;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new ActorManager(ParameterObject);
			ctm = new ContactManager(ParameterObject);

			//Mandatory parameters
            Int32.TryParse(QS["actor"], out actorId);
                
            //If actorId is not supplied, use current company
            if (actorId == 0)
                actorId = SoeCompany.ActorCompanyId;

			//Mode 
			PreOptionalParameterCheck(Request.Url.AbsolutePath + "?actor=" + actorId, Request.Url.PathAndQuery);

			//Optional parameters
			int actorContactPersonId;
			if (Int32.TryParse(QS["contactperson"], out actorContactPersonId))
			{
				if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
				{
					contactPerson = ctm.GetPrevNextContactPerson(actorContactPersonId, actorId, Mode);
					ClearSoeFormObject();
					if (contactPerson != null)
						Response.Redirect(Request.Url.AbsolutePath + "?contactperson=" + contactPerson.ActorContactPersonId + "&actor=" + actorId);
					else
						Response.Redirect(Request.Url.AbsolutePath + "?contactperson=" + actorContactPersonId + "&actor=" + actorId);
				}
				else
				{
					contactPerson = ctm.GetContactPerson(actorContactPersonId, true);
					if (contactPerson == null || contactPerson.Actor == null)
					{
                        Form1.MessageWarning = GetText(1607, "Kontaktperson hittades inte");
						Mode = SoeFormMode.Register;
					}
				}
			}

			//Mode
			string editModeTabHeaderText = GetText(1589, "Redigera kontaktperson");
			string registerModeTabHeaderText = GetText(1591, "Registrera kontaktperson");
			PostOptionalParameterCheck(Form1, contactPerson, true, editModeTabHeaderText, registerModeTabHeaderText);

            #endregion

            #region UserControls

            //Set UserControl parameters
            ActorContactPerson.InitControl(Form1);

            #endregion

            #region Actions

            if (Form1.IsPosted)
			{
				Save();
            }

            #endregion

            #region Populate

            ActorContactPerson.Populate(contactPerson, Repopulate);

            if (contactPerson != null && contactPerson.Actor != null)
            {
                var actors = am.GetActorsFromContactPerson(contactPerson.ActorContactPersonId);
                SoeGrid1.DataSource = actors;
                SoeGrid1.DataBind();
            }
            else
            {
                SoeGrid1.Visible = false;
            }

            #endregion

            #region MessageFromSelf

            if (MessageFromSelf == "SAVED")
                Form1.MessageSuccess = GetText(1592, "Kontaktperson sparad");
            else if (MessageFromSelf == "SAVED_WITH_ERRORS")
                Form1.MessageWarning = GetText(1592, "Kontaktperson sparad") + ". " + GetText(1616, "Alla kontakt och telekomuppgifter kunde inte sparas");
            else if (MessageFromSelf == "NOTSAVED")
                Form1.MessageError = GetText(1593, "Kontaktperson kunde inte sparas");
            else if (MessageFromSelf == "UPDATED")
                Form1.MessageSuccess = GetText(1594, "Kontaktperson uppdaterad");
            else if (MessageFromSelf == "UPDATED_WITH_ERRORS")
                Form1.MessageWarning = GetText(1594, "Kontaktperson uppdaterad") + ". " + GetText(1617, "Alla kontakt och telekomuppgifter kunde inte uppdateras");
            else if (MessageFromSelf == "NOTUPDATED")
                Form1.MessageError = GetText(1595, "Kontaktperson kunde inte uppdateras");
            else if (MessageFromSelf == "DELETED")
                Form1.MessageSuccess = GetText(1985, "Kontaktperson borttagen");
            else if (MessageFromSelf == "NOTDELETED")
                Form1.MessageError = GetText(1596, "Kontaktperson kunde inte tas bort, kontrollera att den inte används");
            else if (MessageFromSelf == "EXIST")
                Form1.MessageInformation = GetText(1597, "Kontaktperson finns redan");

            #endregion

            #region Navigation

            if (contactPerson != null)
            {
                Form1.SetRegLink(GetText(1590, "Registrera kontaktperson"), "?actor=" + actorId,
                    Feature.Manage_ContactPersons_Edit, Permission.Modify);                
            }

            #endregion
        }

        #region Actions

        protected override void Save()
		{
            var result = ActorContactPerson.Save(F, contactPerson, actorId, UserId);
            if (result.Success)
            {
                switch (result.SuccessNumber)
                {
                    case (int)ActionResultSave.ContactPersonSaved:
                        RedirectToSelf("SAVED");
                        break;
                    case (int)ActionResultSave.ContactPersonSavedWithErrors:
                        RedirectToSelf("SAVED_WITH_ERRORS");
                        break;
                    case (int)ActionResultSave.ContactPersonUpdated:
                        RedirectToSelf("UPDATED");
                        break;
                    case (int)ActionResultSave.ContactPersonUpdatedWithErrors:
                        RedirectToSelf("UPDATED_WITH_ERRORS");
                        break;
                }
            }
            else
            {
                switch (result.ErrorNumber)
                {
                    case (int)ActionResultSave.ContactPersonSaved:
                        RedirectToSelf("NOTSAVED", true);
                        break;
                    case (int)ActionResultSave.ContactPersonSavedWithErrors:
                        RedirectToSelf("NOTUPDATED", true);
                        break;
                }
            }
		}

        protected override void Delete()
        {
            if (ctm.DeleteContactPerson(contactPerson, actorId).Success)
            {
                string postBackUrlQs = "&company=" + actorId;
                RedirectToSelf("DELETED", postBackUrlQs);
            }
            else
            {
                RedirectToSelf("NOTDELETED", true);
            }
        }

        #endregion
    }
}
