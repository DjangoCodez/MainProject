import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { Constants } from "../../../../../Util/Constants";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/ProgressHandlerFactory";
import { ISoeGridOptionsAg, SoeGridOptionsAg, TypeAheadOptionsAg } from "../../../../../Util/SoeGridOptionsAg";
import { AccountDimDTO } from "../../../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { CompanyGroupMappingHeadDTO, CompanyGroupMappingRowDTO } from "../../../../../Common/Models/CompanyGroupMappingHeadDTO";

export class AddCompanyGroupMappingController {

    langId: number;
    terms: { [index: string]: string; };

    accountDimStd: AccountDimDTO[];
    accounts: AccountDTO[] = [];
    accountsDict: any[] = [];

    companyGroupMapping: CompanyGroupMappingHeadDTO;

    // Grid
    protected soeGridOptions: ISoeGridOptionsAg;

    progress: IProgressHandler;

    get validToSave() {
        return this.companyGroupMapping && this.companyGroupMapping.number && this.companyGroupMapping.name;
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private companyGroupMappingId?: number) {

        this.progress = progressHandlerFactory.create();
        this.setup();
    }

    private setup() {

        this.soeGridOptions = new SoeGridOptionsAg("CompanyGroupMappingRows", this.$timeout);

        if (this.companyGroupMappingId) {
            this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadAccounts(),
                () => this.loadExistingCompanyGroupMapping(),
            ]).then(() => {
                this.setupGrid();
            });
        }
        else {
            this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadAccounts(),
            ]).then(() => {
                this.companyGroupMapping = new CompanyGroupMappingHeadDTO();
                this.companyGroupMapping.rows = [];

                this.setupGrid();
            });
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.warning",
            "economy.accounting.companygroup.validaterowsmessage",
            "economy.accounting.companygroup.childaccountfrom",
            "economy.accounting.companygroup.childaccountto",
            "economy.accounting.companygroup.parentaccount",
            "common.remove",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadAccounts(): ng.IPromise<any> {
        this.accounts = [];
        this.accountsDict = [];
        return this.accountingService.getAccountDims(true, false, true, false, false).then((result) => {
            this.accountDimStd = result;
            // Only using std for now, same as in SL
            this.accountsDict.push({ id: 0, name: " ", number: undefined });
            _.forEach(this.accountDimStd[0].accounts, (account) => {
                this.accounts.push(account);
                this.accountsDict.push({ id: account.accountId, name: account.accountNr + " - " + account.name, number: account.accountNr });
            });
        });
    }

    private loadExistingCompanyGroupMapping(): ng.IPromise<any> {
        return this.accountingService.getCompanyGroupMapping(this.companyGroupMappingId).then((result) => {
            this.companyGroupMapping = result;
        });
    }

    private setupGrid() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableRowSelection = false;

        var projectOptions = new TypeAheadOptionsAg();
        projectOptions.source = (filter) => this.filterAccounts(filter);
        projectOptions.displayField = "name"
        projectOptions.dataField = "name";
        projectOptions.minLength = 0;
        projectOptions.delay = 0;
        projectOptions.useScroll = true;
        projectOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("childAccountFromName", this.terms["economy.accounting.companygroup.childaccountfrom"], null, { typeAheadOptions: projectOptions, editable: true, suppressSorting: true });
        var invoiceOptions = new TypeAheadOptionsAg();
        invoiceOptions.source = (filter) => this.filterAccounts(filter);
        invoiceOptions.displayField = "name"
        invoiceOptions.dataField = "name";
        invoiceOptions.minLength = 0;
        invoiceOptions.delay = 0;
        invoiceOptions.useScroll = true;
        invoiceOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("childAccountToName", this.terms["economy.accounting.companygroup.childaccountto"], null, { typeAheadOptions: invoiceOptions, editable: true, suppressSorting: true });
        var invoiceOptions = new TypeAheadOptionsAg();
        invoiceOptions.source = (filter) => this.filterAccounts(filter);
        invoiceOptions.displayField = "name"
        invoiceOptions.dataField = "name";
        invoiceOptions.minLength = 0;
        invoiceOptions.delay = 0;
        invoiceOptions.useScroll = true;
        invoiceOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("groupCompanyAccountName", this.terms["economy.accounting.companygroup.parentaccount"], null, { typeAheadOptions: invoiceOptions, editable: true, suppressSorting: true });
        this.soeGridOptions.addColumnIcon(null, " ", null, { onClick: this.deleteRow.bind(this), icon: "fal fa-times iconDelete", toolTip: this.terms["common.remove"]});

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (row, colDef, newValue, oldValue) => { this.afterCellEdit(row, colDef, newValue, oldValue); }));
        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.finalizeInitGrid();

        if (this.companyGroupMapping.rows && this.companyGroupMapping.rows.length > 0) {
            //Handle account names
            _.forEach(this.companyGroupMapping.rows, (row) => {
                if (row.childAccountFrom) {
                    var accountF = _.find(this.accounts, (a) => a.accountNr === row.childAccountTo.toString());
                    if (accountF)
                        row.childAccountFromName = accountF.accountNr + " - " + accountF.name;
                }
                if (row.childAccountTo) {
                    var accountT = _.find(this.accounts, (a) => a.accountNr === row.childAccountTo.toString());
                    if (accountT)
                        row.childAccountToName = accountT.accountNr + " - " + accountT.name;
                }
                if (row.groupCompanyAccount) {
                    var accountA = _.find(this.accounts, (a) => a.accountNr === row.groupCompanyAccount.toString());
                    if (accountA)
                        row.groupCompanyAccountName = accountA.accountNr + " - " + accountA.name;
                }
            });
            this.setGridData();
        }
    }

    protected filterAccounts(filter) {
        return _.orderBy(this.accountsDict.filter(p => {
            return p.name.contains(filter);
        }), 'name');
    }

    protected allowNavigationFromTypeAhead(value, colDef) {
        if (!value)
            return true;
        var matched = _.some(this.accountsDict, { 'name': value });
        if (matched)
            return true;

        return false;
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue) 
            return;

        var account = _.find(this.accountsDict, (a) => a.name === newValue);
        if (account) {
            switch (colDef.field) {
                case "childAccountFromName":
                    row.childAccountFrom = account.number;
                    break;
                case "childAccountToName":
                    row.childAccountTo = account.number;
                    break;
                case "groupCompanyAccountName":
                    row.groupCompanyAccount = account.number;
                    break;
            }
        }
    }

    protected addRow() {
        var row = new CompanyGroupMappingRowDTO();
        row.isDeleted = false;
        this.companyGroupMapping.rows.push(row);
        this.setGridData();
    }

    protected setGridData() {
        this.soeGridOptions.setData(_.filter(this.companyGroupMapping.rows, {'isDeleted': false}))
    }

    protected deleteRow(row: any) {
        if (row.companyGroupMappingRowId) {
            row.isDeleted = true;
            this.setGridData();
        }
        else {
            var index: number = this.soeGridOptions.getRowIndex(row);
            this.companyGroupMapping.rows.splice(index, 1);
            this.setGridData();
        }
    }

    private save() {
        //Validate rows
        var rowsValid: boolean = true;
        _.forEach(this.companyGroupMapping.rows, (row) => {
            if (!row.childAccountFrom || row.childAccountFrom === 0 || !row.groupCompanyAccount || row.groupCompanyAccount === 0) {
                rowsValid = false;
            }
        });

        if (rowsValid) {
            this.$uibModalInstance.close({ item: this.companyGroupMapping });
        }
        else {
            var modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.accounting.companygroup.validaterowsmessage"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        }
    }

    private delete() {
        this.$uibModalInstance.close({ item: this.companyGroupMapping, delete: true });
    }

    private close() {
        this.$uibModalInstance.close();
    }
}