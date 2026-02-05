import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { Constants } from "../../../../../Util/Constants";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";

export class SelectAttestRolesDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/SelectAttestRoles/Views/SelectAttestRoles.html'),
            scope: {
                selectedAttestRoleIds: '=?',
                allAttestRoles: '=',
                isReadonly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: SelectAttestRolesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class SelectAttestRolesController extends GridControllerBase2Ag implements ICompositionGridController {

    private isReadonly: boolean;
    private terms: any;
    private allAttestRoles: any[];
    private selectedAttestRoleIds: number[];
    private timeout = null;

    // Events
    private onChange: Function;

    //ui stuff
    private lastNavigation: { row: any, column: any };
    private gridHeightStyle;
    private controllIsReady: boolean = false;

    //@ngInject
    constructor($http,
        $templateCache,
        private $timeout: ng.ITimeoutService,
        protected $uibModal,
        private $filter: ng.IFilterService,
        protected coreService: ICoreService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private messagingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory
    ) {

        super(gridHandlerFactory, "Common.Directives.SelectEmployees", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());
        this.onInit({});

    }

    // INIT

    // SETUP
    public $onInit() {
        this.gridAg.options.setMinRowsToShow(5);
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableFiltering = true;
        this.doubleClickToEdit = false;
    }
    // INIT
    onInit(parameters: any) {
        this.parameters = parameters;
        this.flowHandler.start({ feature: Feature.None, loadReadPermissions: true, loadModifyPermissions: true });


    }
    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = true;
        this.modifyPermission = true;
    }

    private afterSetup() {
        this.setupWatchers();
    }

    public setupGrid() {

        this.$q.all([
            this.loadTerms()]).then(() => {
                this.gridAndDataIsReady();
            });
    }

    private setupGridColumns() {
        this.gridAg.addColumnBool("selected", this.terms["core.select"], 15, !this.isReadonly, this.selectEmployee.bind(this));
        this.gridAg.addColumnText("name", this.terms["common.name"], 80);
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.finalizeInitGrid("time.employee.employees.directives.selectattestroles", false);
        this.gridAg.setData(this.sortAttestRoles());
    }

    private setupWatchers() {
        if (!this.allAttestRoles)
            this.allAttestRoles = [];
        this.$scope.$watch(() => this.allAttestRoles, () => {
            this.$timeout(() => { this.gridAg.setData(this.allAttestRoles) }, 100);
        });
        this.$scope.$watch(() => this.selectedAttestRoleIds, () => {
            this.$timeout(() => {  this.gridAg.setData(this.sortAttestRoles()) }, 100);
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        // Columns
        var keys: string[] = [
            "common.name",
            "core.select",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // EVENTS

    private selectEmployee(gridRow: any) {
        var row = gridRow.data;
        if (row) {
            if (!this.selectedAttestRoleIds)
                this.selectedAttestRoleIds = new Array();
            if (row.selected === true) {
                if (!_.includes(this.selectedAttestRoleIds, row.id)) {
                    this.selectedAttestRoleIds.push(row.id);
                }
            } else {
                if (_.includes(this.selectedAttestRoleIds, row.id)) {
                    this.selectedAttestRoleIds.splice(this.selectedAttestRoleIds.indexOf(row.id), 1);
                }
            }
            this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            if (this.onChange)
                this.onChange();
        }
    }

    // HELP-METHODS

    private gridAndDataIsReady() {
        this.setupGridColumns();
        this.setupWatchers();
    }

    private gridSelectionChanged(row) {
        this.$scope.$applyAsync(() => {
            this.selectEmployee(row);
        });
    }

    private sortAttestRoles() {
        // Mark selected roles
        if (this.selectedAttestRoleIds) {
            _.forEach(this.allAttestRoles, (role) => {
                role.selected = (_.includes(this.selectedAttestRoleIds, role.id));
            });
        }
        // Sort to get selected at the top
        return _.orderBy(this.allAttestRoles, ['selected', 'name'], ['desc', 'asc'])
    }

}

