using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.common.attest.transition
{
    public partial class _default : PageBase
    {
        #region Variables

        private AttestManager am;
        protected int actorCompanyId;
        protected AttestTransition attestTransition;

        // Module specifics
        protected bool EnableBilling { get; set; }
        protected bool EnableEconomy { get; set; }
        protected bool EnableTime { get; set; }
        private SoeModule TargetSoeModule = SoeModule.None;
        private Feature FeatureEdit = Feature.None;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            // Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/soe/common/attest/transition/texts.js.aspx");
            Scripts.Add("/soe/common/attest/transition/default.js");
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Manage_Attest_Customer_AttestTransitions_Edit:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Manage_Attest_Customer_AttestTransitions_Edit;
                        break;
                    case Feature.Manage_Attest_Supplier_AttestTransitions_Edit:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Manage_Attest_Supplier_AttestTransitions_Edit;
                        break;
                    case Feature.Manage_Attest_Time_AttestTransitions_Edit:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Manage_Attest_Time_AttestTransitions_Edit;
                        break;
                    case Feature.Manage_Attest_CaseProject_AttestTransitions_Edit:
                        TargetSoeModule = SoeModule.Manage;
                        FeatureEdit = Feature.Manage_Attest_CaseProject_AttestTransitions_Edit;
                        break;
                }

                Entity.OnChange = "entityChanged('" + TargetSoeModule.ToString() + "');";
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
            if (Int32.TryParse(QS["transition"], out int attestTransitionId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    attestTransition = am.GetPrevNextAttestTransition(attestTransitionId, Mode);
                    ClearSoeFormObject();
                    if (attestTransition != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?transition=" + attestTransition.AttestTransitionId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?transition=" + attestTransitionId);
                }
                else
                {
                    attestTransition = am.GetAttestTransition(attestTransitionId);
                    if (attestTransition == null)
                    {
                        Form1.MessageWarning = GetText(3333, "Övergång hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(3334, "Redigera övergång");
            string registerModeTabHeaderText = GetText(3311, "Registrera övergång");
            PostOptionalParameterCheck(Form1, attestTransition, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = attestTransition != null ? attestTransition.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            // AttestEntity
            List<GenericType> attestEntities = am.GetAttestEntities(false, true, TargetSoeModule);
            Entity.ConnectDataSource(attestEntities, "Name", "Id");

            int entityId = attestTransition != null ? attestTransition.AttestStateFrom.Entity : attestEntities[0].Id;
            Entity.Value = entityId.ToString();

            // States
            List<AttestState> attestStates = am.GetAttestStates(SoeCompany.ActorCompanyId, (TermGroup_AttestEntity)entityId, TargetSoeModule);
            StateFrom.ConnectDataSource(attestStates, "Name", "AttestStateId");
            StateTo.ConnectDataSource(attestStates, "Name", "AttestStateId");

            if (TargetSoeModule != SoeModule.Time)
                NotifyChangeOfAttestState.Visible = false;

            #endregion

                #region Set data

            if (attestTransition != null)
            {
                Name.Value = attestTransition.Name;
                StateFrom.Value = attestTransition.AttestStateFrom.AttestStateId.ToString();
                StateTo.Value = attestTransition.AttestStateTo.AttestStateId.ToString();
                NotifyChangeOfAttestState.Value = attestTransition.NotifyChangeOfAttestState.ToString();
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3335, "Övergång sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(3336, "Övergång kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3337, "Övergång uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3338, "Övergång kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3339, "Övergång finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(3340, "Övergång kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(3341, "Övergång borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(3342, "Övergång kunde inte tas bort, kontrollera att den inte används");
                else if (MessageFromSelf == "NOTDELETED_INUSE_ROLE")
                    Form1.MessageError = GetText(3196, "Övergång kunde inte tas bort, den används på en attestroll");
                else if (MessageFromSelf == "NOTDELETED_INUSE_EMPLOYEEGROUP")
                    Form1.MessageError = GetText(3197, "Övergång kunde inte tas bort, den används på ett tidavtal");
                else if (MessageFromSelf == "NOTDELETED_INUSE_WORKFLOW")
                    Form1.MessageError = GetText(3198, "Övergång kunde inte tas bort, den används i ett attestflöde");
                else if (MessageFromSelf == "NOTDELETED_INUSE_WORKFLOWTEMPLATE")
                    Form1.MessageError = GetText(3199, "Övergång kunde inte tas bort, den används på en attestflödesmall");
            }

            #endregion

            #region Navigation

            if (attestTransition != null)
            {
                Form1.SetRegLink(GetText(3311, "Registrera övergång"), "",
                    FeatureEdit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            int entity = !String.IsNullOrEmpty(F["Entity"]) ? Int32.Parse(F["Entity"]) : 0;
            string name = F["Name"];
            int stateFromId = !String.IsNullOrEmpty(F["StateFrom"]) ? Int32.Parse(F["StateFrom"]) : 0;
            int stateToId = !String.IsNullOrEmpty(F["StateTo"]) ? Int32.Parse(F["StateTo"]) : 0;
            bool notifyChangeOfAttestState = StringUtility.GetBool(F["NotifyChangeOfAttestState"]);

            if (attestTransition == null)
            {
                // Validation: AttestTransition not already exist
                if (am.ExistsAttestTransition((TermGroup_AttestEntity)entity, name, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Create AttestTransition
                attestTransition = new AttestTransition()
                {
                    ActorCompanyId = SoeCompany.ActorCompanyId,
                    Module = (int)TargetSoeModule,
                    Name = name,
                    AttestStateFromId = stateFromId,
                    AttestStateToId = stateToId,
                    NotifyChangeOfAttestState = notifyChangeOfAttestState,
                };

                ActionResult result = am.AddAttestTransition(attestTransition);
                if (result.Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                if (attestTransition.Name != name && am.ExistsAttestTransition((TermGroup_AttestEntity)entity, name, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Update Condition
                attestTransition.Name = name;
                attestTransition.AttestStateFromId = stateFromId;
                attestTransition.AttestStateToId = stateToId;
                attestTransition.NotifyChangeOfAttestState = notifyChangeOfAttestState;

                ActionResult result = am.UpdateAttestTransition(attestTransition);
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
            ActionResult result = am.DeleteAttestTransition(attestTransition);
            if (result.Success)
                RedirectToSelf("DELETED", false, true);
            else
            {
                switch (result.ErrorNumber)
                {
                    case (int)ActionResultDelete.AttestTransitionInUse_Role:
                        RedirectToSelf("NOTDELETED_INUSE_ROLE", true);
                        break;
                    case (int)ActionResultDelete.AttestTransitionInUse_EmployeeGroup:
                        RedirectToSelf("NOTDELETED_INUSE_EMPLOYEEGROUP", true);
                        break;
                    case (int)ActionResultDelete.AttestTransitionInUse_Workflow:
                        RedirectToSelf("NOTDELETED_INUSE_WORKFLOW", true);
                        break;
                    case (int)ActionResultDelete.AttestTransitionInUse_WorkflowTemplate:
                        RedirectToSelf("NOTDELETED_INUSE_WORKFLOWTEMPLATE", true);
                        break;
                }
                RedirectToSelf("NOTDELETED", true);
            }
        }

        #endregion
    }
}
