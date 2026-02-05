import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature, SoeEntityType, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountDimDTO } from "../../../Common/Models/AccountDimDTO";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { BatchUpdateController } from "../../../Common/Dialogs/BatchUpdate/BatchUpdateDirective";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { UpdateAccountDimStd } from "./UpdateAccountDimStd/UpdateAccountDimStd";
import { INotificationService } from "../../../Core/Services/NotificationService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private changedItems = [];

    items: any[];
    terms: any;

    typeFilterOptions = [];
    vatTypeFilterOptions = [];
    accountDim: AccountDimDTO;

    // Flags
    disableSave = true;
    hasBatchUpdatePermission: boolean = false;
    hasImportAccountStdTypePermission: boolean = false;
    selectedCount: number = 0;

    //@ngInject
    constructor(private $uibModal,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService,
        private messagingService: IMessagingService) {
        super(gridHandlerFactory, "Economy.Accounting.Accounts", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('accountId', 'accountNr');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((response) => {
                const mainPermission = response[Feature.Economy_Accounting_AccountRoles]
                const batchUpdatePermission = response[Feature.Economy_Accounting_Accounts_BatchUpdate]
                const importAccountStdTypePermission = response[Feature.Economy_Import_Sie_Account]

                this.readPermission = mainPermission?.readPermission || false;
                this.modifyPermission = mainPermission?.modifyPermission || false;

                this.hasBatchUpdatePermission = batchUpdatePermission?.modifyPermission || false;
                this.hasImportAccountStdTypePermission = importAccountStdTypePermission?.modifyPermission || false;

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadAccountTypes())
            .onBeforeSetUpGrid(() => this.loadSysVatAccounts())
            .onBeforeSetUpGrid(() => this.loadAccountDim())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(false); });
        }

        this.flowHandler.start([
            { feature: Feature.Economy_Accounting_AccountRoles, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Accounting_Accounts_BatchUpdate, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Import_Sie_Account, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    public loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getAccounts(soeConfig.accountDimId, soeConfig.accountYearId, this.accountDim.linkedToShiftType, !soeConfig.isStdAccount, !soeConfig.isStdAccount, useCache).then((x) => {
                this.items = x;

                _.forEach(this.items, (item) => {
                    if (!item['vatType'])
                        item['vatType'] = "";
                });

                this.setData(this.items);
            });
        }]);
    }

    public onActiveChanged(item: any) {
        // If item exists, remove it (it has been clicked twice and returned to original state),
        // otherwise add it.
        if (_.includes(this.changedItems, item.data)) {
            this.changedItems.splice(this.changedItems.indexOf(item.data), 1);
        } else {
            this.changedItems.push(item.data);
        }
        this.$timeout(() => {
            this.disableSave = this.changedItems.length === 0;
        });
    }

private onSave() {
    if (this.changedItems.length > 0) {
        this.progress.startSaveProgress(completion => {
            let dict: any = {};
            _.forEach(this.changedItems, (entity) => {
                dict[entity.accountId] = entity.isActive;
            });
            this.accountingService.updateAccountsState(dict).then(x => {
                if (x.success === false) 
                    this.notificationService.showDialog(this.terms["core.warning"], x.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);

                completion.completed();
            });
        }, this.guid).then(x => {
            this.changedItems = [];
        }).finally(() => {
            this.loadGridData(false);
        });
    }
}

    private setUpGrid() {
        const translationKeys: string[] = [
            "common.active",
            "common.number",
            "common.name",
            "core.edit",
            "core.warning",
            "economy.accounting.account.externalcode",
            "common.categories",
            "economy.accounting.account.parentaccount",
        ];

        if (soeConfig.isStdAccount) {
            translationKeys.push("economy.accounting.accounttype");
            translationKeys.push("economy.accounting.account.sysvataccount");
            translationKeys.push("economy.accounting.accountbalance");
        } else {
            translationKeys.push("economy.accounting.account.islinkedtoshifttype");
        }
      
        this.translationService.translateMany(translationKeys).then((terms) => {
            // hide selection
            //this.gridAg.options.enableRowSelection = false;
            this.terms = terms;
            this.gridAg.addColumnActive("accountId", terms["common.active"], 60, this.onActiveChanged.bind(this));
            this.gridAg.addColumnText("accountNr", terms["common.number"], 100);
            this.gridAg.addColumnText("name", terms["common.name"], null);

            if (soeConfig.isStdAccount) {
                this.gridAg.addColumnSelect("type", terms["economy.accounting.accounttype"], 100, { selectOptions: this.typeFilterOptions, displayField: "type", enableHiding: true });
                this.gridAg.addColumnSelect("vatType", terms["economy.accounting.account.sysvataccount"], 150, { selectOptions: this.vatTypeFilterOptions, displayField: "vatType", enableHiding: true });
                this.gridAg.addColumnNumber("balance", terms["economy.accounting.accountbalance"], 100, { enableHiding: true, decimals: 2});
            } else {
                this.gridAg.addColumnText("externalCode", terms["economy.accounting.account.externalcode"], null, true);
                this.gridAg.addColumnText("categories", terms["common.categories"], null, true);
                this.gridAg.addColumnText("parentAccountName", terms["economy.accounting.account.parentaccount"], null, true);
                this.gridAg.addColumnIcon("isLinkedToShiftType", " ", 50, { icon: "fal fa-link", showIcon: this.isLinkedToShiftType.bind(this), toolTip: terms["economy.accounting.account.islinkedtoshifttype"], enableResizing: false });
            }

            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            let events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => { this.selectionChanged() }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (rowNode) => { this.selectionChanged() }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("economy.accounting.accounts", true, undefined, true);
        });
    }

    selectionChanged() {
        this.$timeout(() => {
            this.selectedCount = this.gridAg.options.getSelectedCount();
        });
    }

    private isLinkedToShiftType(row) {
        return row.isLinkedToShiftType;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(false), true, () => this.onSave(), () => { return this.disableSave });

        if (soeConfig.isStdAccount && soeConfig.accountDimId) {
            if (this.hasBatchUpdatePermission) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.batchupdate.title", "common.batchupdate.title", IconLibrary.FontAwesome, "fa-pencil",
                    () => { this.openBatchUpdate(); }, () => { return this.selectedCount === 0; }, () => { return false }
                )));
            }


            const group = ToolBarUtility.createGroup();
            group.buttons.push(new ToolBarButton("economy.accounting.accountdimstd", "economy.accounting.accountdimstd", IconLibrary.FontAwesome, "fa-list-alt", () => { this.onEventOpenAccountDim(); }));
            group.buttons.push(new ToolBarButton("economy.accounting.importaccountsysstdtype", "economy.accounting.importaccountsysstdtype",
                IconLibrary.FontAwesome,
                "fa-file-import",
                () => this.onEventChangeAccountStd(),
                null,
                () => !this.hasImportAccountStdTypePermission));

            this.toolbar.addButtonGroup(group);
        }

    }

    private openBatchUpdate() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/BatchUpdate/Views/BatchUpdate.html"),
            controller: BatchUpdateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                entityType: () => { return SoeEntityType.Account },
                selectedIds: () => { return _.map(this.gridAg.options.getSelectedRows(), 'accountId') }
            }
        });

        modal.result.then(data => {
            // Reset cache
            this.loadGridData();
        }, function () {
            // Cancelled
        });
        return modal;
    }

    private onEventOpenAccountDim(): void {
        this.messagingService.publish(Constants.EVENT_OPEN_ACCOUNTDIM, {
            accountDimId: (this.accountDim != null) ? this.accountDim.accountDimId : 0,
            accountDimName: (this.accountDim != null) ? this.accountDim.name : "",
        });
    }

    private onEventChangeAccountStd() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Economy/Accounting/Accounts/UpdateAccountDimStd/UpdateAccountDimStd.html"),
            controller: UpdateAccountDimStd,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
            }
        });

        modal.result.then(data => {
            // Reset cache
            this.loadGridData();
        }, function () {
            // Cancelled
        });
        return modal;
    }

    private loadAccountTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountType, false, true).then((x) => {
            _.forEach(x, (y: any) => {
                this.typeFilterOptions.push({ value: y.name, label: y.name })
            });
        });
    }

    private loadSysVatAccounts(): ng.IPromise<any> {
        return this.accountingService.getSysVatAccounts(CoreUtility.sysCountryId, false).then((x) => {
            _.forEach(x, (y: any) => {
                this.vatTypeFilterOptions.push({ value: y.name, label: y.name })
            });
        });
    }

    private loadAccountDim(): ng.IPromise<any> {
        return this.accountingService.getAccountDim(soeConfig.accountDimId, false, false, false).then((x) => {
            this.accountDim = x;
        });
    }
}
