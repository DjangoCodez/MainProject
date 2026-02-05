using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.common.attest.state
{
    public partial class _default : PageBase
    {
        #region Variables

        private AttestManager am;
        protected int actorCompanyId;
        protected AttestState attestState;

        // Module specifics
        protected bool EnableEconomy { get; set; }
        protected bool EnableBilling { get; set; }
        protected bool EnableTime { get; set; }
        protected bool EnableManage { get; set; }
        private SoeModule TargetSoeModule = SoeModule.None;
        private Feature FeatureEdit = Feature.None;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            // Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/cssjs/jscolor/jscolor.js");
            Scripts.Add("/soe/common/attest/state/controlDependencies.js");
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Manage_Attest_Customer_AttestStates_Edit:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Manage_Attest_Customer_AttestStates_Edit;
                        ImageSource.Visible = false;
                        break;
                    case Feature.Manage_Attest_Supplier_AttestStates_Edit:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Manage_Attest_Supplier_AttestStates_Edit;
                        //Hidden.Visible = false;
                        Locked.Visible = false;
                        ImageSource.Visible = false;
                        break;
                    case Feature.Manage_Attest_Time_AttestStates_Edit:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Manage_Attest_Time_AttestStates_Edit;
                        Hidden.Visible = false;
                        Locked.Visible = false;
                        ImageSource.Visible = false;
                        break;
                    case Feature.Manage_Attest_CaseProject_AttestStates_Edit:
                        EnableManage = true;
                        TargetSoeModule = SoeModule.Manage;
                        FeatureEdit = Feature.Manage_Attest_CaseProject_AttestStates_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AttestManager(ParameterObject);

            //Mandatory parameters

            // Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            // Optional parameters
            int attestStateId;
            if (Int32.TryParse(QS["state"], out attestStateId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    attestState = am.GetPrevNextAttestState(attestStateId, Mode);
                    ClearSoeFormObject();
                    if (attestState != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?state=" + attestState.AttestStateId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?state=" + attestStateId);
                }
                else
                {
                    attestState = am.GetAttestState(attestStateId);
                    if (attestState == null)
                    {
                        Form1.MessageWarning = GetText(3314, "Nivå hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(3315, "Redigera nivå");
            string registerModeTabHeaderText = GetText(3310, "Registrera nivå");
            PostOptionalParameterCheck(Form1, attestState, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = attestState != null ? attestState.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            // AttestEntity
            List<GenericType> attestEntitites = am.GetAttestEntities(false, true, TargetSoeModule);
            Entity.ConnectDataSource(attestEntitites, "Name", "Id");

            Initial.Value = Boolean.FalseString;
            Closed.Value = Boolean.FalseString;
            Hidden.Value = Boolean.FalseString;

            #endregion

            #region Set data

            if (attestState != null)
            {
                Entity.Value = attestState.Entity.ToString();
                Name.Value = attestState.Name;
                Description.Value = attestState.Description;
                Sort.Value = attestState.Sort.ToString();
                Initial.Value = StringUtility.GetBool(attestState.Initial) ? Boolean.TrueString : Boolean.FalseString;
                Closed.Value = StringUtility.GetBool(attestState.Closed) ? Boolean.TrueString : Boolean.FalseString;
                Hidden.Value = StringUtility.GetBool(attestState.Hidden) ? Boolean.TrueString : Boolean.FalseString;
                Locked.Value = StringUtility.GetBool(attestState.Locked) ? Boolean.TrueString : Boolean.FalseString;
                Color.Value = attestState.Color;
                ImageSource.Value = attestState.ImageSource;
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3316, "Nivå sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(91891, "Nivå kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3318, "Nivå uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3319, "Nivå kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3320, "Nivå finns redan");
                else if (MessageFromSelf == "DUPLICATE_INITIAL")
                    Form1.MessageInformation = GetText(3329, "Endast en nivå per typ kan vara markerad som startnivå");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(3321, "Nivå kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(3322, "Nivå borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(3323, "Nivå kunde inte tas bort, kontrollera att den inte används");
                else if (MessageFromSelf == "NOTDELETED_TRANSACTIONSEXIST")
                    Form1.MessageError = GetText(3323, "Nivå kunde inte tas bort, kontrollera att den inte används"); //Ändra term?
                else if (MessageFromSelf == "ATTESTSTATE_DEFAULTCOLOR")
                    Form1.MessageError = GetText(5289, "Färgen är default och kan inte sättas på attestnivå");
            }

            #endregion

            #region Navigation

            if (attestState != null)
            {
                Form1.SetRegLink(GetText(3310, "Registrera nivå"), "",
                    FeatureEdit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            int entity = !String.IsNullOrEmpty(F["Entity"]) ? Int32.Parse(F["Entity"]) : 0;
            string name = F["Name"];
            string description = F["Description"];
            int sort = !String.IsNullOrEmpty(F["Sort"]) ? Int32.Parse(F["Sort"]) : 1;
            bool initial = StringUtility.GetBool(F["Initial"]);
            bool closed = StringUtility.GetBool(F["Closed"]);
            bool hidden = StringUtility.GetBool(F["Hidden"]);
            bool locked = StringUtility.GetBool(F["Locked"]);
            string color = F["Color"];
            string imageSource = F["ImageSource"];

            if (!color.StartsWith("#"))
                color = color.Insert(0, "#");

            //Can not set default color used in AttestTree
            if (EnableTime && (color.Length > 1 && color.Substring(1, color.Length - 1) == Constants.ATTESTSTATE_DEFAULTCOLOR))
                RedirectToSelf("ATTESTSTATE_DEFAULTCOLOR", true);

            if (attestState == null)
            {
                // Validation: AttestState not already exist
                if (am.ExistsAttestState((TermGroup_AttestEntity)entity, name, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Create AttestState
                attestState = new AttestState()
                {
                    Module = (int)TargetSoeModule,
                    Entity = entity,
                    Name = name,
                    Description = description,
                    Sort = sort,
                    Initial = initial,
                    Closed = closed,
                    Hidden = hidden,
                    Locked = locked,
                    Color = color,
                    ImageSource = imageSource
                };

                ActionResult result = am.AddAttestState(attestState, SoeCompany.ActorCompanyId);
                if (result.Success)
                    RedirectToSelf("SAVED");
                else
                {
                    if (result.ErrorNumber == (int)ActionResultSave.DuplicateInitialState)
                        RedirectToSelf("DUPLICATE_INITIAL", true);
                    else
                        RedirectToSelf("NOTSAVED", true);
                }
            }
            else
            {
                if (attestState.Name != name && am.ExistsAttestState((TermGroup_AttestEntity)entity, name, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Update Condition
                attestState.Entity = entity;
                attestState.Name = name;
                attestState.Description = description;
                attestState.Sort = sort;
                attestState.Initial = initial;
                attestState.Closed = closed;
                attestState.Hidden = hidden;
                attestState.Locked = locked;
                attestState.Color = color;
                attestState.ImageSource = imageSource;

                ActionResult result = am.UpdateAttestState(attestState, SoeCompany.ActorCompanyId);
                if (result.Success)
                    RedirectToSelf("UPDATED");
                else
                {
                    if (result.ErrorNumber == (int)ActionResultSave.DuplicateInitialState)
                        RedirectToSelf("DUPLICATE_INITIAL", true);
                    else
                        RedirectToSelf("NOTUPDATED", true);
                }
            }
        }

        protected override void Delete()
        {
            ActionResult result = am.DeleteAttestState(attestState);

            if (result.Success)
            {
                RedirectToSelf("DELETED", false, true);
            }
            else
            {
                if (result.ErrorNumber == (int)ActionResultDelete.AttestStateHasTransactions)
                {
                    RedirectToSelf("NOTDELETED_TRANSACTIONSEXIST", true);
                }
                else
                {
                    RedirectToSelf("NOTDELETED", true);
                }
            }
        }

        #endregion
    }
}
