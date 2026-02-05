import { Feature, TermGroup, SettingMainType, SoeAccountDistributionType } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";



export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private accountDistributions: any[];

    private isPeriodAccountDistribution = false;
    private isAutomaticAccountDistribution = false;

    private triggerTypes: any[];
    private calculationTypes: any[];
    private accountDimForCostplace: string;
    private addDim1Column = false;
    private accountDim1Name = "";
    private addDim2Column = false;
    private accountDim2Name = "";
    private addDim3Column = false;
    private accountDim3Name = "";
    private addDim4Column = false;
    private accountDim4Name = "";
    private addDim5Column = false;
    private accountDim5Name = "";
    private addDim6Column = false;
    private accountDim6Name = "";
    private permission: number;

    private currentDate = new Date().toFormattedDate();
    protected setupComplete: boolean;
    allItemsSelectionSettingType = 0;
    hideAllItemsSelection = false;
    private _loadOpen = false;
    get loadOpen() {
        return this._loadOpen;
    }
    set loadOpen(item: boolean) {
        this._loadOpen = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _loadClosed: boolean = false;
    get loadClosed() {
        return this._loadClosed;
    }
    set loadClosed(item: boolean) {
        this._loadClosed = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    toolbarInclude: any;

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete)
            this.updateItemsSelection();
    }

    get showSearchButton() {
        return this.allItemsSelection === 999 && !this.hideAllItemsSelection;
    }

    private activated = false;

    //@ngInject
    constructor(private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Accounting.AccountDistribution", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('accountDistributionHeadId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify
                this.loadOpen = true;
                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadCalculationTypes())
            .onBeforeSetUpGrid(() => this.loadTriggerTypes())
            .onBeforeSetUpGrid(() => this.getDimLabels())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(true))

        this.onTabActivated(() => this.tabActivated());

        if (soeConfig.accountDistributionType == "Period" || soeConfig.accountDistributionType === SoeAccountDistributionType.Period) {
            this.isPeriodAccountDistribution = true;
            this.permission = Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod;
        }
        if (soeConfig.accountDistributionType == "Auto" || soeConfig.accountDistributionType === SoeAccountDistributionType.Auto) {
            this.isAutomaticAccountDistribution = true;
            this.permission = Feature.Economy_Preferences_VoucherSettings_AccountDistributionAuto;
        }

        this.setupComplete = false;
    }

    private tabActivated() {
        if (!this.activated) {
            this.flowHandler.start({ feature: this.permission, loadReadPermissions: true, loadModifyPermissions: true });
            this.activated = true;
        }
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired( () => { this.reloadData(); });
        }

        this.toolbarInclude = this.urlHelperService.getGlobalUrl("Economy/Accounting/AccountDistribution/Views/gridHeader.html");
    }   

    private loadTriggerTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountDistributionTriggerType, false, false).then(x => {
            this.triggerTypes = [];
            _.forEach(x, (row) => {
                this.triggerTypes.push({ id: row.name, value: row.name, typeId: row.id });
            });             
        });
    }

    private loadCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountDistributionCalculationType, false, false).then(x => {
            this.calculationTypes = [];
            _.forEach(x, (row) => {
                this.calculationTypes.push({ id: row.name, value: row.name, typeId: row.id });                
            });       
        });
    }

    private getDimLabels(): ng.IPromise<any> {
        return this.accountingService.getAccountDims(false, false, false, false, false).then((items: any[]) => {            
            const dim1 = items.find(x =>  x.accountDimNr === 1);
            if (dim1) {
                this.addDim1Column = true;
                this.accountDim1Name = dim1.name;
            }

            const dim2 = items.find(x =>  x.accountDimNr === 2);
            if (dim2) {
                this.addDim2Column = true;
                this.accountDim2Name = dim2.name;
            }

            const dim3 = items.find(x =>  x.accountDimNr === 3);
            if (dim3) {
                this.addDim3Column = true;
                this.accountDim3Name = dim3.name;
            }

            const dim4 = items.find(x =>  x.accountDimNr === 4);
            if (dim4) {
                this.addDim4Column = true;
                this.accountDim4Name = dim4.name;
            }

            const dim5 = items.find(x =>  x.accountDimNr === 5);
            if (dim5) {
                this.addDim5Column = true;
                this.accountDim5Name = dim5.name;
            }

            const dim6 = items.find(x =>  x.accountDimNr === 6);
            if (dim6) {
                this.addDim6Column = true;
                this.accountDim6Name = dim6.name
            }       
        });
    }

    //public edit(row) {        
    //    this.messagingHandler.publishEditRow(row);
    //}

    public setupGrid() {
        const keys: string[] = [
            "common.name",
            "common.description",
            "economy.accounting.accountdistribution.sorting",
            "economy.accounting.accountdistribution.type",
            "economy.accounting.accountdistribution.dayinperiod",
            "economy.accounting.accountdistribution.numberoftimes",
            "economy.accounting.accountdistribution.startdate",
            "economy.accounting.accountdistribution.enddate",
            "economy.accounting.account",
            "economy.accounting.accountdistribution.calculationtype",
            "economy.accounting.accountdistribution.totalcount",
            "economy.accounting.accountdistribution.totalamount",
            "economy.accounting.accountdistribution.saldo",
            "economy.accounting.accountdistribution.transferredcount",
            "economy.accounting.accountdistribution.lasttransferdate",
            "economy.accounting.accountdistribution.periodamount",
            "economy.accounting.accountdistribution.remainingamount",
            "economy.accounting.accountdistribution.remainingcount",
            "economy.accounting.accountdistribution.voucherregister",
            "economy.accounting.accountdistribution.import",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => { 
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnDate("startDate", terms["economy.accounting.accountdistribution.startdate"], null, true);
            this.gridAg.addColumnDate("endDate", terms["economy.accounting.accountdistribution.enddate"], null, true);
            if (this.isPeriodAccountDistribution) {
                this.gridAg.addColumnSelect("triggerTypeName", terms["economy.accounting.accountdistribution.type"], null, { displayField: "triggerTypeName", selectOptions: this.triggerTypes, enableHiding: true });
                this.gridAg.addColumnNumber("dayNumber", terms["economy.accounting.accountdistribution.dayinperiod"], null, { enableHiding: true });            
                this.gridAg.addColumnNumber("periodValue", terms["economy.accounting.accountdistribution.numberoftimes"], null, { enableHiding: true });
            }
            this.gridAg.addColumnSelect("calculationTypeName", terms["economy.accounting.accountdistribution.calculationtype"], null, { displayField: "calculationTypeName", selectOptions: this.calculationTypes, enableHiding: true });
            if(this.addDim1Column)
                this.gridAg.addColumnText("dim1Expression", this.accountDim1Name, null, true);
            if (this.addDim2Column)
                this.gridAg.addColumnText("dim2Expression", this.accountDim2Name, null, true);
            if (this.addDim3Column)
                this.gridAg.addColumnText("dim3Expression", this.accountDim3Name, null, true);
            if (this.addDim4Column)
                this.gridAg.addColumnText("dim4Expression", this.accountDim4Name, null, true);
            if (this.addDim5Column)
                this.gridAg.addColumnText("dim5Expression", this.accountDim5Name, null, true);
            if (this.addDim6Column)
                this.gridAg.addColumnText("dim6Expression", this.accountDim6Name, null, true);            

            if (this.isPeriodAccountDistribution) {
                this.gridAg.addColumnNumber("entryTotalCount", terms["economy.accounting.accountdistribution.totalcount"], null, { enableHiding: true });
                this.gridAg.addColumnNumber("entryTotalAmount", terms["economy.accounting.accountdistribution.totalamount"], null, { enableHiding: true });
                this.gridAg.addColumnNumber("entryTransferredCount", terms["economy.accounting.accountdistribution.transferredcount"], null, { enableHiding: true });
                this.gridAg.addColumnNumber("entryTransferredAmount", terms["economy.accounting.accountdistribution.saldo"], null, { enableHiding: true });
                this.gridAg.addColumnDate("entryLatestTransferDate", terms["economy.accounting.accountdistribution.lasttransferdate"], null, true);
                this.gridAg.addColumnNumber("entryPeriodAmount", terms["economy.accounting.accountdistribution.periodamount"], null, { enableHiding: true });
                this.gridAg.addColumnNumber("entryRemainingCount", terms["economy.accounting.accountdistribution.remainingcount"], null, { enableHiding: true });
                this.gridAg.addColumnNumber("entryRemainingAmount", terms["economy.accounting.accountdistribution.remainingamount"], null, { enableHiding: true });

                //Set up summarizing
                this.gridAg.options.addFooterRow("#sum-footer-grid", {
                    "entryTotalAmount": "sum",
                    "entryTransferredAmount": "sum",
                    "entryPeriodAmount": "sum",
                    "entryRemainingAmount": "sum",
                } as IColumnAggregations);
            }
            else if (this.isAutomaticAccountDistribution) {
                this.gridAg.addColumnBool("useInVoucher", terms["economy.accounting.accountdistribution.voucherregister"], null);
                this.gridAg.addColumnBool("useInImport", terms["economy.accounting.accountdistribution.import"], null);
            }

            if (this.readPermission || this.modifyPermission)
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.accounting.accountdistribution.accountdistributions", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
        this.toolbar.addInclude(this.toolbarInclude);
    }   

    private loadGridData(useCache: boolean = true) {
        if (this.isPeriodAccountDistribution) {
            this.progress.startLoadingProgress([() => {
                return this.accountingService.getAccountDistributionHeads(this.loadOpen, this.loadClosed, true).then((x: any[]) => {
                    x.forEach( (row) => {
                        const triggerType = _.find(this.triggerTypes, { typeId: row.triggerType });
                        row.triggerTypeName = triggerType ? triggerType.value : "";

                        const calculationType = _.find(this.calculationTypes, { typeId: row.calculationType });
                        row.calculationTypeName = calculationType ? calculationType.value : "";

                        row.entryRemainingCount = row.entryTotalCount - row.entryTransferredCount;
                        row.entryRemainingAmount = row.entryTotalAmount - row.entryTransferredAmount;
                        if (row.startDate) {
                            row.startDate = new Date(<any>row.startDate).date();
                        }
                        if (row.endDate) {
                            row.endDate = new Date(<any>row.endDate).date();
                        }
                    });  
                    this.accountDistributions = x;
                });
            }]).then(() => { 
                this.setData(this.accountDistributions);
            });
        }        


        if (this.isAutomaticAccountDistribution) {
            this.progress.startLoadingProgress([() => {
                return this.accountingService.getAccountDistributionHeadsAuto(useCache).then((x) => {
                    _.forEach(x, (row) => {
                        const calculationType = _.find(this.calculationTypes, { typeId: row.calculationType });
                        row.calculationTypeName = calculationType ? calculationType.value : "";
                    });
                    this.accountDistributions = x;        
                });
            }]).then(() => {
                this.setData(this.accountDistributions);
            });
        }

        this.setupComplete = true;
    }

    private reloadData() {
        this.loadGridData(false);
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, this.allItemsSelectionSettingType, this.allItemsSelection)
        this.reloadGridFromFilter();
    }
}

