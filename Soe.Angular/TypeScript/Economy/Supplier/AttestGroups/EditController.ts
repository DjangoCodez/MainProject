import { ICoreService } from "../../../Core/Services/CoreService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IAttestGroupDTO, IAttestWorkFlowTemplateHeadDTO, ISmallGenericType, IAttestWorkFlowTemplateRowDTO } from "../../../Scripts/TypeLite.Net4";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { UserSelectorForTemplateHeadRowDirectiveController } from "./Directives/UserSelectorForTemplateHeadRowDirective";
import { Feature, TermGroup, SoeEntityState, TermGroup_AttestWorkFlowRowProcessType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private attestGroupId: number;
    attestGroup: IAttestGroupDTO;
    terms: any = [];

    // Lookups 
    templates: IAttestWorkFlowTemplateHeadDTO[];
    attestWorkFlowTypes: ISmallGenericType[] = [];
    role_User: ISmallGenericType[] = [];

    private roleOrUser: number = 0;
    private templateRows: IAttestWorkFlowTemplateRowDTO[];
    private userSelectors: UserSelectorForTemplateHeadRowDirectiveController[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        this.userSelectors = [];

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.attestGroupId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups].readPermission;
        this.modifyPermission = response[Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.attestGroupId, recordId => {
            if (recordId !== this.attestGroupId) {
                this.attestGroupId = recordId;

                // reset 
                this.userSelectors = [];

                this.onLoadData();
            }
        });
    }

    // LOOKUPS
    private loadTerms() {
        var keys: string[] = [
            "economy.supplier.attestgroup.attestgroup"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    protected onDoLookups() {
        return this.loadTerms();
    }

    private onLoadData(): ng.IPromise<any> {
        var promises = [];
        if (!(this.attestGroupId > 0)) {
            this.new();
        } else {
            var groupPromise = this.supplierService.getAttestWorkFlowGroup(this.attestGroupId).then((data) => {
                this.attestGroup = data;
                this.isNew = false;
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["economy.supplier.attestgroup.attestgroup"] + ' ' + this.attestGroup.attestGroupName);
            });

            promises.push(groupPromise);
        }

        var templateHeadPromise = this.supplierService.getAttestWorkFlowTemplateHeadsForCurrentCompany().then((data) => {
            this.templates = data;
        });

        var groupTypePromise = this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowType, false, false).then((data) => {
            this.attestWorkFlowTypes = data;
        });

        var roleUserPromise = this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowApproverType, false, false).then((data) => {
            this.role_User = data;
        });

        promises.push(templateHeadPromise, groupTypePromise, roleUserPromise);

        return this.$q.all(promises).then(() => this.doCompanyTemplateRowsLookups());
    }

    // EVENTS
    protected doCompanyTemplateRowsLookups() {
        if (this.attestGroup.attestWorkFlowTemplateHeadId)
            return this.loadCompanyTemplateRows(this.attestGroup.attestWorkFlowTemplateHeadId);
    }

    // ACTIONS
    public save() {
        if (this.attestGroup.rows == null)
            this.attestGroup.rows = [];

        (<any>this.attestGroup).state = SoeEntityState.Active;

        //TODO: not implemented in this part.
        //this.attestGroup.SendMessage = SendMessage.IsChecked != null ? (bool)SendMessage.IsChecked : false;
        //this.attestGroup.AdminInformation = AdminInformation.Text;

        // Supplier attest flow template
        //if (this.supplierId != 0) {
        //    (<any>this.attestGroup).entity = SoeEntityType.Supplier;
        //    this.attestGroup.recordId = this.supplierId;
        //}
        //else {
        //    (<any>this.attestGroup).entity = SoeEntityType.SupplierInvoice;
        //    this.attestGroup.recordId = this.invoiceId;
        //}

        // Create rows from selected users
        var rows = [];
        var regRow = _.find(this.attestGroup.rows, r => r.userId === CoreUtility.userId && (<any>r).processType === TermGroup_AttestWorkFlowRowProcessType.Registered && r.answer);

        if (regRow != null) {
            // Registration row exists
            rows.push(regRow);
        }
        else {
            // Add user that registered the flow, just for logging
            rows.push({
                attestTransitionId: this.userSelectors[0].getAttestTransitionId(),
                userId: CoreUtility.userId,
                processType: TermGroup_AttestWorkFlowRowProcessType.Registered,
                answer: true,
                type: _.filter(this.templateRows, r => r.attestTransitionId == this.userSelectors[0].getAttestTransitionId())[0].type,
            });
        }
        var i = 0;
        this.userSelectors.forEach(us => {
            i++;

            var urows = us.getRowsToSave();

            urows.forEach(r => {
                r.ProcessType = i === 1 ? TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess : TermGroup_AttestWorkFlowRowProcessType.LevelNotReached;
                rows.push(r);
            });
        });

        this.attestGroup.rows = rows;

        this.progress.startSaveProgress((completion) => {
            this.supplierService.saveAttestWorkFlow(this.attestGroup).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.attestGroupId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.attestGroup.attestGroupName));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.attestGroupId = result.integerValue;
                        this.attestGroup.attestWorkFlowHeadId = result.integerValue;
                    }
                    this.attestGroup.name = this.attestGroup.attestGroupName;//its a bit unclear how name and attestgroupname should work, but this makes the tabnames correct.
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.attestGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.supplierService.getAttestWorkFlowGroups(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.attestWorkFlowHeadId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.attestGroupId) {
                    this.attestGroupId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.supplierService.deleteAttestWorkFlow(this.attestGroup.attestWorkFlowHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.attestGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS

    //protected copy() {
    //    super.copy();
    //    this.isNew = true;
    //    this.attestGroupId = 0;
    //    this.attestGroup.attestWorkFlowHeadId = 0;
    //}

    private new() {
        this.isNew = true;
        this.attestGroupId = 0;
        this.attestGroup = <IAttestGroupDTO>{}; //we just fake it
        this.attestGroup.sendMessage = true;
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.attestGroup) {
                if (!this.attestGroup.attestGroupName) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.attestGroup.attestGroupCode) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.attestGroup.attestWorkFlowTemplateHeadId) {
                    mandatoryFieldKeys.push("economy.supplier.attestgroup.choosetemplate");
                }
            }
        });
    }

    //logic

    private templateChanged(templateHeadId: number) {
        var template = _.find(this.templates, t => t.attestWorkFlowTemplateHeadId == templateHeadId);
        this.attestGroup.type = template.type;
        this.attestGroup.rows = [];
        this.templateRows = undefined;
        this.userSelectors = [];
        this.loadCompanyTemplateRows(templateHeadId, true);
    }

    private loadCompanyTemplateRows(templateHeadId: number, templateChanged: boolean = false): ng.IPromise<any> {

        return this.supplierService.getAttestWorkFlowTemplateHeadRows(templateHeadId).then((data) => {
            this.templateRows = data;

            this.templateRows.forEach(row => {
                if (row.type == null)
                    row.type = this.attestGroup.type;

                if (!templateChanged) {
                    _.forEach(this.attestGroup.rows, r => {
                        if (r.attestTransitionId == row.attestTransitionId) {
                            row.type = r.type;
                        }
                    });
                }
            });
        });
    }

    public registerUserSelector(control: UserSelectorForTemplateHeadRowDirectiveController) {
        this.userSelectors.push(control);
    }
}
