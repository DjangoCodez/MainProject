import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IScheduleService } from "../ScheduleService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private holidayId: number;
    holiday: any;
    dayTypesDict: SmallGenericType[];
    sysHolidayTypes: any[];
    holidayDate: Date;
    terms: any = [];

    //@ngInject
    constructor(
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.loadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.holidayId = parameters.id;
        this.guid = parameters.guid;
        this.navigatorRecords = parameters.navigatorRecords;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_Holidays_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            if (this.holiday.sysHolidayTypeId > 0)
                this.holiday.date = CalendarUtility.DefaultDateTime();
            else
                this.holiday.date = this.holidayDate;

            this.scheduleService.saveHoliday(this.holiday).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.holidayId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.holiday.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.holidayId = result.integerValue;
                        this.holiday.holidayId = result.integerValue;
                    }

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.holiday);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadData();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.scheduleService.getHolidays().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.holidayId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.holidayId) {
                    this.holidayId = recordId;
                    this.loadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteHoliday(this.holiday.holidayId).then((result) => {
                if (result.success) {
                    completion.completed(this.holiday);
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
        super.copy();
        this.isNew = true;
        this.holidayId = 0;
        this.holiday.holidayId = 0;

        this.holiday.created = null;
        this.holiday.createdBy = "";
        this.holiday.modified = null;
        this.holiday.modifiedBy = "";

        this.focusService.focusByName("ctrl_holiday_name");
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.holiday) {
                if (!this.holiday.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.holiday.date && (!this.holiday.sysHolidayTypeId || this.holiday.sysHolidayTypeId === 0)) {
                    mandatoryFieldKeys.push("common.date");
                }
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_Holidays_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_Holidays_Edit].modifyPermission;
    }

    private loadTerms() {
        var keys: string[] = [
            "time.schedule.daytype.newholiday",
            "time.schedule.daytype.holiday"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private doLookups() {
        return this.loadTerms().then(() => {
            return this.progress.startLoadingProgress([() => this.loadDayTypes(), () => this.loadSysDayHolidayTypes()])
        });
    }

    private setDate() {
        if (this.holiday && this.holiday.date) {
            this.holiday.date = CalendarUtility.convertToDate(this.holiday.date);
            this.holidayDate = CalendarUtility.convertToDate(this.holiday.date);
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.holidayId, recordId => {
            if (recordId !== this.holidayId) {
                this.holidayId = recordId;
                this.loadData();
            }
        });
    }

    private loadDayTypes(): ng.IPromise<any> {
        return this.scheduleService.getDayTypesDict(false).then(x => {
            this.dayTypesDict = x;
        });
    }

    private loadSysDayHolidayTypes(): ng.IPromise<any> {
        this.sysHolidayTypes = [];
        this.sysHolidayTypes.push({ id: 0, name: "" });
        return this.scheduleService.getSysHolidayTypes().then((x) => {
            _.forEach(x, (y: any) => {
                this.sysHolidayTypes.push({ id: y.sysHolidayTypeId, name: y.name })
            });
        });
    }

    private loadData(): ng.IPromise<any> {
        if (this.holidayId > 0) {
            return this.progress.startLoadingProgress([() => {
                return this.scheduleService.getHoliday(this.holidayId).then((x) => {
                    this.isNew = false;
                    this.holiday = x;
                    this.dirtyHandler.clean();
                    this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.daytype.holiday"] + ' ' + this.holiday.name);
                    this.setDate();
                });
            }]);
        } else {
            this.isNew = true;
            this.holidayId = 0;
            this.holiday = {};
        }

    }
}
