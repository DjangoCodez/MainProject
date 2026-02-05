import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IContractService } from "../ContractService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup, SoeReportTemplateType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ContractGroupDTO } from "../../../Common/Models/ContractDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private contractGroupId: number;
    private periods: any[];
    private priceManagementTypes: any[];
    private orderReportTemplates: any[];
    private invoiceReportTemplates: any[];

    private contractGroup: ContractGroupDTO;
    terms: any = [];
    //@ngInject
    constructor(
        private $q: ng.IQService,
        private contractService: IContractService,
        private coreService: ICoreService,
        private reportService: IReportService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private notificationService: INotificationService,
        private translationService: ITranslationService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
        
    }

    public onInit(parameters: any) {
        if (parameters.id) {
            this.contractGroupId = parameters.id;
        } else {
            this.contractGroupId = 0;
        }
        this.guid = parameters.guid;
        this.navigatorRecords = parameters.navigatorRecords;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Contract_Groups_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private loadTerms() {
        var keys: string[] = [
            "core.warning",
            "billing.contract.contractgroups.intervalrequired",
            "billing.contract.contractgroups.contractgroup"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public save() {
        if (!this.contractGroup.interval || this.contractGroup.interval < 1) {          
            this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["billing.contract.contractgroups.intervalrequired"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);           
            return;
        }

        this.progress.startSaveProgress((completion) => {
            this.contractService.saveContractGroup(this.contractGroup).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {                       
                        if (this.contractGroupId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.contractGroup.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.contractGroupId = result.integerValue;
                        this.contractGroup.contractGroupId = result.integerValue;

                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.contractGroup);
                    }
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

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.contractService.getContractGroups().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.contractGroupId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.contractGroupId) {
                    this.contractGroupId = recordId;
                    this.loadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.contractService.deleteContractGroup(this.contractGroupId).then((result) => {
                if (result.success) {
                    completion.completed(this.contractGroup,true);
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

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.contractGroup) {
                if (!this.contractGroup.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.contractGroup.priceManagement)
                    mandatoryFieldKeys.push("billing.contract.contractgroups.pricemanagement");
                if (!this.contractGroup.interval)
                    mandatoryFieldKeys.push("billing.contract.contractgroups.interval");
                if (!this.contractGroup.period)
                    mandatoryFieldKeys.push("common.period");
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Contract_Groups_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Contract_Groups_Edit].modifyPermission;
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadPeriods(),
            this.loadPriceManagementTypes(),
            this.loadOrderTemplates(),
            this.loadInvoiceTemplates()]).then(() => {
                if(this.contractGroupId)
                    this.loadData()
                else
                    this.newContractGroup();
            });
    }

    private loadPeriods(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ContractGroupPeriod, false, false).then(x => {
            this.periods = x;
        });
    }

    private loadPriceManagementTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ContractGroupPriceManagement, false, false).then(x => {
            this.priceManagementTypes = x;
        });
    }

    private loadOrderTemplates(): ng.IPromise<any> {
        return this.reportService.getReportsDict(SoeReportTemplateType.BillingOrder, false, false, true, false).then(x => {
            this.orderReportTemplates = x;
        });
    }

    private loadInvoiceTemplates(): ng.IPromise<any> {
        return this.reportService.getReportsDict(SoeReportTemplateType.BillingInvoice, false, false, true, false).then(x => {
            this.invoiceReportTemplates = x;
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.contractGroupId, recordId => {
            if (recordId !== this.contractGroupId) {
                this.contractGroupId = recordId;
                this.loadData();
            }
        });
    }

    private loadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () =>  this.contractService.getContractGroup(this.contractGroupId).then((x) => {
            this.isNew = false;
            this.contractGroup = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["billing.contract.contractgroups.contractgroup"] + ' ' + this.contractGroup.name);
            })
        ]);
    }

    private newContractGroup() {
        this.contractGroup = new ContractGroupDTO();
        this.contractGroup.interval = 1;
        this.isNew = true;
    }

    //protected copy() {
    //    super.copy();
    //    this.contractGroupId = 0;
    //    this.contractGroup.contractGroupId = 0;
    //    this.isNew = true;
    //    this.dirtyHandler.isDirty = true;
    //}
}