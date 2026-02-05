import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { Feature, TermGroup, TermGroup_AttestEntity, SoeModule } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { AttestWorkFlowTemplateRowDTO } from "../../../Common/Models/AttestWorkFlowDTOs";
import { IAttestTransitionDTO } from "../../../Scripts/TypeLite.Net4";
import { ToolBarButtonGroup, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { Constants } from "../../../Util/Constants";
import { NumberUtility } from "../../../Util/NumberUtility";

export class AttestWorkFlowTemplateRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AttestWorkFlowTemplateRows', 'AttestWorkFlowTemplateRows.html'),
            scope: {
                entity: '@',
                module: '@',                
                attestTemplateId: "=",
                attestWorkFlowTemplateRows: "=",
                parentGuid: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: AttestWorkFlowTemplateRowsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AttestWorkFlowTemplateRowsController extends GridControllerBase2Ag implements ICompositionGridController {

    // Setup
    private entity: TermGroup_AttestEntity;
    private module: SoeModule;       
    private parentGuid: string;    
    private attestTemplateId: number;
    public transitions: IAttestTransitionDTO[];
    public attestWorkFlowTemplateRows: AttestWorkFlowTemplateRowDTO[];
    private templateTypes: any[];    
    private terms: any;    

    sortMenuButtons = new Array<ToolBarButtonGroup>();


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

        super(gridHandlerFactory, "Common.Directives.WorkFlowTemplateRows", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            //.onSetUpGrid(() => this.setupGrid())
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
        
    }

    public doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadTemplateTypes(),
            this.loadTransitions()]).then(() => {
                this.setupGrid();
                this.loadData();
            }); 
    }

    public setupGrid() {                
        // Grid events
        this.gridAg.options.setMinRowsToShow(8);   
        this.gridAg.addColumnBool("checked", this.terms["common.categories.selected"], 50, true, this.selectionChanged.bind(this), "true");
        this.gridAg.addColumnText("attestTransitionName", this.terms["manage.attest.supplier.attestworkflowtemplate.transition"], 100)        
        this.gridAg.addColumnSelect("typeName", this.terms["common.type"], null,
            {
                editable: true, displayField: "typeName", selectOptions: this.templateTypes,  dropdownIdLabel: "value",
                dropdownValueLabel: "label" ,onChanged:this.typeChanged.bind(this)}
        );        

        //this.gridAg.options.enableRowSelection = true;
        this.gridAg.options.enableSingleSelection();
        

        const events: GridEvent[] = [];
      
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("manage.attest.attestworkflowtemplaterow.attestworkflowtemplaterows", false);
        this.refreshGrid();
        this.setupSortGroup("sort"); 

        this.isDirty = false;
    }

    private setupWatchers() {
        //this.$scope.$watch(() => this.transitions, () => {
        //    this.refreshGrid();
        //});       
    }

    private refreshGrid() {                        
        //this.setSelected();
        this.gridAg.setData(this.attestWorkFlowTemplateRows);           
    }

  

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid }));
    }

    private multiplyRowNr() {
        _.forEach(this.attestWorkFlowTemplateRows, x => {
            x.sort *= 10;
        });
    }

    // SERVICE CALLS

    loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.categories.selected",
            "manage.attest.supplier.attestworkflowtemplate.transition",
            "common.sort",
            "common.type"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadTemplateTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowType, false, false).then(x => {
            this.templateTypes = [];

            _.forEach(x, (row) => {
                this.templateTypes.push({ value: row.id, label: row.name });
            })
        });
    }

    private loadData(): ng.IPromise<any> {

        return this.coreService.getAttestWorkFlowTemplateRows(this.attestTemplateId).then((rows) => {

            if (rows) {
                _.forEach(rows, row => {
                    var type = _.find(this.templateTypes, { value: row.type });
                    if (type)
                        row.typeName = type.label;
                });
                this.attestWorkFlowTemplateRows = rows;
            }

            var data = _.orderBy(this.attestWorkFlowTemplateRows, ['sort'], ['asc']);
            data.forEach(x => x.checked = true);
            this.transitions.forEach(y => {
                if (!_.includes(data, _.find(data, (x: AttestWorkFlowTemplateRowDTO) => x.attestTransitionId === y.attestTransitionId))) {                    
                    let r = new AttestWorkFlowTemplateRowDTO();
                    r.attestTransitionName = y.name;
                    r.sort = data.length + 1;
                    r.attestTransitionId = y.attestTransitionId;
                    data.push(r);
                }
            });
            this.attestWorkFlowTemplateRows = data;            
            this.refreshGrid();
        });
    }

    private loadTransitions(): ng.IPromise<any> {
        return this.coreService.getAttestTransitionGridDTOs(this.entity, this.module, false).then((transitions: any[]) => {
            this.transitions = transitions;            
        });
    }


    // ACTIONS

    // HELP-METHODS
    private selectionChanged(row) {
        this.setParentAsModified();
    }

    private typeChanged(gridRow) {
        var row = gridRow.data;
        this.$scope.$applyAsync(() => {
            console.log(row);
            this.templateTypes.forEach(type => {           
                if (row.typeName == type.label) {
                 //   row.typeName = type.label;
                    row.type = type.value;
                }
            });
            this.setParentAsModified();            
        });
    }

     protected setupSortGroup(sortProp: string = "sort", disabled = () => { }, hidden = () => { }) {
        var group = ToolBarUtility.createSortGroup(
            () => {
                this.sortFirst();
                this.setParentAsModified();
            },
            () => {
                this.sortUp();
                this.setParentAsModified();
            },
            () => {
                this.sortDown();
                this.setParentAsModified();
            },
            () => {
                this.sortLast();
                this.setParentAsModified();
            },
            disabled,
            hidden
        );
        this.sortMenuButtons = [];
        this.sortMenuButtons.push(group);
    }
    private reNumberRows(selectedRow: AttestWorkFlowTemplateRowDTO) {
        this.attestWorkFlowTemplateRows = _.orderBy(this.attestWorkFlowTemplateRows, ['sort'], ['asc']);

        var i: number = 1;

        _.forEach(this.attestWorkFlowTemplateRows, r => {
            r.sort = i++;
        });

        this.refreshGrid();
        this.gridAg.options.refreshRows();
        this.gridAg.options.selectRows([selectedRow]);


        var currentRowIndex = _.findIndex(this.attestWorkFlowTemplateRows, selectedRow);
        console.log(currentRowIndex);
        this.gridAg.options.setFocusedCell(currentRowIndex, this.gridAg.options.getColumnByField('sort'));

        //this.gridAg.options.clearSelectedRows();
        //this.gridAg.options.startEditingCell(selectedRow, "sort");

        //this.$scope.$applyAsync(() => {
        //    this.gridAg.options.selectRowByVisibleIndex(currentRowIndex > 0 ? currentRowIndex : 0);
        //});      
    }

    private sortFirst() {
        // Get current row
        //var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getCurrentRow();
        var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getSelectedRows()[0];
        if (row && row.sort > 1) {
            // Move row to the top
            row.sort = -1;

            this.reNumberRows(row);
        }
    }

    private sortUp() {
        // Get current row
        //var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getCurrentRow();
        var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getSelectedRows()[0];
        if (row && row.sort > 1) {
            // Get previous row
            var prevRow = _.find(this.attestWorkFlowTemplateRows, r => r.sort === row.sort - 1);

            // Move row up
            if (prevRow) {
                this.multiplyRowNr();
                // Move current row before previous row
                row.sort -= 19;

                this.reNumberRows(row);
            }
        }
    }

    private sortDown() {
        // Get current row
        //var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getCurrentRow();
        var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getSelectedRows()[0];
        if (row && row.sort < this.attestWorkFlowTemplateRows.length) {
            // Get next row
            var nextRow = _.head(_.sortBy(_.filter(this.attestWorkFlowTemplateRows, r => r.sort > row.sort), 'sort'));
            // Move row down
            if (nextRow) {
                this.multiplyRowNr();
                // Move current row after next row                    
                row.sort = nextRow.sort + 5;

                this.reNumberRows(row);
            }
        }
    }

    private sortLast() {
        // Get current row
        //var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getCurrentRow();
        var row: AttestWorkFlowTemplateRowDTO = this.gridAg.options.getSelectedRows()[0];
        if (row && row.sort < this.attestWorkFlowTemplateRows.length) {

            // Move row to the bottom
            row.sort = NumberUtility.max(this.attestWorkFlowTemplateRows, 'sort') + 2;

            this.reNumberRows(row);
        }
    }    

}