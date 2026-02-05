import { ICoreService } from "../../../Core/Services/CoreService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { ISysAccountStdDTO } from "../../../Scripts/TypeLite.Net4";
import { AccountEditDTO } from "../../Models/AccountDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { SOEMessageBoxButtons, SOEMessageBoxButton } from "../../../Util/Enumerations";
import { TermGroup } from "../../../Util/CommonEnumerations";

export class AddAccountController {
    private accountNr: string;
    private sysAccount: ISysAccountStdDTO;
    private account: AccountEditDTO;
    private accountTypes: any;
    private sysVatAccounts: any;
    private sysAccountSruCodes: any;

    private searching: boolean = true;
    private sysAccountFound: boolean = false;
    private addingAccount: boolean = false;

    private focusValue: string;

    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        accountNr: string,
        private isFromGrid = false,
        private buttons: SOEMessageBoxButtons = null,
        private initialFocusButton: SOEMessageBoxButton = SOEMessageBoxButton.OK) {

        this.accountNr = accountNr;
        if (this.accountNr.length === 4) {
            this.loadSysAccount();
        } else {
            this.searching = false;
        }

        if (!buttons) {
            buttons = SOEMessageBoxButtons.OK;
        }

        if (isFromGrid) { //the grid tries to steal focus back to itself if you trigger a dialog during edit.
            $uibModalInstance.rendered.then(() => setTimeout(() => {//we use setTimeout since this has nothing to do with angular.
                //Hack, better to send configuration variable instead
                this.setInitialButtonFocus(buttons, initialFocusButton);
            }));
        } else {
            $uibModalInstance.rendered.then(() => {
                this.setInitialButtonFocus(buttons, initialFocusButton);
            });
        }
    }

    private setInitialButtonFocus(buttons: SOEMessageBoxButtons, initialFocusButton: SOEMessageBoxButton) {

        if (initialFocusButton == SOEMessageBoxButton.First) {

            //This code set focus on first element in dialog
            var inputs = angular.element('.messagebox input');
            var focus = null;

            if ((inputs && inputs.length))
                focus = inputs[0];

            if (!focus) {
                var buttonElements = angular.element('.messagebox button');
                if ((buttonElements && buttonElements.length))
                    focus = buttonElements[0];
            }

            if (focus)
                angular.element(focus).focus();

            return;
        }
        switch (initialFocusButton) {
            case SOEMessageBoxButton.OK:
                this.focusValue = "ok";
                break;
            case SOEMessageBoxButton.Cancel:
                this.focusValue = "cancel";
                break;
            case SOEMessageBoxButton.Yes:
                this.focusValue = "yes";
                break;
            case SOEMessageBoxButton.No:
                this.focusValue = "no";
                break;
        }
    }

    private loadSysAccount() {
        this.accountingService.getSysAccountStd(0, this.accountNr).then(x => {
            this.sysAccount = x;
            if (this.sysAccount)
                this.sysAccountFound = true;
            this.searching = false;
        });
    }

    private loadAccountTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountType, false, true).then((x) => {
            this.accountTypes = x;
        });
    }

    private loadSysVatAccounts(): ng.IPromise<any> {
        return this.accountingService.getSysVatAccounts(CoreUtility.sysCountryId, true).then((x) => {
            this.sysVatAccounts = x;
        });
    }

    private loadSysAccountSruCodes(): ng.IPromise<any> {
        return this.accountingService.getSysAccountSruCodes(true).then((x) => {
            this.sysAccountSruCodes = x;
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        var result: any;

        if (!this.sysAccountFound && !this.addingAccount) {
            this.addingAccount = true;
            this.$q.all([
                this.loadAccountTypes(),
                this.loadSysVatAccounts(),
                this.loadSysAccountSruCodes()]).then(() => {
                    this.account = new AccountEditDTO();
                    this.account.accountNr = this.accountNr;
                });

            return;
        } else {
            if (this.sysAccountFound) {
                result = { type: 'copy', data: this.sysAccount };
            } else {
                result = { type: 'new', data: this.account };
            }
        }

        this.$uibModalInstance.close(result);
    }
}