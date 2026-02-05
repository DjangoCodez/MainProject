import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { Feature, SoeEntityType } from "../../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { TrackChangesLogDTO } from "../../../../Common/Models/TrackChangesDTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { UserSmallDTO } from "../../../../Common/Models/UserDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // Selections
    private dateFrom: Date;
    private dateTo: Date;
    private selectedEntity: SoeEntityType = SoeEntityType.None;
    private selectedUsers: any[] = [];

    // Data
    private entities: SmallGenericType[] = [];
    private users: UserSmallDTO[] = [];
    private logs: TrackChangesLogDTO[] = [];

    // Flags
    private loadingLookups: boolean = true;
    private searching: boolean = false;

    gridHeaderComponentUrl: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Manage.Logs.ChangeLogs.Search", progressHandlerFactory, messagingHandlerFactory);

        this.gridHeaderComponentUrl = urlHelperService.getViewUrl("gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid());
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.dateFrom = CalendarUtility.getDateToday();
        this.dateTo = CalendarUtility.getDateToday();

        this.flowHandler.start([{ feature: Feature.Manage_Logs_ChangeLogs_Search, loadReadPermissions: true, loadModifyPermissions: false },]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Logs_ChangeLogs_Search].readPermission;
    }

    public setupGrid() {
        this.gridAg.addColumnText("topRecordName", this.terms["common.entitylogviewer.changelog.toprecordname"], null);
        this.gridAg.addColumnText("actionMethodText", this.terms["common.entitylogviewer.changelog.actionmethod"], null);
        this.gridAg.addColumnText("entityText", this.terms["common.entitylogviewer.changelog.entity"], null);
        this.gridAg.addColumnText("recordName", this.terms["common.name"], null);
        this.gridAg.addColumnText("columnText", this.terms["common.entitylogviewer.changelog.columnname"], null);
        this.gridAg.addColumnText("actionText", this.terms["common.entitylogviewer.changelog.action"], null);
        this.gridAg.addColumnText("fromValueText", this.terms["common.from"], null);
        this.gridAg.addColumnText("toValueText", this.terms["common.to"], null);
        this.gridAg.addColumnDateTime("created", this.terms["common.modified"], null);
        this.gridAg.addColumnText("createdBy", this.terms["common.modifiedby"], null);

        this.gridAg.finalizeInitGrid("manage.logs.changelogs.search", true);
    }

    // SERVICE CALLS

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadEntities(),
            this.loadUsers()
        ]).then(() => {
            this.loadingLookups = false;
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.entitylogviewer.changelog.action",
            "common.entitylogviewer.changelog.actionmethod",
            "common.entitylogviewer.changelog.columnname",
            "common.entitylogviewer.changelog.entity",
            "common.entitylogviewer.changelog.toprecordname",
            "common.from",
            "common.name",
            "common.modified",
            "common.modifiedby",
            "common.to"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadEntities(): ng.IPromise<any> {
        return this.coreService.getTrackChangesLogEntities().then(x => {
            this.entities = x;
            this.entities.splice(0, 0, new SmallGenericType(0, ''));
        });
    }

    private loadUsers(): ng.IPromise<any> {
        return this.coreService.getUsers(false).then(x => {
            this.users = x;
            this.users.forEach(u => u['displayName'] = '({0}) {1}'.format(u.loginName, u.name));
        });
    }

    // EVENTS

    private decreaseDate() {
        const diffDays = this.dateFrom.diffDays(this.dateTo) - 1;
        this.dateFrom = this.dateFrom.addDays(diffDays);
        this.dateTo = this.dateTo.addDays(diffDays);
    }

    private increaseDate() {
        const diffDays = this.dateTo.diffDays(this.dateFrom) + 1;
        this.dateFrom = this.dateFrom.addDays(diffDays);
        this.dateTo = this.dateTo.addDays(diffDays);
    }

    private search() {
        this.clearLogs();

        this.searching = true;
        this.progress.startLoadingProgress([() => {
            return this.coreService.getTrackChangesLogForEntity(this.selectedEntity, this.dateFrom, this.dateTo, _.map(this.selectedUsers, u => u.loginName)).then(x => {
                this.logs = x;
                this.gridAg.setData(this.logs);
                this.searching = false;
            });
        }]);
    }

    // HELP-METHODS

    private clearLogs() {
        this.logs = [];
    }

    private get disableSearch() {
        return (!this.dateFrom || !this.dateTo || this.loadingLookups);
    }
}