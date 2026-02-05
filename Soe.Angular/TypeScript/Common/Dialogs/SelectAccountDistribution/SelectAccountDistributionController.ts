import { IAccountDistributionHeadDTO } from "../../../Scripts/TypeLite.Net4";
import { AccountDistributionRowDTO } from "../../Models/AccountDistributionRowDTO";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { AccountingRowDTO } from "../../Models/AccountingRowDTO";
import { StringUtility } from "../../../Util/StringUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, AccountingRowsContainers } from "../../../Util/Enumerations";
import { SoeAccountDistributionType, TermGroup, TermGroup_AccountDistributionCalculationType, TermGroup_AccountDistributionTriggerType, TermGroup_AccountDistributionPeriodType, WildCard, CompanySettingType } from "../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class SelectAccountDistributionController {

    buttonOkLabel: string = "";
    buttonYesLabel: string = "";
    buttonNoLabel: string = "";
    buttonCancelLabel: string = "";
    templateName: string = "";

    private nbrOfPeriods: number = 1;
    private startDate: Date;
    private endDate: Date;
    private calculationType: number;
    private accountingRowsDifference: number;
    private distributionHead: IAccountDistributionHeadDTO;
    private accountingRows: AccountDistributionRowDTO[] = [];
    private newDistribution: boolean = true;
    private buttonOkEnabled: boolean = true;
    private valid: boolean = true;
    voucherSeriesTypeFilterOptions: Array<any> = [];
    accountdistributionVoucherSeriesType: number;
    accountDistributionCalculationType: any;

    protected validationHandler: IValidationSummaryHandler;
    private invalidMandatoryFieldKeys: string[] = [];
    private invalidValidationErrorKeys: string[] = [];
    private companySettingsPromise?: ng.IPromise<any>;

    // Flags
    private hasDifference = false;

    //@ngInject
    constructor(private $uibModalInstance,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private accountingService: IAccountingService,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        private title: string,
        private text: string,
        private rowItem: AccountingRowDTO,
        private selectedAccountDistribution: IAccountDistributionHeadDTO,
        private periodAccountDistributions: IAccountDistributionHeadDTO[],
        private container: number,
        private accountDistributionHeadName: string,
        validationSummaryHandlerFactory?: IValidationSummaryHandlerFactory,) {

        if (validationSummaryHandlerFactory) {
            this.validationHandler = validationSummaryHandlerFactory.create();
        }

        this.setupLabels();

        this.companySettingsPromise = this.loadCompanySettings().then(() => {
            this.loadCalculationTypes();
        });

        this.$scope.$watch(() => this.startDate, () => {
            this.$timeout(() => {
                this.loadAccountYear(this.startDate).then(() => {
                    this.updateValidity();
                })
            });
        });

        this.$scope.$watchGroup([
            () => this.nbrOfPeriods,
            () => this.newDistribution,
            () => this.templateName,
            () => this.calculationType,
            () => this.accountingRows], () => {
            this.$timeout(() => {
                this.updateValidity();
            }, 500);
        })

        this.messagingService.subscribe(Constants.EVENT_ACCOUNTING_ROWS_MODIFIED, () => {
            this.$timeout(() => {
                this.updateValidity();
            }, 500);
        });

        this.init();

        // Replace \n with <br/>
        this.text = StringUtility.ToBr(text);

        this.$scope.$watch(() => this.accountingRowsDifference, () => {
            this.$timeout(() => {
                this.hasDifference = this.newDistribution ? (this.accountingRowsDifference !== 0) : false;
                this.updateValidity();
            });
        });

        $uibModalInstance.rendered.then(() => setTimeout(() => {//we use setTimeout since this has nothing to do with angular.
            var inputs = angular.element('.messagebox input');
            var focus = null;

            if ((inputs && inputs.length))
                focus = inputs[0];

            if (!focus) {
                var buttons = angular.element('.messagebox button');
                if ((buttons && buttons.length))
                    focus = buttons[0];
            }

            if (focus)
                angular.element(focus).focus();
        }));
    }

    private init() {
        this.distributionHead = this.selectedAccountDistribution;

        if (this.distributionHead.accountDistributionHeadId == 0)
            this.newDistribution = true;
        else
            this.newDistribution = false;

        // Set suggested number of periods from distribution head
        this.nbrOfPeriods = this.rowItem.accountDistributionNbrOfPeriods ? this.rowItem.accountDistributionNbrOfPeriods : this.distributionHead.periodValue;

        // Set suggested start date from distribution head
        this.setStartDate();

        if (this.container == AccountingRowsContainers.Voucher)
            this.translationService.translate("economy.accounting.distribution.perioddistribution.voucher").then((term) => {
                this.templateName = term + " " + this.accountDistributionHeadName;
            });
        else if (this.container == AccountingRowsContainers.SupplierInvoice)
            this.translationService.translate("economy.accounting.distribution.perioddistribution.supplierinvoice").then((term) => {
                this.templateName = term + " " + this.accountDistributionHeadName;
            });
        else if (this.container == AccountingRowsContainers.CustomerInvoice)
            this.translationService.translate("economy.accounting.distribution.perioddistribution.customerinvoice").then((term) => {
                this.templateName = term + " " + this.accountDistributionHeadName;
            });

        //set suggested entry rows
        this.setDefaultAccountingRows();
    }

    private setDefaultAccountingRows() {

        var defaultRow: AccountDistributionRowDTO = new AccountDistributionRowDTO();
        defaultRow.rowNbr = 1;
        defaultRow.dim1Id = this.rowItem.dim1Id;
        defaultRow.dim1Nr = this.rowItem.dim1Nr;
        defaultRow.dim1Name = this.rowItem.dim1Name;
        defaultRow.dim2Id = this.rowItem.dim2Id;
        defaultRow.dim2Nr = this.rowItem.dim2Nr;
        defaultRow.dim2Name = this.rowItem.dim2Name;
        defaultRow.dim3Id = this.rowItem.dim3Id;
        defaultRow.dim3Nr = this.rowItem.dim3Nr;
        defaultRow.dim3Name = this.rowItem.dim3Name;
        defaultRow.dim4Id = this.rowItem.dim4Id;
        defaultRow.dim4Nr = this.rowItem.dim4Nr;
        defaultRow.dim4Name = this.rowItem.dim4Name;
        defaultRow.dim5Id = this.rowItem.dim5Id;
        defaultRow.dim5Nr = this.rowItem.dim5Nr;
        defaultRow.dim5Name = this.rowItem.dim5Name;
        defaultRow.dim6Id = this.rowItem.dim6Id;
        defaultRow.dim6Nr = this.rowItem.dim6Nr;
        defaultRow.dim6Name = this.rowItem.dim6Name;
        defaultRow.oppositeBalance = 100;
        defaultRow.sameBalance = 0;

        this.accountingRows.push(defaultRow);

        var secondRow: AccountDistributionRowDTO = new AccountDistributionRowDTO();
        secondRow.rowNbr = 2;
        secondRow.sameBalance = 100;
        secondRow.oppositeBalance = 0;
        secondRow.dim1Id = null;
        secondRow.dim1Nr = null;
        secondRow.dim1Name = "";
        secondRow.dim2Id = null;
        secondRow.dim2Nr = null;
        secondRow.dim2Name = "";
        secondRow.dim3Id = null;
        secondRow.dim3Nr = null;
        secondRow.dim3Name = "";
        secondRow.dim4Id = null;
        secondRow.dim4Nr = null;
        secondRow.dim4Name = "";
        secondRow.dim5Id = null;
        secondRow.dim5Nr = null;
        secondRow.dim5Name = "";
        secondRow.dim6Id = null;
        secondRow.dim6Nr = null;
        secondRow.dim6Name = "";

        this.accountingRows.push(secondRow);
    }

    setupLabels() {

        this.translationService.translate("core.ok").then((term) => {
            this.buttonOkLabel = term;
        });

        this.translationService.translate("core.yes").then((term) => {
            this.buttonYesLabel = term;
        });

        this.translationService.translate("core.no").then((term) => {
            this.buttonNoLabel = term;
        });

        this.translationService.translate("core.cancel").then((term) => {
            this.buttonCancelLabel = term;
        });

    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        // Makes sure company settings is loaded before trying to load voucherSeries.
        return this.companySettingsPromise.then(() => 
            this.accountingService.getVoucherSeriesByYear(accountYearId, false, true).then((x) => {
                this.voucherSeriesTypeFilterOptions = x;

                if (this.voucherSeriesTypeFilterOptions != null && this.voucherSeriesTypeFilterOptions.length > 0 && this.accountdistributionVoucherSeriesType != null)
                    this.distributionHead.voucherSeriesTypeId = this.accountdistributionVoucherSeriesType;
            })
        );
    }

    private loadAccountYear(date: Date): ng.IPromise<any> {
        if (!date)
            return this.$q.resolve();

        return this.accountingService.getAccountYearId(date).then((id: number) =>
            this.loadVoucherSeries(id)
        );
    }

    private loadCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountDistributionCalculationType, false, false).then((x) => {
            this.accountDistributionCalculationType = x;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountdistributionVoucherSeriesType);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.accountdistributionVoucherSeriesType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountdistributionVoucherSeriesType);
        })
    }

    private setStartDate() {
        this.startDate = new Date();
        var daysInMonth = this.startDate.daysInMonth();
        var dayNumber = this.distributionHead.dayNumber > 0 ? this.distributionHead.dayNumber : daysInMonth;
        dayNumber = Math.min(dayNumber, daysInMonth);
        this.startDate.setDate(dayNumber);
    }

    private setEndDate() {
        this.$timeout(() => {
            if (this.newDistribution) {
                this.endDate = this.startDate.addMonths(this.nbrOfPeriods - 1).endOfMonth();
                this.calculateRowAmounts();
            }
        });
    }

    private setNbrOfPeriods() {
        this.$timeout(() => {
            const originalStartDate = new Date(this.startDate);
            const originalEndDate = new Date(this.endDate);
            const dayOfPeriod = 31;
            let endDate = new Date(originalEndDate);

            if (!originalStartDate || !originalEndDate) return;
            if (originalStartDate > originalEndDate) return;


            const daysInEndMonth = this.daysInMonth(
                endDate.getMonth(),
                endDate.getFullYear()
            );

            endDate = new Date(
                endDate.getFullYear(),
                endDate.getMonth(),
                daysInEndMonth < dayOfPeriod ? daysInEndMonth : dayOfPeriod
            );

            let months = CalendarUtility.getMonthsBetweenDates(originalStartDate, endDate);

            if (originalEndDate < endDate) {
                months -= 1;
            }

            this.nbrOfPeriods = months;
            this.calculateRowAmounts();
        });
    }

    private daysInMonth(month: number, year: number) {
        return new Date(year, month + 1, 0).getDate();
    }

    private accDistChanged(item: any) {
        this.selectedAccountDistribution = item;
        this.distributionHead = this.selectedAccountDistribution;
        this.nbrOfPeriods = this.distributionHead.periodValue;
        this.setStartDate();
        if (this.distributionHead.accountDistributionHeadId == 0)
            this.newDistribution = true;
        else
            this.newDistribution = false;
    }

    createAccountDistributionHead() {
        this.accountingService.getAccount(this.rowItem.dim1Id)
            .then(account => {
                this.distributionHead.dim1Id = account.accountDimId;
            })
            .then(() => {

                this.distributionHead.name = this.templateName;
                this.distributionHead.type = SoeAccountDistributionType.Period;
                this.distributionHead.sort = 0;
                this.distributionHead.calculationType = TermGroup_AccountDistributionCalculationType.Percent.valueOf();
                this.distributionHead.triggerType = TermGroup_AccountDistributionTriggerType.Registration.valueOf();
                this.distributionHead.useInCustomerInvoice = this.container == AccountingRowsContainers.CustomerInvoice;
                this.distributionHead.useInImport = false;
                this.distributionHead.useInPayrollVacationVoucher = false;
                this.distributionHead.useInPayrollVoucher = false;
                this.distributionHead.useInSupplierInvoice = this.container == AccountingRowsContainers.SupplierInvoice;
                this.distributionHead.useInVoucher = this.container == AccountingRowsContainers.Voucher;
                this.distributionHead.periodType = TermGroup_AccountDistributionPeriodType.Unknown.valueOf();
                this.distributionHead.periodValue = this.nbrOfPeriods;
                this.distributionHead.dayNumber = this.startDate.getDate();
                this.distributionHead.startDate = this.startDate.date();
                this.distributionHead.endDate = this.endDate.date();
                this.distributionHead.calculationType = this.calculationType;
                this.distributionHead.amount = 0;
                this.distributionHead.amountOperator = WildCard.Equals;
                this.distributionHead.dim1Expression = this.rowItem.dim1Nr;

                this.distributionHead.rows = this.accountingRows;

                // remove the last possibly empty row before saving
                var distributionEntryRows = _.filter(this.distributionHead.rows, r => r.dim1Nr != null);

                var distributionRows = [];

                if (this.calculationType == TermGroup_AccountDistributionCalculationType.Percent) {
                    _.forEach(distributionEntryRows, (row: AccountDistributionRowDTO, index: number) => {
                        row.sameBalance = this.nbrOfPeriods > 0 ? row.sameBalance.roundToNearest(2) : Math.abs(row.sameBalance);
                        row.oppositeBalance = this.nbrOfPeriods > 0 ? row.oppositeBalance.roundToNearest(2) : Math.abs(row.oppositeBalance);
                        distributionRows.push(row);
                    });
                }
                else
                    distributionRows = distributionEntryRows;


                this.accountingService.saveAccountDistribution(this.distributionHead, <AccountDistributionRowDTO[]>distributionRows).then((result) => {
                    if (result.success) {
                        if (result.integerValue && result.integerValue > 0) {
                            this.distributionHead.accountDistributionHeadId = result.integerValue;
                        }

                        this.buttonOkClick();
                    }
                    else {
                        var keys: string[] = [
                            "core.warning",
                            "economy.accounting.accountdistribution.savingnotsucceed"
                        ];

                        this.translationService.translateMany(keys).then((terms) => {
                            this.notificationService.showDialog(terms["core.warning"], terms["economy.accounting.accountdistribution.savingnotsucceed"], SOEMessageBoxImage.OK, SOEMessageBoxButtons.OK);
                        });
                        this.buttonOkEnabled = true;
                    }
                });
            });
    }

    private calculateRowAmounts() {
        this.$timeout(() => {
            var balance: number = 100;

            if (this.calculationType == TermGroup_AccountDistributionCalculationType.Amount ||
                this.calculationType == TermGroup_AccountDistributionCalculationType.TotalAmount) {
                balance = this.nbrOfPeriods > 0 ? (Math.abs(this.rowItem.amount) / this.nbrOfPeriods).roundToNearest(2) : Math.abs(this.rowItem.amount);
            }

            _.forEach(this.accountingRows, (row: AccountDistributionRowDTO) => {
                if (row.sameBalance.valueOf() > 0) {
                    row.sameBalance = balance;
                    row.oppositeBalance = 0;
                }
                else {
                    row.sameBalance = 0;
                    row.oppositeBalance = balance;
                }

            });

            this.$scope.$broadcast('rowsChanged');
        });
    }

    buttonOkClick() {
        if (this.distributionHead.accountDistributionHeadId == 0) {
            this.buttonOkEnabled = false;
            //this.buttonOkIsValid();
            this.createAccountDistributionHead();
        }
        else
            this.$uibModalInstance.close({ result: true, nbrOfPeriods: this.nbrOfPeriods, startDate: this.startDate, distributionHead: this.distributionHead });
    }


    buttonCancelClick() {
        this.$uibModalInstance.close({ result: false });
    }

    updateValidity() {
        var valid: boolean = this.buttonOkEnabled;
        this.invalidMandatoryFieldKeys = [];
        this.invalidValidationErrorKeys = [];

        if (!this.nbrOfPeriods) {
            this.invalidMandatoryFieldKeys.push("economy.accounting.accountdistribution.numberOfPeriods");
            valid = false;
        }

        if (!this.startDate) {
            this.invalidMandatoryFieldKeys.push("economy.accounting.accountdistribution.startdate");
            valid = false;
        }

        if (this.newDistribution) {

            if (!this.templateName || this.templateName.trim() == "") {
                this.invalidMandatoryFieldKeys.push("common.name");
                valid = false;
            }

            if (!this.distributionHead.voucherSeriesTypeId) {
                this.invalidMandatoryFieldKeys.push("economy.accounting.accountdistribution.voucherserie");
                valid = false;
            }

            if (!this.calculationType) {
                this.invalidMandatoryFieldKeys.push("economy.accounting.accountdistribution.calculationtype");
                valid = false;
            }

            if (valid == true && this.accountingRows) {

                if (_.filter(this.accountingRows, row => row.dim1Id > 0).length != this.accountingRows.length ||
                    this.accountingRows.length <= 1) {
                    valid = false;
                }

                const sameTotal = +_.sum(_.map(this.accountingRows, 'sameBalance')).roundToNearest(2);
                const total = (this.calculationType == TermGroup_AccountDistributionCalculationType.Amount ||
                               this.calculationType == TermGroup_AccountDistributionCalculationType.TotalAmount)
                    ? (this.rowItem.amount / this.nbrOfPeriods).roundToNearest(2)
                    : 100;

                const totalDiff: number = +(sameTotal - Math.abs(total)).roundToNearest(2);
                if (totalDiff != 0) {
                    const error = this.translationService.translateInstant("economy.accounting.accountdistribution.invalidamount").format(this.rowItem.dim1Nr);
                    this.invalidValidationErrorKeys.push(error);
                    valid = false;
                }
            }

            this.$timeout(() => {
                if (valid == true && this.accountingRows) {

                    var same: number = +_.sum(_.map(this.accountingRows, 'sameBalance')).toFixed(2);
                    var opposite: number = +_.sum(_.map(this.accountingRows, 'oppositeBalance')).toFixed(2);
                    if (Math.abs((same - opposite)) !== 0) {
                        this.invalidValidationErrorKeys.push("economy.accounting.accountdistribution.diffinrows");
                        valid = false;
                    }
                    valid = +(same - opposite).toFixed(2) === 0;
                }
            });
        }

        this.valid = valid;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            mandatoryFieldKeys.push(...this.invalidMandatoryFieldKeys);
            validationErrorKeys.push(...this.invalidValidationErrorKeys);
        });
    }
}