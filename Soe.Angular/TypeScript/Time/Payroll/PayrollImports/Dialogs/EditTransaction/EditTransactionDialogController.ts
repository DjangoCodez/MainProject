import { AccountingSettingsRowDTO } from "../../../../../Common/Models/AccountingSettingsRowDTO";
import { PayrollImportEmployeeTransactionAccountInternalDTO, PayrollImportEmployeeTransactionDTO, PayrollImportEmployeeTransactionLinkDTO } from "../../../../../Common/Models/PayrollImport";
import { PayrollProductGridDTO } from "../../../../../Common/Models/ProductDTOs";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { TermGroup_PayrollImportEmployeeTransactionStatus, TermGroup_PayrollResultType } from "../../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { IPayrollService } from "../../../PayrollService";

export class EditTransactionDialogController {

    private trans: PayrollImportEmployeeTransactionDTO;
    private links: PayrollImportEmployeeTransactionLinkDTO[];
    private isNew: boolean;

    private selectedProduct: PayrollProductGridDTO;
    private settings: AccountingSettingsRowDTO[];

    private get showTime(): boolean {
        return this.selectedProduct && this.selectedProduct.resultType === TermGroup_PayrollResultType.Time;
    }

    private get showQuantity(): boolean {
        return this.selectedProduct && this.selectedProduct.resultType === TermGroup_PayrollResultType.Quantity;
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private payrollService: IPayrollService,
        private transactionTypes: ISmallGenericType[],
        private transactionStatuses: ISmallGenericType[],
        private products: PayrollProductGridDTO[],
        private causes: ISmallGenericType[],
        private timeCodes: ISmallGenericType[],
        private accounts: ISmallGenericType[],
        private settingTypes: SmallGenericType[],
        payrollImportEmployeeId: number,
        trans: PayrollImportEmployeeTransactionDTO) {

        this.isNew = !trans;

        this.trans = new PayrollImportEmployeeTransactionDTO();
        angular.extend(this.trans, trans);
        if (this.isNew) {
            this.trans.payrollImportEmployeeId = payrollImportEmployeeId;
            this.trans.status = TermGroup_PayrollImportEmployeeTransactionStatus.Unprocessed;
            this.trans.date = CalendarUtility.getDateToday();
            this.trans.startTime = CalendarUtility.DefaultDateTime();
            this.trans.stopTime = CalendarUtility.DefaultDateTime();
        }

        this.$timeout(() => {
            this.setSelectedProduct();
        });
    }

    // SERVICE CALLS

    private loadLinks(): ng.IPromise<any> {
        return this.payrollService.getPayrollImportEmployeeTransactionLinks(this.trans.payrollImportEmployeeTransactionId).then(x => {
            this.links = x;
        });
    }

    // HELP-METHODS

    private setSelectedProduct() {
        this.selectedProduct = _.find(this.products, p => p.productId === this.trans.payrollProductId);
    }

    private setAccounting() {
        if (!this.trans.accountingSettings)
            this.trans.accountingSettings = [];

        if (this.settings.length === 1) {
            this.trans.accountingSettings = this.settings;
            let accountingSetting = this.trans.accountingSettings[0];

            // Standard account
            if (this.trans.accountStdId) {
                accountingSetting.accountDim1Nr = this.trans.accountStdDimNr ? this.trans.accountStdDimNr : 1;
                accountingSetting.account1Id = this.trans.accountStdId;
                accountingSetting.account1Nr = this.trans.accountStdNr;
                accountingSetting.account1Name = this.trans.accountStdName;
            }

            // Internal accounts
            if (this.trans.accountInternals) {
                let orderedAccountInternals = _.orderBy(this.trans.accountInternals, 'accountDimNr');

                orderedAccountInternals.forEach((accInt, index) => {
                    accountingSetting[`accountDim${index + 2}Nr`] = accInt.accountDimNr;
                    accountingSetting[`account${index + 2}Id`] = accInt.accountId;
                    accountingSetting[`account${index + 2}Nr`] = accInt.accountNr;
                    accountingSetting[`account${index + 2}Name`] = accInt.accountName;
                });
            }
        }
        this.settings = CoreUtility.cloneDTOs(this.trans.accountingSettings);
    }

    private setAccountingForSave() {
        this.trans.accountingSettings = this.settings;
        if (this.trans.accountingSettings.length === 1) {
            let accountingSetting = this.trans.accountingSettings[0];

            // Standard account
            this.trans.accountStdId = accountingSetting.account1Id;

            // Internal accounts
            this.trans.accountInternals = [];
            if (accountingSetting.account2Id)
                this.addAccountInternal(accountingSetting.account2Id, accountingSetting.account2Nr, accountingSetting.accountDim2Nr);
            if (accountingSetting.account3Id)
                this.addAccountInternal(accountingSetting.account3Id, accountingSetting.account3Nr, accountingSetting.accountDim3Nr);
            if (accountingSetting.account4Id)
                this.addAccountInternal(accountingSetting.account4Id, accountingSetting.account4Nr, accountingSetting.accountDim4Nr);
            if (accountingSetting.account5Id)
                this.addAccountInternal(accountingSetting.account5Id, accountingSetting.account5Nr, accountingSetting.accountDim5Nr);
            if (accountingSetting.account6Id)
                this.addAccountInternal(accountingSetting.account6Id, accountingSetting.account6Nr, accountingSetting.accountDim6Nr);
        }
    }

    private addAccountInternal(accountId: number, accountNr: string, dimNr: number) {
        let accInt = new PayrollImportEmployeeTransactionAccountInternalDTO();
        accInt.accountId = accountId;
        accInt.accountCode = accountNr;
        accInt.accountDimNr = dimNr;
        this.trans.accountInternals.push(accInt);
    }

    // EVENTS

    public cancel() {
        this.$uibModalInstance.close();
    }

    public save() {
        this.setAccountingForSave();

        this.payrollService.savePayrollImportEmployeeTransaction(this.trans).then(result => {
            if (result.success)
                this.$uibModalInstance.close({ trans: this.trans });
            else {
                this.translationService.translate("error.default_error").then(term => {
                    this.notificationService.showErrorDialog(term, result.errorMessage, result.stackTrace);
                });
            }
        });
    }
}
