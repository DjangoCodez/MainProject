import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { TimeCodeAdditionDeductionDTO } from "../../../Common/Models/TimeCode";
import { Feature, SoeTimeCodeType, TermGroup, SoeEntityState, TermGroup_ExpenseType, TermGroup_TimeCodeRegistrationType, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timeCodeId: number;
    private timeCode: TimeCodeAdditionDeductionDTO;
    private expenseTypes: ISmallGenericType[] = [];
    private registrationTypes: ISmallGenericType[] = [];

    // Flags
    private showStopAtDateStart: boolean;
    private showStopAtDateStop: boolean;
    private showStopAtPrice: boolean;
    private showStopAtVat: boolean;
    private showStopAtComment: boolean;
    private showStopAtAccounting: boolean;
    private hideShowInTerminal: boolean = true;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeCodeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeCodeAdditionDeduction_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeAdditionDeduction_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeAdditionDeduction_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeCodeId);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeCodeId, recordId => {
            if (recordId !== this.timeCodeId) {
                this.timeCodeId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadExpenseTypes(),
            this.loadRegistrationTypes()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        this.loadCompanySettings();
        if (this.timeCodeId) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.time.timecodeadditiondeductions.timecodeadditiondeduction"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.PossibilityToRegisterAdditionsInTerminal);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.hideShowInTerminal = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PossibilityToRegisterAdditionsInTerminal);
        });
    }

    private loadExpenseTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ExpenseType, false, true).then(x => {
            this.expenseTypes = x;
        });
    }

    private loadRegistrationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeCodeRegistrationType, false, true).then(x => {            
            this.registrationTypes = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.timeService.getTimeCode(SoeTimeCodeType.AdditionDeduction, this.timeCodeId, true, true).then(x => {
            this.isNew = false;
            this.timeCode = x;            
            if (this.timeCode.registrationType == TermGroup_TimeCodeRegistrationType.Unknown)
                this.timeCode.registrationType = null;            
            this.expenseTypeChanged();
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.timecodeadditiondeductions.timecodeadditiondeduction"] + ' ' + this.timeCode.name);
            
        });
    }

    private new() {
        this.isNew = true;
        this.timeCodeId = 0;
        this.timeCode = new TimeCodeAdditionDeductionDTO();
        this.timeCode.type = SoeTimeCodeType.AdditionDeduction;
        this.timeCode.state = SoeEntityState.Active;
        this.timeCode.expenseType = TermGroup_ExpenseType.Mileage;        
        this.expenseTypeChanged();
    }

    // EVENTS

    private expenseTypeChanged() {
        this.$timeout(() => {
            switch (this.timeCode.expenseType) {
                case TermGroup_ExpenseType.Mileage:
                    this.showStopAtDateStart = true;
                    this.showStopAtDateStop = false;
                    this.showStopAtPrice = false;
                    this.showStopAtVat = false;
                    this.showStopAtComment = true;
                    this.showStopAtAccounting = true;
                    break;
                case TermGroup_ExpenseType.AllowanceDomestic:
                    this.showStopAtDateStart = true;
                    this.showStopAtDateStop = true;
                    this.showStopAtPrice = false;
                    this.showStopAtVat = false;
                    this.showStopAtComment = true;
                    this.showStopAtAccounting = true;
                    break;
                case TermGroup_ExpenseType.AllowanceAbroad:
                    this.showStopAtDateStart = true;
                    this.showStopAtDateStop = true;
                    this.showStopAtPrice = true;
                    this.showStopAtVat = false;
                    this.showStopAtComment = true;
                    this.showStopAtAccounting = true;
                    break;
                case TermGroup_ExpenseType.Expense:
                case TermGroup_ExpenseType.TravellingTime:
                case TermGroup_ExpenseType.Time:
                    this.showStopAtDateStart = true;
                    this.showStopAtDateStop = false;
                    this.showStopAtPrice = true;
                    this.showStopAtVat = true;
                    this.showStopAtComment = true;
                    this.showStopAtAccounting = true;
                    break;
            }
        });
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.timeCodeId = this.timeCode.timeCodeId = 0;
        _.forEach(this.timeCode.payrollProducts, p => {
            p.timeCodePayrollProductId = 0;
            p.timeCodeId = 0;
        });
        _.forEach(this.timeCode.invoiceProducts, p => {
            p.timeCodeInvoiceProductId = 0;
            p.timeCodeId = 0;
        });

        this.focusService.focusByName("ctrl_timeCode_code");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeCode(this.timeCode).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeCodeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeCode.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                    this.timeCodeId = result.integerValue;
                    this.timeCode.timeCodeId = this.timeCodeId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeCode);
                    }
                    
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.timeService.getTimeCodesGrid(SoeTimeCodeType.AdditionDeduction, false, true).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeCodeId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeCodeId) {
                    this.timeCodeId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimeCode(this.timeCode.timeCodeId).then(result => {
                if (result.success) {
                    completion.completed(this.timeCode, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeCode) {
                if (!this.timeCode.code)
                    mandatoryFieldKeys.push("common.code");
                if (!this.timeCode.name)
                    mandatoryFieldKeys.push("common.name");                
                if (!this.timeCode.registrationType)
                    mandatoryFieldKeys.push("time.time.timecode.registrationtype");
            }
        });
    }
}