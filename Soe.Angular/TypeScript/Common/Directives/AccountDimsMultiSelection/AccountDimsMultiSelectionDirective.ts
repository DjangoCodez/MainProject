import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";
import { AccDimSelectionDTO, IdListSelectionDTO, AccountDimSelectionDTO } from "../../Models/ReportDataSelectionDTO";
import { AccountDTO } from "../../Models/AccountDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";
import { SmallGenericType } from "../../Models/SmallGenericType";

interface AccountDimSelectModel {
    id: number;
    label: string;
}

export class AccountDimsMultiSelectionDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AccountDimsMultiSelection', 'AccountDimsMultiSelection.html'),
            scope: {
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
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: AccountDimsMultiSelectionController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AccountDimsMultiSelectionController {

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

    // Collections
    private accountDims: AccountDimSmallDTO[];
    private accDim: AccountDimSelectionDTO[];

    private dim1label: string = '';
    private dim2label: string = '';
    private dim3label: string = '';
    private dim4label: string = '';
    private dim5label: string = '';
    private dim6label: string = '';

    private dim1accounts: SmallGenericType[] = [];
    private dim2accounts: SmallGenericType[] = [];
    private dim3accounts: SmallGenericType[] = [];
    private dim4accounts: SmallGenericType[] = [];
    private dim5accounts: SmallGenericType[] = [];
    private dim6accounts: SmallGenericType[] = [];

    //binding properties
    private accounts: AccDimSelectionDTO[] = [];
    private userSelectionInputAccount: IdListSelectionDTO;

    private availableSelectableDims: AccountDimSelectModel[] = [];
    private selectedAccountd: AccountDimSelectModel[] = [];

    // Properties
    private _selectedAccount1: AccountDTO;
    get selectedAccount1() {
        return this._selectedAccount1;
    }
    set selectedAccount1(item: AccountDTO) {
        if (item && item.accountId !== 0) {
            this._selectedAccount1 = item;
            this.account1 = item ? item.accountId : null;
        }
        else {
            this._selectedAccount1 = this.account1 = undefined;
        }
    }

    private _selectedAccount2: AccountDTO;
    get selectedAccount2() {
        return this._selectedAccount2;
    }
    set selectedAccount2(item: AccountDTO) {
            if (item && item.accountId !== 0) {
                this._selectedAccount2 = item;
                this.account2 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount2 = this.account2 = undefined;
            }
    }

    private _selectedAccount3: AccountDTO;
    get selectedAccount3() {
        return this._selectedAccount3;
    }
    set selectedAccount3(item: AccountDTO) {
            if (item && item.accountId !== 0) {
                this._selectedAccount3 = item;
                this.account3 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount3 = this.account3 = undefined;
            }
    }

    private _selectedAccount4: AccountDTO;
    get selectedAccount4() {
        return this._selectedAccount4;
    }
    set selectedAccount4(item: AccountDTO) {
            if (item && item.accountId !== 0) {
                this._selectedAccount4 = item;
                this.account4 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount4 = this.account4 = undefined;
            }
    }

    private _selectedAccount5: AccountDTO;
    get selectedAccount5() {
        return this._selectedAccount5;
    }
    set selectedAccount5(item: AccountDTO) {
            if (item && item.accountId !== 0) {
                this._selectedAccount5 = item;
                this.account5 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount5 = this.account5 = undefined;
            }
    }

    private _selectedAccount6: AccountDTO;
    get selectedAccount6() {
        return this._selectedAccount6;
    }
    set selectedAccount6(item: AccountDTO) {
            if (item && item.accountId !== 0) {
                this._selectedAccount6 = item;
                this.account6 = item ? item.accountId : null;
            }
            else {
                this._selectedAccount6 = this.account6 = undefined;
            }
    }

    //@ngInject
    constructor(private $scope,
        private accountingService: IAccountingService,
        private messagingService: IMessagingService) {

        this.$scope.$on(Constants.EVENT_RELOAD_ACCOUNT_DIMS, (e, a) => {
            this.loadAccounts(false);
        });
    }

    public $onInit() {
        this.loadAccounts(!this.skipCache).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.account1, (newVal, oldVal) => {
            this.selectedAccount1 = this.getAccount(0, this.account1);
        });
        this.$scope.$watch(() => this.account2, (newVal, oldVal) => {
            this.selectedAccount2 = this.getAccount(1, this.account2);
        });
        this.$scope.$watch(() => this.account3, (newVal, oldVal) => {
            this.selectedAccount3 = this.getAccount(2, this.account3);
        });
        this.$scope.$watch(() => this.account4, (newVal, oldVal) => {
            this.selectedAccount4 = this.getAccount(3, this.account4);
        });
        this.$scope.$watch(() => this.account5, (newVal, oldVal) => {
            this.selectedAccount5 = this.getAccount(4, this.account5);
        });
        this.$scope.$watch(() => this.account6, (newVal, oldVal) => {
            this.selectedAccount6 = this.getAccount(5, this.account6);
        });
    }

    private loadAccounts(useCache = true): ng.IPromise<any> {

        return this.accountingService.getAccountDimsSmall(false, this.hideStdDim, true, false, useCache).then(x => {
            this.accountDims = x;

            let i = 0;
            this.accountDims.forEach(ad => {
                i++;

                this[`dim${i}label`] = ad.name;

                // Add empty row
                if (!ad.accounts) {
                    ad.accounts = [];
                }

                _.forEach(ad.accounts, (row) => {
                    this[`dim${i}accounts`].push(new SmallGenericType(row.accountId, row.numberName));
                });
                this[`dim${i}accounts`] = _.sortBy(this[`dim${i}accounts`], (c) => { return c.name; });
            });
        });
    }

    private getAccount(dimIdx: number, accountId: number): AccountDTO {
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

    private onDimSelectionChanged(selection: any, accountDimId: any, selectionAccountKey: string) { 
        if (selection) {
            if (this.accountDims) {
                const accountId = this.accountDims.find((ad) => (ad.name == accountDimId))?.accountDimId;

                this.accDim = [];
                this.accDim.push(new AccountDimSelectionDTO(accountId, selection.ids, selectionAccountKey, 1));
                this.onChange({ selectedValues: this.accDim });
            }
        }
    }
}