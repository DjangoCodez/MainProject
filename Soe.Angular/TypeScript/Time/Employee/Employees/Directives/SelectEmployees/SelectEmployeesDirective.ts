import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmployeeListSmallDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { Constants } from "../../../../../Util/Constants";

export class SelectEmployeesDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/SelectEmployees/Views/SelectEmployees.html'),
            scope: {
                selectedParticipantIds: '=',
                allParticipants: '=',
                selectedParticipantNames: '=?',
                isReadonly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: SelectEmployeesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class SelectEmployeesController extends GridControllerBase2Ag implements ICompositionGridController {

    // Init parameters
    private selectedParticipantIds: number[];
    private allParticipants: EmployeeListSmallDTO[];
    private selectedParticipantNames: string;
    private isReadonly: boolean;
    private terms: any;
    private timeout = null;

    // Events
    private onChange: Function;

    //@ngInject
    constructor($http,
        $templateCache,
        private $timeout: ng.ITimeoutService,
        private $filter: ng.IFilterService,
        protected coreService: ICoreService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private messagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
               
        super(gridHandlerFactory, "Time.Employee.Employees.Directives.SelectEmployees", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());
        this.onInit({});
        
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
    // SETUP
    public $onInit() {
        this.gridAg.options.setMinRowsToShow(5);
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableFiltering = true;
        this.doubleClickToEdit = false;
  
    }

    public doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms()
        ]);
    }

    public setupGrid() {
       
        this.$q.all([
            this.loadTerms()]).then(() => {
                this.gridAndDataIsReady();
            });
    }

    private setupGridColumns() {
        this.gridAg.options.setMinRowsToShow(5);
        this.gridAg.addColumnBool("selected", this.terms["core.select"], 15, !this.isReadonly, this.selectParticipant.bind(this),"true");
        this.gridAg.addColumnText("numberAndName", this.terms["common.name"], 80);
        
        this.gridAg.options.enableRowSelection = false;
        const events: GridEvent[] = [];
        this.gridAg.options.subscribe(events);
        this.isDirty = false;
        this.gridAg.finalizeInitGrid("time.employee.employees.directives.selectemployees", false);
        this.gridAg.setData(this.sortParticipants());
    }   

    private setupWatchers() {
        if (!this.allParticipants)
            this.allParticipants = [];
        this.$scope.$watch(() => this.allParticipants, () => {
            this.$timeout(() => { this.gridAg.setData(this.allParticipants) }, 100);
        });
        this.$scope.$watch(() => this.selectedParticipantIds, () => {
            this.$timeout(() => { this.gridAg.setData(this.sortParticipants()) }, 100);
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

    private selectParticipant(gridRow: any) {
        var row = gridRow.data;
        this.$timeout(() => {
            if (row) {
            if (!this.selectedParticipantIds)
                this.selectedParticipantIds = new Array();
            if (row.selected === true) {
                    if (!_.includes(this.selectedParticipantIds, row.employeeId)) {
                        this.selectedParticipantIds.push(row.employeeId);
                    }
            } else {
                if (_.includes(this.selectedParticipantIds, row.employeeId)) {
                    this.selectedParticipantIds.splice(this.selectedParticipantIds.indexOf(row.employeeId), 1);
                }
            }
            
            this.setSelectedParticipantNames();
            if (this.onChange)
                this.onChange();
            }
        },100);
    }

    // HELP-METHODS

    private gridAndDataIsReady() {
        this.setupGridColumns();
        this.setupWatchers();
    }

    private setSelectedParticipantNames() {
        var names: string = '';

        _.forEach(_.filter(this.sortParticipants(), p => p.selected), participant => {
            if (names.length > 0)
                names += ', ';

            names += participant.name;
        });
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
        this.selectedParticipantNames = names;
    }

    private sortParticipants() {
        // Mark selected employees
        if (this.selectedParticipantIds) {
            _.forEach(this.allParticipants, (user) => {
                user.selected = (_.includes(this.selectedParticipantIds, user.employeeId));
            });
        }
        // Sort to get selected at the top
        return _.orderBy(this.allParticipants, ['selected', 'name'], ['desc', 'asc'])
    }
}