import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IPayrollService } from "../PayrollService";
import { IconLibrary } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature, TermGroup, TermGroup_SoePayrollPriceType } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../Util/Constants";
import { IUiGridConstants } from "../../../../custom_typings/ui-grid";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    payrollPriceType: any;
    payrollPriceTypeId: any;
    terms: any = [];

    // Lookups
    types: any;

    // Subgrid
    protected periodGridOptions: ISoeGridOptions;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();
    isDirty: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private payrollService: IPayrollService,
        private uiGridConstants: IUiGridConstants,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookUp())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.initRowsGrid();
    }

    // SETUP
    public onInit(parameters: any) {
        this.payrollPriceTypeId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_SalarySettings_PriceType_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.payrollPriceTypeId, recordId => {
            if (recordId !== this.payrollPriceTypeId) {
                this.payrollPriceTypeId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_SalarySettings_PriceType_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_SalarySettings_PriceType_Edit].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    // LOOKUPS
    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollPriceTypes, false, false).then((x) => {
            this.types = x;
        });
    }
    private onDoLookUp(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.setupRowsGrid(), //must be called after permissions in base class is done      
            this.setupToolBar(),            
            this.loadTypes()
        ]);
    }

    private onLoadData() {
        if (this.payrollPriceTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        }
        else {
            this.new();
        }
    }

    private setupToolBar() {
        if (this.modifyPermission) {
            this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addRow();
            })));
        }
    }

    private initRowsGrid() {
        this.periodGridOptions = new SoeGridOptions("Time.Payroll.PayrollPriceTypes", this.$timeout, this.uiGridConstants);
        this.periodGridOptions.enableGridMenu = false;
        this.periodGridOptions.showGridFooter = false;
        this.periodGridOptions.setMinRowsToShow(5);
    }

    private setupRowsGrid() {
        var keys: string[] = [
            "common.fromdate",
            "common.amount",
            "core.delete",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.periodGridOptions.addColumnDate("fromDate", terms["common.fromdate"], null);
            this.periodGridOptions.addColumnNumber("amount", terms["common.amount"], null, null, 3);

            _.forEach(this.periodGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableCellEdit = true;
            });

            if (this.modifyPermission)
                this.periodGridOptions.addColumnDelete(terms["core.delete"], "deleteRow");
        });
    }

    // LOOKUPS

    private load(): ng.IPromise<any> {

        if (this.payrollPriceTypeId > 0) {
            return this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollPriceType(this.payrollPriceTypeId, true).then((x) => {
                this.isNew = false;
                this.payrollPriceType = x;
                _.forEach(this.payrollPriceType.periods, (period: any) => {
                    period.fromDate = new Date(period.fromDate).date();
                });
                this.resetPriceTypePeriods(null);
                    this.dirtyHandler.clean();
                    this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.payroll.payrollpricetype.payrollpricetype"] + ' ' + this.payrollPriceType.name);
            });
            }]);
        }
        else {
            this.new();

        }
    }

    private loadTerms() {
        var keys: string[] = [
            "time.payroll.payrollpricetype.payrollpricetype"
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // ACTIONS

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.payrollService.savePayrollPriceType(this.payrollPriceType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.payrollPriceTypeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.payrollPriceType.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.payrollPriceTypeId = result.integerValue;
                        this.payrollPriceType.payrollPriceTypeId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.payrollPriceType);
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
                this.load();
            });
    }
    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.payrollService.getPayrollPriceTypesGrid(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.payrollPriceTypeId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.payrollPriceTypeId) {
                    this.payrollPriceTypeId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }


    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.payrollService.deletePayrollPriceType(this.payrollPriceTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.payrollPriceType, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    // HELP-METHODS

    protected copy() {
        super.copy();
        this.isNew = true;
        this.payrollPriceTypeId = 0;
        this.payrollPriceType.payrollPriceTypeId = 0;
    }

    private new() {
        this.isNew = true;
        this.payrollPriceTypeId = 0;
        this.payrollPriceType = {
            type: TermGroup_SoePayrollPriceType.Misc,
            periods: []
        };
    }

    private addRow() {
        var row = {
            amount: 0,
            fromDate: CalendarUtility.getDateNow(),
            payrollPriceTypeId: this.payrollPriceType.payrollPriceTypeId,
            payrollPriceTypePeriodId: 0,
        };
        this.payrollPriceType.periods.push(row);
        this.resetPriceTypePeriods(row);
    }

    protected deleteRow(row) {
        this.dirtyHandler.isDirty = true;
        this.periodGridOptions.deleteRow(row);
        this.resetPriceTypePeriods(null);
    }

    private resetPriceTypePeriods(rowItem: any) {
        if (this.payrollPriceType.periods) {
            this.periodGridOptions.setData(this.payrollPriceType.periods);
        }
        this.periodGridOptions.scrollToFocus(rowItem, 1);
    }

    // VALIDATION

    protected validate() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.payrollPriceType) {
                if (!this.payrollPriceType.type) {
                    mandatoryFieldKeys.push("common.type");
                }
                if (!this.payrollPriceType.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.payrollPriceType.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}