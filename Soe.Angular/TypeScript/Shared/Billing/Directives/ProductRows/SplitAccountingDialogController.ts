import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { IAccountDTO } from "../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../Common/Models/smallgenerictype";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { SplitAccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeInvoiceRowDiscountType } from "../../../../Util/CommonEnumerations";
import { AccountDimSmallDTO } from "../../../../Common/Models/AccountDimDTO";
import { CoreUtility } from "../../../../Util/CoreUtility";


export class SplitAccountingDialogController {

    // Terms
    private terms: any;

    // Lookups
    private accountDims: AccountDimSmallDTO[];
    private splitTypes: SmallGenericType[];

    // Properties
    private splitAmount: number;
    private diffAmount: number;
    private hasDiff: boolean;

    private splitType: SmallGenericType;

    // GUI
    private toolbarInclude: any;
    private modalInstance: any;
    private steppingRules: any;
    private buttonGroups: any[];
    private currentlyEditing: any;

    public soeGridOptions: ISoeGridOptionsAg;

    private templateRow: SplitAccountingRowDTO;

    private editForm: ng.IFormController;

    //@ngInject
    constructor(
        protected $timeout: ng.ITimeoutService,
        //private $uibModal: ng.
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        protected uiGridConstants: uiGrid.IUiGridConstants,
        protected coreService: ICoreService,
        protected translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private isReadonly: boolean,
        private accountingRows: SplitAccountingRowDTO[],
        private productRowAmount: number,
        private isCredit: boolean,
        private multipleRowSplit: boolean,
        private negativeRow: boolean) {
    }

    // SETUP
    private $onInit() {
        this.toolbarInclude = this.urlHelperService.getGlobalUrl("Shared/Billing/Directives/ProductRows/Views/SplitAccountingDialogGridHeader.html");
        this.soeGridOptions = new SoeGridOptionsAg("SplitAccountingRows", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(8);
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.customTabToCellHandler = (params) => {
            this.$timeout(() => {
                if (this.diffAmount !== 0)
                    this.addRow(true);
            }, 150);
            return this.handleNavigateToNextCell(params);
        };
        this.setupSteppingRules();

        //this.soeGridOptions.ste
        this.$q.all([
            this.loadAccounts(true),
            this.loadTerms()])
            .then(() => {
                this.setupSplitTypes();
                this.setupGridColumns();
                this.setupCustomToolBar();
                this.setupRows();
                this.updateGridData();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.productrows.splitaccounting.splitvalue",
            "common.amount",
            "common.percentage",
            "billing.project.central.outcome",
            "core.delete",
            "common.accountingrows.missingaccount",
            "common.accountingrows.invalidaccount",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        })

    }
    private setupSplitTypes() {
        this.splitTypes = [];
        this.splitTypes.push(new SmallGenericType(SoeInvoiceRowDiscountType.Percent, "%"));
        this.splitTypes.push(new SmallGenericType(SoeInvoiceRowDiscountType.Amount, this.terms["common.amount"]));
        this.splitType = this.splitTypes[0];
    }

    protected setupCustomToolBar() {
        this.buttonGroups = [];
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.newrow", "core.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
            this.addRow(true);
            //this.soeGridOptions.focusRowByRow(row, 0);
        }, null, () => { return this.isReadonly })));
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("billing.productrows.splitaccounting.spliteven", "billing.productrows.splitaccounting.spliteven", IconLibrary.FontAwesome, "fa-balance-scale", () => {
            this.splitEven();
        }, null, () => { return this.isReadonly })));
    }

    private setupGridColumns() {

        this.accountDims.forEach((ad, i) => {
            let index = i + 1;

            const field = "dim" + index + "Nr";
            const secondRowField = "dim" + index + "Name";
            const errorField = "dim" + index + "Error";
            const editable = (data) => {
                const disabled = data["dim" + index + "Disabled"] || this.isReadonly || (index === 1 && data.excludeFromSplit);

                return !disabled;
            };


            const onCellChanged = ({ data }) => {
                this.onAccountingDimChanged(data, index)
            }

            const allowNavigateFrom = (value, data) => {
                return this.allowNavigationFromTypeAhead(value, data, index);
            };

            const col = this.soeGridOptions.addColumnTypeAhead(field, ad.name, null, {
                editable: editable.bind(this),
                error: errorField,
                secondRow: secondRowField,
                onChanged: onCellChanged,
                typeAheadOptions: {
                    source: (filter) => this.filterAccounts(i, filter),
                    updater: null,
                    displayField: "numberName",
                    dataField: "accountNr",
                    minLength: 0,
                    allowNavigationFromTypeAhead: allowNavigateFrom.bind(this)
                }
            }, {
                dimIndex: index,
            });
        });

        let splitCol = this.soeGridOptions.addColumnNumber("splitValue", this.terms["billing.productrows.splitaccounting.splitvalue"], null, { decimals: 2, editable: (data) => data && !data.excludeFromSplit && !this.isReadonly, onChanged: this.calculateRowSum.bind(this) });
        splitCol.headerValueGetter = () => {
            if (this.splitType.id === SoeInvoiceRowDiscountType.Amount)
                return this.terms["common.amount"];
            else
                return this.terms["common.percentage"];
        };
        this.soeGridOptions.addColumnNumber("amountCurrency", this.terms["billing.project.central.outcome"], null, { decimals: 2, maxDecimals: 2, editable: false /*(data) => data && !data.excludeFromSplit*/ });

        if (!this.isReadonly)
            this.soeGridOptions.addColumnDelete(this.terms["core.delete"], this.deleteRow.bind(this), null, (data) => data && !data.excludeFromSplit);

        // Grid events
        const event: GridEvent = new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (rowEntity, colDef, newValue, oldValue) => {
            if (colDef.field === 'splitValue') {
                this.calculateRowSum(rowEntity);
                this.updateGridData();
            }
            this.validateAccountingRow(rowEntity);
        });
        this.soeGridOptions.subscribe([event]);
        this.soeGridOptions.finalizeInitGrid();
    }

    private setupSteppingRules() {
        const mappings =
            {
                dim1Nr(row: SplitAccountingRowDTO) { return row.dim1Stop || row.dim1Mandatory },
                dim2Nr(row: SplitAccountingRowDTO) { return row.dim2Stop || row.dim2Mandatory },
                dim3Nr(row: SplitAccountingRowDTO) { return row.dim3Stop || row.dim3Mandatory },
                dim4Nr(row: SplitAccountingRowDTO) { return row.dim4Stop || row.dim4Mandatory },
                dim5Nr(row: SplitAccountingRowDTO) { return row.dim5Stop || row.dim5Mandatory },
                dim6Nr(row: SplitAccountingRowDTO) { return row.dim6Stop || row.dim6Mandatory },
                splitValue(row: SplitAccountingRowDTO) { return true },
            };

        this.steppingRules = mappings;
    }

    private setupRows() {
        if (!this.accountingRows)
            this.accountingRows = [];

        if (this.accountingRows.length > 0)
            this.templateRow = this.accountingRows[0];
        // Use split type from first row, or percent as default if no rows
        var splitTypeId = this.accountingRows.length > 0 ? this.accountingRows[0].splitType : SoeInvoiceRowDiscountType.Percent;
        if (!splitTypeId)
            this.splitType = this.splitTypes[0];
        else {
            this.splitType = _.find(this.splitTypes, t => t.id === splitTypeId);
            if (this.splitType.id == SoeInvoiceRowDiscountType.Amount)
                this.splitTypeChanged()
        }

        // Fix to always work with positive numbers
        this.productRowAmount = Math.abs(this.productRowAmount);

        _.forEach(this.accountingRows, row => {
            row.splitType = this.splitType.id;
            row.amountCurrency = Math.abs(row.amountCurrency);

            // Only one row, set amount to full
            if (this.accountingRows.length === 1) {
                row.splitValue = (row.splitType == SoeInvoiceRowDiscountType.Percent ? 100 : this.productRowAmount);
                this.calculateRowSum(row);
            }
            this.validateAccountingRow(row);
        });
    }

    // LOOKUPS

    private loadAccounts(useCache: boolean): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, false, true, true, false, false, useCache).then(x => {
            this.accountDims = x;

            this.accountDims.forEach(ad => {
                if (!ad.accounts)
                    ad.accounts = [];

                if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0)
                    (<any[]>ad.accounts).unshift({ accountId: 0, accountNr: '', name: '', numberName: ' ' });
            });
            this.updateMandatoryAndAccountNamesOnAllRows();
            this.setRowItemAccountsOnAllRows(false);
        });
    }


    // ACTIONS

    private addRow(setFocusManually = false): SplitAccountingRowDTO {
        // Get last row
        var lastRow = this.accountingRows.length > 0 ? this.accountingRows[this.accountingRows.length - 1] : this.templateRow;
        if (!lastRow)
            return;

        // Place the remaining amount (diff) on the new row
        var diff = this.diffAmount;
        if (diff < 0)
            diff = 0;
        var percent = 0;
        var value = 0;

        if (diff > 0 && this.productRowAmount > 0) {
            percent = (diff / this.productRowAmount * 100);
            value = (this.splitType.id == SoeInvoiceRowDiscountType.Amount ? diff : percent);
        }

        // Copy account from last row
        var row = new SplitAccountingRowDTO();
        row = CoreUtility.cloneDTO(lastRow);
        row.splitType = this.splitType.id;
        row.splitValue = value;
        row.splitPercent = percent;
        row.amountCurrency = Math.abs(diff);

        if (this.isCredit) {
            row.creditAmountCurrency = this.negativeRow ? diff : 0;
            row.debitAmountCurrency = this.negativeRow ? 0 : diff;
            row.isCreditRow = !this.negativeRow;
            row.isDebitRow = this.negativeRow;
        }
        else {
            row.creditAmountCurrency = !this.negativeRow ? row.amountCurrency : 0;
            row.debitAmountCurrency = !this.negativeRow ? 0 : row.amountCurrency;
            row.isCreditRow = !this.negativeRow;
            row.isDebitRow = this.negativeRow;
        }

        this.validateAccountingRow(row);
        this.accountingRows.push(row);

        this.updateGridData();
        if (setFocusManually) {
            this.soeGridOptions.startEditingCell(row, this.getDim1Column());
        }
    }

    private splitEven() {
        this.updateGridData();
        if (this.accountingRows.length === 0)
            return;

        var splitValue: number = 0;
        var left: number = 0;
        if (this.splitType.id == SoeInvoiceRowDiscountType.Percent) {
            splitValue = (100 / this.accountingRows.length);
            left = 100;
        } else {
            splitValue = (this.productRowAmount / this.accountingRows.length);
            left = this.productRowAmount;
        }

        splitValue = splitValue.round(2);

        for (let i = 0; i < this.accountingRows.length; i++) {
            if (!this.accountingRows[i].excludeFromSplit) {
                this.accountingRows[i].splitValue = (i === this.accountingRows.length - 1) ? left : splitValue;
                left -= splitValue;
            }
        }

        this.recalculateAllRows();
    }

    private deleteRow(row: SplitAccountingRowDTO) {
        var index: number = this.accountingRows.indexOf(row);
        this.accountingRows.splice(index, 1);
        this.updateGridData();
    }

    private ok() {
        this.removeZeroRows();
        if (this.validate()) {
            this.$uibModalInstance.close(this.accountingRows);
        }
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // EVENTS

    private splitTypeChanged() {
        this.$timeout(() => {
            this.recalculateAllRows(this.splitType.id);
        });
    }

    // HELP-METHODS

    private removeZeroRows() {
        var l = this.accountingRows.length;
        for (var i = 0; i < l; i++) {
            if (this.accountingRows[i] && this.accountingRows[i].amountCurrency == 0) {
                this.accountingRows.splice(i, 1);
                i--;
            }
        }
        this.updateGridData();
    }

    protected getSecondRowBindingValue(entity, colDef) {
        const acc = this.findAccount(entity, colDef);
        return acc ? acc.name : null;
    }

    protected allowNavigationFromTypeAhead(value, entity, dimIndex) {

        var currentValue = value; 

        if (!currentValue) 
            return true;
        const valueHasMatchingAccount = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.accountNr === currentValue);
        if (valueHasMatchingAccount.length)
            return true;

        return false;
    }

    public filterAccounts(dimIndex, filter) {
        return _.orderBy(this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter);
        }), 'accountNr');
    }

    private onBlur(entity, colDef) {
        var acc = this.findAccount(entity, colDef);
        entity['dim' + colDef.soeData.additionalData.dimIndex + 'Id'] = acc ? acc.accountId : 0;

        if (colDef.soeData.additionalData.dimIndex === 1) {
            this.setRowItemAccounts(entity, acc, false);
        }
    }

    private findAccount(entity, dimIndex) {
        var nrToFind = entity['dim' + dimIndex + 'Nr'];

        if (!nrToFind)
            return null;

        var found = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.accountNr === nrToFind);
        return found.length ? found[0] : null;
    }

    private getAccount(row: SplitAccountingRowDTO, dimIndex: number): IAccountDTO {
        var account = null;
        if (this.accountDims && this.accountDims[dimIndex - 1])
            account = _.find(this.accountDims[dimIndex - 1].accounts, (acc: IAccountDTO) => acc.accountId === row['dim' + dimIndex + 'Id']);

        return account;
    }

    private updateMandatoryAndAccountNamesOnAllRows() {
        this.accountingRows.forEach(ar => {
            if (ar.dim1Id) {
                this.updateMandatoryAndAccountNames(ar, this.getAccount(ar, 1), true, true);
            }
        });
    }

    private updateMandatoryAndAccountNames(row: SplitAccountingRowDTO, account: IAccountDTO, isSalesRow: boolean, setInternalAccount: boolean) {
        // Set standard account
        row.dim1Id = account != null ? account.accountId : 0;
        row.dim1Nr = account != null ? account.accountNr : '';
        row.dim1Name = account != null ? account.name : '';
        row.dim1Disabled = false;
        row.dim1Mandatory = true;
        row.dim1Stop = true;

        // Set internal accounts
        if (account != null) {

            this.accountDims.forEach((accDim, i) => {
                var index = i + 1;
                var acc = _.find(accDim.accounts, acc => acc.accountId === row['dim' + index + 'Id']);
                if (acc) {
                    row['dim' + index + 'Nr'] = acc.accountNr;
                    row['dim' + index + 'Name'] = acc.name;
                }
            });

            if (account.accountInternals != null) {
                account.accountInternals.forEach(ai => {
                    var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based

                    row['dim' + index + 'Disabled'] = ai.mandatoryLevel === 1;
                    row['dim' + index + 'Mandatory'] = ai.mandatoryLevel === 2;
                    row['dim' + index + 'Stop'] = ai.mandatoryLevel === 3;
                });
            }
        }
    }

    private setRowItemAccountsOnAllRows(setInternalAccountFromAccount: boolean) {
        if (this.accountDims && this.accountingRows) {
            _.forEach(this.accountingRows, row => {
                this.setRowItemAccountsOnRow(row, setInternalAccountFromAccount);
            });
        }
    }

    private setRowItemAccountsOnRow(row: SplitAccountingRowDTO, setInternalAccountFromAccount: boolean) {
        if (row.dim1Id) {
            this.setRowItemAccounts(row, this.getAccount(row, 1), setInternalAccountFromAccount);
        }
        else {
            this.setRowItemAccounts(row, null, true);
        }
    }

    private onAccountingDimChanged(data, dimIndex) {
        var acc = this.findAccount(data, dimIndex)

        data['dim' + dimIndex + 'Id'] = acc ? acc.accountId : 0;
        data['dim' + dimIndex + 'Name'] = acc ? acc.name : "";


        if (dimIndex === 1) {
            this.setRowItemAccounts(data, acc, true);
        }

        this.updateGridData();
    }

    private setRowItemAccounts(row: SplitAccountingRowDTO, account: IAccountDTO, setInternalAccountFromAccount: boolean) {
        // Set standard account

        row.dim1Id = account != null ? account.accountId : 0;
        row.dim1Nr = account != null ? account.accountNr : '';
        row.dim1Name = account != null ? account.name : '';
        row.dim1Disabled = false;
        row.dim1Mandatory = true;
        row.dim1Stop = true;

        if (setInternalAccountFromAccount) {
            // Clear internal accounts
            row.dim2Id = 0;
            row.dim2Nr = '';
            row.dim2Name = '';
            row.dim2Disabled = false;
            row.dim2Mandatory = false;
            row.dim2Stop = false;
            row.dim3Id = 0;
            row.dim3Nr = '';
            row.dim3Name = '';
            row.dim3Disabled = false;
            row.dim3Mandatory = false;
            row.dim3Stop = false;
            row.dim4Id = 0;
            row.dim4Nr = '';
            row.dim4Name = '';
            row.dim4Disabled = false;
            row.dim4Mandatory = false;
            row.dim4Stop = false;
            row.dim5Id = 0;
            row.dim5Nr = '';
            row.dim5Name = '';
            row.dim5Disabled = false;
            row.dim5Mandatory = false;
            row.dim5Stop = false;
            row.dim6Id = 0;
            row.dim6Nr = '';
            row.dim6Name = '';
            row.dim6Disabled = false;
            row.dim6Mandatory = false;
            row.dim6Stop = false;

            // Set internal accounts
            if (account != null && account.accountInternals != null) {
                // Get internal accounts from the account
                account.accountInternals.forEach(ai => {
                    if (ai.accountDimNr > 1) {
                        var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based

                        row[`dim${index}Id`] = ai.accountId || 0;
                        row[`dim${index}Nr`] = ai.accountNr || '';
                        row[`dim${index}Name`] = ai.name || '';
                        row[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
                        row[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
                        row[`dim${index}Stop`] = ai.mandatoryLevel === 3;
                    }
                });
            }
        } else {
            // Keep internal accounts, just set number and names
            var index = 1;
            _.forEach(_.filter(this.accountDims, d => d.accountDimNr !== 1), dim => {
                index++;
                var account = _.find(dim.accounts, a => a.accountId === row[`dim${index}Id`]);
                row[`dim${index}Nr`] = account ? account.accountNr : '';
                row[`dim${index}Name`] = account ? account.name : '';
            });
        }
    }

    private calculateRowSum(item: any, changeTo: SoeInvoiceRowDiscountType = SoeInvoiceRowDiscountType.Unknown, handleRest = false, total: number = undefined) {
        if (!item)
            return;

        var updateData = false;
        var row: SplitAccountingRowDTO;

        if (item.soeData) {
            row = item.soeData;
            updateData = true;
        }
        else {
            row = item;
        }
        //Fix splitvalue
        row.splitValue = NumberUtility.parseDecimal(row.splitValue.toString()).round(2);

        if (changeTo === SoeInvoiceRowDiscountType.Amount) {
            row.splitPercent = (row.amountCurrency / this.productRowAmount) * 100;
            row.splitValue = ((row.splitPercent * this.productRowAmount) / 100).round(2);
            row.amountCurrency = row.splitValue;
        }
        else if (changeTo === SoeInvoiceRowDiscountType.Percent) {
            row.splitPercent = ((row.amountCurrency / this.productRowAmount) * 100).round(2);
            row.splitValue = row.splitPercent
        }
        else {

            row.splitType = this.splitType.id;
            row.splitPercent = 0;
            row.amountCurrency = 0;

            if (this.productRowAmount) {
                if (this.splitType.id == SoeInvoiceRowDiscountType.Amount) {
                    row.splitPercent = ((row.splitValue / this.productRowAmount) * 100).round(2);
                    row.amountCurrency = handleRest && total ? (this.productRowAmount - total).round(2) : row.splitValue;
                } else if (this.splitType.id == SoeInvoiceRowDiscountType.Percent) {
                    row.splitPercent = row.splitValue;
                    row.amountCurrency = handleRest && total ? (this.productRowAmount - total).round(2) : (this.productRowAmount * row.splitValue / 100).round(2);
                }
            }
        }

        if (this.isCredit) {
            row.creditAmountCurrency = this.negativeRow ? row.amountCurrency : 0;
            row.debitAmountCurrency = this.negativeRow ? 0 : row.amountCurrency;
            row.isCreditRow = !this.negativeRow;
            row.isDebitRow = this.negativeRow;
        }
        else {
            row.creditAmountCurrency = !this.negativeRow ? row.amountCurrency : 0;
            row.debitAmountCurrency = !this.negativeRow ? 0 : row.amountCurrency;
            row.isCreditRow = !this.negativeRow;
            row.isDebitRow = this.negativeRow;
        }

        if (updateData) {
            this.updateGridData();
        }
    }

    private updateGridData() {
        this.soeGridOptions.setData(this.accountingRows);
        this.updateSums();
    }

    private getDim1Column() {
        return this.soeGridOptions.getColumnByField('dim1Nr');
    }

    private handleNavigateToNextCell(params: any, skipChecks?: boolean): { rowIndex: number, column: any, addRow: boolean } {
        const { nextCellPosition, previousCellPosition, backwards } = params;

        let { rowIndex, column } = nextCellPosition;
        let gridRow = this.soeGridOptions.getVisibleRowByIndex(rowIndex);

        if (!gridRow)
            return;

        let row: SplitAccountingRowDTO = gridRow.data;

        if (rowIndex > 0 && previousCellPosition.field === this.getDim1Column().field) {
        }

        while (!!column && !!this.steppingRules) {
            const { colDef } = column;
            if (this.soeGridOptions.isCellEditable(row, colDef)) {
                const steppingRule = this.steppingRules[colDef.field];
                const stop = !!steppingRule ? steppingRule.call(this, row) : false;

                if (stop) {
                    return { rowIndex, column, addRow: true };
                }
            }
            column = backwards === true ? this.soeGridOptions.getPreviousVisibleColumn(column) : this.soeGridOptions.getNextVisibleColumn(column);
        }

        const nextRowResult = backwards ? this.soeGridOptions.getPreviousRow(row) : this.soeGridOptions.getNextRow(row);
        if (nextRowResult && nextRowResult.rowIndex === this.accountingRows.length) {
            return { rowIndex: nextRowResult.rowIndex, column: this.getDim1Column(), addRow: true };
        }
        else {
            return { rowIndex: nextRowResult.rowIndex, column: this.getDim1Column(), addRow: false };
        }
    }

    private recalculateAllRows(changeTo: SoeInvoiceRowDiscountType = SoeInvoiceRowDiscountType.Unknown) {
        var handleCount = 1;
        var total = 0;
        _.forEach(_.filter(this.accountingRows, (r) => !r.excludeFromSplit), row => {
            this.calculateRowSum(row, changeTo, handleCount === (this.accountingRows.length), total);
            total += row.amountCurrency;
            handleCount++;
        });
        this.soeGridOptions.refreshColumns();
        this.updateGridData();
    }

    private validate(): boolean {
        var errors = this['editForm'].$error;
        var keys: string[] = [];

        if (errors['accountStandard']) {
            keys.push("economy.accounting.voucher.accountstandardmissing");
        }
        if (errors['accountInternal']) {
            keys.push("economy.accounting.voucher.accountinternalmissing");
        }

        if (keys.length > 0) {
            keys.push("error.unabletosave_title");
            this.translationService.translateMany(keys).then((terms) => {
                var message: string = "";
                _.forEach(terms, term => {
                    if (term !== terms["error.unabletosave_title"])
                        message += term + ".\\n";
                });

                this.notificationService.showDialog(terms["error.unabletosave_title"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
            return false;
        }

        return true;
    }

    private updateSums() {
        let sAmount = 0;
        _.forEach(this.accountingRows, (row) => {

            /*if (!this.isCredit && row.isDebitRow)
                sAmount += -row.amountCurrency;
            else
                sAmount += row.amountCurrency;*/


            sAmount += row.amountCurrency;
        });
        this.splitAmount = sAmount.round(2);
        this.diffAmount = this.productRowAmount - this.splitAmount;
        this.hasDiff = this.diffAmount !== 0;
    }

    private validateAccountingRow(row: SplitAccountingRowDTO) {
        if (this.accountDims) {
            this.accountDims.forEach((ad, i) => {
                var prop = 'dim' + (i + 1) + 'Nr';
                var val = row[prop];
                var mandatory = row['dim' + (i + 1) + 'Mandatory'];

                if (!val && mandatory) {
                    row['dim' + (i + 1) + 'Error'] = this.terms["common.accountingrows.missingaccount"];
                } else if (val && !row['dim' + (i + 1) + 'Name']) { //no name means we couldnt find the account name, which means this is invalid.
                    row['dim' + (i + 1) + 'Error'] = this.terms["common.accountingrows.invalidaccount"];
                } else {
                    row['dim' + (i + 1) + 'Error'] = null;
                }
            });
        }
    }
}
