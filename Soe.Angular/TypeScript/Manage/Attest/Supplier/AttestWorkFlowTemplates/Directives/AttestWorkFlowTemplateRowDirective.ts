import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IAttestService } from "../../../AttestService";
import { GridControllerBaseAg } from "../../../../../Core/Controllers/GridControllerBaseAg";
import { IAttestTransitionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Feature, TermGroup, TermGroup_AttestEntity, SoeModule } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { ToolBarUtility } from "../../../../../Util/ToolBarUtility";
import { AttestWorkFlowTemplateRowDTO } from "../../../../../Common/Models/AttestWorkFlowDTOs";

//@ngInject
export class AttestWorkFlowTemplateRowDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/Attest/Supplier/AttestWorkFlowTemplates/Directives/AttestWorkFlowTemplateRow.html'),
            scope: {
                attestTemplateId: "=",
                attestWorkFlowTemplateRows: "=",
                parentGuid: '=?',
            },
            controller: AttestWorkFlowTemplateRowDirectiveController,
            controllerAs: "directiveCtrl",
            bindToController: true,
            restrict: "E",
            replace: true,
        };
    }
}


export class AttestWorkFlowTemplateRowDirectiveController extends GridControllerBaseAg {

    private parentGuid: string;
    private terms: { [index: string]: string; };
    private attestTemplateId: number;
    public transitions: IAttestTransitionDTO[];
    public attestWorkFlowTemplateRows: AttestWorkFlowTemplateRowDTO[];
    private templateTypes: any[];

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private attestService: IAttestService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Manage.Attest.Supplier.AttestWorkFlowTemplateRows", "Manage.Attest.Supplier.AttestWorkFlowTemplateRows", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(10);
    }

    private setupWatchers() {
        //this.$scope.$watch(() => this.attestTemplateId, (newVal, oldVal) => {
        //    this.loadData();
        //});        
    }

    public setupGrid() {

        this.$q.all([this.loadTerms(),
        this.loadTemplateTypes(),
        this.loadTransitions(),
        ]).then(() => {
            this.loadData().then(() => {
                this.setUpGridColumns()
                this.gridAndDataIsReady();
            });
        });
    }

    private gridAndDataIsReady() {
        this.setupWatchers();
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
        console.log(this.attestTemplateId);
        return this.attestService.getAttestWorkFlowTemplateRows(this.attestTemplateId || 0).then((rows) => {

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

        });
    }

    private loadTransitions(): ng.IPromise<any> {
        return this.attestService.getAttestTransitionGridDTOs(TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, false).then((transitions: any[]) => {
            this.transitions = transitions;
        });
    }

    private setUpGridColumns() {
        this.soeGridOptions.clearData();
        this.soeGridOptions.clearColumnDefs();

        this.soeGridOptions.addColumnBool("checked", this.terms["common.categories.selected"], 5, { enableEdit: true, onChanged: this.selectionChanged.bind(this) });
        this.soeGridOptions.addColumnText("attestTransitionName", this.terms["manage.attest.supplier.attestworkflowtemplate.transition"], 50);
        this.soeGridOptions.addColumnSelect("type", this.terms["common.type"], 10,
            {
                selectOptions: this.templateTypes,
                enableHiding: false,
                editable: true,
                displayField: "typeName",
                dropdownIdLabel: "value",
                dropdownValueLabel: "label",
                onChanged: this.typeChanged.bind(this),
            });

        this.soeGridOptions.finalizeInitGrid();
        this.gridDataLoaded(this.attestWorkFlowTemplateRows);
        this.setupSortGroup("sort");
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid }));
    }

    private reNumberRows(selectedRow: AttestWorkFlowTemplateRowDTO) {
        this.attestWorkFlowTemplateRows = _.orderBy(this.attestWorkFlowTemplateRows, ['sort'], ['asc']);

        var i: number = 1;

        _.forEach(this.attestWorkFlowTemplateRows, r => {
            r.sort = i++;
        });

        this.gridDataLoaded(this.attestWorkFlowTemplateRows);
        this.soeGridOptions.refreshRows();

        var currentRowIndex = _.findIndex(this.attestWorkFlowTemplateRows, selectedRow);
        this.soeGridOptions.clearSelectedRows();
        this.soeGridOptions.startEditingCell(selectedRow, "sort");

        this.$timeout(() => {
            this.soeGridOptions.selectRowByVisibleIndex(currentRowIndex > 0 ? currentRowIndex : 0);
        });
    }

    private multiplyRowNr() {
        _.forEach(this.attestWorkFlowTemplateRows, x => {
            x.sort *= 10;
        });
    }

    // EVENTS
    private selectionChanged(row) {
        this.setParentAsModified();
    }

    private typeChanged(row) {
        this.templateTypes.forEach(type => {
            if (type.value == row.type)
                row.typeName = type.label;
        });
        this.setParentAsModified();
    }

    private sortFirst() {
        // Get current row
        var row: AttestWorkFlowTemplateRowDTO = this.soeGridOptions.getCurrentRow();
        if (row && row.sort > 1) {
            // Move row to the top
            row.sort = -1;

            this.reNumberRows(row);
        }
    }

    private sortUp() {
        // Get current row
        var row: AttestWorkFlowTemplateRowDTO = this.soeGridOptions.getCurrentRow();
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
        var row: AttestWorkFlowTemplateRowDTO = this.soeGridOptions.getCurrentRow();
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
        var row: AttestWorkFlowTemplateRowDTO = this.soeGridOptions.getCurrentRow();
        if (row && row.sort < this.attestWorkFlowTemplateRows.length) {

            // Move row to the bottom
            row.sort = NumberUtility.max(this.attestWorkFlowTemplateRows, 'sort') + 2;

            this.reNumberRows(row);
        }
    }
}
