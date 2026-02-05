import { BudgetHeadDTO, BudgetRowDTO } from "../../../../Common/Models/BudgetDTOs";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { AngularFeatureCheckService, IAngularFeatureCheckService } from "../../../../Core/Services/AngularFeatureCheckService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CompanySettingType, Feature, ProjectCentralBudgetRowType, SoeEntityState, TermGroup_ExpenseType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { Guid } from "../../../../Util/StringUtility";
import { AddProjectBudgetRowController } from "../Dialogs/AddProjectBudgetRow/AddProjectBudgetRowController";

export class ProjectBudgetDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Projects/Directives/ProjectBudget.html'),
            scope: {
                budgetHead: '=',
                readOnly: '=?',
                parentGuid: '='
            },
            restrict: 'E',
            replace: true,
            controller: ProjectBudgetDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

enum SumType {
    Amount = "totalAmount",
    Hours = "totalQuantity",
}

export class BudgetRowEx extends BudgetRowDTO {
    name: string;
    budget: number;
    hours: number;
    totalHours: number;
    ib: any;
    ibHours: number;
    ibType: ProjectCentralBudgetRowType;
    ibQuantityType: ProjectCentralBudgetRowType;
    timeCodeId: number;
    hidden: boolean;
    icon: string;
    expander: " " | undefined;
    showIbAmount: boolean;
    showIbHours: boolean;
    state: SoeEntityState;
}


class ProjectBudgetDirectiveController extends GridControllerBase2Ag implements ICompositionGridController {
    parentGuid: Guid;
    readOnly: boolean;
    budgetHead: any;
    timeCodes: any[];
    allowedTypes = [
        ProjectCentralBudgetRowType.BillableMinutesInvoiced,
        ProjectCentralBudgetRowType.IncomePersonellTotal,
        ProjectCentralBudgetRowType.IncomeMaterialTotal,
        ProjectCentralBudgetRowType.IncomeTotal,
        ProjectCentralBudgetRowType.CostPersonell,
        ProjectCentralBudgetRowType.CostMaterial,
        ProjectCentralBudgetRowType.CostExpense,
        ProjectCentralBudgetRowType.OverheadCostPerHour,
        ProjectCentralBudgetRowType.OverheadCost,
        ProjectCentralBudgetRowType.CostTotal,
        ProjectCentralBudgetRowType.BillableMinutesInvoicedIB,
        ProjectCentralBudgetRowType.IncomePersonellTotalIB,
        ProjectCentralBudgetRowType.IncomeMaterialTotalIB,
        //ProjectCentralBudgetRowType.IncomeTotalIB,
        ProjectCentralBudgetRowType.CostPersonellIB,
        ProjectCentralBudgetRowType.CostMaterialIB,
        ProjectCentralBudgetRowType.CostExpenseIB,
        ProjectCentralBudgetRowType.OverheadCostPerHourIB,
        ProjectCentralBudgetRowType.OverheadCostIB,
        ProjectCentralBudgetRowType.CostTotalIB,
    ]


    terms: { [index: string]: string; };

    // Flags
    private overheadCostAsFixedAmount: boolean;
    private overheadCostAsAmountPerHour: boolean;


    get totalHoursIB(): number {
        const row = this.getRow(ProjectCentralBudgetRowType.BillableMinutesInvoicedIB);
        if (row && row.totalAmount)
            return row.totalAmount / 60
        else
            return 0
    }

    get totalHoursBudget(): number {
        const row = this.getRow(ProjectCentralBudgetRowType.BillableMinutesInvoiced);
        if (row && row.totalAmount)
            return row.totalAmount / 60
        else
            return 0
    }

    get incomePersonell(): number {
        return this.getRow(ProjectCentralBudgetRowType.IncomePersonellTotal)?.totalAmount || 0;
    }

    get incomeMaterial(): number {
        return this.getRow(ProjectCentralBudgetRowType.IncomeMaterialTotal)?.totalAmount || 0;
    }

    get incomeTotal(): number {
        return this.incomeMaterial + this.incomePersonell;
    }

    get costPersonell(): number {
        return this.getRow(ProjectCentralBudgetRowType.CostPersonell)?.budget || 0;
    }

    get totalHours(): number {
        return this.getSum(ProjectCentralBudgetRowType.CostPersonell, SumType.Hours);
    }

    get costMaterial(): number {
        return this.getRow(ProjectCentralBudgetRowType.CostMaterial)?.budget || 0;
    }

    get costExpense(): number {
        return this.getRow(ProjectCentralBudgetRowType.CostExpense)?.totalAmount || 0;
    }

    get costOverhead(): number {
        return this.getRow(ProjectCentralBudgetRowType.OverheadCost)?.totalAmount || 0;
    }

    get costTotal(): number {
        return this.costPersonell + this.costMaterial + this.costExpense + this.costOverhead;
    }

    get result(): number {
        return this.incomeTotal - this.costTotal;
    }

    get resultRatio(): string {
        const inc = this.incomeTotal;
        if (inc === 0)
            return "100 %";
        return ((this.result / this.incomeTotal) * 100).round(2) + " %";
    }

    get hasRows(): boolean {
        return this.budgetHead && this.rows && this.rows.length > 0;
    }

    get rows(): BudgetRowEx[] {
        if (!this.budgetHead) {
            this.createHead();
        }
        if (!this.budgetHead.rows) {
            this.budgetHead.rows = [];
        }
        return this.budgetHead?.rows;
    }


    getRow(type: ProjectCentralBudgetRowType): BudgetRowEx {
        return this.rows.find(r => r.type === type);
    }

    getBaseRow(type: ProjectCentralBudgetRowType) {
        return this.rows.find(r => r.type === type && !r.timeCodeId)
    }

    getSum(type: ProjectCentralBudgetRowType, sumType: SumType) {
        let sum = this.rows
            .filter(r => r.type === type && !r.isDeleted)
            .reduce((tot, r) => tot += r[sumType], 0) || 0;
        if (sumType === SumType.Hours)
            sum = sum / 60;
        return sum;
    }

    showBudget: boolean = true;
    budgetInfoText: string = "";

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private angularFeatureCheckService: AngularFeatureCheckService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Billing.Project.Budget", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.setData());

        this.showBudget = !this.angularFeatureCheckService.shouldUseAngularSpa(Feature.Billing_Project_Edit_Budget);

        if (!this.showBudget) {
            this.budgetInfoText = translationService.translateInstant("billing.projects.list.budgetmigratedmsg");
        }

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.flowHandler.start({ feature: Feature.Billing_Project_Edit_Budget, loadReadPermissions: true, loadModifyPermissions: true });
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.budgetHead, (oldValue, newValue) => {
            if (oldValue !== newValue) {
                this.setData();
            }
        })
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Project_Edit_Budget].readPermission;
        this.modifyPermission = response[Feature.Billing_Project_Edit_Budget].modifyPermission;

        if (this.modifyPermission) {
            // Send messages to TabsController
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadTimecodes(),
        ]);
    }

    private loadTimecodes(): ng.IPromise<any> {
        return this.coreService.getAdditionDeductionTimeCodes(true).then(x => {
            this.timeCodes = x.filter(t => t.expenseType !== TermGroup_ExpenseType.Time && t.expenseType !== TermGroup_ExpenseType.Expense);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.ProjectOverheadCostAsFixedAmount);
        settingTypes.push(CompanySettingType.ProjectOverheadCostAsAmountPerHour);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.overheadCostAsFixedAmount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectOverheadCostAsFixedAmount);
            this.overheadCostAsAmountPerHour = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectOverheadCostAsAmountPerHour);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const translationKeys: string[] = [
            "billing.projects.list.budgethours",
            "billing.projects.list.budgethourstotal",
            "billing.projects.list.budgetincomepersonel",
            "billing.projects.list.budgetincomematerial",
            "billing.projects.list.budgetincometotal",
            "billing.projects.list.budgetcostpersonal",
            "billing.projects.list.budgetcostmaterial",
            "billing.projects.list.budgetcost",
            "billing.projects.list.budgetoverheadperhour",
            "billing.projects.list.budgetoverhead",
            "billing.projects.list.budgetcosttotal"
        ];
        return this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;
        })
    }

    public setupGrid(): void {
        const translationKeys: string[] = [
            "common.type",
            "common.modified",
            "common.modifiedby",
            "billing.projects.list.budgetheader",
            "billing.projects.list.budgethours",
            "billing.projects.list.ib",
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.gridAg.options.setAutoHeight(true);
            this.gridAg.options.enableRowSelection = false;

            this.gridAg.enableMasterDetail(true, null, null, true);
            this.gridAg.options.setDetailCellDataCallback(params => {
                this.getDetailData(params)
            });

            this.gridAg.detailOptions.enableRowSelection = false;
            this.gridAg.detailOptions.enableFiltering = false;
            this.gridAg.detailOptions.addColumnText("name", terms["common.type"], null);
            this.gridAg.detailOptions.addColumnText("modified", terms["common.modified"], null);
            this.gridAg.detailOptions.addColumnText("modifiedBy", terms["common.modifiedby"], null);
            this.gridAg.detailOptions.addColumnNumber("totalAmount", terms["billing.projects.list.budgetheader"], null);
            this.gridAg.detailOptions.addColumnNumber("ib", terms["billing.projects.list.ib"], null);
            this.gridAg.detailOptions.addColumnNumber("hours", terms["billing.projects.list.budgethours"], null);
            this.gridAg.detailOptions.addColumnEdit(terms["core.edit"], (row) => this.editRow(row, true));
            this.gridAg.detailOptions.finalizeInitGrid();

            this.gridAg.addColumnText("name", terms["common.type"], null);
            this.gridAg.addColumnText("modified", terms["common.modified"], null);
            this.gridAg.addColumnText("modifiedBy", terms["common.modifiedby"], null);
            this.gridAg.addColumnNumber("budget", terms["billing.projects.list.budgetheader"], null);
            this.gridAg.addColumnNumber("ib", terms["billing.projects.list.ib"], null);
            this.gridAg.addColumnNumber("totalHours", terms["billing.projects.list.budgethours"], null);
            this.gridAg.addColumnIcon("icon", null, null, { onClick: (row) => this.editRow(row, false), pinned: "right" })

            let events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.editRow(row, false); }));
            this.gridAg.options.subscribe(events);
            this.gridAg.finalizeInitGrid("Billing.Project.Budget", false);

        });
    }

    private getDetailData(params) {
        const row = params.data;
        const rows = this.rows.filter(r => r.type === row.type && !r.isDeleted);
        params.successCallback(rows);
    }

    public editRow(row, isDetail) {
        if (!row)
            return

        let showIbAmount = true;
        let showIbQuantity = false;
        let allowTimeCode = false;
        let isNewRow = false;

        if (row.type === ProjectCentralBudgetRowType.CostPersonell && !row.timeCodeId && isDetail) {
            showIbQuantity = true;
        }

        if (row.type === ProjectCentralBudgetRowType.OverheadCostPerHour) {
            //showIbAmount = false;
        }

        if ([ProjectCentralBudgetRowType.CostMaterial, ProjectCentralBudgetRowType.CostPersonell].find(x => x === row.type)) {
            if ((row.budget || row.budget == 0) && !isDetail) {
                allowTimeCode = true
                isNewRow = true;
                showIbAmount = false;
            }
            else if (row.timeCodeId && row.timeCodeId > 0) {
                showIbAmount = false;
                allowTimeCode = true
            }

        }

        this.showBudgetRowDialog(row, allowTimeCode, isNewRow, showIbAmount, showIbQuantity);
    }

    public setData() {
        this.prepareData();
        const data = this.rows.filter(r => !r.hidden && r.state != SoeEntityState.Deleted)
        this.gridAg.setData(data);
    }

    private setRowProps(row: BudgetRowEx) {
        //timecode budget rows are handled separately
        if (row.isDeleted)
            return;

        if (row.timeCodeId && row.timeCodeId > 0) {
            row.ib = row.ib || 0
            row.hidden = true;
            if (row.type == ProjectCentralBudgetRowType.CostPersonell) {
                row.hours = row.totalQuantity / 60 || 0;
            }
            return;
        }

        let ibRow;
        let ibQuantityRow;
        row.icon = "fal fa-pen iconEdit";
        row.budget = 0;
        switch (row.type) {
            case ProjectCentralBudgetRowType.BillableMinutesInvoiced:
                row.name = this.terms["billing.projects.list.budgethours"];
                row.budget = row.totalAmount / 60;
                ibRow = this.getRow(ProjectCentralBudgetRowType.BillableMinutesInvoicedIB);
                row.ib = ibRow.totalAmount / 60;
                row.hidden = true;
                break;
            case ProjectCentralBudgetRowType.IncomePersonellTotal:
                row.name = this.terms["billing.projects.list.budgetincomepersonel"];
                ibRow = this.getRow(ProjectCentralBudgetRowType.IncomePersonellTotalIB);
                break;
            case ProjectCentralBudgetRowType.IncomeMaterialTotal:
                row.name = this.terms["billing.projects.list.budgetincomematerial"];
                ibRow = this.getRow(ProjectCentralBudgetRowType.IncomeMaterialTotalIB);
                break;
            case ProjectCentralBudgetRowType.CostPersonell:
                row.expander = " ";
                row.name = this.terms["billing.projects.list.budgetcostpersonal"];
                row.icon = "fal fa-plus iconEdit";
                row.totalQuantity = this.getRow(ProjectCentralBudgetRowType.BillableMinutesInvoiced).totalAmount || 0;
                row.budget = this.getSum(ProjectCentralBudgetRowType.CostPersonell, SumType.Amount);
                row.totalHours = this.getSum(ProjectCentralBudgetRowType.CostPersonell, SumType.Hours);
                row.hours = row.totalQuantity / 60 || 0;

                ibRow = this.getRow(ProjectCentralBudgetRowType.CostPersonellIB);
                row.ib = ibRow.totalAmount;

                ibQuantityRow = this.getRow(ProjectCentralBudgetRowType.BillableMinutesInvoicedIB);
                row.ibHours = (ibQuantityRow.totalAmount || 0) / 60;
                break;
            case ProjectCentralBudgetRowType.CostMaterial:
                row.expander = " ";
                row.name = this.terms["billing.projects.list.budgetcostmaterial"];
                row.icon = "fal fa-plus iconEdit";
                row.budget = this.getSum(ProjectCentralBudgetRowType.CostMaterial, SumType.Amount);

                ibRow = this.getRow(ProjectCentralBudgetRowType.CostMaterialIB);
                row.ib = ibRow.totalAmount;
                break;
            case ProjectCentralBudgetRowType.CostExpense:
                row.name = this.terms["billing.projects.list.budgetcost"];
                ibRow = this.getRow(ProjectCentralBudgetRowType.CostExpenseIB);
                break;
            case ProjectCentralBudgetRowType.OverheadCostPerHour:
                if (this.overheadCostAsFixedAmount) {
                    const overheadBudgetRow = this.getRow(ProjectCentralBudgetRowType.OverheadCost);
                    const overheadIBRow = this.getRow(ProjectCentralBudgetRowType.OverheadCostIB);
                    const overheadPerHourIBRow = this.getRow(ProjectCentralBudgetRowType.OverheadCostPerHourIB);
                    const sumHours = this.getSum(ProjectCentralBudgetRowType.CostPersonell, SumType.Hours);
                    row.hidden = true;
                    row.totalAmount = overheadBudgetRow && overheadBudgetRow.totalAmount && sumHours != 0 ? overheadBudgetRow.totalAmount / sumHours : 0;
                    overheadPerHourIBRow.totalAmount = overheadIBRow && overheadIBRow.totalAmount != undefined && this.totalHoursIB != 0 ? overheadIBRow.totalAmount / this.totalHoursIB : 0;
                    row.ib = 0;
                    row.budget = 0;
                }
                row.showIbAmount = false;
                row.name = this.terms["billing.projects.list.budgetoverheadperhour"];
                ibRow = this.getRow(ProjectCentralBudgetRowType.OverheadCostPerHourIB);
                break;
            case ProjectCentralBudgetRowType.OverheadCost:
                if (this.overheadCostAsAmountPerHour) {
                    //overHeadCost is set per hour
                    const overheadPerHourBudgetRow = this.getRow(ProjectCentralBudgetRowType.OverheadCostPerHour);
                    const overheadPerHourIBRow = this.getRow(ProjectCentralBudgetRowType.OverheadCostPerHourIB);
                    const overheadIBRow = this.getRow(ProjectCentralBudgetRowType.OverheadCostIB);
                    const sumHours = this.getSum(ProjectCentralBudgetRowType.CostPersonell, SumType.Hours);
                    row.hidden = true;
                    row.totalAmount = overheadPerHourBudgetRow && overheadPerHourBudgetRow.totalAmount != undefined ? overheadPerHourBudgetRow.totalAmount * sumHours : 0;
                    overheadIBRow.totalAmount = overheadPerHourIBRow && overheadPerHourIBRow.totalAmount != undefined ? overheadPerHourIBRow.totalAmount * this.totalHoursIB : 0;
                    row.ib = 0;
                    row.budget = 0;
                }
                row.name = this.terms["billing.projects.list.budgetoverhead"];
                ibRow = this.getRow(ProjectCentralBudgetRowType.OverheadCostIB);
                break;
            case ProjectCentralBudgetRowType.IncomeTotal:
                row.hidden = true;
                row.name = this.terms["billing.projects.list.budgethours"];
                ibRow = this.getRow(ProjectCentralBudgetRowType.IncomeTotalIB);
                break;
            case ProjectCentralBudgetRowType.CostTotal:
                row.hidden = true;
                row.name = this.terms["billing.projects.list.budgethours"];
                ibRow = this.getRow(ProjectCentralBudgetRowType.CostTotalIB);
                break;
            default:
                row.hidden = true;
        }

        if (!row.budget) {
            row.budget = row.totalAmount || 0;
        }

        if (ibRow) {
            row.ibType = ibRow?.type;
            if (!row.ib) {
                row.ib = ibRow?.totalAmount || 0;
            }
        }
        if (ibQuantityRow) {
            if (!row.ibHours) {
                row.ibHours = (ibQuantityRow?.totalAmount || 0) / 60;
            }
            row.ibQuantityType = ibQuantityRow.type;
        }
    }

    private showBudgetRowDialog(budgetRow, allowTimeCode?, isNewRow?, showIbAmount?, showIbQuantity?) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Projects/Dialogs/AddProjectBudgetRow/AddProjectBudgetRow.html"),
            controller: AddProjectBudgetRowController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                budgetRow: () => budgetRow || null,
                budgetRows: () => this.rows,
                allowTimeCode: () => allowTimeCode || false,
                isNewRow: () => isNewRow || false,
                showIbAmount: () => showIbAmount || false,
                showIbQuantity: () => showIbQuantity || false,
            }
        }

        this.$uibModal.open(options).result.then(x => {
            this.budgetHead.rows = x.filter(r => {
                if (!(r.isDeleted && !r.budgetRowId)) {
                    return r
                }
            })
            this.setData();
            this.setAsDirty();
        })
    }

    // #region Help methods
    private addRow(row) {
        if (!this.budgetHead) {
            this.createHead()
        }
        this.budgetHead.rows.push(row);
    }

    private createHead() {
        this.budgetHead = new BudgetHeadDTO();
        this.budgetHead.actorCompanyId = CoreUtility.actorCompanyId;
        this.budgetHead.rows = [];
        this.budgetHead.name = "Proj";
    }

    private createMissingRows() {
        this.allowedTypes.forEach(t => {
            if (!this.getBaseRow(t))
                this.addRow(this.createBudgetRow(t))
        })
    }

    private createBudgetRow(rowType: ProjectCentralBudgetRowType) {
        var row = new BudgetRowDTO();
        row.type = rowType;
        row.totalAmount = 0;
        row.totalQuantity = 0;
        return row;
    }


    private prepareData() {
        this.createMissingRows();
        this.rows.forEach(row => {
            this.setRowProps(row);
        })
    }
    // #endregion

    private setAsDirty(isDirty: boolean = true) {
        this.messagingService.publish(
            Constants.EVENT_SET_DIRTY,
            {
                guid: this.parentGuid,
                dirty: isDirty
            }
        );
    }
}