import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private accountYearId: number;
    private voucherSeriesTypeId: number;
    voucherSeriesType: any;

    // Collections
    private voucherSeriesIds: number[];

    // Flags
    private showNavigationButtons: boolean = true;

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private messagingService: IMessagingService,
        private translationService: ITranslationService,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            //.onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {

        this.voucherSeriesTypeId = parameters.id;
        this.accountYearId = soeConfig.accountYearId;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        if (parameters.ids && parameters.ids.length > 0)
            this.voucherSeriesIds = parameters.ids;
        else
            this.showNavigationButtons = false;

        this.flowHandler.start([{ feature: Feature.Economy_Accounting_VoucherSeries_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_VoucherSeries_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_VoucherSeries_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        // Navigation
        this.toolbar.setupNavigationGroup(() => { return this.isNew }, () => { return false }, (voucherSeriesTypeId) => { this.onLoadData(true, voucherSeriesTypeId); }, this.voucherSeriesIds, this.voucherSeriesTypeId);
    }

    private onLoadData(updateCaption: boolean = false, newVoucherSeriesTypeId: number = 0): ng.IPromise<any> {
        if (newVoucherSeriesTypeId > 0) {
            this.voucherSeriesTypeId = newVoucherSeriesTypeId;
        }

        if (this.voucherSeriesTypeId > 0) {
            return this.progress.startLoadingProgress([() => {
                return this.accountingService.getVoucherSeriesType(this.voucherSeriesTypeId).then((x) => {
                    this.isNew = false;
                    this.voucherSeriesType = x;
                });
            }]).then(() => {
                if (updateCaption)
                    this.updateTabCaption();
            });
        }
        else {
            this.new();
        }
    }

    // ACTIONS
    public save() {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveVoucherSeriesType(this.voucherSeriesType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.voucherSeriesTypeId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.voucherSeriesType);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                if (this.isNew)
                    this.updateTabCaption();
                this.dirtyHandler.clean();
                this.onLoadData();
                this.isNew = false;

                // Release cache
                this.accountingService.getVoucherSeriesByYear(this.accountYearId, false, false);
                this.accountingService.getVoucherSeriesByYear(this.accountYearId, true, false);

            }, error => {

            });
    }

    public delete() {

        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deleteVoucherSeriesType(this.voucherSeriesType.voucherSeriesTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.voucherSeriesType);
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

    private new() {
        this.isNew = true;
        this.voucherSeriesTypeId = 0;
        this.voucherSeriesType = {};
    }

    protected copy() {
        this.isNew = true;
        this.voucherSeriesTypeId = 0;
        this.voucherSeriesType.voucherSeriesTypeId = 0;

        this.dirtyHandler.setDirty();

        this.updateTabCaption();
    }

    private updateTabCaption() {
        const keys: string[] = [
            "common.connect.voucherserie",
            "economy.accounting.newvoucherseriestype"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.messagingHandler.publishSetTabLabel(this.guid, this.isNew ? terms["economy.accounting.newvoucherseriestype"] : terms["common.connect.voucherserie"] + " " + this.voucherSeriesType.voucherSeriesTypeNr);
        });
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.voucherSeriesType) {
                if (!this.voucherSeriesType.voucherSeriesTypeNr) {
                    mandatoryFieldKeys.push("economy.accounting.voucherseriestype.voucherseriestypenr");
                }
                if (!this.voucherSeriesType.name) {
                    mandatoryFieldKeys.push("economy.accounting.voucherseriestype.name");
                }
                if (!this.voucherSeriesType.startNr) {
                    mandatoryFieldKeys.push("economy.accounting.voucherseriestype.startnr");
                }
            }
        });
    }
}