import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature, TermGroup, CompanySettingType, SoeEntityState, UserSettingType, TermGroup_AccountStatus, TermGroup_AccountType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountDimDTO, AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IAccountDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { AccountYearBalanceFlatDTO } from "../../../Common/Models/AccountYearBalanceFlatDTO";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SetAccountDialogController } from "./Dialogs/SetAccountDialog/SetAccountDialogController";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    terms: { [index: string]: string; };

    // Collections
    items: AccountYearBalanceFlatDTO[] = [];
    accountYears: any = [];
    accountYearsDict: any = [];
    accountDims: AccountDimSmallDTO[];
    accountTypes: ISmallGenericType[] = [];

    // Flags
    isReadonly = false;
    disableSave = true;
    setupComplete;

    // Settings
    useQuantityInVoucher = false;
    userAccountYearId = 0;

    // Includes
    toolbarInclude: any;
    gridFooterComponentUrl: any;

    // Properties
    private _selectedAccountYear: number;
    public get selectedAccountYear(): number {
        return this._selectedAccountYear;
    }
    public set selectedAccountYear(value: number) {
        this._selectedAccountYear = value;

        var accYear = _.find(this.accountYears, (a) => a.accountYearId === value);
        if (accYear) {
            // Get previous
            var prevYear = _.find(this.accountYears, (a) => a.from < accYear.from);
            this.$timeout(() => {
                this.previousYearDisabled = !prevYear;
            });
        }
        this.isReadonly = !accYear || accYear.status === TermGroup_AccountStatus.Closed || accYear.status === TermGroup_AccountStatus.Locked;

        if (this.gridAg && !_.find(this.items, (i) => i.isModified))
            this.loadGridData();
        else {
            this.gridAg.options.stopEditing(false);
            this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["economy.accounting.balance.changeyearerror"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        }
    }

    public previousYearDisabled = false;

    public get saveDisabled(): boolean {
        var selectedRows = this.gridAg.options.getSelectedRows();
        return !(_.filter(this.items, (i) => i.isModified).length > 0);
    }

    public get deleteDisabled(): boolean {
        return this.gridAg.options.getSelectedRows().length === 0;
    }

    debitAmount = 0;
    creditAmount = 0;
    diffAmount = 0;

    public createModified: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private urlHelperService: IUrlHelperService,
        gridHandlerFactory: IGridHandlerFactory,
        private messagingService: IMessagingService) {
        super(gridHandlerFactory, "Economy.Accounting.Balance", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.doLookup())

        this.onTabActivated(() => this.localOnTabActivated());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        // Set footer
        this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("economy/accounting/balance/views/gridFooter.html");

        // Add navigation
        this.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);
    }

    private localOnTabActivated() {
        if (!this.setupComplete) {
            this.flowHandler.start({ feature: Feature.Economy_Accounting_Balance, loadReadPermissions: true, loadModifyPermissions: true });
            this.setupComplete = true;
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.accounting.balance.transferbalance", "economy.accounting.balance.transferbalance", IconLibrary.FontAwesome, "fa-arrow-right", () => {
            this.transferBalanceFromPreviousYear();
        }, () => {
            return (this.isReadonly || this.previousYearDisabled)
        }
        )));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
            this.addRow()
        }, () => {
            return this.isReadonly
        }))); 

        this.toolbar.addInclude(this.urlHelperService.getGlobalUrl("economy/accounting/balance/views/gridHeader.html"));
    }

    private doLookup() {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadAccountYears(),
            this.loadAccounts(),
            this.loadOrderTypes(),
        ]).then(() => {
            this.setUpGrid();

            if (soeConfig.accountYearId)
                this.selectedAccountYear = soeConfig.accountYearId;
            else if (this.userAccountYearId > 0)
                this.selectedAccountYear = this.userAccountYearId;
        });
    }

    private loadOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountType, false, false).then((x) => {
            this.accountTypes = [];
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.newrow",
            "common.accountingrows.rownr",
            "common.quantity",
            "common.debit",
            "common.credit",
            "core.deleterow",
            "economy.accounting.accounttype",
            "core.warning",
            "economy.accounting.balance.changeyearerror"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountingUseQuantityInVoucher);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useQuantityInVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingUseQuantityInVoucher);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        
        settingTypes.push(UserSettingType.AccountingAccountYear);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.userAccountYearId = SettingsUtility.getIntUserSetting(x, UserSettingType.AccountingAccountYear);
        });
    }

    private loadAccountYears(): ng.IPromise<any> {
        this.accountYears = [];
        this.accountYearsDict = [];
        return this.accountingService.getAccountYears().then((x) => {
            this.accountYears = x;
            _.forEach(_.orderBy(this.accountYears, 'from', 'desc'), (year) => {
                year.from = new Date(year.from);
                year.to = new Date(year.to);
                this.accountYearsDict.push({ id: year.accountYearId, name: year.yearFromTo });
            });
        });
    }

    private loadAccounts(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, false, true, true, false, true).then(x => {
            this.accountDims = x;

            this.accountDims.forEach(ad => {
                if (!ad.accounts)
                    ad.accounts = [];

                if (ad.accountDimNr === 1)
                    ad.accounts = _.filter(ad.accounts, (a) => a.accountTypeSysTermId === TermGroup_AccountType.Asset || a.accountTypeSysTermId === TermGroup_AccountType.Debt);

                if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0) {
                    (<any[]>ad.accounts).unshift({ accountId: 0, accountNr: '', name: '', numberName: ' ', state: SoeEntityState.Active });
                }
            });
        });
    }

    public loadGridData() {
        if (!this.selectedAccountYear)
            return;

        this.progress.startLoadingProgress([() => {
            return this.accountingService.getAccountYearBalance(this.selectedAccountYear).then((x) => {
                this.items = x;
                _.forEach(this.items, (i) => {
                    if (!i["dim2Name"])
                        i["dim2Name"] = "";
                    if (!i["dim3Name"])
                        i["dim3Name"] = "";
                    if (!i["dim4Name"])
                        i["dim4Name"] = "";
                    if (!i["dim5Name"])
                        i["dim5Name"] = "";
                    if (!i["dim6Name"])
                        i["dim6Name"] = "";
                    if (!i["dim2Nr"])
                        i["dim2Nr"] = "";
                    if (!i["dim3Nr"])
                        i["dim3Nr"] = "";
                    if (!i["dim4Nr"])
                        i["dim4Nr"] = "";
                    if (!i["dim5Nr"])
                        i["dim5Nr"] = "";
                    if (!i["dim6Nr"])
                        i["dim6Nr"] = "";
                    if (!i["quantity"])
                        i["quantity"] = 0;
                });

                this.setGridData();

                // Sum
                this.summarize();

                // Created modified
                var created = _.first(_.orderBy(_.filter(this.items, (r) => r.created), 'created'));
                var modified = _.last(_.orderBy(_.filter(this.items, (r) => r.modified), 'modified'));

                this.createModified = { modified: modified ? modified.modified : undefined, modifiedBy: modified ? modified.modifiedBy : undefined, created: created ? created.created : undefined, createdBy: created ? created.createdBy : undefined };
            });
        }]);
    }

    private save(ignoreAccountValidation = false) {
        var validatedItems = [];
        var itemsToSave = _.orderBy(_.filter(this.items, (i) => i.isModified || i.isDeleted), 'rowNr');

        // Check empty account
        if (_.filter(itemsToSave, (i) => !i.dim1Id || i.dim1Id === 0).length > 0) {
            if (ignoreAccountValidation) {
                itemsToSave = _.filter(itemsToSave, (i) => i.dim1Id && i.dim1Id > 0);
            }
            else {
                var keys: string[] = [
                    "core.verifyquestion",
                    "economy.accounting.balance.missingaccount",
                ];

                this.translationService.translateMany(keys).then(terms => {
                    var modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["economy.accounting.balance.missingaccount"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                    modal.result.then(val => {
                        if (val)
                            this.save(true);
                    });
                });
                return;
            }
        }

        if (itemsToSave.length === 0) {
            this.items = _.filter(this.items, (i) => i.dim1Id && i.dim1Id > 0);
            this.setGridData();
            return;
        }

        // Validate doubles
        var clonesExist = false;
        var clonesStr = "";
        var handledRows = [];
        var rowsToEmpty = [];

        _.forEach(itemsToSave, (item: AccountYearBalanceFlatDTO) => {
            if (item.isDeleted) {
                if (item.accountYearBalanceHeadId && item.accountYearBalanceHeadId > 0)
                    validatedItems.push(item);
            }
            else if (!_.includes(handledRows, item.rowNr)) {
                var clones = _.filter(this.items, (i) => !i.isDeleted && i.rowNr !== item.rowNr && i.dim1Id === item.dim1Id && i.dim2Id === item.dim2Id && i.dim3Id === item.dim3Id && i.dim4Id === item.dim4Id && i.dim5Id === item.dim5Id && i.dim6Id === item.dim6Id);
                if (clones.length) {
                    // Add first
                    var group = [item];

                    // Add clones
                    _.forEach(clones, (c) => {
                        handledRows.push(c.rowNr);
                        group.push(c);
                    });

                    // Sort
                    group = _.orderBy(group, 'rowNr');

                    if (clonesExist)
                        clonesStr += ", ";

                    var first = true;
                    _.forEach(group, (c) => {
                        clonesStr += first ? ("(" + c.rowNr) : (", " + c.rowNr);
                        first = false;
                    });
                    clonesStr += ")";
                    clonesExist = true;

                    rowsToEmpty.push({ item: group[0], doubles: group.slice(1) });
                }
                else {
                    validatedItems.push(item);
                }
                handledRows.push(item.rowNr);
            }
        });

        if (clonesExist) {
            const cloneKeys: string[] = [
                "core.verifyquestion",
                "economy.accounting.balance.sameaccounts",
                "economy.accounting.balance.willmerge",
            ];

            this.translationService.translateMany(cloneKeys).then(terms => {
                const modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["economy.accounting.balance.willmerge"] + "\n\n" + terms["economy.accounting.balance.sameaccounts"] + "\n" + clonesStr, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        // merge rows
                        _.forEach(rowsToEmpty, (x) => {
                            _.forEach(x.doubles, (y: AccountYearBalanceFlatDTO) => {
                                x.item.balance += y.balance;
                                x.item.debitAmount += y.debitAmount;
                                x.item.creditAmount += y.creditAmount;

                                if (y.accountYearBalanceHeadId && y.accountYearBalanceHeadId > 0) {
                                    y.isDeleted = true;
                                    validatedItems.push(y);
                                }
                            });
                            x.item.isModified = true;
                            validatedItems.push(x.item);
                        });
                        this.onSave(validatedItems);
                    }
                });
            });
        }
        else {
            this.onSave(itemsToSave);
        }
    }

    private delete() {
        const selectedRows = this.gridAg.options.getSelectedRows();
        _.forEach(selectedRows, (r) => { r.isDeleted = true });
        this.onSave(selectedRows);
    }

    private onSave(items: AccountYearBalanceFlatDTO[]) {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveAccountYearBalances(this.selectedAccountYear, items).then((result) => {
                if (result.success) {
                    completion.completed(null, null, true);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
        });
    }

    private setUpGrid() {
        this.gridAg.addColumnIsModified();

        this.gridAg.addColumnNumber("rowNr", this.terms["common.accountingrows.rownr"], 80, { editable: false, pinned: 'left' });

        this.accountDims.forEach((ad, i) => {
            let index = i + 1;

            const field = "dim" + index + "Nr";
            const secondRowField = "dim" + index + "Name";
            const errorField = "dim" + index + "Error";
            const editable = (data) => {
                return !this.isReadonly;
            };

            const onCellChanged = ({ data }) => {
                this.onAccountingDimChanged(data, index);
            };

            const allowNavigateFrom = (value, data) => {
                return this.allowNavigationFromTypeAhead(value, data, index);
            };

            const col = this.gridAg.addColumnTypeAhead(field, ad.name, null, {
                editable: editable.bind(this),
                error: errorField,
                secondRow: secondRowField,
                onChanged: onCellChanged.bind(this),
                typeAheadOptions: {
                    source: (filter) => this.filterAccounts(i, filter),
                    updater: null,
                    allowNavigationFromTypeAhead: allowNavigateFrom.bind(this),
                    displayField: "numberName",
                    dataField: "accountNr",
                    minLength: 0,
                },
                enableRowGrouping: true,
                ignoreColumnOnGrouping: true,
            }, {
                dimIndex: index,
            });

            if (index === 1)
                this.gridAg.addColumnText("dim1TypeName", this.terms["economy.accounting.accounttype"], null, true, { enableRowGrouping: true });
        });

        if (this.useQuantityInVoucher)
            this.gridAg.addColumnNumber("quantity", this.terms["common.quantity"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });

        this.gridAg.addColumnNumber("debitAmount", this.terms["common.debit"], null, { editable: () => { return !this.isReadonly; }, enableHiding: false, decimals: 2, enableRowGrouping: false, aggFuncOnGrouping: 'sum' });

        this.gridAg.addColumnNumber("creditAmount", this.terms["common.credit"], null, { editable: () => { return !this.isReadonly; }, enableHiding: false, decimals: 2, enableRowGrouping: false, aggFuncOnGrouping: 'sum' });

        // Events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => {
            this.afterCellEdit(entity, colDef, newValue, oldValue);
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
            this.summarize();
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => {
            this.summarize();
        }));

        this.gridAg.options.subscribe(events);

        this.gridAg.options.useGrouping(true, true, { keepColumnsAfterGroup: false, selectChildren: true });

        this.gridAg.finalizeInitGrid("economy.accounting.accounts", true);
    }

    protected allowNavigationFromTypeAhead(value, entity, dimIndex) {

        var currentValue = value;

        if (!currentValue)
            return true;

        var valueHasMatchingAccount = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.state === SoeEntityState.Active && acc.accountNr === currentValue);
        if (valueHasMatchingAccount.length) 
            return true;

        return false;
    }

    public filterAccounts(dimIndex, filter) {
        return _.orderBy(this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.state === SoeEntityState.Active && acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.state === SoeEntityState.Active && (acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter));
        }), 'accountNr');
    }

    protected onAccountingDimChanged(data, dimIndex) {
        var acc = this.findAccount(data, dimIndex);

        data['dim' + dimIndex + 'Id'] = acc ? acc.accountId : 0;
        data['dim' + dimIndex + 'Name'] = acc ? acc.name : "";        

        if (dimIndex === 1)
            this.setRowItemAccounts(data, acc, false)

        this.gridAg.options.refreshRows(data);
    }

    protected findAccount(entity: any, dimIndex: number) {
        var nrToFind = entity['dim' + dimIndex + 'Nr'];

        if (!nrToFind)
            return null;

        var found = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.accountNr === nrToFind && acc.state === SoeEntityState.Active);
        return found.length ? found[0] : null;
    }

    private setRowItemAccounts(rowItem: any, account: IAccountDTO, setInternalAccountFromAccount: boolean, internalsFromStdIfMissing: boolean = false) {
        // Set standard account
        rowItem.dim1Id = account != null ? account.accountId : 0;
        rowItem.dim1Nr = account != null ? account.accountNr : '';
        rowItem.dim1Name = account != null ? account.name : '';
        rowItem.dim1Disabled = false;
        rowItem.dim1Mandatory = true;
        rowItem.dim1Stop = true;
        rowItem.quantityStop = account != null ? account.unitStop : false;
        rowItem.unit = account != null ? account.unit : '';
        rowItem.amountStop = account != null ? account.amountStop : 1;
        rowItem.rowTextStop = account != null ? account.rowTextStop : true;
        rowItem.isAccrualAccount = account != null ? account.isAccrualAccount : false;

        if (setInternalAccountFromAccount) {
            // Clear internal accounts
            rowItem.dim2Id = 0;
            rowItem.dim2Nr = '';
            rowItem.dim2Name = '';
            rowItem.dim2Disabled = false;
            rowItem.dim2Mandatory = false;
            rowItem.dim2Stop = false;
            rowItem.dim3Id = 0;
            rowItem.dim3Nr = '';
            rowItem.dim3Name = '';
            rowItem.dim3Disabled = false;
            rowItem.dim3Mandatory = false;
            rowItem.dim3Stop = false;
            rowItem.dim4Id = 0;
            rowItem.dim4Nr = '';
            rowItem.dim4Name = '';
            rowItem.dim4Disabled = false;
            rowItem.dim4Mandatory = false;
            rowItem.dim4Stop = false;
            rowItem.dim5Id = 0;
            rowItem.dim5Nr = '';
            rowItem.dim5Name = '';
            rowItem.dim5Disabled = false;
            rowItem.dim5Mandatory = false;
            rowItem.dim5Stop = false;
            rowItem.dim6Id = 0;
            rowItem.dim6Nr = '';
            rowItem.dim6Name = '';
            rowItem.dim6Disabled = false;
            rowItem.dim6Mandatory = false;
            rowItem.dim6Stop = false;
            // Set internal accounts
            if (account != null && account.accountInternals != null) {
                // Get internal accounts from the account
                account.accountInternals.forEach(ai => {
                    if (ai.accountDimNr > 1) {
                        var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based
                        rowItem[`dim${index}Id`] = ai.accountId || 0;
                        rowItem[`dim${index}Nr`] = ai.accountNr || '';
                        rowItem[`dim${index}Name`] = ai.name || '';
                        rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
                        rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
                        rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
                    }
                });
            }
        }
        else if (internalsFromStdIfMissing) {
            if (account != null && account.accountInternals != null) {
                // Get internal accounts from the account
                account.accountInternals.forEach(ai => {
                    if (ai.accountDimNr > 1) {
                        var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based
                        if (!rowItem[`dim${index}Id`] || ai.mandatoryLevel === 1) {
                            rowItem[`dim${index}Id`] = ai.accountId || 0;
                            rowItem[`dim${index}Nr`] = ai.accountNr || '';
                            rowItem[`dim${index}Name`] = ai.name || '';
                            rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
                            rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
                            rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
                        }
                    }
                });
            }
        }
        else {
            // Keep internal accounts, just set number and names
            // If not found, keep values from server since it can be an account that has been inactivated but we should 
            // always show choosen account dims...
            var index = 1;
            _.forEach(_.filter(this.accountDims, d => d.accountDimNr !== 1), dim => {
                index = index + 1;
                var account = _.find(dim.accounts, a => a.accountId === rowItem[`dim${index}Id`]);
                if (account) {
                    rowItem[`dim${index}Nr`] = account.accountNr;
                    rowItem[`dim${index}Name`] = account.name;
                }
                else {
                    rowItem[`dim${index}Nr`] = rowItem[`dim${index}Nr`] ? rowItem[`dim${index}Nr`] : "";
                    rowItem[`dim${index}Name`] = rowItem[`dim${index}Name`] ? rowItem[`dim${index}Name`] : "";
                }
            });

        }
    }

    protected handleNavigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition } = params;

        if (!nextCellPosition)
            return null;

        let { rowIndex, column } = nextCellPosition;
        const row = this.gridAg.options.getVisibleRowByIndex(rowIndex).data;

        if(!row)
            return { rowIndex, column };

        if (column.colId === 'soe-grid-menu-column') {
            const nextRowResult = this.gridAg.findNextRowInfo(row);

            if (nextRowResult) {
                return {
                    rowIndex: nextRowResult.rowIndex,
                    column: this.gridAg.options.getColumnByField('dim1Nr')
                };
            } else {
                this.gridAg.options.stopEditing(false);
                this.addRow();
                return null;
            }
        }
        else if (column.colId === 'isModified') {
            const prevRowResult = this.gridAg.findPreviousRowInfo(row);

            if (prevRowResult) {
                return {
                    rowIndex: prevRowResult.rowIndex,
                    column: this.gridAg.options.getColumnByField('creditAmount')
                };
            }
        }
        else {
            return { rowIndex, column };
        }
    }

    private afterCellEdit(entity: AccountYearBalanceFlatDTO, colDef, newValue, oldValue) {
        const field: string = colDef.field;

        if (field.startsWithCaseInsensitive('debit') && newValue !== oldValue) {
            if (newValue !== 0)
                entity.creditAmount = 0;
            entity.isModified = true;
        }
        else if (field.startsWithCaseInsensitive('credit') && newValue !== oldValue) {
            if(newValue !== 0)
                entity.debitAmount = 0;
            entity.isModified = true;
        }
        else if (field === "dim1Nr") {
            var acc = this.findAccount(entity, colDef.soeData.dimIndex);
            this.setRowItemAccounts(entity, acc, true);
        }

        this.gridAg.options.refreshRows(entity);
        this.summarize();
    }

    private addRow(): void {
        var row = new AccountYearBalanceFlatDTO();
        row.rowNr = _.filter(this.items, (i) => !i.isDeleted).length + 1;

        row.dim1Id = 0;
        row.dim2Id = 0;
        row.dim3Id = 0;
        row.dim4Id = 0;
        row.dim5Id = 0;
        row.dim6Id = 0;
        row.dim1Name = "";
        row.dim2Name = "";
        row.dim3Name = "";
        row.dim4Name = "";
        row.dim5Name = "";
        row.dim6Name = "";
        row.dim1Nr = "";
        row.dim2Nr = "";
        row.dim3Nr = "";
        row.dim4Nr = "";
        row.dim5Nr = "";
        row.dim6Nr = "";

        row.debitAmount = 0;
        row.creditAmount = 0;

        row.isModified = true;

        this.gridAg.options.addRow(row);
        this.items.push(row);

        this.gridAg.options.startEditingCell(row, this.gridAg.options.getColumnByField('dim1Nr'));
    }

    private transferBalanceFromPreviousYear() {
        _.forEach(this.items, (item) => {
            item.isDeleted = true;
        });

        this.progress.startLoadingProgress([() => {
            return this.accountingService.getAccountYearBalanceForPreviousYear(this.selectedAccountYear).then((result) => {
                if (result.success) {
                    _.forEach(result.value.$values, (i) => {
                        if (!i["dim2Name"])
                            i["dim2Name"] = "";
                        if (!i["dim3Name"])
                            i["dim3Name"] = "";
                        if (!i["dim4Name"])
                            i["dim4Name"] = "";
                        if (!i["dim5Name"])
                            i["dim5Name"] = "";
                        if (!i["dim6Name"])
                            i["dim6Name"] = "";
                        if (!i["dim2Nr"])
                            i["dim2Nr"] = "";
                        if (!i["dim3Nr"])
                            i["dim3Nr"] = "";
                        if (!i["dim4Nr"])
                            i["dim4Nr"] = "";
                        if (!i["dim5Nr"])
                            i["dim5Nr"] = "";
                        if (!i["dim6Nr"])
                            i["dim6Nr"] = "";
                        if (!i["quantity"])
                            i["quantity"] = 0;
                        i.isModified = true;

                        this.items.push(i);
                    });

                    const diffRow = _.find(this.items, (r) => r.isDiffRow && !r.isDeleted);
                    if (diffRow) {
                        const modal = this.$uibModal.open({
                            templateUrl: this.urlHelperService.getGlobalUrl("Economy/Accounting/Balance/Dialogs/SetAccountDialog/SetAccountDialog.html"),
                            controller: SetAccountDialogController,
                            controllerAs: 'ctrl',
                            backdrop: 'static',
                            size: 'sm',
                            resolve: {
                                translationService: () => { return this.translationService },
                                coreService: () => { return this.coreService },
                                amount: () => { return diffRow.creditAmount && diffRow.creditAmount != 0 ? diffRow.creditAmount : diffRow.debitAmount },
                                accounts: () => { return this.accountDims[0].accounts}
                            }
                        });

                        modal.result.then((account) => {
                            if (account) {
                                this.setRowItemAccounts(diffRow, account, false);
                            }
                            else {
                                diffRow.isDeleted = true;
                            }

                            this.setGridData();

                            // Sum
                            this.summarize();
                        });
                    }
                    else {
                        this.setGridData();

                        // Sum
                        this.summarize();
                    }
                }
            });
        }]);
    }

    private setGridData() {
        this.setData(_.filter(this.items, (r) => !r.isDeleted));
    }

    private summarize() {
        this.debitAmount = 0;
        this.creditAmount = 0;
        this.diffAmount = 0;
        this.$timeout(() => {
            this.debitAmount = _.sum(_.map(_.filter(this.items, (i) => !i.isDeleted), i => i.debitAmount));
            this.creditAmount = _.sum(_.map(_.filter(this.items, (i) => !i.isDeleted), i => i.creditAmount));
            var diff = this.debitAmount - this.creditAmount;
            this.diffAmount = diff > -0.001 && diff < 0.001 ? 0 : diff;
        });
    }
}
