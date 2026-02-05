import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { SysPayrollPriceDTO, SysPayrollPriceIntervalDTO } from "../../../Common/Models/SysPayrollPriceDTO";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ISystemService } from "../SystemService";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup, SoeEntityState } from "../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SysPayrollPriceIntervalDialogController } from "./Dialogs/SysPayrollPriceIntervalDialogController";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private sysPayrollPriceId: number;
    private sysPayrollPrice: SysPayrollPriceDTO;
    private amountTypes: ISmallGenericType[];

    private selectedInterval: SysPayrollPriceIntervalDTO;
    private selectedIntervalId: number;


    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.sysPayrollPriceId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_System].readPermission;
        this.modifyPermission = response[Feature.Manage_System].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => { this.copy() });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadAmountTypes()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.sysPayrollPriceId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadAmountTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysPayrollPriceAmountType, true, false).then(x => {
            this.amountTypes = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.systemService.getSysPayrollPrice(this.sysPayrollPriceId, true, true, true, true).then(x => {
            this.isNew = false;
            this.sysPayrollPrice = x;

            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.sysPayrollPrice.name);
        });
    }

    private new() {
        this.isNew = true;
        this.sysPayrollPriceId = 0;
    }

    protected copy() {
        this.sysPayrollPriceId = 0;
        this.sysPayrollPrice.sysPayrollPriceId = 0;
        this.sysPayrollPrice.amount = 0;
        if (this.sysPayrollPrice.fromDate)
            this.sysPayrollPrice.fromDate = this.sysPayrollPrice.fromDate.addYears(1);
        this.sysPayrollPrice.created = null;
        this.sysPayrollPrice.createdBy = null;
        this.sysPayrollPrice.modified = null;
        this.sysPayrollPrice.modifiedBy = null;

        this.setDirty();

        this.focusService.focusByName("ctrl_sysPayrollPrice_amount");
    }

    // ACTIONS

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.systemService.saveSysPayrollPrice(this.sysPayrollPrice).then(result => {
                if (result.success) {
                    this.dirtyHandler.clean();
                    this.closeMe(true);
                    completion.completed(Constants.EVENT_EDIT_SAVED, this.sysPayrollPrice);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    private delete() {
        this.sysPayrollPrice.state = SoeEntityState.Deleted;

        this.progress.startDeleteProgress((completion) => {
            this.systemService.saveSysPayrollPrice(this.sysPayrollPrice).then(result => {
                if (result.success) {
                    this.dirtyHandler.clean();
                    this.closeMe(true);
                    completion.completed(this.sysPayrollPrice, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    // EVENTS

    private editInterval(interval: SysPayrollPriceIntervalDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/SysPayrollPrices/Dialogs/SysPayrollPriceIntervalDialog.html"),
            controller: SysPayrollPriceIntervalDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                amountTypes: () => { return this.amountTypes },
                interval: () => { return interval },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.interval) {
                if (!interval) {
                    // Add new interval to the original collection
                    interval = new SysPayrollPriceIntervalDTO();
                    if (!this.sysPayrollPrice.intervals)
                        this.sysPayrollPrice.intervals = [];
                    this.sysPayrollPrice.intervals.push(interval);
                }

                interval.fromInterval = result.interval.fromInterval;
                interval.toInterval = result.interval.toInterval;
                interval.amount = result.interval.amount;
                interval.amountType = result.interval.amountType;
                interval.amountTypeName = this.getAmountTypeName(interval.amountType);

                this.selectedInterval = interval;

                this.setDirty();
            }
        });
    }

    private deleteInterval(interval: SysPayrollPriceIntervalDTO) {
        _.pull(this.sysPayrollPrice.intervals, interval);
        this.setDirty();
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private getAmountTypeName(amountType: number): string {
        var type = _.find(this.amountTypes, a => a.id === amountType);
        return type ? type.name : '';
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.sysPayrollPrice) {
                if (!this.sysPayrollPrice.code)
                    mandatoryFieldKeys.push("common.code");

                if (!this.sysPayrollPrice.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}