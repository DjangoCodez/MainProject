import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IRegistryService } from "../RegistryService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature, TermGroup, TermGroup_CompanyExternalCodeEntity } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { CompanyExternalCodeDTO } from "../../../Common/Models/CompanyExternalCodeDTOs";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IFocusService } from "../../../Core/Services/focusservice";
import { IAttestService } from "../../Attest/AttestService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private companyExternalCodeId: number;
    private companyExternalCode: CompanyExternalCodeDTO;

    private entities: SmallGenericType[] = [];
    private records: SmallGenericType[] = [];
    private _selectedEntity: SmallGenericType;
    private get selectedEntity(): SmallGenericType {
        return this._selectedEntity;
    }
    private set selectedEntity(item: SmallGenericType) {
        this._selectedEntity = item;
        if (this.companyExternalCode) {
            this.companyExternalCode.entity = item.id;
        }
        switch (<TermGroup_CompanyExternalCodeEntity>item.id) {
            case TermGroup_CompanyExternalCodeEntity.PayrollProduct:
                this.loadPayrollProducts();
                break;
            default:
                this.records = [];
                break;
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private registryService: IRegistryService,
        private attestService: IAttestService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.companyExternalCodeId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_ExternalCodes, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_ExternalCodes].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_ExternalCodes].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadEntities()
        ]).then(() => {
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.companyExternalCodeId) {
            return this.loadData();
        } else {
            this.new();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => { this.copy() }, () => this.isNew);
    }

    // SERVICE CALLS

    private loadEntities(): ng.IPromise<any> {
        this.entities = [];
        return this.coreService.getTermGroupContent(TermGroup.CompanyExternalCodeEntity, true, true).then((x) => {
            this.entities = x;
        });
    }

    private loadPayrollProducts(): ng.IPromise<any> {
        this.records = [];
        return this.attestService.getPayrollProductsDict(false, true, true).then((x) => {
            this.records = x;
        });
    }

    private loadData() {
        return this.registryService.getCompanyExternalCode(this.companyExternalCodeId).then(x => {
            this.companyExternalCode = x;
            this.selectedEntity = _.find(this.entities, e => e.id === this.companyExternalCode.entity);
            this.isNew = false;
        });
    }

    // ACTIONS

    private new() {
        this.isNew = true;
        this.companyExternalCodeId = 0;
        this.companyExternalCode = new CompanyExternalCodeDTO();
        this.companyExternalCode.entity = 0;
        this.companyExternalCode.recordId = 0;
        this.focusService.focusById("ctrl_companyExternalCode_externalCode");
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.companyExternalCode.actorCompanyId = CoreUtility.actorCompanyId;

            this.registryService.saveCompanyExternalCode(this.companyExternalCode).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.companyExternalCodeId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.companyExternalCode);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.registryService.deleteCompanyExternalCode(this.companyExternalCode.companyExternalCodeId).then((result) => {
                if (result.success) {
                    completion.completed(this.companyExternalCode, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(false);
        });
    }

    protected copy() {
        super.copy();

        this.companyExternalCode.companyExternalCodeId = this.companyExternalCodeId = 0;
        this.companyExternalCode.externalCode = undefined;

        this.dirtyHandler.isDirty = true;
        this.focusService.focusById("ctrl_companyExternalCode_externalCode");
    }

    // EVENTS

    private entityChanged() {
        this.$timeout(() => {
            // If entity is changed. Load and populate records
            if (this.companyExternalCode.entity !== 0) {
                //this.chosenEntity = this.companyExternalCode.entity;
                //this.loadRecords();
            }
        });
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.companyExternalCode) {
                if (!this.companyExternalCode.externalCode)
                    mandatoryFieldKeys.push("manage.registry.companyexternalcode.externalcode");
            }
        });
    }
}