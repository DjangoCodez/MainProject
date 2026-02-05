import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { Guid } from "../../../Util/StringUtility";
import { Feature, TermGroup_AttestEntity, SoeModule } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { AttestRoleMappingDTO } from "../../Models/AttestRoleMappingDTO";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";

export class AttestRoleMappingDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AttestRoleMapping', 'AttestRoleMapping.html'),
            scope: {
                module: '@',
                entity: '@',
                parentGuid: '=?',
                selectedAttestRoles: '=?',
                readOnly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: AttestRoleMappingController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AttestRoleMappingController extends GridControllerBase2Ag implements ICompositionGridController {

    // Setup
    private module: SoeModule;
    private entity: TermGroup_AttestEntity;
    private parentGuid: Guid;
    private selectedAttestRoles: AttestRoleMappingDTO[];
    private onChange: Function;
    private readOnly: boolean;

    // Collections
    private terms: any;
    //private allAttestRoles: ISmallGenericType[];    
    private allAttestRoles: AttestRoleMappingDTO[];
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

        super(gridHandlerFactory, "Common.Directives.AttestRoleMapping", progressHandlerFactory, messagingHandlerFactory);

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
            this.loadAllAttestRoles()]);
    }

    public setupGrid() {

        // Grid events
        this.gridAg.options.setMinRowsToShow(6);
        this.gridAg.addColumnBool("selected", this.terms["common.selected"], 50, !this.readOnly, this.selectedChanged.bind(this), "true");        
        this.gridAg.addColumnText("childtAttestRoleName", this.terms["common.name"], null);
        this.gridAg.addColumnDate("dateFrom", this.terms["common.datefrom"], null, false, null, { maxWidth: 80, minWidth: 80, editable: !this.readOnly });
        this.gridAg.addColumnDate("dateTo", this.terms["common.dateto"], null, false, null, { maxWidth: 80, minWidth: 80, editable: !this.readOnly });

        this.gridAg.options.enableRowSelection = false;

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));

        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("manage.attest.transition.transitions", false);
        this.refreshGrid();

        this.isDirty = false;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.allAttestRoles, () => {
            this.refreshGrid();
        });
        this.$scope.$watch(() => this.selectedAttestRoles, () => {
            this.refreshGrid();
        });
    }

    private refreshGrid() {
        this.setSelected();
        this.gridAg.setData(this.allAttestRoles);
    }
    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "manage.attest.transition.entityname",
            "common.name",
            "common.selected",
            "common.datefrom",
            "common.dateto"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadAllAttestRoles(): ng.IPromise<any> {
        
        const deferral = this.$q.defer();
        this.coreService.getAttestRolesDict(this.module, false).then(x => {
        
            let attestRoleDict = x;
            let tempAattestRoles: AttestRoleMappingDTO[] = [];

            attestRoleDict.forEach(ar => {                
                let mapping = new AttestRoleMappingDTO();
                mapping.childtAttestRoleId = ar.id;
                mapping.childtAttestRoleName = ar.name;
                mapping.entity = this.entity;
                tempAattestRoles.push(mapping);                
            });

            this.allAttestRoles = tempAattestRoles;

            deferral.resolve();
        });

        return deferral.promise;
    }


    // ACTIONS

    // HELP-METHODS

    private setSelected() {
        _.forEach(this.allAttestRoles, arm => {
            let selectedAttestRole: AttestRoleMappingDTO = this.getAttestRoleFromSelectedAttestRole(arm.childtAttestRoleId);
            if (selectedAttestRole) {
                arm.selected = true;
                arm.dateFrom = selectedAttestRole.dateFrom;
                arm.dateTo = selectedAttestRole.dateTo;                
            } else {
                arm.selected = false;
                arm.dateFrom = null;
                arm.dateTo = null;
            }
        });
    }

    private setAsModified() {
        if (this.onChange)
            this.onChange();
    }

    private afterCellEdit(row: AttestRoleMappingDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        let selectedAttestRole: AttestRoleMappingDTO = this.getAttestRoleFromSelectedAttestRole(row.childtAttestRoleId);
        if (!selectedAttestRole)
            return;

        switch (colDef.field) {          
            case "dateFrom":
                {
                    selectedAttestRole.dateFrom = row.dateFrom;                    
                    break;
                }
            case "dateTo":
                {
                    selectedAttestRole.dateTo = row.dateTo;                    
                    break;
                }  
        }

        this.setAsModified();
    }

    private selectedChanged(gridRow) {
        var row = gridRow.data;
        
        this.$scope.$applyAsync(() => {
            if (row.selected) {
                if (!this.selectedAttestRoles)
                    this.selectedAttestRoles = [];

                let selectedAttestRole = this.getAttestRoleFromSelectedAttestRole(row.childtAttestRoleId);
                if (selectedAttestRole) {
                    selectedAttestRole.selected = true;               
                } else {
                    selectedAttestRole = new AttestRoleMappingDTO();
                    selectedAttestRole.selected = true;      
                    selectedAttestRole.childtAttestRoleId = row.childtAttestRoleId;
                    selectedAttestRole.entity = this.entity
                    this.selectedAttestRoles.push(selectedAttestRole); 
                }                
            } else {
                if (!this.selectedAttestRoles)
                    this.selectedAttestRoles = [];

                let selectedAttestRole = this.getAttestRoleFromSelectedAttestRole(row.childtAttestRoleId);
                if (selectedAttestRole) {
                    _.pull(this.selectedAttestRoles, selectedAttestRole);
                }

                let selectedAllAttestRole = this.getAttestRoleFromAllAttestRole(row.childtAttestRoleId);
                selectedAllAttestRole.dateFrom = null;
                selectedAllAttestRole.dateTo = null;
                this.refreshGrid();
            }

            this.setAsModified();
        });

       
    }

    getAttestRoleFromSelectedAttestRole(attestRoleId): AttestRoleMappingDTO {
        return _.find(this.selectedAttestRoles, c => c.childtAttestRoleId === attestRoleId);
    }
    getAttestRoleFromAllAttestRole(attestRoleId): AttestRoleMappingDTO {
        return _.find(this.allAttestRoles, c => c.childtAttestRoleId === attestRoleId);
    }
}