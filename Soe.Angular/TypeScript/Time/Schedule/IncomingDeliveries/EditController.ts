import { ICoreService } from "../../../Core/Services/CoreService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Guid } from "../../../Util/StringUtility";
import { IncomingDeliveryHeadDTO, IncomingDeliveryRowDTO, IncomingDeliveryTypeDTO } from "../../../Common/Models/StaffingNeedsDTOs";
import { ShiftTypeDTO } from "../../../Common/Models/ShiftTypeDTO";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { TaskAndDeliveryFunctions, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { DailyRecurrenceParamsDTO, DailyRecurrenceRangeDTO } from "../../../Common/Models/DailyRecurrencePatternDTOs";
import { DailyRecurrencePatternController } from "../../../Common/Dialogs/DailyRecurrencePattern/DailyRecurrencePatternController";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { Constants } from "../../../Util/Constants";
import { Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

enum ValidationError {
    Unknown = 0,
    LengthIsLowerThanAllowed = 1,
    MinSplitLengthIsLowerThanAllowed = 2,
    StartLaterThanStop = 3,
    PlannedTimeLowerThanLength = 4,
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    //Company settings
    private useAccountsHierarchy: boolean;
    companySettingMinLength: number = 15;

    private accounts: AccountDTO[];
    private incomingDeliveryHeadId: number;
    terms: any;
    incomingDeliveryHead: IncomingDeliveryHeadDTO;
    selectedRow: IncomingDeliveryRowDTO;
    private shiftTypes: ShiftTypeDTO[];
    private incomingDeliveryTypes: IncomingDeliveryTypeDTO[];
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    private modal;
    isModal = false;
    rowNr: number = 0;
    private edit: ng.IFormController;
    saveFunctions: any = [];

    private _selectedAccount: AccountDTO;
    public get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    public set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        if (account) {
            this.incomingDeliveryHead.accountId = account.accountId;
            this.incomingDeliveryHead.accountName = account.name;
        }
    }

    //@ngInject
    constructor(
        protected $uibModal,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
            this.focusService.focusByName("ctrl_incomingDeliveryHead_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    //SETUP

    public onInit(parameters: any) {
        this.incomingDeliveryHeadId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public closeModal(modified: boolean) {
        if (this.isModal) {
            if (modified && this.incomingDeliveryHeadId) {
                this.modal.close(this.incomingDeliveryHeadId);
            } else {
                this.modal.dismiss();
            }
        }
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.incomingDeliveryHeadId, recordId => {
            if (!this.isNew && recordId !== this.incomingDeliveryHeadId) {
                this.incomingDeliveryHeadId = recordId;
                this.onLoadData();
            }
        });
    }

    //LOOKUPS

    private doLookups() {
        if (this.incomingDeliveryHeadId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadCompanySettings(),
                () => this.loadShiftTypes(),
                () => this.loadIncomingDeliveryTypes(),
                () => this.loadAccountStringIdsByUserFromHierarchy(),
            ]).then(x => {
                this.onLoadData()
            });
        } else {
            this.new(false);

            return this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadCompanySettings(),
                () => this.loadShiftTypes(),
                () => this.loadIncomingDeliveryTypes(),
                () => this.loadAccountStringIdsByUserFromHierarchy(),
            ])
        }
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.save",
            "core.saveandnew",
            "core.saveandclose",
            "core.warning",
            "common.name",
            "time.schedule.incomingdelivery.reloadrowswarning",
            "time.schedule.incomingdelivery.incomingdelivery",
            "core.notselected",
            "core.notspecified",
            "time.schedule.incomingdelivery.hasrecalculatedwarning",
            "time.schedule.incomingdelivery.validation.startlaterthanstop",
            "time.schedule.incomingdelivery.validation.plannedtimelowerthanlength",
            "time.schedule.incomingdelivery.validation.lengthislowerthanallowed",
            "time.schedule.incomingdelivery.validation.minsplitlengthislowerthanallowed",
            "time.schedule.incomingdelivery.new"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.saveFunctions.push({ id: TaskAndDeliveryFunctions.Save, name: terms["core.save"] });
            this.saveFunctions.push({ id: TaskAndDeliveryFunctions.SaveAndNew, name: terms["core.saveandnew"] });
            this.saveFunctions.push({ id: TaskAndDeliveryFunctions.SaveAndClose, name: terms["core.saveandclose"] });
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.companySettingMinLength = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(false, false, false, false, false, false).then(x => {
            this.shiftTypes = x;

            // Insert empty
            let shiftType: ShiftTypeDTO = new ShiftTypeDTO();
            shiftType.shiftTypeId = 0;
            shiftType.name = this.terms['core.notspecified'];
            shiftType.color = Constants.SHIFT_TYPE_UNSPECIFIED_COLOR;
            this.shiftTypes.splice(0, 0, shiftType);

        });
    }

    private loadIncomingDeliveryTypes(): ng.IPromise<any> {
        return this.scheduleService.getIncomingDeliveryTypes(true).then(x => {
            this.incomingDeliveryTypes = x;

            // Insert empty
            let incomingDeliveryType: IncomingDeliveryTypeDTO = new IncomingDeliveryTypeDTO();
            incomingDeliveryType.incomingDeliveryTypeId = 0;
            incomingDeliveryType.name = this.terms['core.notselected'];
            this.incomingDeliveryTypes.splice(0, 0, incomingDeliveryType);

        });
    }

    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];

        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true, false, false, true).then(x => {
            this.accounts = x;

            // Insert empty
            if (this.isNew) {
                this._selectedAccount = this.accounts.find(a => a.accountId);
                let empty: AccountDTO = new AccountDTO;
                empty.accountId = null;
                empty.name = '';
                this.accounts.splice(0, 0, empty);
            }

        });
    }

    private tryReloadRows() {
        if (!this.incomingDeliveryHead)
            return;

        const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.incomingdelivery.reloadrowswarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val) {
                return this.progress.startLoadingProgress([
                    () => this.reloadRows(),
                ])
            }
        });

    }

    private reloadRows(): ng.IPromise<any> {
        return this.scheduleService.getIncomingDelivery(this.incomingDeliveryHeadId, true, true, true, true).then(x => {
            this.incomingDeliveryHead.rows = x.rows;
            this.recalculateRows(false);
        });
    }

    private onLoadData(): ng.IPromise<any> {
        return this.scheduleService.getIncomingDelivery(this.incomingDeliveryHeadId, true, true, true, true).then(x => {
            this.isNew = false;
            this.incomingDeliveryHead = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.incomingdelivery.incomingdelivery"] + ' ' + this.incomingDeliveryHead.name);
            if (this.incomingDeliveryHead) {
                this.selectedAccount = _.find(this.accounts, a => a.accountId == this.incomingDeliveryHead.accountId);
                this.setRecurrenceInfo();
                this.recalculateRows(true);
            }
        });
    }

    //ACTIONS

    private save(newAfterSave: boolean, closeAfterSave: boolean) {
        this.progress.startSaveProgress((completion) => {
            if (this.incomingDeliveryHead?.rows) {
                _.forEach(this.incomingDeliveryHead.rows, row => {
                    if (row.selectedIncomingDeliveryType && row.selectedIncomingDeliveryType.incomingDeliveryTypeId != row.incomingDeliveryTypeId)
                        row.incomingDeliveryTypeId = row.selectedIncomingDeliveryType.incomingDeliveryTypeId;
                    if (row.selectedShiftType && row.selectedShiftType.shiftTypeId != row.shiftTypeId)
                        row.shiftTypeId = row.selectedShiftType.shiftTypeId;
                    row.minSplitLength = CalendarUtility.timeSpanToMinutes(row.minSplitLengthTimeSpan);
                    row.length = CalendarUtility.timeSpanToMinutes(row.lengthTimeSpan);
                    row.totalLength = CalendarUtility.timeSpanToMinutes(row.totalLengthTimeSpan);
                });
            }
            this.scheduleService.saveIncomingDelivery(this.incomingDeliveryHead).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {

                        if (this.incomingDeliveryHeadId == 0) {

                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.incomingDeliveryHead.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.incomingDeliveryHeadId = result.integerValue;
                        this.incomingDeliveryHead.incomingDeliveryHeadId = result.integerValue;

                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.incomingDeliveryHead);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                if (this.isModal)
                    this.closeModal(true);
                else {
                    if (newAfterSave) {
                        this.new(true);
                    } else if (closeAfterSave) {
                        this.closeMe(true);
                    } else {
                        this.onLoadData();
                    }
                }
            });
    }


    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.scheduleService.getIncomingDeliveriesGrid(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.incomingDeliveryHeadId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.incomingDeliveryHeadId) {
                    this.incomingDeliveryHeadId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteIncomingDelivery(this.incomingDeliveryHead.incomingDeliveryHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.incomingDeliveryHead, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    protected copy() {
        if (!this.incomingDeliveryHead)
            return;
        super.copy();
        this.isNew = true;
        this.incomingDeliveryHeadId = 0;
        this.incomingDeliveryHead.name = "";
        this.incomingDeliveryHead.incomingDeliveryHeadId = 0;
        this.incomingDeliveryHead.created = null;
        this.incomingDeliveryHead.createdBy = "";
        this.incomingDeliveryHead.modified = null;
        this.incomingDeliveryHead.modifiedBy = "";
        _.forEach(this.incomingDeliveryHead.rows, row => {
            row.incomingDeliveryHeadId = 0;
            row.incomingDeliveryRowId = 0;
        });
    }

    private new(keepStartDate: boolean) {
        this.messagingHandler.publishSetTabLabelNew(this.guid);

        const startDate: Date = keepStartDate && this.incomingDeliveryHead ? this.incomingDeliveryHead.startDate : CalendarUtility.getDateToday();
        const accountId = keepStartDate && this.incomingDeliveryHead ? this.incomingDeliveryHead.accountId : undefined;

        this.isNew = true;
        this.incomingDeliveryHeadId = 0;
        this.incomingDeliveryHead = new IncomingDeliveryHeadDTO;
        this.incomingDeliveryHead.accountId = accountId;
        this.incomingDeliveryHead.startDate = startDate;
        this.incomingDeliveryHead.excludedDates = [];
        this.setRecurrenceInfo();

        this.focusService.focusByName("ctrl_incomingDeliveryHead_name");
    }

    private executeSaveFunction(option) {
        switch (option.id) {
            case TaskAndDeliveryFunctions.Save:
                this.save(false, false);
                break;
            case TaskAndDeliveryFunctions.SaveAndNew:
                this.save(true, false);
                break;
            case TaskAndDeliveryFunctions.SaveAndClose:
                this.save(false, true);
                break;
        }
    }

    //HELP-METHODS

    private addRow() {
        if (this.incomingDeliveryHead) {
            let row: IncomingDeliveryRowDTO = new IncomingDeliveryRowDTO();
            row.name = '';
            row.description = '';
            row.shiftTypeId = 0;
            row.incomingDeliveryTypeId = 0;
            row.nbrOfPackages = 1;
            row.nbrOfPersons = 1;
            row.length = this.companySettingMinLength;
            row.startTime = CalendarUtility.getDateToday();
            row.stopTime = CalendarUtility.getDateToday();
            row.offsetDays = 0;
            row.minSplitLength = this.companySettingMinLength;
            row.onlyOneEmployee = false;
            row.allowOverlapping = false;
            row.minSplitLengthTimeSpan = CalendarUtility.minutesToTimeSpan(this.companySettingMinLength);
            row.lengthTimeSpan = CalendarUtility.minutesToTimeSpan(this.companySettingMinLength);
            this.refreshRelations(row);

            if (!this.incomingDeliveryHead.rows)
                this.incomingDeliveryHead.rows = [];
            this.incomingDeliveryHead.rows.push(row);
        }
    }

    private deleteRow(row: IncomingDeliveryRowDTO) {
        if (this.incomingDeliveryHead?.rows && row) {
            _.pull(this.incomingDeliveryHead.rows, row);
            this.dirtyHandler.setDirty();
        }
    }

    private setRowNr(row: IncomingDeliveryRowDTO) {
        if (row) {
            this.rowNr = this.rowNr + 1;
            row['rowNr'] = this.rowNr;
        }
    }

    private refreshRelations(row: IncomingDeliveryRowDTO) {
        if (!row)
            return;

        if (this.loadIncomingDeliveryTypes)
            row.selectedIncomingDeliveryType = (_.filter(this.incomingDeliveryTypes, { incomingDeliveryTypeId: row.incomingDeliveryTypeId }))[0];
        if (this.shiftTypes)
            row.selectedShiftType = (_.filter(this.shiftTypes, { shiftTypeId: row.shiftTypeId }))[0];

        this.calculateTotalLength(row);
        this.calculateLength(row);
        this.setRowNr(row);
    }

    private hasRows(): boolean {
        return this.incomingDeliveryHead?.rows && this.incomingDeliveryHead.rows.length > 0;
    }

    private calculateStopTime(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            if (row && row.length > 0) {
                row.stopTime = row.startTime.addMinutes(row.length);
            }
        });
    }

    private calculateTotalLength(row: IncomingDeliveryRowDTO) {
        if (!row)
            return;

        if (row.nbrOfPackages && row.nbrOfPackages > 0 && row.selectedIncomingDeliveryType && row.selectedIncomingDeliveryType.incomingDeliveryTypeId > 0) {
            row.totalLength = row.nbrOfPackages * row.selectedIncomingDeliveryType.length;
        }
        else {
            row.totalLength = 0;
        }
        row.totalLengthTimeSpan = CalendarUtility.minutesToTimeSpan(row.totalLength);
    }

    private calculateLength(row: IncomingDeliveryRowDTO) {
        if (!row)
            return;

        if (row.nbrOfPersons === 0 || row.totalLength === 0) {
            row.length = 0;
        }
        else if (row.nbrOfPersons > 0 && row.totalLength > 0) {
            row.length = (row.totalLength / row.nbrOfPersons).round(0);
        }
        row.lengthTimeSpan = CalendarUtility.minutesToTimeSpan(row.length);
    }

    private recalculateRows(alertOnChanges: boolean = false) {
        let hasChangedRows = false;

        _.forEach(this.incomingDeliveryHead.rows, row => {
            //default values and formats
            if (!row.shiftTypeId)
                row.shiftTypeId = 0;
            row.minSplitLengthTimeSpan = CalendarUtility.minutesToTimeSpan(row.minSplitLength);
            row.lengthTimeSpan = CalendarUtility.minutesToTimeSpan(row.length);
            row.totalLengthTimeSpan = CalendarUtility.minutesToTimeSpan(row.totalLength);

            //refresh relations and look for changes on type
            const length = row.length;
            this.refreshRelations(row);
            if (length !== row.length) {
                this.calculateStopTime(row);
                hasChangedRows = true;
            }

        });

        if (alertOnChanges && hasChangedRows) {
            // Show verification dialog           
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.incomingdelivery.hasrecalculatedwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            modal.result.then(val => {
            });
        }
    }

    // EVENTS

    private incomingDeliveryTypeChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            this.calculateTotalLength(row);
            this.calculateLength(row);
        });
    }

    private nbrOfPackagesChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            this.calculateTotalLength(row);
            this.calculateLength(row);
        });
    }

    private nbrOfPersonsChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            this.calculateLength(row);
            this.calculateStopTime(row);
        });
    }

    private startTimeChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            this.calculateStopTime(row);
        });
    }

    private stopTimeChanged(row: IncomingDeliveryRowDTO) {
        //Do nothing
    }

    private lengthTimeSpanChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            row.length = CalendarUtility.timeSpanToMinutes(row.lengthTimeSpan);
        });
    }

    private minSplitLengthTimeSpanChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            row.minSplitLength = CalendarUtility.timeSpanToMinutes(row.minSplitLengthTimeSpan);
        });
    }

    private onlyOneEmployeeChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            if (row?.onlyOneEmployee && row.allowOverlapping)
                row.allowOverlapping = false; //Not allowed same time with allowOverlapping
        });
    }

    private allowOverLappingChanged(row: IncomingDeliveryRowDTO) {
        this.$timeout(() => {
            if (row?.onlyOneEmployee && row.allowOverlapping)
                row.onlyOneEmployee = false; //Not allowed same time with onlyoneperson
        });
    }

    //VALIDATION

    private isDisabled() {
        return !this.dirtyHandler.isDirty || this.edit.$invalid;
    }

    private isRowValid(row: IncomingDeliveryRowDTO): boolean {
        return this.validateRow(row).length > 0;
    }

    private validateRow(row: IncomingDeliveryRowDTO): ValidationError[] {
        let validationErrors: ValidationError[] = [];

        if (row.startTime && row.stopTime && row.startTime > row.stopTime) {
            validationErrors.push(ValidationError.StartLaterThanStop);
        }
        if (!row.length || (row.stopTime && row.stopTime.diffMinutes(row.startTime) < row.length)) {
            validationErrors.push(ValidationError.PlannedTimeLowerThanLength);
        }
        if (!row.length || row.length < this.companySettingMinLength) {
            validationErrors.push(ValidationError.LengthIsLowerThanAllowed);
        }
        if (!row.minSplitLength || row.minSplitLength < this.companySettingMinLength) {
            validationErrors.push(ValidationError.MinSplitLengthIsLowerThanAllowed);
        }

        return validationErrors;
    }

    protected showRowValidation(row: IncomingDeliveryRowDTO) {
        const validationErrors: ValidationError[] = this.validateRow(row);
        if (validationErrors.length == 0)
            return;

        let message: string = '';

        _.forEach(validationErrors, validationError => {
            if (validationError == ValidationError.StartLaterThanStop) {
                message += this.terms["time.schedule.incomingdelivery.validation.startlaterthanstop"];
                message += "<br />";
            }
            if (validationError == ValidationError.PlannedTimeLowerThanLength) {
                message += this.terms["time.schedule.incomingdelivery.validation.plannedtimelowerthanlength"];
                message += "<br />";
            }
            if (validationError == ValidationError.LengthIsLowerThanAllowed) {
                message += this.terms["time.schedule.incomingdelivery.validation.lengthislowerthanallowed"];
                message += "<br />";
            }
            if (validationError == ValidationError.MinSplitLengthIsLowerThanAllowed) {
                message += this.terms["time.schedule.incomingdelivery.validation.minsplitlengthislowerthanallowed"];
                message += "<br />";
            }
        });
        this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);

    }

    private showValidationError() {

        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.incomingDeliveryHead) {
                const errors = this['edit'].$error;

                if (!this.incomingDeliveryHead.name) {
                    mandatoryFieldKeys.push("common.name");
                }

                if (errors['length'])
                    validationErrorStrings.push(this.terms["time.schedule.incomingdelivery.validation.lengthislowerthanallowed"]);
                if (errors['stopTime'])
                    validationErrorStrings.push(this.terms["time.schedule.incomingdelivery.validation.startlaterthanstop"]);
                if (errors['minSplitLength'])
                    validationErrorStrings.push(this.terms["time.schedule.incomingdelivery.validation.minsplitlengthislowerthanallowed"]);
            }
        });

    }

    private openRecurrencePatternDialog() {
        let params = new DailyRecurrenceParamsDTO(this.incomingDeliveryHead);

        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/DailyRecurrencePattern/Views/dailyRecurrencePattern.html"),
            controller: DailyRecurrencePatternController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                pattern: () => { return params.pattern },
                range: () => { return params.range },
                excludedDates: () => { return this.incomingDeliveryHead.excludedDates },
                date: () => { return params.date },
                hideRange: () => { return false }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                params.parseResult(this.incomingDeliveryHead, result);
                this.setRecurrenceInfo();
                this.dirtyHandler.setDirty();
            }
        });
    }

    private setRecurrenceInfo() {
        if (this.incomingDeliveryHead) {
            DailyRecurrenceRangeDTO.setRecurrenceInfo(this.incomingDeliveryHead, this.translationService);
            this.scheduleService.getRecurrenceDescription(this.incomingDeliveryHead.recurrencePattern).then((x) => {
                this.incomingDeliveryHead["patternDescription"] = x;
            });
            if (this.incomingDeliveryHead.excludedDates && this.incomingDeliveryHead.excludedDates.length > 0)
                this.incomingDeliveryHead["excludedDatesDescription"] = _.map(this.incomingDeliveryHead.excludedDates, d => d.toLocaleDateString()).join(', ');
        }
    }
}
