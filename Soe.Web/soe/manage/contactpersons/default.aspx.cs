using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.contactpersons
{
	public partial class _default : PageBase
    {
        #region Variables

        protected ActorManager am;
        protected ContactManager ctm;

        protected Actor actor;
        protected int actorId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_ContactPersons;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
        {
            #region OLD
            #region Init

            /*am = new ActorManager(ParameterObject);
            ctm = new ContactManager(ParameterObject);

			Int32.TryParse(QS["actor"], out actorId);

            #endregion

            //Get data
            actor = am.GetActor(actorId, true);
            List<ContactPerson> contactPersons = null;
            if (actorId == 0)
                contactPersons = ctm.GetContactPersonsAll(SoeCompany.ActorCompanyId);
            else
                contactPersons = ctm.GetContactPersons(actorId);

            //Title
            SoeGrid1.Title = GetText(1588, "Kontaktpersoner");
			if (actor != null)
                SoeGrid1.Title += " " + GetText(1604, "för") + " " + " '" + am.GetActorTypeName(actor) + "'";
            
            //Bind
            SoeGrid1.DataSource = contactPersons;
			SoeGrid1.DataBind();

            #region Navigation

            //Can only register ContactPersons in the current Company
            SoeGrid1.AddRegLink(GetText(1590, "Registrera kontaktperson"), "edit/?actor=" + SoeCompany.ActorCompanyId,
				Feature.Manage_ContactPersons_Edit, Permission.Modify);*/

            #endregion
            #endregion
        }
    }
}
