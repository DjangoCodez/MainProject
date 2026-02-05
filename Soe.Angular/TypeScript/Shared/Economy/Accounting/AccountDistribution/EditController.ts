import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IAccountDistributionHeadDTO, ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IAccountingService } from "../AccountingService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { AccountDistributionRowDTO } from "../../../../Common/Models/AccountDistributionRowDTO";
import { AccountDistributionHeadDTO } from "../../../../Common/Models/AccountDistributionHeadDTO";
import { AccountDistributionEntryDTO } from "../../../../Common/Models/AccountDistributionEntryDTO";
import { Feature, WildCard, TermGroup_AccountDistributionTriggerType, TermGroup_AccountDistributionPeriodType, TermGroup_AccountDistributionCalculationType, TermGroup, SoeAccountDistributionType, CompanySettingType, SoeEntityState } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { AccountDimSmallDTO } from "../../../../Common/Models/AccountDimDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data    
    accountDistributionHeadId: number;
    accountDistribution: IAccountDistributionHeadDTO;
    accountDistributionEntries: AccountDistributionEntryDTO[];

    //Lookups
    accountDistributionCalculationType: any;
    accountDistributionTriggerType: any;
    accountDistributionPeriodType: any;
    accountDistributionFilteredPeriodType: ISmallGenericType[];
    voucherSeriesTypeFilterOptions: Array<any> = [];
    accountYearId: number;
    accountYearIsOpen: boolean;

    amountOperatorWildCardOptions: Array<any> = [];

    //accountDistributionType: any;
    accountdistributionVoucherSeriesType: number;
    isPeriodAccountDistribution = false;
    isAutomaticAccountDistribution = false;

    registrationAsTriggerType = false;
    amountAsPeriodType = false;
    isTotalAmount = false;
    entriesIsStarted = false;

    editPanelName: string;

    entryTransferredCount: number;
    entryTotalCount: number;
    entryTotalAmount: number;
    entryPeriodAmount: number;
    entryTransferredAmount: number;
    entryLatestTransferDate: Date;
    entryRemainingCount: number;
    entryRemainingAmount: number;

    private _dayInPeriod = 31;
    get dayInPeriod(): number {
        return this._dayInPeriod;
    }
    set dayInPeriod(value: number) {
        value = !value || value > 31 ? 31 : value;
        value = value < 1 ? 1 : value;
        this._dayInPeriod = value;
        this.accountDistribution.dayNumber = value;
        this.dayInPeriodChanged();
    }

    //accountdims
    showAccountDim1 = true;
    showAccountDim2 = false;
    showAccountDim3 = false;
    showAccountDim4 = false;
    showAccountDim5 = false;
    showAccountDim6 = false;

    accountDim1Name: string;
    accountDim2Name: string;
    accountDim3Name: string;
    accountDim4Name: string;
    accountDim5Name: string;
    accountDim6Name: string;

    private permission: number;

    // Flags
    hasDiff = false;
    terms: any = [];

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        if (soeConfig.accountDistributionType == "Period" || soeConfig.accountDistributionType === SoeAccountDistributionType.Period) {
            this.editPanelName = "economy.accounting.accountdistribution.accountdistribution";
            this.isPeriodAccountDistribution = true;
            this.permission = Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod;
        } else
            if (soeConfig.accountDistributionType == "Auto" || soeConfig.accountDistributionType === SoeAccountDistributionType.Auto) {
                this.editPanelName = "economy.accounting.accountdistribution.accountdistributionauto";
                this.isAutomaticAccountDistribution = true;
                this.permission = Feature.Economy_Preferences_VoucherSettings_AccountDistributionAuto;
            }

        // Config parameters
        this.accountYearId = soeConfig.accountYearId;
        this.accountYearIsOpen = soeConfig.accountYearIsOpen;
        this.amountOperatorWildCardOptions.push({ id: WildCard.LessThan, name: "<" });
        this.amountOperatorWildCardOptions.push({ id: WildCard.LessThanOrEquals, name: "<=" });
        this.amountOperatorWildCardOptions.push({ id: WildCard.Equals, name: "=" });
        this.amountOperatorWildCardOptions.push({ id: WildCard.GreaterThan, name: ">=" });
        this.amountOperatorWildCardOptions.push({ id: WildCard.GreaterThanOrEquals, name: ">" });
        this.amountOperatorWildCardOptions.push({ id: WildCard.NotEquals, name: "<>" });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.accountDistributionHeadId = parameters.id;
        if (parameters.accountDistributionType) {
            this.editPanelName = "economy.accounting.accountdistribution.accountdistribution";
            this.isPeriodAccountDistribution = true;
            this.permission = Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod;
        }

        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: this.permission, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadTerms().then(() => {
                if (this.isPeriodAccountDistribution) {
                    if (this.accountDistributionHeadId && this.accountDistributionHeadId > 0) {
                        return this.progress.startLoadingProgress([
                            () => this.$q.all([
                                this.loadCalculationTypes(),
                                this.loadTriggerTypes(),
                                this.loadPeriodTypes(),
                                this.loadVoucherSeriesTypes(),
                                this.loadCompanySettings(),
                            ])
                        ]);
                    }
                    else {
                        return this.progress.startLoadingProgress([
                            () => this.$q.all([
                                this.loadCalculationTypes(),
                                this.loadTriggerTypes(),
                                this.loadPeriodTypes(),
                                this.loadVoucherSeriesTypes(),
                                this.loadCompanySettings()
                            ])
                        ]);
                    }
                } 

                if (this.isAutomaticAccountDistribution) {
                    return this.progress.startLoadingProgress([
                        () => this.loadCalculationTypes()
                    ]);
                }
            })
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[this.permission].readPermission;
        this.modifyPermission = response[this.permission].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.accountDistributionHeadId, recordId => {
            if (recordId !== this.accountDistributionHeadId) {
                this.accountDistributionHeadId = recordId;
                this.onLoadData();
            }
        });
    }

    private onLoadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.load(), 
            () => this.loadExistingEntries()
        ]).then(() => {
            this.entryRemainingAmount = this.entryTotalAmount - this.entryTransferredAmount;
            this.entryRemainingCount = this.entryTotalCount - this.entryTransferredCount;
        });
    }
    private load(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.accountDistributionHeadId > 0) {

            this.accountingService.getAccountDistributionHead(this.accountDistributionHeadId).then((x: IAccountDistributionHeadDTO) => {
                this.accountDistribution = x;
                this.isNew = false;

                if (this.isPeriodAccountDistribution) {
                    this.accountDistribution.keepRow = false;
                }

                if (this.accountDistribution.triggerType.valueOf() == TermGroup_AccountDistributionTriggerType.Registration) {
                    this.registrationAsTriggerType = true;
                }
                else {
                    this.registrationAsTriggerType = false;
                }

                if (this.accountDistribution.periodType.valueOf() == TermGroup_AccountDistributionPeriodType.Amount)
                    this.amountAsPeriodType = true;
                else
                    this.amountAsPeriodType = false;

                if (this.accountDistribution.calculationType.valueOf() == TermGroup_AccountDistributionCalculationType.TotalAmount) {
                    this.isTotalAmount = true;
                }

                this.accountDistributionCalculationTypeChanged(this.accountDistribution.calculationType);

                //This is a workaround for the datepicker. This must be done, savebutton will be disabled otherwise
                if (this.accountDistribution.startDate)
                    this.accountDistribution.startDate = new Date(this.accountDistribution.startDate.valueOf());
                if (this.accountDistribution.endDate)
                    this.accountDistribution.endDate = new Date(this.accountDistribution.endDate.valueOf());

                //Load account dims, first now when this.accountDistribution is set                    
                this.loadAccountDimStd();

                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, (this.isPeriodAccountDistribution ? this.terms["economy.accounting.accountdistribution.accountdistribution"] : this.terms["economy.accounting.accountdistribution.accountdistributionauto"]) + ' ' + this.accountDistribution.name);

                deferral.resolve();
            }); //.then(() => this.loadAccountDimStd());
        } else {
            this.new();

            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadTerms() {
        const keys: string[] = [
            "economy.accounting.accountdistribution.accountdistribution",
            "economy.accounting.accountdistribution.accountdistributionauto"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountDistributionCalculationType, false, false).then((x) => {
            this.accountDistributionCalculationType = x;
        });
    }

    private loadTriggerTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountDistributionTriggerType, false, false).then((x) => {
            this.accountDistributionTriggerType = x;
        });
    }

    private loadPeriodTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountDistributionPeriodType, false, false).then((x) => {
            this.accountDistributionPeriodType = x;
        });
    }

    private loadVoucherSeriesTypes(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesTypes().then((x) => {
            _.forEach(x, (y: any) => {
                this.voucherSeriesTypeFilterOptions.push({ id: y.voucherSeriesTypeId, name: y.name })
            });
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.AccountdistributionVoucherSeriesType];
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.accountdistributionVoucherSeriesType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountdistributionVoucherSeriesType);
        })
    }

    private loadAccountDimStd(): ng.IPromise<any> {
        return this.accountingService.getAccountDimStd().then((x) => {
            this.accountDim1Name = x.name;
            this.accountDistribution.dim1Id = parseInt(x.accountDimId);

            this.loadAccountDims();
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, true, false, false).then((dims: AccountDimSmallDTO[]) => {
            let pos = 2;
            for (let dim of dims) {
                if (pos == 2) {
                    this.showAccountDim2 = true;
                    this.accountDim2Name = dim.name;
                    this.accountDistribution.dim2Id = dim.accountDimId;
                }
                if (pos == 3) {
                    this.showAccountDim3 = true;
                    this.accountDim3Name = dim.name;
                    this.accountDistribution.dim3Id = dim.accountDimId;
                }
                if (pos == 4) {
                    this.showAccountDim4 = true;
                    this.accountDim4Name = dim.name;
                    this.accountDistribution.dim4Id = dim.accountDimId;
                }
                if (pos == 5) {
                    this.showAccountDim5 = true;
                    this.accountDim5Name = dim.name;
                    this.accountDistribution.dim5Id = dim.accountDimId;
                }
                if (pos == 6) {
                    this.showAccountDim6 = true;
                    this.accountDim6Name = dim.name;
                    this.accountDistribution.dim6Id = dim.accountDimId;
                }
                pos++
            }
        });
    }

    private loadExistingEntries(): ng.IPromise<any> {
        return this.accountingService.getAccountDistributionEntriesForHead(this.accountDistributionHeadId).then((x) => {
            this.accountDistributionEntries = x;
            this.entriesIsStarted = this.accountDistributionEntries.some(h => h.voucherHeadId &&
                h.voucherHeadId != 0 &&
                h.state !== SoeEntityState.Deleted);

            this.entryTransferredCount = 0;
            this.entryTotalCount = this.accountDistributionEntries.length;
            this.entryTotalAmount = 0;
            this.entryPeriodAmount = 0;
            this.entryTransferredAmount = 0;

            _.forEach(x, (entry) => {
                let rowAmount: number = 0;
                if (entry.accountDistributionEntryRowDTO[0].debitAmount > 0)
                    rowAmount = _.round(entry.accountDistributionEntryRowDTO[0].debitAmount, 2);
                else
                    rowAmount = _.round(entry.accountDistributionEntryRowDTO[0].creditAmount, 2);

                this.entryTotalAmount += rowAmount;

                if (entry.voucherHeadId) {
                    this.entryTransferredCount++;
                    this.entryTransferredAmount += rowAmount;
                    this.entryLatestTransferDate = entry.date;
                }

                if (this.entryPeriodAmount == 0)
                    this.entryPeriodAmount = rowAmount;
            });

            this.entryTotalAmount = _.round(this.entryTotalAmount, 2);
            this.entryLatestTransferDate = CalendarUtility.convertToDate(this.entryLatestTransferDate);

        });
    }

    private accountDistributionTriggerTypeChanged(triggerTypeId: number) {
        if (triggerTypeId == TermGroup_AccountDistributionTriggerType.Registration) {
            this.registrationAsTriggerType = true;
        } else {
            this.registrationAsTriggerType = false;
            this.accountDistribution.useInCustomerInvoice = false;
            this.accountDistribution.useInImport = false;
            this.accountDistribution.useInSupplierInvoice = false;
            this.accountDistribution.useInVoucher = false;
            this.accountDistribution.useInPayrollVoucher = false;
            this.accountDistribution.useInPayrollVacationVoucher = false;
        }
    }

    private accountDistributionCalculationTypeChanged(calculationTypeId: number) {
        if (!this.isPeriodAccountDistribution)
            return;

        this.isTotalAmount = (calculationTypeId == TermGroup_AccountDistributionCalculationType.TotalAmount);
        if (
          calculationTypeId == TermGroup_AccountDistributionCalculationType.Amount
        ) {
            this.accountDistributionFilteredPeriodType = this.accountDistributionPeriodType.filter(
            pt => pt.id == TermGroup_AccountDistributionPeriodType.Amount
          );
        } else {
            this.accountDistributionFilteredPeriodType = this.accountDistributionPeriodType.filter(
            pt => pt.id != TermGroup_AccountDistributionPeriodType.Amount
          );
        }

        let periodType = this.accountDistributionFilteredPeriodType.find(
            pt => pt.id === this.accountDistribution.periodType
        );
        if (!periodType) {
            periodType = this.accountDistributionFilteredPeriodType[0];
        }

        this.accountDistribution.periodType = periodType ? periodType.id : null;
    }

    private accountDistributionPeriodTypeChanged(periodTypeId: number) {
        this.amountAsPeriodType = (periodTypeId == TermGroup_AccountDistributionPeriodType.Amount);
    }

    private dayInPeriodChanged() {
        this.accountDistribution.startDate = this.accountDistribution.startDate || new Date();
        if (this.accountDistribution.dayNumber > 28) {
            const today = new Date();
            this.accountDistribution.startDate = new Date(today.getFullYear(), today.getMonth() + 1,);
        }
        else {
            this.accountDistribution.startDate.setDate(this.accountDistribution.dayNumber);
        }
    }

    private save() {
        this.$scope.$broadcast('stopEditing', {
            functionComplete: () => {
                this.performSave();
            }
        });
    }

    private performSave() {
        // Fix decimals
        this.accountDistribution.amount = NumberUtility.parseDecimal(this.accountDistribution.amount.toString());

        // remove the last possibly empty row before saving
        const distributionRows = _.filter(this.accountDistribution.rows, r => r.dim1Nr != null);

        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveAccountDistribution(this.accountDistribution, <AccountDistributionRowDTO[]>distributionRows).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.accountDistributionHeadId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.accountDistribution.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }

                        this.accountDistributionHeadId = result.integerValue;
                        this.accountDistribution.accountDistributionHeadId = result.integerValue;
                    }

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.accountDistribution);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                // Reset cache
                if (this.accountDistribution.useInCustomerInvoice)
                    this.accountingService.getAccountDistributionHeadsUsedIn(null, TermGroup_AccountDistributionTriggerType.Registration, null, false, false, true, null, false, false, false);
                if (this.accountDistribution.useInImport)
                    this.accountingService.getAccountDistributionHeadsUsedIn(null, TermGroup_AccountDistributionTriggerType.Registration, null, false, false, false, true, false, false, false);
                if (this.accountDistribution.useInSupplierInvoice)
                    this.accountingService.getAccountDistributionHeadsUsedIn(null, TermGroup_AccountDistributionTriggerType.Registration, null, false, true, false, false, false, false, false);
                if (this.accountDistribution.useInVoucher)
                    this.accountingService.getAccountDistributionHeadsUsedIn(null, TermGroup_AccountDistributionTriggerType.Registration, null, true, false, false, false, false, false, false);
                if (this.accountDistribution.useInPayrollVoucher)
                    this.accountingService.getAccountDistributionHeadsUsedIn(null, TermGroup_AccountDistributionTriggerType.Registration, null, false, false, false, false, true, false, false);
                if (this.accountDistribution.useInPayrollVacationVoucher)
                    this.accountingService.getAccountDistributionHeadsUsedIn(null, TermGroup_AccountDistributionTriggerType.Registration, null, false, false, false, false, false, true, false);

                this.dirtyHandler.clean();
                this.onLoadData();
                //this.updateTabCaption();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.accountingService.getAccountDistributionHeads(true, false, true).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.accountDistributionHeadId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.accountDistributionHeadId) {
                    this.accountDistributionHeadId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private updateTabCaption() {
        this.translationService.translate("economy.accounting.accountdistribution.accountdistribution").then((term) => {
            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: term + " " + this.accountDistribution.name,
                id: this.accountDistributionHeadId,
            });
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deleteAccountDistribution(this.accountDistribution.accountDistributionHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.accountDistribution);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    protected copy() {
        this.isNew = true;
        this.accountDistributionHeadId = 0;
        this.accountDistribution.accountDistributionHeadId = 0;

        this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
            guid: this.guid,
        });

        this.dirtyHandler.setDirty();
    }

    private new() {
        this.isNew = true;
        this.accountDistributionHeadId = 0;

        this.accountDistribution = new AccountDistributionHeadDTO();
        this.accountDistribution.rows = new Array<AccountDistributionRowDTO>();

        //Set default values
        if (this.isAutomaticAccountDistribution) {
            this.accountDistribution.type = SoeAccountDistributionType.Auto;
            this.accountDistribution.sort = 0;
            this.accountDistribution.calculationType = 1;
            this.accountDistribution.amount = 0;
            this.accountDistribution.amountOperator = 2;
            this.accountDistribution.triggerType = 1;
            this.accountDistributionTriggerTypeChanged(1);
        }
        if (this.isPeriodAccountDistribution) {
            this.accountDistribution.type = SoeAccountDistributionType.Period;
            this.accountDistribution.keepRow = false;
            this.accountDistribution.sort = 0;
            this.accountDistribution.calculationType = 1;
            this.accountDistributionCalculationTypeChanged(this.accountDistribution.calculationType);
            this.accountDistribution.triggerType = 1;
            this.accountDistributionTriggerTypeChanged(1);
            this.accountDistribution.periodType = 1;
            this.accountDistribution.periodValue = 1;
            this.accountDistribution.dayNumber = 31;
            this.accountDistribution.startDate = CalendarUtility.getDateToday();
            this.accountDistribution.startDate.setDate(31);
            if (this.voucherSeriesTypeFilterOptions != null && this.voucherSeriesTypeFilterOptions.length > 0 && this.accountdistributionVoucherSeriesType != null)
                this.accountDistribution.voucherSeriesTypeId = this.accountdistributionVoucherSeriesType;
            this.accountDistribution.amount = 0;
            this.accountDistribution.amountOperator = 2;
        }

        //Load account dims
        this.loadAccountDimStd();
    }

    private showValidationError() {
        const errors = this['edit'].$error;
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (errors['name'])
                mandatoryFieldKeys.push("common.name");

            if (errors['sort'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.sorting");

            if (errors['calculationtype'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.calculationtype");

            if (errors['triggertype'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.type");

            if (errors['voucherseriestypeid'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.voucherserie");

            if (errors['periodtype'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.periodtype");

            if (errors['periodvalue'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.periodvalue");

            if (errors['daynumber'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.dayinperiod");

            if (errors['diff'])
                validationErrorKeys.push("economy.accounting.accountdistribution.diffinrows");

            if (errors['startdate'])
                mandatoryFieldKeys.push("economy.accounting.accountdistribution.startdate");
        });
    }
}