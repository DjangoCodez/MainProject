import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IRegistryService } from "../RegistryService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IFocusService } from "../../../Core/Services/focusservice";
import { ScheduledJobHeadDTO, ScheduledJobRowDTO } from "../../../Common/Models/ScheduledJobDTOs";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { EditRecurrenceIntervalController } from "../../../Common/Dialogs/EditRecurrenceInterval/EditRecurrenceIntervalController";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    private scheduledJobHeadId: number;
    private head: ScheduledJobHeadDTO;

    // Data
    private heads: ISmallGenericType[];
    private sysTimeIntervals: ISmallGenericType[];
    private selectedRow: ScheduledJobRowDTO;

    // Flags
    private logsExpanderOpen: boolean = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private registryService: IRegistryService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.scheduledJobHeadId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_ScheduledJobs, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_ScheduledJobs].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_ScheduledJobs].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadHeads(),
            this.loadSysTimeIntervals()
        ]).then(() => {
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.scheduledJobHeadId) {
            return this.loadData();
        } else {
            this.new();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => { this.copy() }, () => this.isNew);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.yes",
            "core.no"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadHeads(): ng.IPromise<any> {
        return this.registryService.getScheduledJobHeadsDict(true, true, false).then(x => {
            this.heads = x;
        });
    }

    private loadSysTimeIntervals(): ng.IPromise<any> {
        return this.registryService.getSysTimeIntervals().then(x => {
            this.sysTimeIntervals = x;
        });
    }

    private loadData() {
        return this.registryService.getScheduledJobHead(this.scheduledJobHeadId, true, false, true, true, true, true).then(x => {
            this.head = x;

            if (this.head && this.head.rows && this.head.rows.length > 0)
                this.selectedRow = this.head.rows[0];

            this.isNew = false;

            _.forEach(this.head.settings, setting => {
                setting.setValue(this.terms["core.yes"], this.terms["core.no"]);
            });
        });
    }

    private getNextExecutionTime(row: ScheduledJobRowDTO) {
        return this.coreService.getNextExecutionTime(row.recurrenceInterval).then(x => {
            row.nextExecutionTime = x;
        });
    }

    // ACTIONS

    private new() {
        this.isNew = true;
        this.scheduledJobHeadId = 0;
        this.head = new ScheduledJobHeadDTO();

        this.focusService.focusById("ctrl_head_name");
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.registryService.saveScheduledJobHead(this.head).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.scheduledJobHeadId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.head);
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

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.registryService.deleteScheduledJobHead(this.head.scheduledJobHeadId).then(result => {
                if (result.success) {
                    completion.completed(this.head, true);
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
        super.copy();

        this.head.scheduledJobHeadId = this.scheduledJobHeadId = 0;
        this.head.name = undefined;

        this.setDirty();
        this.focusService.focusById("ctrl_head_name");
    }

    // EVENTS

    private addRow() {
        if (!this.head.rows)
            this.head.rows = [];

        let row: ScheduledJobRowDTO = new ScheduledJobRowDTO();
        row.recurrenceInterval = '0 0 * * *';
        this.getNextExecutionTime(row);

        this.head.rows.push(row);
    }

    private deleteRow(row: ScheduledJobRowDTO) {
        _.pull(this.head.rows, row);
    }

    private editRecurrenceInterval(row: ScheduledJobRowDTO) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/EditRecurrenceInterval/EditRecurrenceInterval.html"),
            controller: EditRecurrenceIntervalController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                singleSelectTime: () => { return true },
                interval: () => { return row.recurrenceInterval }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.interval) {
                row.recurrenceInterval = result.interval;
                this.getNextExecutionTime(row);
                this.setDirty();
            }
        });
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.head) {
                if (!this.head.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}