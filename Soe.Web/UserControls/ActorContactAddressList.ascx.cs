using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class ActorContactAddressList : ControlBase
    {
        #region Variables

        private bool initialized;
        public int ActorId { get; set; }
        public string CssClass { get; set; }
        public TermGroup_SysContactType Type { get; set; }

        private ContactManager ctm;

        private IEnumerable<SysContactAddressRowType> sysContactAddressRowTypesAll;
        private IEnumerable<SysContactAddressType> sysContactAddressTypes;
        private Contact contact;

        #endregion

        #region Ctor

        public ActorContactAddressList()
        {
            // Default initializer
            if (Type == TermGroup_SysContactType.Undefined)
                Type = TermGroup_SysContactType.Company;
        }

        #endregion

        public void InitControl(Controls.Form Form1)
        {
            ctm = new ContactManager(PageBase.ParameterObject);

            this.SoeForm = Form1;

            initialized = true;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!initialized)
                return;

            #region Get data

            // Get contact
            contact = ctm.GetContactFromActor(ActorId, loadActor: true);

            // Get address and row types
            sysContactAddressTypes = ctm.GetSysContactAddressTypes((int)Type);
            sysContactAddressRowTypesAll = ctm.GetSysContactAddressRowTypes((int)Type);

            // Get contact addresses for the actor
            List<ContactAddress> contactAddresses = contact != null ? ctm.GetContactAddresses(contact.ContactId) : new List<ContactAddress>();

            #endregion

            #region Create elements

            var div = new HtmlGenericControl("div");

            var fieldset = new HtmlGenericControl("fieldset");
            var legend = new HtmlGenericControl("legend");
            legend.InnerText = PageBase.GetText(5394, "Adressuppgifter");
            fieldset.Controls.Add(legend);
            div.Controls.Add(fieldset);

            var adressContainer = new HtmlGenericControl("div");
            adressContainer.Attributes.Add("class", "addressContainer");
            adressContainer.Attributes.Add("id", "addressContainer");
            fieldset.Controls.Add(adressContainer);

            var selector = new HtmlGenericControl("div");
            selector.Attributes.Add("class", "selector");

            var addressType = new DropDownList()
            {
                ID = "AddressType"
            };
            addressType.DataSource = PageBase.GetGrpText(TermGroup.SysContactAddressType);
            addressType.DataTextField = "value";
            addressType.DataValueField = "key";
            addressType.DataBind();

            var button = new HtmlGenericControl("div")
            {
                InnerHtml = PageBase.GetText(5554, "Lägg till adress"),
            };
            button.Attributes.Add("onclick", "addAdress();");
            button.Attributes.Add("class", "button btn btn-default");

            var adresses = new HtmlGenericControl("div");
            adresses.Attributes.Add("class", "adresses");
            adresses.Attributes.Add("id", "adresses");

            var adressrows = new HtmlGenericControl("div");
            adressrows.Attributes.Add("class", "rows");
            adressrows.Attributes.Add("id", "adressrows");

            var labelcontainer = new HtmlGenericControl("div");
            labelcontainer.Attributes.Add("class", "labelcontainer");
            labelcontainer.Attributes.Add("id", "labelcontainer");

            var inputcontainer = new HtmlGenericControl("div");
            inputcontainer.Attributes.Add("class", "inputcontainer");
            inputcontainer.Attributes.Add("id", "inputcontainer");

            var labelcontainer2 = new HtmlGenericControl("div");
            labelcontainer2.Attributes.Add("class", "labelcontainer2");
            labelcontainer2.Attributes.Add("id", "labelcontainer2");

            var inputcontainer2 = new HtmlGenericControl("div");
            inputcontainer2.Attributes.Add("class", "inputcontainer2");
            inputcontainer2.Attributes.Add("id", "inputcontainer2");

            #endregion

            #region Create Hierarchy

            adressrows.Controls.Add(labelcontainer);
            adressrows.Controls.Add(inputcontainer);
            adressrows.Controls.Add(GetClear());
            adressrows.Controls.Add(labelcontainer2);
            adressrows.Controls.Add(inputcontainer2);

            selector.Controls.Add(addressType);
            selector.Controls.Add(button);

            adressContainer.Controls.Add(GetClear());
            adressContainer.Controls.Add(selector);
            adressContainer.Controls.Add(GetClear());
            adressContainer.Controls.Add(adresses);
            adressContainer.Controls.Add(adressrows);
            adressContainer.Controls.Add(GetClear());

            #endregion

            div.RenderControl(writer);

            #region Render Javascript

            RenderDataForExisting(writer, contactAddresses);
            RenderJavascriptMetaData(writer);

            #endregion
        }

        private void RenderDataForExisting(HtmlTextWriter writer, List<ContactAddress> contactAddresses)
        {
            writer.WriteLine();
            writer.Write("<script");
            writer.WriteAttribute("type", @"text/javascript");
            writer.Write(">");
            //Write data objects to array
            writer.WriteLine("var initialAdresses = new Array();");
            foreach (var adress in contactAddresses)
            {
                writer.WriteLine("var b=new Object();");
                writer.WriteLine("b.SysContactAddressTypeId=" + adress.SysContactAddressTypeId + ";");
                writer.WriteLine("b.ContactAddressId=" + adress.ContactAddressId + ";");
                writer.WriteLine("b.Label='" + StringUtility.XmlEncode(!String.IsNullOrEmpty(adress.Name) ? adress.Name : PageBase.TextService.GetText(adress.SysContactAddressTypeId, (int)TermGroup.SysContactAddressType)) + "';");
                writer.WriteLine("b.Rows = new Array();");
                var sysRows = (from i in sysContactAddressRowTypesAll
                               where i.SysContactAddressTypeId == adress.SysContactAddressTypeId
                               orderby i.SysTermId
                               select i).ToList<SysContactAddressRowType>();

                foreach (var row in sysRows)
                {
                    var contactAddressRow = (from i in adress.ContactAddressRow
                                             where i.ContactAddress.SysContactAddressTypeId == adress.SysContactAddressTypeId &&
                                             i.SysContactAddressRowTypeId == row.SysContactAddressRowTypeId
                                             select i).FirstOrDefault<ContactAddressRow>();

                    string value = string.Empty;
                    if (contactAddressRow != null)
                        value = contactAddressRow.Text;

                    writer.WriteLine("var r = new Object();");
                    writer.WriteLine("r.Label='" + PageBase.TextService.GetText(row.SysTermId, (int)TermGroup.SysContactAddressRowType) + "';");
                    writer.WriteLine("r.SysContactAddressRowTypeId=" + row.SysContactAddressRowTypeId + ";");
                    writer.WriteLine("r.value='" + value + "';");
                    writer.WriteLine("b.Rows.push(r);");
                }
                writer.WriteLine("initialAdresses.push(b);");
            }
            writer.Write("</script>");
            writer.WriteLine();
        }

        private void RenderJavascriptMetaData(HtmlTextWriter writer)
        {
            writer.WriteLine();
            writer.Write("<script");
            writer.WriteAttribute("type", @"text/javascript");
            writer.Write(">");

            //Write data objects to array
            writer.WriteLine("var actorAdresses = new Array();");
            foreach (var sysContactAddress in sysContactAddressTypes)
            {
                writer.WriteLine("var a=new Object();");
                writer.WriteLine("a.SysContactAddressTypeId=" + sysContactAddress.SysContactAddressTypeId + ";");
                writer.WriteLine("a.SysContactAddressId=" + sysContactAddress.SysContactTypeId + ";");
                writer.WriteLine("a.Label='" + PageBase.TextService.GetText(sysContactAddress.SysTermId, sysContactAddress.SysTermGroupId) + "';");
                writer.WriteLine("a.Rows = new Array();");
                var sysRows = (from i in sysContactAddressRowTypesAll
                               where i.SysContactAddressTypeId == sysContactAddress.SysContactAddressTypeId
                               && i.SysContactTypeId == sysContactAddress.SysContactTypeId
                               select i).ToList();

                foreach (var row in sysRows)
                {
                    writer.WriteLine("var row = new Object();");
                    writer.WriteLine("row.Label='" + PageBase.TextService.GetText(row.SysTermId, row.SysTermGroupId) + "';");
                    writer.WriteLine("row.SysContactAddressRowTypeId=" + row.SysContactAddressRowTypeId + ";");
                    writer.WriteLine("a.Rows.push(row);");
                }
                writer.WriteLine("actorAdresses.push(a);");
            }
            writer.Write("</script>");
            writer.WriteLine();
        }

        private HtmlGenericControl GetClear()
        {
            HtmlGenericControl clear = new HtmlGenericControl("div");
            clear.Attributes.Add("class", "clear");
            return clear;
        }

        #region Save

        public bool Save(NameValueCollection F, int actorId, bool saveContact)
        {
            #region Init

            ActorId = actorId;

            if (ctm == null)
                ctm = new ContactManager(PageBase.ParameterObject);

            if (saveContact)
            {
                var result = ctm.SaveContact(actorId);
                if (!result.Success)
                    return false;

                contact = result.Value as Contact;
            }

            if (contact == null)
                contact = ctm.GetContactFromActor(ActorId);
            if (contact == null)
                return false;

            #endregion

            #region Prereq

            #region Parse Adress's and Row's

            List<Address> addresses = new List<Address>();
            List<Row> rows = new List<Row>();

            foreach (string key in F.AllKeys)
            {
                string[] keys = key.Split('_');

                if (keys[0] == "adress" && keys.Length >= 6)
                {
                    #region Adress

                    int sysContactAddressId = 0;
                    Int32.TryParse(keys[4], out sysContactAddressId);

                    int contactAddressId = 0;
                    Int32.TryParse(keys[2], out contactAddressId);

                    int id = 0;
                    Int32.TryParse(keys[8], out id);

                    int sysContactAddressTypeId = 0;
                    Int32.TryParse(keys[6], out sysContactAddressTypeId);

                    Address address = new Address()
                    {
                        ContactAddressId = contactAddressId,
                        Id = id,
                        SysContactAddressTypeId = sysContactAddressTypeId,
                        Value = F[key],
                    };
                    addresses.Add(address);

                    #endregion
                }

                else if (keys[0] == "row" && keys.Length >= 2)
                {
                    #region Row

                    int id = 0;
                    Int32.TryParse(keys[2], out id);

                    int sysContactAddressRowTypeId = 0;
                    Int32.TryParse(keys[1], out sysContactAddressRowTypeId);

                    Row row = new Row()
                    {
                        Id = id,
                        SysContactAddressRowTypeId = sysContactAddressRowTypeId,
                        Value = F[key],
                    };
                    rows.Add(row);

                    #endregion
                }
            }

            List<int> adressIds = (from a in addresses select a.Id).ToList();

            #endregion

            //Get all ContactAddresse's and SysContactAddressRowType's once
            List<ContactAddress> contactAddresses = ctm.GetContactAddresses(contact.ContactId);
            List<ContactAddressRow> contactAddressRows = ctm.GetContactAddressRows(contact.ContactId);
            ContactAddress currentContactAddress = null;

            #endregion

            #region Save

            foreach (int adressId in adressIds)
            {
                #region Adress

                Address address = addresses.FirstOrDefault(i => i.Id == adressId);
                List<Row> rowsForAdress = rows.Where(i => i.Id == adressId).ToList();

                if (currentContactAddress != null)
                    contactAddresses.Remove(currentContactAddress);
                currentContactAddress = null;

                #endregion

                #region Rows

                foreach (Row row in rowsForAdress)
                {
                    #region Row

                    int sysContactAddressTypeId = address.SysContactAddressTypeId;
                    int contactAddressId = address.ContactAddressId;
                    int sysContactAddressRowTypeId = row.SysContactAddressRowTypeId;

                    if (sysContactAddressTypeId != 0 && sysContactAddressRowTypeId != 0 && row.Value != null)
                    {
                        #region ContactAddress

                        if (currentContactAddress == null)
                        {
                            if (contactAddressId > 0)
                            {
                                currentContactAddress = (from i in contactAddresses
                                                         where i.SysContactAddressTypeId == sysContactAddressTypeId &&
                                                         i.ContactAddressId == contactAddressId
                                                         select i).FirstOrDefault();
                            }

                            // No need to store empty string if no address object exist
                            if (currentContactAddress == null && String.IsNullOrEmpty(row.Value))
                                continue;

                            if (currentContactAddress == null)
                            {
                                currentContactAddress = new ContactAddress()
                                {
                                    Contact = contact,
                                    SysContactAddressTypeId = sysContactAddressTypeId,
                                    Name = address.Value,
                                };

                                var result = ctm.AddContactAddress(currentContactAddress);
                                if (result.Success)
                                    currentContactAddress.ContactAddressId = result.IntegerValue;

                                //Add to collection, so ContactAddress wont added once for each ContactAddressRow (causes exception)
                                contactAddresses.Add(currentContactAddress);
                            }
                        }

                        #endregion

                        #region ContactAddressRow

                        ContactAddressRow contactAddressRow = (from i in contactAddressRows
                                                               where i.ContactAddress.SysContactAddressTypeId == sysContactAddressTypeId &&
                                                               i.SysContactAddressRowTypeId == sysContactAddressRowTypeId &&
                                                               i.ContactAddress.ContactAddressId == currentContactAddress.ContactAddressId
                                                               select i).FirstOrDefault<ContactAddressRow>();

                        // No need to store empty string if no address object exist
                        if (contactAddressRow == null && String.IsNullOrEmpty(row.Value))
                            continue;

                        if (contactAddressRow == null)
                        {
                            contactAddressRow = new ContactAddressRow()
                            {
                                SysContactAddressRowTypeId = sysContactAddressRowTypeId,
                                Text = row.Value
                            };
                            ctm.AddContactAddressRow(contactAddressRow, currentContactAddress);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(row.Value))
                            {
                                // Remove row if empty string
                                ctm.DeleteContactAddressRow(contactAddressRow);
                            }
                            else if (!row.Value.Equals(contactAddressRow.Text))
                            {
                                contactAddressRow.Text = row.Value;
                                ctm.UpdateContactAddressRow(contactAddressRow);
                            }
                        }

                        #endregion
                    }

                    #endregion
                }

                #endregion
            }

            if (currentContactAddress != null)
                contactAddresses.Remove(currentContactAddress);

            foreach (ContactAddress contactAddress in contactAddresses)
            {
                #region ContactAddress

                List<ContactAddressRow> adressRows = (from i in contactAddressRows
                                                      where i.ContactAddress.SysContactAddressTypeId == contactAddress.SysContactAddressTypeId &&
                                                      i.ContactAddress.ContactAddressId == contactAddress.ContactAddressId
                                                      select i).ToList();

                for (int i = adressRows.Count - 1; i >= 0; i--)
                {
                    ctm.DeleteContactAddressRow(adressRows[i]);
                }

                ctm.DeleteContactAddress(contactAddress, contact.ContactId);

                #endregion
            }

            #endregion

            return true;
        }

        #endregion
    }

    #region Help-classes

    public class Address
    {
        public int Id { get; set; }
        public int ContactAddressId { get; set; }
        public int SysContactAddressId { get; set; }
        public int SysContactAddressTypeId { get; set; }
        public string Value { get; set; }
    }

    public class Row
    {
        public int SysContactAddressRowTypeId { get; set; }
        public int Id { get; set; }
        public string Value { get; set; }
    }

    #endregion
}