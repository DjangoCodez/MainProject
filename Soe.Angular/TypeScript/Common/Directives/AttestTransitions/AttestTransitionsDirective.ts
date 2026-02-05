import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { Guid} from "../../../Util/StringUtility";
import { Feature, TermGroup_AttestEntity, SoeModule } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";

export class AttestTransitionsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AttestTransitions', 'AttestTransitions.html'),
            scope: {
                entity: '@',
                module: '@',
                parentGuid: '=?',
                selectedTransitions: '=?',
                readOnly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: AttestTransitionsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AttestTransitionsController extends GridControllerBase2Ag implements ICompositionGridController {

    // Setup
    private entity: TermGroup_AttestEntity;
    private module: SoeModule;
    private parentGuid: Guid;
    private selectedTransitions: number[]; 
    private onChange: Function;
    private readOnly: boolean;    

    // Collections
    private terms: any;
    private allTransitions: any[];    

    // GoogleMaps
    private modalInstance: any;

    //@ngInject
    constructor(
        private $uibModal,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,                
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {

        super(gridHandlerFactory, "Common.Directives.Transitions", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());

        this.onInit({});
    }

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
        
        this.gridAg.options.enableGridMenu = false;        
        this.gridAg.options.enableFiltering = false;       
        this.gridAg.options.setMinRowsToShow(8);
        this.doubleClickToEdit = false;

        this.modalInstance = this.$uibModal;
        
    }

    public doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadTransitions()]);
    }

    public setupGrid() {
                
        // Grid events
        this.gridAg.options.setMinRowsToShow(8);   
        this.gridAg.addColumnBool("selected", this.terms["common.categories.selected"], 50, !this.readOnly, this.selectTransition.bind(this), "true");
        this.gridAg.addColumnText("entityName", this.terms["manage.attest.transition.entityname"], 100)
        this.gridAg.addColumnText("name", this.terms["common.name"], null);

        this.gridAg.options.enableRowSelection = false;

        const events: GridEvent[] = [];
      
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("manage.attest.transition.transitions", false);
        this.refreshGrid();
      
        this.isDirty = false;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.allTransitions, () => {
            this.refreshGrid();
        });
        this.$scope.$watch(() => this.selectedTransitions, () => {
            this.refreshGrid();
        });
    }

    private refreshGrid() {                
        this.setSelected();
        this.gridAg.setData(this.allTransitions);       
    }
    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "manage.attest.transition.entityname",
            "common.name",
            "common.categories.selected"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadTransitions(): ng.IPromise<any> {
        
        const deferral = this.$q.defer();
        this.coreService.getAttestTransitionGridDTOs(this.entity, this.module, true).then(x => {
            this.allTransitions = x;            
            deferral.resolve();            
        });

        return deferral.promise;
    }


    // ACTIONS

    // HELP-METHODS

    private setSelected() {        
        _.forEach(this.allTransitions, t => {
            let selectTransition = _.find(this.selectedTransitions, c => c === t.attestTransitionId);
            if (selectTransition) {
                t.selected = true;              
            } else {                
                t.selected = false;              
            }
        });        
    }

    private setAsModified() {    
        if (this.onChange)
            this.onChange();
    }

    private selectTransition(gridRow) {
        var row = gridRow.data;
        this.$scope.$applyAsync(() => {            
            if (row.selected) {            
                if (!this.selectedTransitions)
                    this.selectedTransitions = [];
                if (!_.includes(this.selectedTransitions, row.attestTransitionId))
                    this.selectedTransitions.push(row.attestTransitionId);

            } else {                
                if (!this.selectedTransitions)
                    this.selectedTransitions = [];
                if (_.includes(this.selectedTransitions, row.attestTransitionId))
                    this.selectedTransitions = _.filter(this.selectedTransitions, v => v != row.attestTransitionId);

            }

            this.setAsModified();           
        });
    }

}