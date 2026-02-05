import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SmallGenericType } from "../../Models/smallgenerictype";
import { AccountingSettingsRowDTO } from "../../Models/AccountingSettingsRowDTO";
import { AccountDTO } from "../../Models/AccountDTO";
import { Feature, SoeEntityState } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";
import { CoreUtility } from "../../../Util/CoreUtility";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IColumnAggregate, IColumnAggregations } from "../../../Util/SoeGridOptionsAg";

export class AccountingSettingsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AccountingSettings', 'AccountingSettings.html'),
            scope: {
                settingTypes: '=',
                settings: '=',
                baseAccounts: '=',
                showBaseAccount: '=?',
                minRows: '@',
                readOnly: '=?',
                hideStdDim: '=?',
                mustHaveStandardIfInternal: '=?',
                useFixedAccounting: '=?',
                fixedAccounting: '=?',
                fixedAccountingDisabled: '=?',
                onFixedAccountingChanged: '&',
                onChange: '&',
                onInitialized: '&',
                isModified: '=?',
                isValid: '=?',
                parentGuid: '=?',
                useNoAccount: '=?',
                labelKey: '@',
                setDimNrIfZero: '@',
                ignoreHierarchyOnly: '@',
                forceBaseAccount: '=?',
                hideAboveCompanyStd : '=?'
            },
            restrict: 'E',
            replace: true,
            controller: AccountingSettingsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AccountingSettingsController extends GridControllerBase2Ag implements ICompositionGridController {

    // Setup
    private settingTypes: SmallGenericType[];
    private settings: AccountingSettingsRowDTO[];
    private baseAccounts: SmallGenericType[];
    private showBaseAccount: boolean;
    private minRows: number;
    private readOnly: boolean;
    private hideStdDim = false;
    private mustHaveStandardIfInternal: boolean;
    private useFixedAccounting: boolean;
    private fixedAccounting: boolean;
    private fixedAccountingDisabled: boolean;
    private onFixedAccountingChanged: Function;
    private onChange: Function;
    private onInitialized: Function;
    private isModified: boolean;
    private parentGuid: string;
    private useNoAccount: boolean;
    private labelKey: string;
    private setDimNrIfZero: boolean;
    private forceBaseAccount: boolean;
    private ignoreHierarchyOnly: boolean;
    private hideAboveCompanyStd: boolean;

    // Collections
    private terms: any;
    public accountDims: AccountDimSmallDTO[];

    // Flags
    private settingsInitialized = false;

    // Validation
    private validationErrors: string = '';
    private isValid: boolean;

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Common.Directives.AccountingSettings", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setupGrid())

        this.onInit();
    }

    public onInit() {
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableFiltering = false;

        this.flowHandler.start({
            feature: Feature.None,
            loadReadPermissions: true,
            loadModifyPermissions: true,
        });
    }

    public $onInit() {
        if (!this.labelKey)
            this.labelKey = "common.accountingsettings.accountingsettings";
    }

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadTerms()]).then(() => {
            this.$q.all([
                this.loadAccounts(true),
            ]).then(() => {
                this.setupWatchers();
            });
        });
    }

    public setupGrid() {
        if (!this.minRows)
            this.minRows = 8;
        this.gridAg.options.setMinRowsToShow(this.minRows);
        this.gridAg.options.setAutoHeight(true);

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridAg.options.subscribe(events);
    }

    private initSettings() {
        if (!this.settings)
            this.settings = [];

        // Copy setting type names and base accounts to setting rows
        _.forEach(this.settingTypes, (settingType: SmallGenericType) => {
            // Get setting
            var setting = _.find(this.settings, { type: settingType.id });
            if (!setting) {
                // No setting found, add one
                setting = new AccountingSettingsRowDTO(settingType.id);
                this.settings.push(setting);
            }
            setting.typeName = settingType.name;

            // Set base account
            var account;
            if (this.accountDims && this.accountDims.length > 0) {
                var baseAccount = _.find(this.baseAccounts, { id: settingType.id });
                if (baseAccount)
                    account = _.find(this.accountDims[0].accounts, { accountId: Number(baseAccount.name) });
            }
            setting.baseAccount = account ? account.numberName : '';
        });

        this.isModified = false;
        this.validate();

        if (!this.settingsInitialized) {
            this.settingsInitialized = true;
            if (this.onInitialized) {
                this.$timeout(() => {
                    this.onInitialized();
                });
            }
        }
    }

    private setupGridColumns() {
        this.initSettings();
        this.gridAg.options.resetColumnDefs();
        this.gridAg.addColumnText("typeName", this.terms["common.type"], null);

        const editable = (data) => {
            return !this.readOnly;
        };
        this.accountDims.forEach((ad, i) => {
            if (this.hideStdDim && i == 0)
                return;

            if (this.hideAboveCompanyStd && ad.isAboveCompanyStdSetting)
                return;

            const errorField = i == 0 ? "account1Error" : null;

            const dimNr = i + 1;
            const field = `account${dimNr}Nr`;
            const secondField = `account${dimNr}Name`
            const onCellChanged = (params) => {
                this.setNewAccount(params, dimNr);
            };

            this.gridAg.addColumnTypeAhead(field, ad.accountDimNr === 1 ? this.terms["common.accountingsettings.account"] : ad.name, null, {
                editable: editable.bind(this),
                secondRow: secondField,
                onChanged: onCellChanged.bind(this),
                error: errorField,
                typeAheadOptions: {
                    source: (filter) => this.filterAccounts(i, filter),
                    updater: null,
                    displayField: "numberName",
                    dataField: "accountNr",
                    minLength: 0,
                    useScroll: true,
                    allowNavigationFromTypeAhead: () => true,
                }
            });
        })

        // Percent
        if (this.useFixedAccounting) {
            this.gridAg.addColumnNumber("percent", this.terms["common.percent"], null, {
                decimals: 2,
                editable: editable.bind(this),
                hide: !this.fixedAccounting
            });
        }

        if (this.showBaseAccount) {
            this.gridAg.addColumnText("baseAccount", this.terms["common.accountingsettings.baseaccount"], null);
        }

        this.gridAg.addColumnDelete(this.terms["core.delete"], this.deleteRow.bind(this), null, this.showDeleteRow.bind(this));
        this.sortSettings();
        this.gridAg.finalizeInitGrid("accounts", false);
        this.gridIsReady();
    }

    private percentAggregateRenderer({ data, colDef, formatValue }) {
        const value = this.getPercentAggregation() || 0;
        let cssClass = "pull-right"
        if (value != 100) cssClass += " errorColor indiscreet";
        return `<div class="${cssClass}">${value}</div>`;
    }

    private getPercentAggregation(): number {
        return this.settings.reduce((sum, { percent }) => { return sum + (percent || 0) }, 0);
    }

    private gridIsReady() {
        if (this.useFixedAccounting) {
            const percentColumnAggregate = {
                getSeed: () => 0,
                accumulator: (acc, next) => acc + next,
                cellRenderer: this.percentAggregateRenderer.bind(this)
            } as IColumnAggregate;

            this.gridAg.options.addFooterRow("#accounting-sum-footer-grid", { "percent": percentColumnAggregate } as IColumnAggregations);
        }
    }

    private setupWatchers() {
        this.$scope.$on('reloadAccounts', (e, a) => {
            if (!a?.guid || (a.guid === this.parentGuid)) {
                this.loadAccounts(false);
            }
        });

        this.$scope.$watchCollection(() => this.settingTypes, (newVal, oldVal) => {
            this.initSettings();
            this.sortSettings();
        });
        this.$scope.$watchCollection(() => this.settings, (newVal, oldVal) => {
            if (this.setDimNrIfZero) {
                this.accountDims.forEach((ad, i) => {
                    if (!this.hideStdDim || i > 0) {
                        _.forEach(this.settings, settingRow => {
                            if (settingRow[`accountDim${i + 1}Nr`] === 0)
                                settingRow[`accountDim${i + 1}Nr`] = ad.accountDimNr;
                        });
                    }
                });
            }

            this.initSettings();
            this.sortSettings();
        });
        this.$scope.$watch(() => this.fixedAccounting, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.fixedAccountingChanged();
        });
        this.$scope.$watch(() => this.hideStdDim, (newVal, oldVal) => {
            this.hideStdDimChanged();
        });
        this.$scope.$on('stopEditing', (e, a) => {
            this.gridAg.options.stopEditing(false);
            this.$timeout(() => {
                a.functionComplete();
            }, 100)
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.delete",
            "common.type",
            "common.accountingsettings.account",
            "common.accountingsettings.baseaccount",
            "common.accountingsettings.fixed",
            "common.accountingsettings.noaccount",
            "common.accountingsettings.error.musthavestandardifinternal",
            "common.accountingsettings.error.missingaccount",
            "common.accountingsettings.error.invalidpercent",
            "common.percent",
            "common.accountingrows.missingaccount",
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadAccounts(useCache: boolean) {
        return this.coreService.getAccountDimsSmall(false, this.hideStdDim, true, false, true, true, useCache, this.ignoreHierarchyOnly).then(x => {
            this.accountDims = x;
            if (this.hideStdDim) {
                // Create empty standard dim, to make indexing easier
                let stdDim = new AccountDimSmallDTO();
                stdDim.accountDimId = 0;
                stdDim.accountDimNr = 1;
                stdDim.accounts = [];
                this.accountDims.splice(0, 0, stdDim);
            }
            _.forEach(this.accountDims, (dim) => {
                if (!dim.accounts)
                    dim.accounts = [];

                if (this.useNoAccount) {
                    // Add "no account"
                    let noAccount = new AccountDTO();
                    noAccount.accountId = -1;
                    noAccount.accountDimId = dim.accountDimId;
                    noAccount.accountNr = '-';
                    noAccount.name = noAccount.numberName = this.terms["common.accountingsettings.noaccount"];
                    dim.accounts.splice(0, 0, noAccount);
                }

                // Add empty account
                let emptyAccount = new AccountDTO();
                emptyAccount.accountId = 0;
                emptyAccount.accountDimId = dim.accountDimId;
                emptyAccount.accountNr = '';
                emptyAccount.name = '';
                emptyAccount.numberName = '';
                dim.accounts.splice(0, 0, emptyAccount);
            });
        });
    }

    // EVENTS
    public filterAccounts(dimIndex, filter) {
        return this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter);
        });
    }

    private afterCellEdit(row: AccountingSettingsRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;

        if (colDef.field == "percent") {
            row[colDef.field] = parseFloat(newValue) || 0;
            this.validate();
        }

        if ((colDef.field.startsWithCaseInsensitive("account") && colDef.field.endsWithCaseInsensitive("nr"))) {
            this.validate();
        }

        this.isModified = (newValue || oldValue);

        if (this.onChange)
            this.onChange();

        if (this.isModified) {
            this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
        }
    }

    private fixedAccountingChanged() {
        this.$timeout(() => {
            if (this.fixedAccounting)
                this.gridAg.options.showColumn("percent");
            else
                this.gridAg.options.hideColumn("percent");

            this.isModified = true;
            this.validate();

            if (this.onFixedAccountingChanged)
                this.onFixedAccountingChanged();
            if (this.onChange)
                this.onChange();
        });
    }

    private hideStdDimChanged() {
        this.loadAccounts(true).then(() => {
            this.setupGridColumns();
        });
    }

    private addRow() {
        var type: number = this.settings.length + 3;
        var newRow: AccountingSettingsRowDTO = new AccountingSettingsRowDTO(type);
        newRow.typeName = '{0} {1}'.format(this.terms["common.accountingsettings.fixed"], (type - 2).toString());
        newRow.percent = 0;
        this.settings.push(newRow);

        this.isModified = true;
        this.validate();
        if (this.onChange)
            this.onChange();
    }

    private showDeleteRow(row): boolean {
        // Can only remove last row
        // Always keep two rows
        return !this.readOnly && this.fixedAccounting && row.type == (this.settings.length + 2) && row.type > 4;
    }

    private deleteRow(row) {
        _.pull(this.settings, row);

        this.isModified = true;
        this.validate();
        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private sortSettings() {
        _.forEach(this.settings, settingRow => {
            let clone = CoreUtility.cloneDTO(settingRow);
            this.clearDim(1, settingRow);
            this.clearDim(2, settingRow);
            this.clearDim(3, settingRow);
            this.clearDim(4, settingRow);
            this.clearDim(5, settingRow);
            this.clearDim(6, settingRow);

            this.setNewDim(_.find(this.accountDims, d => d.accountDimNr === clone.accountDim1Nr), 1, settingRow, clone);
            this.setNewDim(_.find(this.accountDims, d => d.accountDimNr === clone.accountDim2Nr), 2, settingRow, clone);
            this.setNewDim(_.find(this.accountDims, d => d.accountDimNr === clone.accountDim3Nr), 3, settingRow, clone);
            this.setNewDim(_.find(this.accountDims, d => d.accountDimNr === clone.accountDim4Nr), 4, settingRow, clone);
            this.setNewDim(_.find(this.accountDims, d => d.accountDimNr === clone.accountDim5Nr), 5, settingRow, clone);
            this.setNewDim(_.find(this.accountDims, d => d.accountDimNr === clone.accountDim6Nr), 6, settingRow, clone);
        });
        this.setData(this.settings);
    }

    private clearDim(dimNr: number, settingRow: AccountingSettingsRowDTO) {
        settingRow[`account${dimNr}Id`] = 0;
        settingRow[`account${dimNr}Nr`] = null;
        settingRow[`account${dimNr}Name`] = null;
        settingRow[`accountDim${dimNr}Nr`] = 0;
    }

    private setNewDim(dim: AccountDimSmallDTO, dimNr: number, settingRow: AccountingSettingsRowDTO, originalSettingRow: AccountingSettingsRowDTO) {
        if (!dim)
            return;

        let idx = this.accountDims.indexOf(dim);
        settingRow[`account${idx + 1}Id`] = originalSettingRow[`account${dimNr}Id`];
        settingRow[`account${idx + 1}Nr`] = originalSettingRow[`account${dimNr}Nr`];
        settingRow[`account${idx + 1}Name`] = originalSettingRow[`account${dimNr}Name`];
        settingRow[`accountDim${idx + 1}Nr`] = originalSettingRow[`accountDim${dimNr}Nr`];
    }

    private setNewAccount({ data, oldValue, newValue, ...params }, dimIndex: number) {
        if ((!oldValue && !newValue) || oldValue == newValue) return

        const accountNumber = newValue;
        const account = this.accountDims[dimIndex - 1].accounts.find(acc => acc.accountNr === accountNumber && acc.state === SoeEntityState.Active);
        const row = this.settings.find(r => r.type === data.type);

        if (row) {
            row[`account${dimIndex}Id`] = account ? account.accountId : 0;
            row[`account${dimIndex}Nr`] = account ? account.accountNr : null;
            row[`account${dimIndex}Name`] = account ? account.name : null;
            if (account && this.forceBaseAccount) {
                if (account.accountDimNr == 1) {
                    row["account1Error"] = undefined;
                } else if (!row.account1Id && account.accountDimNr != 1) {
                    row["account1Error"] = this.terms["common.accountingrows.missingaccount"];
                }
            }
            this.settings = [...this.settings];
            this.setData(this.settings);
        }
    }

    protected getSecondRowValue(entity, colDef) {
        let dimNr: number = colDef.soeData.additionalData.dimIndex + 1;

        let idToFind = entity[`account${dimNr}Nr`];
        entity[`account${dimNr}Id`] = 0;
        if (!idToFind)
            return null;

        //TODO: this use of dimindex is wrong, since they dont need to be in order or sequence
        let found = this.accountDims[colDef.soeData.additionalData.dimIndex].accounts.filter(a => a.accountNr === idToFind);
        if (found.length) {
            let acc = found[0];
            entity[`account${dimNr}Id`] = acc.accountId;
            return acc.name;
        }

        return null;
    }

    private validate() {
        this.isValid = true;
        this.validationErrors = '';

        if (this.mustHaveStandardIfInternal) {
            _.forEach(this.settings, settingRow => {
                if (!settingRow.account1Id &&
                    (settingRow.account2Id || settingRow.account3Id || settingRow.account4Id || settingRow.account5Id || settingRow.account6Id)) {
                    this.validationErrors += this.terms["common.accountingsettings.error.musthavestandardifinternal"] + "\n";
                    this.isValid = false;
                    return;
                }
            });
        }

        if (this.fixedAccounting) {

            // Check that percent total is 100%
            var total: number = 0;
            _.forEach(this.settings, row => {
                total += row.percent || 0;
            });

            if (total !== 100) {
                this.validationErrors += this.terms["common.accountingsettings.error.invalidpercent"];
                this.isValid = false;
            }

            this.gridAg.options.refreshColumns();
        }
    }
}