import { AttestPayrollTransactionDTO } from "../../../../../Common/Models/AttestPayrollTransactionDTO";
import { NotificationService } from "../../../../../Core/Services/NotificationService";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { AccountingSettingsRowDTO } from "../../../../../Common/Models/AccountingSettingsRowDTO";
import { IQService } from "angular";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { ITimeService } from "../../../../Time/TimeService";
import { ProductAccountType } from "../../../../../Util/CommonEnumerations";
import { IAccountingPrioDTO } from "../../../../../Scripts/TypeLite.Net4";

export class TimePayrollTransactionDialogController {

    private trans: AttestPayrollTransactionDTO;
    private isNew: boolean;

    private settingTypes: SmallGenericType[];
    private settings: AccountingSettingsRowDTO[];
    private isAccountingSettingsValid: boolean = true;

    private updateQuantityOnChildren: boolean = false;

    //@ngInject
    constructor(
        private $q: IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: TranslationService,
        private notificationService: NotificationService,
        private timeService: ITimeService,
        private payrollProducts: SmallGenericType[],
        private employeeId: number,
        private date: Date,
        trans: AttestPayrollTransactionDTO) {

        this.isNew = !trans;

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.trans = new AttestPayrollTransactionDTO();
            if (trans) {
                angular.extend(this.trans, trans);
                this.trans.accountingSettings = trans.accountingSettings;
            }

            if (this.isNew)
                this.new()
            else
                this.populate();
        });
    }

    // SETUP

    private new() {
        this.trans.manuallyAdded = true;
        this.setAccounting();
    }

    private populate() {
        this.setAccounting();
    }

    private setAccounting() {
        if (!this.trans.accountingSettings)
            this.trans.accountingSettings = [];

        if (this.trans.accountingSettings.length === 0) {
            this.trans.accountingSettings.push(new AccountingSettingsRowDTO(1));
            var accountingSetting = this.trans.accountingSettings[0];

            // Standard account
            if (this.trans.accountStdId) {
                accountingSetting.accountDim1Nr = this.trans.accountStd ? this.trans.accountStd.accountDimNr : 1;
                accountingSetting.account1Id = this.trans.accountStdId;
                accountingSetting.account1Nr = this.trans.accountStd ? this.trans.accountStd.accountNr : '';
                accountingSetting.account1Name = this.trans.accountStd ? this.trans.accountStd.name : '';
            }

            // Internal accounts
            if (this.trans.accountInternals) {
                var orderedAccountInternals = _.orderBy(this.trans.accountInternals, ['accountDimNr'], ['asc']);

                orderedAccountInternals.forEach((accInt, index) => {
                    accountingSetting[`accountDim${index + 2}Nr`] = accInt.accountDimNr;
                    accountingSetting[`account${index + 2}Id`] = accInt.accountId;
                    accountingSetting[`account${index + 2}Nr`] = accInt.accountNr;
                    accountingSetting[`account${index + 2}Name`] = accInt.name;
                });
            }
        }

        this.settings = this.trans.accountingSettings;
    }

    // SERVICE CALLS

    private loadTerms() {
        var keys: string[] = [
            "common.accountingsettings.account",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.settingTypes = [];
            this.settingTypes.push(new SmallGenericType(1, terms["common.accountingsettings.account"]));
        });
    }

    private loadProductAccounts(): ng.IPromise<any> {
        return this.timeService.getPayrollProductAccount(ProductAccountType.Purchase, this.employeeId, this.trans.payrollProductId, 0, 0, true, this.date).then(x => {
            this.setProductAccounts(x);
        });
    }

    // EVENTS

    private payrollProductChanged() {
        this.$timeout(() => {
            this.loadProductAccounts();
        });
    }

    private quantityChanged() {
        this.$timeout(() => {
            if (this.trans.isPayrollProductChainMainParent && this.trans.manuallyAdded) {
                var keys: string[] = [
                    "core.verifyquestion",
                    "time.time.attest.transaction.changequantityforchildren"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    var modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["time.time.attest.transaction.changequantityforchildren"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                    modal.result.then(val => {
                        this.updateQuantityOnChildren = val;
                    });
                });
            }
        });
    }

    // VALIDATION

    private isValid(): boolean {
        if (!this.trans.payrollProductId)
            return false;

        if (!this.isAccountingSettingsValid)
            return false;

        return true;
    }

    // ACTIONS

    private cancel() {
        this.$uibModalInstance.close();
    }

    private delete() {
        if (this.trans.isPayrollProductChainMainParent && this.trans.manuallyAdded) {
            var keys: string[] = [
                "core.verifyquestion",
                "time.time.attest.transaction.deletechildren"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["time.time.attest.transaction.deletechildren"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    this.$uibModalInstance.close({ trans: this.trans, delete: true, deleteChildren: val });
                });
            });
        } else {
            this.$uibModalInstance.close({ trans: this.trans, delete: true });
        }
    }

    private ok() {
        this.$uibModalInstance.close({ trans: this.trans, save: true, updateQuantityOnChildren: this.updateQuantityOnChildren });
    }

    // HELP-METHODS

    private setProductAccounts(prio: IAccountingPrioDTO) {
        var accountingSetting: AccountingSettingsRowDTO = this.trans.accountingSettings[0];

        // Clear accounts
        for (let i = 1; i <= 6; i++) {
            accountingSetting[`account${i}Id`] = 0;
            accountingSetting[`account${i}Nr`] = '';
            accountingSetting[`account${i}Name`] = '';
        }

        if (prio) {
            // Standard account
            if (prio.accountId) {
                accountingSetting.account1Id = prio.accountId;
                accountingSetting.account1Nr = prio.accountNr;
                accountingSetting.account1Name = prio.accountName;
            }

            // Internal accounts
            if (prio.accountInternals) {
                _.forEach(prio.accountInternals, accInt => {
                    if (accInt.account) {
                        this.$scope.$broadcast('setAccount', {
                            rowIndex: 0,
                            accountDimId: accInt.accountDimId,
                            accountId: accInt.account
                        });
                    }
                });
            }
        }
    }
}
