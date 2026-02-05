using Newtonsoft.Json;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;

namespace SoftOne.Soe.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class SoeForm : SoeTabView
    {
        #region Constants

        private const string SOEFORM_POSTED = "PostedSoeForm";

        #endregion

        #region Static fields

        public static string SOEFORM_BUTTON_BACK = "Back";
        public static string SOEFORM_BUTTON_SUBMIT = "Submit";
        public static string SOEFORM_BUTTON_DELETEPOST = "DeletePost";
        public static string SOEFORM_BUTTON_RUNREPORT = "RunReport";        

        #endregion

        #region Form properties

        /// <summary>
        /// The text that will appear as caption on the form submit button.
        /// </summary>
        public virtual string ButtonText { get; set; }

        /// <summary>
        /// The encoding type to post data back to the server 
        /// </summary>
        public string EncType { get; set; }

        public string Target { get; set; }

        private string action = null;

        /// <summary>
        /// The form submit action URL. Default is pages current page and query string.
        /// </summary>
        public string Action
        {
            get { return action ?? Page.Request.Url.PathAndQuery; }
            set { action = value; }
        }

        public string OnSubmit { get; set; }

        /// <summary>
        /// True if the current page load is the result of posting the form.
        /// </summary>
        public bool IsPosted
        {
            get
            {
                return Page.Request.Form[SOEFORM_POSTED] == ID;
            }
        }

        #endregion

        #region Form message

        /// <summary>
        /// The Message set (Information, Success, Warning or Error)
        /// </summary>
        public string Message
        {
            get
            {
                return message;
            }
        }
        private string message;

        /// <summary>
        /// A textual information message that will appear in the form footer.
        /// Examples of usage:
        /// - Entity already exist
        /// - Report selection yielded no data
        /// </summary>
        public string MessageInformation
        {
            get
            {
                if (MessageType == SoeMessageType.Information)
                    return message;
                return String.Empty;
            }
            set
            {
                this.message = value;
                this.MessageType = SoeMessageType.Information;
            }
        }

        /// <summary>
        /// A textual success information message that will appear in the form footer.
        /// Examples of usage:
        /// - Entity saved
        /// - Entity uppdated
        /// - Import/Export done
        /// </summary>
        public string MessageSuccess
        {
            get
            {
                if (MessageType == SoeMessageType.Success)
                    return message;
                return String.Empty;
            }
            set
            {
                this.message = value;
                this.MessageType = SoeMessageType.Success;
            }
        }

        /// <summary>
        /// A textual warning message that will appear in the form footer.
        /// Examples of usage:
        /// - Validation failed
        /// - Incorrect indata
        /// - Entity not found
        /// - Entity saved with errors
        /// - Entity updated with errors
        /// - Business rules violation
        /// </summary>
        public string MessageWarning
        {
            get
            {
                if (MessageType == SoeMessageType.Warning)
                    return message;
                return String.Empty;
            }
            set
            {
                this.message = value;
                this.MessageType = SoeMessageType.Warning;
            }
        }

        /// <summary>
        /// A textual error message that will appear in the form footer.
        /// Examples of usage:
        /// - Entity could not be saved
        /// - Entity could not be uppdated
        /// - Entity could not be deleted
        /// - Import/Export failed
        /// </summary>
        public string MessageError
        {
            get
            {
                if (MessageType == SoeMessageType.Error)
                    return message;
                return String.Empty;
            }
            set
            {
                this.message = value;
                this.MessageType = SoeMessageType.Error;
            }
        }

        #endregion

        #region Action properties

        /// <summary>
        /// Hide the Save button on the SoeForm
        /// Detault: Show Save button
        /// </summary>
        public bool DisableSave { get; set; }

        /// <summary>
        /// Show the back button on the SoeForm
        /// Default: Dont show back button
        /// </summary>
        public bool EnableBack { get; set; }

        /// <summary>
        /// Show the Copy icon on the SoeForm
        /// Default: Dont show Copy link
        /// </summary>
        public bool EnableCopy { get; set; }

        /// <summary>
        /// Show the Delete icon on the SoeForm
        /// Default: Dont show Delete link
        /// </summary>
        public bool EnableDelete { get; set; }

        /// <summary>
        /// Show the prev/next post icon on the SoeForm
        /// Default: Dont show prev/next buttons
        /// </summary>
        public bool EnablePrevNext { get; set; }

        /// <summary>
        /// Show the print report icon on the SoeForm
        /// Default: Dont show RunReport button
        /// </summary>
        public bool EnableRunReport { get; set; }

        /// <summary>
        /// Hide the switch labeltext icon on the SoeForm
        /// Detault: Show switch labeltext button
        /// </summary>
        public bool DisableSwitchLabelText { get; set; }

        #endregion

        #region Client data

        public void DistributeClientData()
        {
            string clientDataString = Page.Request.Form["ClientData"];
            if (clientDataString == null)
                throw new SoeGeneralException("No ClientData available", this.ToString());

            // Fyll en dictionary med Json-strängar associerade till kontrollnamn
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();

            JsonTextReader r = new JsonTextReader(new StringReader(clientDataString));
            while (r.Read())
            {
                if (r.TokenType == JsonToken.PropertyName)
                {
                    string name = r.Value.ToString();
                    if (!r.Read())
                        throw new SoeGeneralException("Error in JSON string", this.ToString());

                    if (r.TokenType == JsonToken.StartObject)
                    {
                        int open = 0;
                        StringBuilder jsonString = new StringBuilder();
                        JsonWriter w = new JsonTextWriter(new StringWriter(jsonString));
                        do
                        {
                            switch (r.TokenType)
                            {
                                case JsonToken.StartObject:
                                    w.WriteStartObject();
                                    open++;
                                    break;
                                case JsonToken.EndObject:
                                    w.WriteEndObject();
                                    open--;
                                    break;

                                case JsonToken.Boolean: w.WriteValue((bool)r.Value); break;
                                case JsonToken.Date: w.WriteValue((DateTime)r.Value); break;
                                case JsonToken.EndArray: w.WriteEndArray(); break;
                                case JsonToken.Float: w.WriteValue((float)r.Value); break;
                                case JsonToken.Integer: w.WriteValue((long)r.Value); break;
                                case JsonToken.Null: w.WriteNull(); break;
                                case JsonToken.PropertyName: w.WritePropertyName((string)r.Value); break;
                                case JsonToken.StartArray: w.WriteStartArray(); break;
                                case JsonToken.String: w.WriteValue((string)r.Value); break;
                                case JsonToken.Undefined: w.WriteUndefined(); break;

                                case JsonToken.None:
                                case JsonToken.Comment:
                                break;
                            }
                            if (open == 0)
                                break;
                        } while (r.Read());
                        dict.Add(name, jsonString.ToString());
                    }
                    else if (r.TokenType != JsonToken.Null)
                        throw new SoeGeneralException("Value at this position must be a JSON object on Null", this.ToString());
                }
            }
            r.Close();

            // Populera alla ClientData-kontroller med Json-data från dictionary:n

            foreach (Control c in Tabs.Controls)
            {
                SoeTab tab = c as SoeTab;
                if (tab != null)
                {
                    foreach (Control cc in tab.Controls)
                    {
                        ISoeFormClientDataControl formControl = cc as ISoeFormClientDataControl;
                        if (formControl != null)
                        {
                            string jsonString;
                            if (dict.TryGetValue(formControl.ID, out jsonString))
                            {
                                formControl.SetClientDataJson(jsonString);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

        protected override void Render(HtmlTextWriter writer)
        {
            RenderSoeFormPrefix(writer);
            base.Render(writer);
            RenderSoeFormFooter(writer);
            RenderSoeFormPostfix(writer);
        }
        
        private void RenderSoeFormPrefix(HtmlTextWriter writer)
        {
            writer.Write("<form");
            writer.WriteAttribute("method", "post");
            writer.WriteAttribute("action", Action);
            writer.WriteAttribute("class", "SoeForm");
            if (!string.IsNullOrEmpty(OnSubmit))
                writer.WriteAttribute("onsubmit", "return " + this.OnSubmit);

            if (!String.IsNullOrEmpty(Target))
                writer.WriteAttribute("target", Target);
            if (!String.IsNullOrEmpty(EncType))
                writer.WriteAttribute("enctype", EncType);
            //if (!DisableSave)
            //    writer.WriteAttribute("defaultbutton", SOEFORM_BUTTON_SUBMIT);
            writer.Write(">");
        }

        private void RenderSoeFormPostfix(HtmlTextWriter writer)
        {
            #region ClientData

            writer.Write("<input");
            writer.WriteAttribute("name", "ClientData");
            var ids = new List<string>();
            StringBuilder clientData = new StringBuilder("{");
            bool hasVal = false;
            int tabCount = 0;
            foreach (Control c in Tabs.Controls)
            {
                SoeTab tab = c as SoeTab;
                if (tab != null)
                {
                    tabCount++;
                    bool getId = true;
                    foreach (Control cc in tab.Controls)
                    {
                        if ((getId) && (cc is SoeFormEntryBase))
                        {
                            if (tabCount > 1 && ids.Count == 0)
                            {
                                //workaround to prevent first tab from having no items setting focus on a hidden item on load (=>exception)
                                ids.Clear();
                            }
                            else
                            {
                                ids.Add(cc.ID);
                                getId = false;
                            }
                        }
                        else if (cc is System.Web.UI.HtmlControls.HtmlContainerControl)
                        {
                            var htmlContainer = (System.Web.UI.HtmlControls.HtmlContainerControl)cc;
                        }

                        ISoeFormClientDataControl formControl = cc as ISoeFormClientDataControl;
                        if (formControl != null)
                        {
                            if (hasVal)
                                clientData.Append(",");
                            else
                                hasVal = true;
                            clientData.Append('"');
                            clientData.Append(formControl.ID);
                            clientData.Append("\":");
                            if (formControl.ClientData != null)
                                clientData.Append(JsonConvert.SerializeObject(formControl.ClientData));
                            else
                                clientData.Append(JsonConvert.Null);
                        }
                    }
                }
            }
            clientData.Append("}");

            writer.WriteAttribute("value", clientData.ToString(), true);
            writer.WriteAttribute("type", "hidden");

            #endregion

            writer.Write("></form>");

            string script = "<script type='text/javascript'>function setFocus(id){};</script>"; //default string, if no method is created
            if (ids.Count > 0) //create programatically to get right startids
            {
                script = "<script type='text/javascript'>function setFocus(id){;var a=new Array();";
                foreach (var id in ids)
                    script += "a.push('" + id + "');";
                script += "var e=document.getElementById(a[id]);if(e!=null && (e.disabled != true))try{e.focus();}catch(err){}}setTimeout('setFocus(0);',10);</script>";
            }
            writer.Write(script);
        }

        private void RenderSoeFormFooter(HtmlTextWriter writer)
        {
            #region Prefix

            writer.Write("<div");
            writer.WriteAttribute("class", "row formFooter");
            writer.Write(">");

            #endregion

            #region SoeForm ID

            writer.Write("<input");
            writer.WriteAttribute("type", "hidden");
            writer.WriteAttribute("name", SOEFORM_POSTED);
            writer.WriteAttribute("value", ID);
            writer.Write(">");

            #endregion

            #region Message

            writer.Write("<div");
            writer.WriteAttribute("class", "col-sm-12 messageFooter");
            writer.Write(">");

            if (!String.IsNullOrEmpty(Message))
            {
                writer.Write("<span");
                writer.WriteAttribute("class", "fal fa-info-circle");
                writer.Write(">");
                writer.Write("</span>");

                writer.Write("<span");
                writer.WriteAttribute("class", "message");
                writer.Write(">");
                writer.WriteEncodedText(Message);
                writer.Write("</span>");
            }

            writer.Write("</div>");

            #endregion

            #region Status

            writer.Write("<div");
            writer.WriteAttribute("class", "col-sm-6 statusFooter");
            writer.Write(">");

            RenderFormStatus(writer);

            writer.Write("</div>");

            #endregion

            #region Buttons

            writer.Write("<div");
            writer.WriteAttribute("class", "col-sm-6 buttonsFooter");
            writer.Write(">");
            writer.Write("<div");
            writer.WriteAttribute("class", "pull-right");
            writer.Write(">");

            RenderFormButtons(writer);

            writer.Write("</div>");
            writer.Write("</div>");

            #endregion

            #region Links

            writer.Write("<div");
            writer.WriteAttribute("class", "col-sm-8 linksFooter");
            writer.Write(">");

            RenderFormLinks(writer);

            writer.Write("</div>");

            #endregion

            #region Postfix

            writer.Write("</div>");

            #endregion
        }

        #region Override (parents)

        protected override void RenderFormHeader(HtmlTextWriter writer)
        {
            base.RenderFormHeader(writer);

            #region ToolBar

            RenderFormToolBar(writer);

            #endregion
        }

        #endregion

        #region Virtual (overrided by descendants)

        protected virtual void RenderFormToolBar(HtmlTextWriter writer)
        {
            //Overrided by Form
        }

        protected virtual void RenderFormLinks(HtmlTextWriter writer)
        {
            //Overrided by Form
        }

        protected virtual void RenderFormButtons(HtmlTextWriter writer)
        {
            //Overrided by Form
        }

        protected virtual void RenderFormStatus(HtmlTextWriter writer)
        {
            //Overrided by Form
        }

        #endregion

        #endregion
    }
}
