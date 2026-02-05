import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { SoeGridOptionsEvent, IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature, TermGroup_AccountStatus, TermGroup, CompanySettingType } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { EditController } from "./EditController";
import { Constants } from "../../../Util/Constants";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { AccountYearDTO } from "../../../Common/Models/AccountYear";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //Items
    accountYears: any = [];
    accountStatuses: any = [];
    budgetSubTypes: any = [];

    // Settings
    maxNoOfYearsOpen = 0;

    // Values
    latestTo: Date;

    // Flags
    createNewYear = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Accounting.AccountYear", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onDoLookUp(() => this.doLookup())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (soeConfig.createNewAccountYear && soeConfig.createNewAccountYear === true) {
            this.createNewYear = true;
            soeConfig.createNewAccountYear = undefined;
        }

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Accounting_AccountPeriods, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.accounting.accountyear.createaccountyear", "economy.accounting.accountyear.createaccountyear", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addAccountYear() },
            () => { return !this.modifyPermission })));
    }

    private doLookup() {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadAccountStatuses(),
            this.loadBudgetSubTypes()
        ]).then(() => {
            this.setUpGrid();
            this.loadGridData();
        });
    }

    private loadAccountStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountStatus, true, false).then(x => {
            this.accountStatuses = x;
        });
    }

    // Used for month name
    private loadBudgetSubTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountingBudgetSubType, false, false, true).then(x => {
            this.budgetSubTypes = x;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountingMaxYearOpen);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.maxNoOfYearsOpen = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingMaxYearOpen);
        });
    }

    private setUpGrid() {

        var translationKeys: string[] = [
            "economy.accounting.accountyear",
            "common.status",
            "common.number",
            "common.period",
            "core.time.month"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.enableMasterDetail(true, 450);

            this.gridAg.options.setDetailCellDataCallback((params) => {
                params.successCallback(params.data['periods']);

                this.$scope.$applyAsync(() => {
                    this.gridAg.detailOptions.enableRowSelection = false;
                    this.gridAg.detailOptions.sizeColumnToFit();
                });
            });

            this.gridAg.detailOptions.setMinRowsToShow(15);

            this.gridAg.detailOptions.addColumnNumber("periodNr", terms["common.number"], 40, { enableHiding: true, pinned: "left" });
            this.gridAg.detailOptions.addColumnText("periodName", terms["common.period"], null, { enableHiding: true });
            this.gridAg.detailOptions.addColumnText("monthName", terms["core.time.month"], null, { enableHiding: true });
            this.gridAg.detailOptions.addColumnText("statusName", terms["common.status"], null, { enableHiding: true });
            this.gridAg.detailOptions.addColumnIcon("statusIcon", null, 30, { enableHiding: true, toolTipField: "statusName", showTooltipFieldInFilter: true, pinned: 'right' });
            this.gridAg.detailOptions.finalizeInitGrid();

            this.gridAg.addColumnText("yearFromTo", terms["economy.accounting.accountyear"], null, true);
            this.gridAg.addColumnText("statusText", terms["common.status"], null, true);
            this.gridAg.addColumnIcon("statusIcon", null, 30, { enableHiding: true, toolTipField: "statusName", showTooltipFieldInFilter: true, pinned: 'right' });
            this.gridAg.addColumnEdit(terms["core.edit"], this.editAccountYear.bind(this));

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.editAccountYear(row); }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("economy.accounting.accountyear", true);
        });
    }

    edit(row) {
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private loadGridData(forceRefresh: boolean = false) {
        this.latestTo = undefined;
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.accountingService.getAccountYears(true, false, false).then((x) => {
                this.accountYears = x;
                _.forEach(this.accountYears, (a: AccountYearDTO) => {
                    a['expander'] = "";
                    const yearStatus = _.find(this.accountStatuses, o => o.id === a.status);
                    if (yearStatus)
                        a['statusText'] = yearStatus.name;

                    a['statusIcon'] = this.getStatusIcon(a.status);

                    if (!this.latestTo || a.to > this.latestTo)
                        this.latestTo = a.to;

                    _.forEach(a.periods, (p) => {
                        const periodStatus = _.find(this.accountStatuses, o => o.id === p.status);
                        if (periodStatus)
                            p['statusName'] = periodStatus.name;

                        p.from = CalendarUtility.convertToDate(p.from);

                        const monthName = _.find(this.budgetSubTypes, t => t.id === (p.from.getMonth() + 1));
                        if (monthName)
                            p['monthName'] = monthName.name;

                        p["periodName"] = p.from.getFullYear().toString() + "-" + (p.from.getMonth() + 1).toString();

                        p['statusIcon'] = this.getStatusIcon(p.status);
                    });
                })

                this.setData(_.orderBy(this.accountYears, 'from', 'desc'));

                if (this.createNewYear) {
                    this.addAccountYear();
                    this.createNewYear = false;
                }
                else if (soeConfig.openAccountYearId && soeConfig.openAccountYearId > 0) {
                    const row = _.find(this.accountYears, (a) => a.accountYearId === soeConfig.openAccountYearId);
                    if (row)
                        this.editAccountYear(row);
                    soeConfig.openAccountYearId = undefined;
                }
            });
        }]);
    }

    private getStatusIcon(status: number): string {
        switch (status) {
            case TermGroup_AccountStatus.New:
                return "fas fa-circle";
            case TermGroup_AccountStatus.Open:
                return "fas fa-circle okColor";
            case TermGroup_AccountStatus.Closed:
                return "fas fa-circle warningColor";
            case TermGroup_AccountStatus.Locked:
                return "fas fa-circle errorColor";
            default:
                return "";
        }
    }

    private addAccountYear() {
        this.editAccountYear(undefined);
    }

    private editAccountYear(row: any = null) {
        const translationKeys: string[] = [
            "economy.accounting.accountyear.accountyear",
            "economy.accounting.accountyear.newaccountyear"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            const years = [];
            _.forEach(this.gridAg.options.getFilteredRows(), row => {
                years.push({ id: row.accountYearId });
            });
            const message = new TabMessage(
                `${row ? terms["economy.accounting.accountyear.accountyear"] : terms["economy.accounting.accountyear.newaccountyear"]} ${row ? row.yearFromTo : ""}`,
                row ? "year_" + row.accountYearId.toString() : "year_0",
                EditController,
                { id: row ? row.accountYearId : undefined, ids: years, latestTo: !row ? this.latestTo : undefined },
                this.urlHelperService.getGlobalUrl("/Economy/Accounting/AccountYear/Views/edit.html")
            );
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
        });
    }

    private reloadData() {
        this.loadGridData(true);
    }
}
