import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { PayrollGroupAccountsDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { TermGroup_Languages, CompanySettingType, TermGroup_SysPayrollPrice } from "../../../../../Util/CommonEnumerations";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { AccountSmallDTO } from "../../../../../Common/Models/AccountDTO";
import { ITimeService } from "../../../../Time/timeservice";
import { SysPayrollPriceDTO } from "../../../../../Common/Models/SysPayrollPriceDTO";
import { PayrollGroupAccountDialogController } from "./PayrollGroupAccountDialogController";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";

export class PayrollGroupAccountsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/PayrollGroups/Directives/PayrollGroupAccounts/Views/PayrollGroupAccounts.html'),
            scope: {
                payrollGroupId: '=',
                accounts: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollGroupAccountsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollGroupAccountsController {

    // Init parameters
    private payrollGroupId: number;
    private accounts: PayrollGroupAccountsDTO[] = [];

    // Company settings
    private usePayrollTax: boolean = false;

    // Data
    private dates: Date[] = [];
    private accountStds: AccountSmallDTO[] = [];
    private sysPayrollPrices: SysPayrollPriceDTO[] = [];
    private accountRows: PayrollGroupAccountsDTO[] = [];

    // Properties
    private _selectedDate: Date;
    private get selectedDate(): Date {
        return this._selectedDate;
    }
    private set selectedDate(date: Date) {
        if (_.filter(this.accounts, a => a.isModified).length > 0) {
            this.askChangeDate().then(result => {
                if (result) {
                    this._selectedDate = date;
                    this.loadSysPayrollPrices();
                }
            })
        } else {
            this._selectedDate = date;
            this.loadSysPayrollPrices();
        }
    }

    // Flags
    private readOnly: boolean;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private payrollService: IPayrollService,
        private timeService: ITimeService) {
    }

    public $onInit() {
        this.$q.all([
            this.loadCompanySettings(),
            this.loadAccountStds()
        ]).then(() => {
            this.$q.all([
                this.loadDates()
            ]).then(() => {
                this.setupWatchers();
            });
        });
    }

    private setupWatchers() {
        this.$scope.$watchCollection(() => this.accounts, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.setAccounts();
            }
        });
    }

    // SERVICE CALLS

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.PayrollAgreementUsePayrollTax);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.usePayrollTax = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUsePayrollTax);
        });
    }

    private loadDates(): ng.IPromise<any> {
        // TODO: Hard coded for Sweden
        return this.payrollService.getPayrollGroupAccountDates(TermGroup_Languages.Swedish).then(x => {
            this.dates = x.map(y => {
                return CalendarUtility.convertToDate(y);
            });
            // Set current date (or closest before) as default
            if (this.dates.length > 0)
                this.selectedDate = _.find(this.dates, d => d.isSameOrBeforeOnDay(new Date()));
        })
    }

    private loadAccountStds(): ng.IPromise<any> {
        return this.timeService.getAccountsSmall(0, 0, true).then(x => {
            this.accountStds = x;

            // Add empty account
            let acc: AccountSmallDTO = new AccountSmallDTO();
            acc.accountId = 0;
            acc.number = acc.name = '';
            this.accountStds.splice(0, 0, acc);
        });
    }

    private loadSysPayrollPrices(): ng.IPromise<any> {
        let prices = [];
        prices.push(TermGroup_SysPayrollPrice.SE_EmploymentTax);
        if (this.usePayrollTax)
            prices.push(TermGroup_SysPayrollPrice.SE_PayrollTax);

        // TODO: Hard coded for Sweden
        return this.payrollService.getSysPayrollPrices(TermGroup_Languages.Swedish, prices, false, false, false, true, false, this.selectedDate).then(x => {
            this.sysPayrollPrices = x;
            this.setAccounts();
        });
    }

    private setAccounts() {
        this.accountRows = [];
        _.forEach(_.filter(this.sysPayrollPrices, s => s.intervals), sysPayrollPrice => {
            _.forEach(_.orderBy(sysPayrollPrice.intervals, 'fromInterval'), interval => {
                let accountRow: PayrollGroupAccountsDTO = _.find(this.accountRows, a => a.fromInterval === interval.fromInterval && a.toInterval === interval.toInterval);
                if (!accountRow) {
                    accountRow = new PayrollGroupAccountsDTO();
                    accountRow.fromInterval = interval.fromInterval;
                    accountRow.toInterval = interval.toInterval;
                    this.accountRows.push(accountRow);
                }

                // Set default percents
                switch (interval.sysPayrollPrice) {
                    case TermGroup_SysPayrollPrice.SE_EmploymentTax:
                        accountRow.employmentTaxPercent = interval.amount;
                        break;
                    case TermGroup_SysPayrollPrice.SE_PayrollTax:
                        accountRow.payrollTaxPercent = interval.amount;
                        break;
                }

                // Get existing values from payroll group accounts
                let account: PayrollGroupAccountsDTO = _.find(this.accounts, a => a.fromInterval === interval.fromInterval && a.toInterval === interval.toInterval);
                if (account) {
                    if (account.employmentTaxAccountId) {
                        accountRow.employmentTaxAccountId = account.employmentTaxAccountId;
                        accountRow.employmentTaxAccountNr = account.employmentTaxAccountNr;
                        accountRow.employmentTaxAccountName = account.employmentTaxAccountName;
                    }
                    if (account.payrollTaxAccountId) {
                        accountRow.payrollTaxAccountId = account.payrollTaxAccountId;
                        accountRow.payrollTaxAccountNr = account.payrollTaxAccountNr;
                        accountRow.payrollTaxAccountName = account.payrollTaxAccountName;
                    }
                    if (account.ownSupplementChargePercent)
                        accountRow.ownSupplementChargePercent = account.ownSupplementChargePercent;
                    if (account.ownSupplementChargeAccountId) {
                        accountRow.ownSupplementChargeAccountId = account.ownSupplementChargeAccountId;
                        accountRow.ownSupplementChargeAccountNr = account.ownSupplementChargeAccountNr;
                        accountRow.ownSupplementChargeAccountName = account.ownSupplementChargeAccountName;
                    }
                }
            });
        });
    }

    // EVENTS

    private editAccountRow(accountRow: PayrollGroupAccountsDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/PayrollGroups/Directives/PayrollGroupAccounts/Views/PayrollGroupAccountDialog.html"),
            controller: PayrollGroupAccountDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                accountRow: () => { return accountRow },
                accountStds: () => { return this.accountStds },
                usePayrollTax: () => { return this.usePayrollTax }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.accountRow) {
                accountRow.employmentTaxAccountId = result.accountRow.employmentTaxAccountId;
                accountRow.payrollTaxAccountId = result.accountRow.payrollTaxAccountId;
                accountRow.ownSupplementChargePercent = result.accountRow.ownSupplementChargePercent;
                accountRow.ownSupplementChargeAccountId = result.accountRow.ownSupplementChargeAccountId;

                // Set account names
                let employmentTaxAccount = _.find(this.accountStds, a => a.accountId === accountRow.employmentTaxAccountId);
                accountRow.employmentTaxAccountNr = employmentTaxAccount ? employmentTaxAccount.number : '';
                accountRow.employmentTaxAccountName = employmentTaxAccount ? employmentTaxAccount.name : '';

                let payrollTaxAccount = _.find(this.accountStds, a => a.accountId === accountRow.payrollTaxAccountId);
                accountRow.payrollTaxAccountNr = payrollTaxAccount ? payrollTaxAccount.number : '';
                accountRow.payrollTaxAccountName = payrollTaxAccount ? payrollTaxAccount.name : '';

                let ownSupplementCharge = _.find(this.accountStds, a => a.accountId === accountRow.ownSupplementChargeAccountId);
                accountRow.ownSupplementChargeAccountNr = ownSupplementCharge ? ownSupplementCharge.number : '';
                accountRow.ownSupplementChargeAccountName = ownSupplementCharge ? ownSupplementCharge.name : '';

                accountRow.isModified = true;

                // Update account (on payroll group)
                let account = _.find(this.accounts, a => a.fromInterval === accountRow.fromInterval && a.toInterval === accountRow.toInterval);
                if (!account) {
                    account = new PayrollGroupAccountsDTO();
                    this.accounts.push(account);
                }
                angular.extend(account, accountRow);

                this.setAsDirty();
            }
        });
    }

    // HELP-METHODS

    private askChangeDate(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var keys: string[] = [
            "core.warning",
            "time.employee.payrollgroup.account.changedatewarning"];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.employee.payrollgroup.account.changedatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val)
                    deferral.resolve(true);
            }, (reason) => {
                deferral.resolve(false);
            });
        });

        return deferral.promise;
    }
    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}