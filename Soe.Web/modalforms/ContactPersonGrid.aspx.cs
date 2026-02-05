using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class ContactPersonGrid : PageBase
    {
        private ActorManager am;
        private ContactManager ctm;

        protected Actor actor;
        protected string actorName;        

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new ActorManager(ParameterObject);
            ctm = new ContactManager(ParameterObject);

            bool connectView = false;
            string connect = QS["connect"];

            bool disconnectView = false;
            string disconnect = QS["disconnect"];

            int actorId;
            Int32.TryParse(QS["actor"], out actorId);

            if (StringUtility.GetBool(connect))
                connectView = true;
            else if (StringUtility.GetBool(disconnect))
                disconnectView = true;     

            #endregion
                
            List<ContactPerson> contactPersons = null;

            if (connectView)
            {                                               
                contactPersons = ctm.GetContactPersonsAll(SoeCompany.ActorCompanyId).ToList();
                var contactPersonsFilter = ctm.GetContactPersons(actorId);                
                foreach (ContactPerson cp in contactPersonsFilter)
                {
                    foreach (ContactPerson cp2 in contactPersons)
                    {
                        if (cp.ActorContactPersonId == cp2.ActorContactPersonId)
                        {
                            contactPersons.Remove(cp2);
                            break;
                        }
                    }
                }

                actor = am.GetActor(actorId, true);
                if (actor != null)
                    actorName = am.GetActorTypeName(actor);                       
            }
            else
            {
                contactPersons = ctm.GetContactPersons(actorId).ToList();
                actor = am.GetActor(actorId, true);
                if (actor != null)
                    actorName = am.GetActorTypeName(actor);
            }

            if (F.Count > 0)
            {
                if (F.Count == 1 || (!connectView && !disconnectView))
                    Response.Redirect("/soe/manage/contactpersons/edit/?actor=" + actorId);
                
                int contactPersonId;
                for (int i = 0; i < F.Count; i++)
                {
                    Int32.TryParse(F[i], out contactPersonId);
                    if (contactPersonId > 0)
                    {
                        if (connectView)
                            ctm.MapActorToContactPerson(contactPersonId, actorId); 
                        else if (disconnectView)
                            ctm.UnMapActorFromContactPerson(contactPersonId, actorId); 
                    }
                }

                Response.Redirect(Request.UrlReferrer.ToString());
            }

            if (connectView)
            {
                ((ModalFormMaster)Master).HeaderText = GetText(2262, "Lägg till kontaktpersoner för") + " " + actorName;
                ((ModalFormMaster)Master).SubmitButtonText = GetText(2259, "Lägg till");
                ((ModalFormMaster)Master).ActionButtonText = GetText(2261, "Registrera ny");
                ((ModalFormMaster)Master).showActionButton = true;
            }
            else if (disconnectView)
            {
                ((ModalFormMaster)Master).HeaderText = GetText(2258, "Ta bort kontaktpersoner från") + " " + actorName;
                ((ModalFormMaster)Master).SubmitButtonText = GetText(2260, "Ta bort");
            }
            else
            {
                ((ModalFormMaster)Master).HeaderText = GetText(1588, "Kontaktpersoner") + " " + GetText(1604, "för") + " " + " " + actorName;
                ((ModalFormMaster)Master).ActionButtonText = GetText(2261, "Registrera ny");
                ((ModalFormMaster)Master).showSubmitButton = false;
                ((ModalFormMaster)Master).showActionButton = true;
            }

            ((ModalFormMaster)Master).Action = Url;

            #region Render Table

            HtmlTableRow tRow;
            HtmlTableCell tCell;
            HtmlInputCheckBox checkBox;
            Text label;
            LiteralControl value;

            #region Header

            tRow = new HtmlTableRow();

            //Firstname
            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 2094,
                DefaultTerm = "Förnamn",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tCell.Style["Padding"] = "5px";
            tRow.Cells.Add(tCell);

            //Lastname
            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 2095,
                DefaultTerm = "Efternamn",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tCell.Style["Padding"] = "5px";
            tRow.Cells.Add(tCell);

            //Email
            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 2252,
                DefaultTerm = "Epost",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tCell.Style["Padding"] = "5px";
            tRow.Cells.Add(tCell);

            //Phone work
            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 2253,
                DefaultTerm = "Telefon arbete",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tCell.Style["Padding"] = "5px";
            tRow.Cells.Add(tCell);

            //Phone mobile
            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 2254,
                DefaultTerm = "Mobiltelefon",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tCell.Style["Padding"] = "5px";
            tRow.Cells.Add(tCell);

            //Phone home
            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 2255,
                DefaultTerm = "Telefon hem",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tCell.Style["Padding"] = "5px";
            tRow.Cells.Add(tCell);

            TableContactInfo.Rows.Add(tRow);

            #endregion

            string ecomMail, ecomPhoneMobile, ecomPhoneJob, ecomPhoneHome;
            bool odd = true;

            foreach (var contactPerson in contactPersons)
            {
                #region ContactPerson

                tRow = new HtmlTableRow();

                if (odd)
                {
                    tRow.Attributes.Add("class", "odd");
                    odd = false;
                }
                else
                {
                    tRow.Attributes.Add("class", "even");
                    odd = true;
                }

                tCell = new HtmlTableCell();

                //Id
                //text = new LiteralControl(cp.ActorContactPersonId.ToString());
                //tCell = new HtmlTableCell();
                //tCell.Style["Padding"] = "5px";
                //tCell.Controls.Add(text);
                //tRow.Cells.Add(tCell);

                //Firstname
                value = new LiteralControl(contactPerson.FirstName);
                tCell = new HtmlTableCell();
                tCell.Style["Padding"] = "5px";
                tCell.Controls.Add(value);
                tRow.Cells.Add(tCell);

                //Lastname
                value = new LiteralControl(contactPerson.LastName);
                tCell = new HtmlTableCell();
                tCell.Style["Padding"] = "5px";
                tCell.Controls.Add(value);
                tRow.Cells.Add(tCell);

                ecomMail = "&nbsp;";
                ecomPhoneMobile = "&nbsp;";
                ecomPhoneJob = "&nbsp;";
                ecomPhoneHome = "&nbsp;";

                var contactEcoms = ctm.GetContactEComsFromActor(contactPerson.ActorContactPersonId, false);
                foreach (var contactEcom in contactEcoms)
                {
                    #region ContactECom

                    if (contactEcom.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)
                    {
                        if (ecomMail.Equals("&nbsp;"))
                            ecomMail = contactEcom.Text;
                        else
                            ecomMail = ecomMail + "<br>" + contactEcom.Text;
                    }
                    else if (contactEcom.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneHome)
                    {
                        if (ecomPhoneHome.Equals("&nbsp;"))
                            ecomPhoneHome = contactEcom.Text;
                        else
                            ecomPhoneHome = ecomPhoneHome + "<br>" + contactEcom.Text;

                    }
                    else if (contactEcom.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob)
                    {
                        if (ecomPhoneJob.Equals("&nbsp;"))
                            ecomPhoneJob = contactEcom.Text;
                        else
                            ecomPhoneJob = ecomPhoneJob + "<br>" + contactEcom.Text;
                    }
                    else if (contactEcom.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile)
                    {
                        if (ecomPhoneMobile.Equals("&nbsp;"))
                            ecomPhoneMobile = contactEcom.Text;
                        else
                            ecomPhoneMobile = ecomPhoneMobile + "<br>" + contactEcom.Text;
                    }

                    #endregion
                }

                //Email
                value = new LiteralControl(ecomMail);
                tCell = new HtmlTableCell();
                tCell.Style["Padding"] = "5px";
                tCell.Controls.Add(value);
                tRow.Cells.Add(tCell);

                //Phone job
                value = new LiteralControl(ecomPhoneJob);
                tCell = new HtmlTableCell();
                tCell.Style["Padding"] = "5px";
                tCell.Controls.Add(value);
                tRow.Cells.Add(tCell);

                //Phone mobile
                value = new LiteralControl(ecomPhoneMobile);
                tCell = new HtmlTableCell();
                tCell.Style["Padding"] = "5px";
                tCell.Controls.Add(value);
                tRow.Cells.Add(tCell);

                //Phone home
                value = new LiteralControl(ecomPhoneHome);
                tCell = new HtmlTableCell();
                tCell.Style["Padding"] = "5px";
                tCell.Controls.Add(value);
                tRow.Cells.Add(tCell);

                if (connectView || disconnectView)
                {
                    checkBox = new HtmlInputCheckBox();
                    checkBox.ID = contactPerson.ActorContactPersonId.ToString();
                    checkBox.Value = contactPerson.ActorContactPersonId.ToString();                        
                    checkBox.Attributes.Add("Class", "Checkbox");                    
                    tCell = new HtmlTableCell();
                    tCell.Style["Padding"] = "5px";                    
                    tCell.Controls.Add(checkBox);                    
                    tRow.Cells.Add(tCell);
                }
                else
                {
                    value = new LiteralControl("<a Href='/soe/manage/contactpersons/edit/?contactperson=" + contactPerson.ActorContactPersonId + "&actor=" + actorId + "'><div class='ModalEdit'></div></a>");
                    //text = new LiteralControl("<A Href='/soe/manage/contactpersons/edit/?contactperson=" + cp.ActorContactPersonId + "&actor=" + actorId + "'><img border='0' width='16' height='16' src='/img/edit.png'></a> ");
                    tCell = new HtmlTableCell();
                    tCell.Style["Padding"] = "5px";
                    tCell.Controls.Add(value);
                    tRow.Cells.Add(tCell);                    
                }

                TableContactInfo.Rows.Add(tRow);

                #endregion
            }
            
            #endregion
        }
    }
}
