import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature } from "../../../Util/CommonEnumerations";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IScheduleService } from "../ScheduleService";
import { DayTypeDTO } from "../../../Common/Models/DayTypeDTO";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    dayTypeId: number;
    private dayType: DayTypeDTO;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    terms: any = [];

    showNavigationButtons: any;
    readPermission: boolean;
    standardWeekdayFrom: any = [];
    standardWeekdayTo: any = [];
    //@ngInject
    constructor(
        private scheduleService: IScheduleService,
        private $q: ng.IQService,
        private focusService: IFocusService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }


    public onInit(parameters: any) {
        this.dayTypeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_ScheduleSettings_DayTypes, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_ScheduleSettings_DayTypes].readPermission
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_DayTypes].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.dayTypeId, recordId => {
            if (recordId !== this.dayTypeId) {
                this.dayTypeId = recordId;
                this.onLoadData();
            }
        });
    }
    protected doLookups(): ng.IPromise<any> {
        return this.loadTerms().then(() => {
            return this.$q.all([

            ]).then(x => {

            });
        });
    }
    private onLoadData() {
        if (this.dayTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadDayType()
            ]);
        } else {
            this.new();
        }
    }
    private loadDayType(): ng.IPromise<any> {
        return this.scheduleService.getDayType(this.dayTypeId).then((x) => {
            this.isNew = false;
            this.loadDay();
            this.dayType = x;
            if (this.dayType.standardWeekdayFrom >= 0)
                this.dayType.standardWeekdayFrom++;
            if (this.dayType.standardWeekdayTo >= 0)
                this.dayType.standardWeekdayTo++;
            this.dirtyHandler.clean();
           
           
        });
    }
    private loadDay() : ng.IPromise < any > {
        return this.scheduleService.getDaysOfWeekDict(true).then((x) => {
            this.standardWeekdayFrom = [];
            this.standardWeekdayTo = [];
            this.standardWeekdayFrom = x;
            this.standardWeekdayTo = x;
        });

    }
    private loadTerms() {
        var keys: string[] = [
            "tim.schedule.daytype.daytype"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }
    
    // ACTIONS
    private save() {
        if (this.dayType.standardWeekdayFrom < 0)
            this.dayType.standardWeekdayFrom = null;
        else
            this.dayType.standardWeekdayFrom--;
        if (this.dayType.standardWeekdayTo < 0)
            this.dayType.standardWeekdayTo = null;
        else
            this.dayType.standardWeekdayTo--;

        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveDayType(this.dayType).then((result) => {

                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.dayTypeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.dayType.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.dayTypeId = result.integerValue;
                        this.dayType.dayTypeId = this.dayTypeId;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.dayType);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadDayType();
            });

    }
    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.scheduleService.getDayTypes().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.dayTypeId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.dayTypeId) {
                    this.dayTypeId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }
    // HELP-METHODS
    private delete() {
        if (!this.dayType.dayTypeId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteDayType(this.dayType.dayTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.dayType, true);
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

    protected copy() {
        super.copy();
        this.isNew = true;
        this.dayTypeId = 0;
        this.dayType.name = "";
        this.focusService.focusByName("ctrl_dayType_name");
        this.loadDay();
    }

    private new() {
        this.isNew = true;
        this.dayTypeId = 0;
        this.dayType = new DayTypeDTO;
        this.loadDay();
        this.dayType.standardWeekdayFrom = -1;
        this.dayType.standardWeekdayTo = -1;
    }

    // VALIDATION
}
