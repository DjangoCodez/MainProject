import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";
import { AccountDTO } from "../../Models/AccountDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";
import { SoeOriginStatus, SoeOriginType } from "../../../Util/CommonEnumerations";
import { INgModelController } from "angular";

export class AccountDimsDirectiveFactory {
    private dim2Mandatory: boolean;
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AccountDims', 'AccountDims.html'),
            scope: {
                ngModel: '=',
                hideStdDim: '@',
                account1: '=',
                account2: '=',
                account3: '=',
                account4: '=',
                account5: '=',
                account6: '=',
                isReadonly: '=', 
                parentGuid: '=',
                skipCache: '=',
                addNotUsed: '=?',
                originType: '=?',
                originStatus: '=?',
                onChange: '&',
            },
            restrict: 'E',
            replace: true,
            controller: AccountDimsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AccountDimsController {

    // Init parameters
    private hideStdDim: boolean;
    private onChange: Function;
    private account1: number;
    private account2: number;
    private account3: number;
    private account4: number;
    private account5: number;
    private account6: number;
    private isReadonly: boolean;
    private parentGuid: Guid;
    private skipCache = false;
    private addNotUsed = false;
    private doNotUseTerm;
    private originType?: SoeOriginType;
    private originStatus?: SoeOriginStatus;

    // Collections
    private accountDims: AccountDimSmallDTO[];

    private dim1label: string = '';
    private dim2label: string = '';
    private dim3label: string = '';
    private dim4label: string = '';
    private dim5label: string = '';
    private dim6label: string = '';

    private dim1Mandatory = false;
    private dim2Mandatory = false;
    private dim3Mandatory = false;
    private dim4Mandatory = false;
    private dim5Mandatory = false;
    private dim6Mandatory = false;

    private dim1accounts: AccountDTO[] = [];
    private dim2accounts: AccountDTO[] = [];
    private dim3accounts: AccountDTO[] = [];
    private dim4accounts: AccountDTO[] = [];
    private dim5accounts: AccountDTO[] = [];
    private dim6accounts: AccountDTO[] = [];

    private validationModel;

    // Properties
    private _selectedAccount1: AccountDTO;
    get selectedAccount1() {
        return this._selectedAccount1;
    }
    set selectedAccount1(item: AccountDTO) {
        if (item && item.accountId !== 0) {
            this._selectedAccount1 = item;
            this.account1 = item ? item.accountId : null;
            this.validationModel.account1 = item ? item.accountId : null;
        }
        else {
            this._selectedAccount1 = this.account1 = this.validationModel.account1 = undefined;
        }
    }

    private _selectedAccount2: AccountDTO;
    get selectedAccount2() {
        return this._selectedAccount2;
    }
    set selectedAccount2(item: AccountDTO) {
        //if (item !== undefined) {
            if (item && item.accountId !== 0) {
                this._selectedAccount2 = item;
                this.account2 = item ? item.accountId : null;
                this.validationModel.account2 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount2 = this.account2 = this.validationModel.account2 = undefined;
            }
        //}
    }

    private _selectedAccount3: AccountDTO;
    get selectedAccount3() {
        return this._selectedAccount3;
    }
    set selectedAccount3(item: AccountDTO) {
        //if (item !== undefined) {
            if (item && item.accountId !== 0) {
                this._selectedAccount3 = item;
                this.account3 = item ? item.accountId : null;
                this.validationModel.account3 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount3 = this.account3 = this.validationModel.account3 = undefined;
            }
        //}
    }

    private _selectedAccount4: AccountDTO;
    get selectedAccount4() {
        return this._selectedAccount4;
    }
    set selectedAccount4(item: AccountDTO) {
        //if (item !== undefined) {
            if (item && item.accountId !== 0) {
                this._selectedAccount4 = item;
                this.account4 = item ? item.accountId : null;
                this.validationModel.account4 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount4 = this.account4 = this.validationModel.account4 = undefined;
            }
        //}
    }

    private _selectedAccount5: AccountDTO;
    get selectedAccount5() {
        return this._selectedAccount5;
    }
    set selectedAccount5(item: AccountDTO) {
        //if (item !== undefined) {
            if (item && item.accountId !== 0) {
                this._selectedAccount5 = item;
                this.account5 = item ? item.accountId : null;
                this.validationModel.account5 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount5 = this.account5 = this.validationModel.account5 = undefined;
            }
        //}
    }

    private _selectedAccount6: AccountDTO;
    get selectedAccount6() {
        return this._selectedAccount6;
    }
    set selectedAccount6(item: AccountDTO) {
        //if (item !== undefined) {
            if (item && item.accountId !== 0) {
                this._selectedAccount6 = item;
                this.account6 = item ? item.accountId : null;
                this.validationModel.account6 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount6 = this.account6 = this.validationModel.account6 = undefined;
            }
        //}
    }

    //@ngInject
    constructor(private $scope,
        private accountingService: IAccountingService,
        private messagingService: IMessagingService,
        private translationService: ITranslationService) {

        this.$scope.$on(Constants.EVENT_RELOAD_ACCOUNT_DIMS, (e, a) => {
            this.loadAccounts(false);
        });

        this.validationModel = { account1: undefined, account2: undefined, account3: undefined, account4: undefined, account5: undefined, account6: undefined };
    }

    public $onInit() {
        this.initLoadAccounts(!this.skipCache);
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.account1, (newVal, oldVal) => {
            //if(newVal !== oldVal)
            this.selectedAccount1 = this.getAccount(0, this.account1);
        });
        this.$scope.$watch(() => this.account2, (newVal, oldVal) => {
            //if (newVal !== oldVal)
            this.selectedAccount2 = this.getAccount(1, this.account2);
        });
        this.$scope.$watch(() => this.account3, (newVal, oldVal) => {
            //if (newVal !== oldVal)
            this.selectedAccount3 = this.getAccount(2, this.account3);
        });
        this.$scope.$watch(() => this.account4, (newVal, oldVal) => {
            //if (newVal !== oldVal)
            this.selectedAccount4 = this.getAccount(3, this.account4);
        });
        this.$scope.$watch(() => this.account5, (newVal, oldVal) => {
            //if (newVal !== oldVal)
            this.selectedAccount5 = this.getAccount(4, this.account5);
        });
        this.$scope.$watch(() => this.account6, (newVal, oldVal) => {
            //if (newVal !== oldVal)
            this.selectedAccount6 = this.getAccount(5, this.account6);
        });
        this.$scope.$watch(() => this.originType, (newVal, oldVal) => {
            let i = 1;
            this.accountDims.forEach(ad => {
                if (this.originType)
                    this.setMandatory(ad, i);
                i++;
            });
        });
        this.$scope.$watch(() => this.originStatus, (newVal, oldVal) => {
            let i = 1;
            this.accountDims.forEach(ad => {
                if (this.originType)
                    this.setMandatory(ad, i);
                i++;
            });
        });
    }

    private initLoadAccounts(useCache = true) {
        if (this.addNotUsed) {
            this.translationService.translate("common.donotuse").then((term) => {
                this.doNotUseTerm = term;
                this.loadAccounts(useCache);
            });
        }
        else {
            this.loadAccounts(useCache);
        }
    }

    private loadAccounts(useCache = true): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, this.hideStdDim, true, false, useCache).then(x => {
            this.accountDims = x;

            // Add empty standard dim for placeholder
            if (this.hideStdDim) {
                const newDim = new AccountDimSmallDTO();
                newDim.accountDimId = 0;
                newDim.accountDimNr = 1;
                newDim.name = '';
                newDim.accounts = [];
                this.accountDims.unshift(newDim);
            }

            let i = 0;
            this.accountDims.forEach(ad => {
                i++;
                this[`dim${i}label`] = ad.name;

                if (this.originType)
                    this.setMandatory(ad, i);

                // Add empty row
                if (!ad.accounts)
                    ad.accounts = [];

                if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0) {
                    if (this.addNotUsed)
                        (<any[]>ad.accounts).unshift({ accountId: -1, accountNr: ' ', name: this.doNotUseTerm, numberName: this.doNotUseTerm });

                    (<any[]>ad.accounts).unshift({ accountId: 0, accountNr: ' ', name: ' ', numberName: ' ' });
                }

                this[`dim${i}accounts`] = ad.accounts;
            });

            this.setupWatchers();
        });
    }

    private setMandatory(accountDim: AccountDimSmallDTO, i: number) {
        switch (this.originType) {
            case SoeOriginType.Order:
                this[`dim${i}Mandatory`] = accountDim.mandatoryInOrder;
                break;
            case SoeOriginType.CustomerInvoice:
                this[`dim${i}Mandatory`] = (this.originStatus === SoeOriginStatus.Draft || this.originStatus === SoeOriginStatus.Origin) ? accountDim.mandatoryInCustomerInvoice : false;
                break;
        }
    }

    private getAccount(dimIdx: number, accountId: number): AccountDTO {
        //if (accountId === null)
        //    return null;

        return dimIdx < this.accountDims.length ? _.find(this.accountDims[dimIdx].accounts, { accountId: accountId }) : undefined;
    }

    private selectionChanged(item, dimNr) {
        switch (dimNr) {
            case 1:
                this.selectedAccount1 = item;
                break;
            case 2:
                this.selectedAccount2 = item;
                break;
            case 3:
                this.selectedAccount3 = item;
                break;
            case 4:
                this.selectedAccount4 = item;
                break;
            case 5:
                this.selectedAccount5 = item;
                break;
            case 6:
                this.selectedAccount6 = item;
                break;
        }

        var selectedValues = {
            acct1: this.selectedAccount1,
            acct2: this.selectedAccount2,
            acct3: this.selectedAccount3,
            acct4: this.selectedAccount4,
            acct5: this.selectedAccount5,
            acct6: this.selectedAccount6,
        }

        this.onChange({ selectedValues: selectedValues });
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid });
    }


    public selectedValues() {
        var selectedArray = [this.selectedAccount1, this.selectedAccount2, this.selectedAccount3, this.selectedAccount4, this.selectedAccount5];
        return selectedArray;
    }
}