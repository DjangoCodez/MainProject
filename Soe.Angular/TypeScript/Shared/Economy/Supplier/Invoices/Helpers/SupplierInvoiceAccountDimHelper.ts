import { TermGroup_BillingType } from "../../../../../Util/CommonEnumerations";
import { SupplierInvoiceDTO } from "../../../../../Common/Models/InvoiceDTO";


export class SupplierInvoiceAccountDimHelper {

    public useInternalAccountWithBalanceSheetAccounts: boolean = false;

    // Default account dimensions
    private localLoading: boolean = false;
    private _defaultAccountDim2Id: number;
    get defaultAccountDim2Id(): number {
        return this._defaultAccountDim2Id;
    }
    set defaultAccountDim2Id(item: number) {
        var previousDimId = this._defaultAccountDim2Id;
        this._defaultAccountDim2Id = item;

        this.setVatDeduction(2, this._defaultAccountDim2Id);
        if (!this.loadingInvoice() && this.invoice()) {
            this.invoice().defaultDim2AccountId = item;
            if (!this.isLocked()) {
                this.updateAccountRowDimAccounts(2, this.invoice().defaultDim2AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim3Id: number;
    get defaultAccountDim3Id(): number {
        return this._defaultAccountDim3Id;
    }
    set defaultAccountDim3Id(item: number) {
        var previousDimId = this._defaultAccountDim3Id;
        this._defaultAccountDim3Id = item;
        this.setVatDeduction(3, this._defaultAccountDim3Id);
        if (!this.loadingInvoice() && this.invoice()) {
            this.invoice().defaultDim3AccountId = item;
            if (!this.isLocked()) {
                this.updateAccountRowDimAccounts(3, this.invoice().defaultDim3AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim4Id: number;
    get defaultAccountDim4Id(): number {
        return this._defaultAccountDim4Id;
    }
    set defaultAccountDim4Id(item: number) {
        var previousDimId = this._defaultAccountDim4Id;
        this._defaultAccountDim4Id = item;
        this.setVatDeduction(4, this._defaultAccountDim4Id);
        if (!this.loadingInvoice() && this.invoice()) {
            this.invoice().defaultDim4AccountId = item;
            if (!this.isLocked()) {
                this.updateAccountRowDimAccounts(4, this.invoice().defaultDim4AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim5Id: number;
    get defaultAccountDim5Id(): number {
        return this._defaultAccountDim5Id;
    }
    set defaultAccountDim5Id(item: number) {
        var previousDimId = this._defaultAccountDim5Id;
        this._defaultAccountDim5Id = item;
        this.setVatDeduction(5, this._defaultAccountDim5Id);
        if (!this.loadingInvoice() && this.invoice()) {
            this.invoice().defaultDim5AccountId = item;
            if (!this.isLocked()) {
                this.updateAccountRowDimAccounts(5, this.invoice().defaultDim5AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim6Id: number;
    get defaultAccountDim6Id(): number {
        return this._defaultAccountDim6Id;
    }
    set defaultAccountDim6Id(item: number) {
        var previousDimId = this._defaultAccountDim6Id;
        this._defaultAccountDim6Id = item;
        this.setVatDeduction(6, this._defaultAccountDim6Id);
        if (!this.loadingInvoice() && this.invoice()) {
            this.invoice().defaultDim6AccountId = item;
            if (!this.isLocked()) {
                this.updateAccountRowDimAccounts(6, this.invoice().defaultDim6AccountId, previousDimId);
            }
        }
    }

    private setVatDeduction(dim: number, accountId: number) {
        if (this.setVatDeductionCallback) {
            this.setVatDeductionCallback(dim, accountId);
        }
    }

    private updateAccountRowDimAccounts(dimNumber: number, accountId: number, previousDimNr) {
        if (!this.loadingInvoice() && !this.localLoading) {
            const invoice = this.invoice();
            
            _.forEach(invoice.accountingRows, (accRow) => {
                if ((this.useInternalAccountWithBalanceSheetAccounts || (!accRow.isVatRow && !accRow.isHouseholdRow && !accRow.isCentRoundingRow)) && !accRow.isDeleted) {
                    if (this.useInternalAccountWithBalanceSheetAccounts ||
                        ((invoice.billingType === TermGroup_BillingType.Credit && accRow.isCreditRow === true) || (invoice.billingType == TermGroup_BillingType.Debit && accRow.isDebitRow))
                    ) {
                        switch (dimNumber) {
                            case 2:
                                if (!accRow.dim2Disabled && ((!accRow.dim2Id) || (accRow.dim2Id == previousDimNr))) {
                                    accRow.dim2Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 3:
                                if (!accRow.dim3Disabled && ((!accRow.dim3Id) || (accRow.dim3Id == previousDimNr))) {
                                    accRow.dim3Id = accountId ? accountId : 0;
                                    console.log("dimChanged","accRow.dim3Id");
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 4:
                                if (!accRow.dim4Disabled && ((!accRow.dim4Id) || (accRow.dim4Id == previousDimNr))) {
                                    accRow.dim4Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 5:
                                if (!accRow.dim5Disabled && ((!accRow.dim5Id) || (accRow.dim5Id == previousDimNr))) {
                                    accRow.dim5Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 6:
                                if (!accRow.dim6Disabled && ((!accRow.dim6Id) || (accRow.dim6Id == previousDimNr))) {
                                    accRow.dim6Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                        }
                    }
                }
            });
        }
    }

    //@ngInject
    constructor(
        private $scope,
        private invoice: () => SupplierInvoiceDTO,
        private setVatDeductionCallback: (dim: number, accountId: number) => void,
        private isLocked: () => boolean,
        private loadingInvoice: () => boolean
    ) {

    }

    public getdDimIdValues(): { [index: string]: string } {
        var result: { [index: string]: string } = {};

        result["dim2Id"] = this.defaultAccountDim2Id ? this.defaultAccountDim2Id.toString() : "" ;
        result["dim3Id"] = this.defaultAccountDim3Id ? this.defaultAccountDim3Id.toString() : "" ;
        result["dim4Id"] = this.defaultAccountDim4Id ? this.defaultAccountDim4Id.toString() : "" ;
        result["dim5Id"] = this.defaultAccountDim5Id ? this.defaultAccountDim5Id.toString() : "" ;
        result["dim6Id"] = this.defaultAccountDim6Id ? this.defaultAccountDim6Id.toString() : "" ;

        return result;
    }

    public setDimIdValues(invoice: SupplierInvoiceDTO) {
        this.localLoading = true;
        this.defaultAccountDim2Id = invoice.defaultDim2AccountId;
        this.defaultAccountDim3Id = invoice.defaultDim3AccountId;
        this.defaultAccountDim4Id = invoice.defaultDim4AccountId;
        this.defaultAccountDim5Id = invoice.defaultDim5AccountId;
        this.defaultAccountDim5Id = invoice.defaultDim5AccountId;
        this.localLoading = false;
    }
}