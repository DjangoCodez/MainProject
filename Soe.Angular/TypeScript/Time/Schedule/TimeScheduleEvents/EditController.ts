import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { TimeScheduleEventDTO, TimeScheduleEventMessageGroupDTO } from "../../../Common/Models/TimeScheduleEventDTO";
import { Guid } from "../../../Util/StringUtility";
import { MessageGroupMemberDTO, MessageGroupDTO } from "../../../Common/Models/MessageDTOs";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Constants } from "../../../Util/Constants";
import { Feature, SoeEntityType } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Init parameters
    private timeScheduleEventId: number;
    timeScheduleEvent: TimeScheduleEventDTO;
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    private modal;
    isModal = false;
    groupMembersLength: number = 0;

    groupMembers: MessageGroupMemberDTO[] = [];
    messageGroups: ISmallGenericType[] = [];
    selectedDate: Date = new Date;

    terms : any = [];

    //@ngInject
    constructor(
        protected $uibModal,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
            this.focusService.focusByName("ctrl_timeScheduleEvent_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeScheduleEventId = parameters.id;
        this.guid = parameters.guid;
        this.navigatorRecords = parameters.navigatorRecords;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Schedule_SchedulePlanning_SalesCalender, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeScheduleEventId, recordId => {
            if (recordId !== this.timeScheduleEventId) {
                this.timeScheduleEventId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_SchedulePlanning_SalesCalender].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_SchedulePlanning_SalesCalender].modifyPermission;
    }

    // LOOKUPS

    private doLookups() {
        return this.loadTerms();
        
    }

    private loadTerms() {
        var keys: string[] = [
            "time.schedule.timescheduleevent.event",
            "time.schedule.timescheduleevent.new_event"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeScheduleEventId > 0) {
            return this.scheduleService.getTimeScheduleEvent(this.timeScheduleEventId).then((x) => {
                this.isNew = false;
                this.timeScheduleEvent = x;
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.timescheduleevent.event"] + ' ' + this.timeScheduleEvent.name);
                this.selectedDate = CalendarUtility.convertToDate(this.timeScheduleEvent.date);
                this.loadAllMessageGroups();
                
            });
        } else {
            this.isNew = true;
            this.timeScheduleEventId = 0;
            this.timeScheduleEvent = new TimeScheduleEventDTO;
        }
    }

    private loadAllMessageGroups(): ng.IPromise<any> {
        this.groupMembers = [];
        return this.coreService.getMessageGroupsDict(false).then(x => {
            this.messageGroups = x;
            _.forEach(x, (group: ISmallGenericType) => {
                var moveItem = new MessageGroupMemberDTO;
                moveItem.name = group.name;
                moveItem.username = group.name;
                moveItem.recordId = group.id;
                moveItem.entity = SoeEntityType.MessageGroup;
                var allreadyThere = _.find(this.timeScheduleEvent.timeScheduleEventMessageGroups, { messageGroupId: group.id });
                if (allreadyThere)
                    this.groupMembers.push(moveItem);
                this.groupMembersLength = this.groupMembers.length;
            });
        });
    }

    //ACTIONS

    public save() {
        if (this.selectedDate)
            this.timeScheduleEvent.date = this.selectedDate;
        this.timeScheduleEvent.timeScheduleEventMessageGroups = [];
        _.forEach(this.groupMembers, (group: any) => {
            var moveItem = new TimeScheduleEventMessageGroupDTO;
            moveItem.timeScheduleEventId = this.timeScheduleEventId;
            moveItem.messageGroupId = group.recordId;
            this.timeScheduleEvent.timeScheduleEventMessageGroups.push(moveItem);
        });

        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveTimeScheduleEvent(this.timeScheduleEvent).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeScheduleEventId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeScheduleEvent.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.timeScheduleEventId = result.integerValue;
                        this.timeScheduleEvent.timeScheduleEventId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeScheduleEvent);
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
                    this.closeModal();
                else
                    this.onLoadData();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.scheduleService.getTimeScheduleEvents(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeScheduleEventId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeScheduleEventId) {
                    this.timeScheduleEventId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteTimeScheduleEvent(this.timeScheduleEvent.timeScheduleEventId).then((result) => {
                if (result.success) {
                    completion.completed(this.timeScheduleEvent);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(false);
        });
    }

    protected copy() {
        if (!this.timeScheduleEvent)
            return;

        super.copy();

        this.isNew = true;
        this.timeScheduleEventId = 0;
        this.timeScheduleEvent.timeScheduleEventId = 0;
        this.timeScheduleEvent.created = null;
        this.timeScheduleEvent.createdBy = "";
        this.timeScheduleEvent.modified = null;
        this.timeScheduleEvent.modifiedBy = "";

        this.focusService.focusByName("ctrl_timeScheduleEvent_name");
    }

    //EVENTS

    public closeModal() {
        if (this.isModal) {
            if (this.timeScheduleEventId) {
                this.modal.close(this.timeScheduleEventId);
            } else {
                this.modal.dismiss();
            }
        }
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeScheduleEvent) {
                if (!this.timeScheduleEvent.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.timeScheduleEvent.description) {
                    mandatoryFieldKeys.push("common.description");
                }
                if (this.groupMembers.length == 0) {
                    mandatoryFieldKeys.push("core.xemail.selectedreceivers");
                }
            }
        });
    }
}
