import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { SoeCategoryType, SoeEntityType, Feature } from "../../../Util/CommonEnumerations";
import { CompanyCategoryRecordDTO } from "../../Models/Category";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { Guid } from "../../../Util/StringUtility";

export class CategoriesDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('Categories', 'Categories.html'),
            scope: {
                categoryType: '@',
                categoryRecordEntity: '@',
                parentGuid: '=?',
                selectedCategories: '=?',
                nbrOfRows: '@',
                showDefault: '=?',
                showDateFrom: '=?',
                showDateTo: '=?',
                showIsExecutive: '=?',
                hideHeader: '=?',
                readOnly: '=?',
                recordId: '=?',
                useCompCategories: '@',
                compCategories: '=?',
                useFilters: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: CategoriesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class CategoriesController extends GridControllerBaseAg {

    // Setup
    private categoryType: SoeCategoryType;
    private categoryRecordEntity: SoeEntityType;
    private parentGuid: Guid;
    private selectedCategories: number[];
    private nbrOfRows: number;
    private showDefault: boolean;
    private showDateFrom: boolean;
    private showDateTo: boolean;
    private showIsExecutive: boolean;
    private hideHeader: boolean;
    private readOnly: boolean;
    private recordId: number;
    private useCompCategories: boolean;
    private compCategories: CompanyCategoryRecordDTO[];
    private useFilters: boolean;
    private onChange: Function;

    // Converted init parameters
    private minRowsToShow: number = 8; // Default

    // Collections
    private terms: any;
    private allCategories: any[];

    // Watchers
    private watchers = [];

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Common.Directives.Categories", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        if (!this.selectedCategories)
            this.selectedCategories = [];
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = this.useFilters;
        this.soeGridOptions.setMinRowsToShow(this.nbrOfRows ? this.nbrOfRows : this.minRowsToShow);
    }

    public setupGrid() {
        this.$q.all([
            this.loadTerms(),
            this.loadAllCategories()
        ]).then(() => this.setupGridColumns());
    }

    private setupGridColumns() {
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.enableFiltering = true;

        this.soeGridOptions.resetColumnDefs();
        this.soeGridOptions.addColumnBool("selected", this.terms["common.categories.selected"], 50, { suppressFilter: true, enableEdit: !this.readOnly, onChanged: this.selectCategory.bind(this) });
        this.soeGridOptions.addColumnText("name", this.terms["common.categories.category"], null);
        if (this.showDefault)
            this.soeGridOptions.addColumnBool("default", this.terms["common.categories.standard"], 75, { suppressFilter: true, onChanged: this.defaultClicked.bind(this), enableEdit: true, disabledField: "disabled" });
        if (this.showDateFrom)
            this.soeGridOptions.addColumnDate("dateFrom", this.terms["common.categories.datefrom"], 100, null, null, null, { suppressFilter: true, editable: this.enableEdit.bind(this) });
        if (this.showDateTo)
            this.soeGridOptions.addColumnDate("dateTo", this.terms["common.categories.dateto"], 100, null, null, null, { suppressFilter: true, editable: this.enableEdit.bind(this) });
        if (this.showIsExecutive)
            this.soeGridOptions.addColumnBool("isExecutive", this.terms["common.categories.isexecutive"], 75, { suppressFilter: true, onChanged: this.isExecutiveClicked.bind(this), enableEdit: true, disabledField: "disabled" });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.soeGridOptions.subscribe(events);

        if (this.useCompCategories)
            super.gridDataLoaded(this.sortCompCategories());
        else
            super.gridDataLoaded(this.sortCategories());


        this.soeGridOptions.finalizeInitGrid();
        this.restoreState();
        this.setupWatchers();
    }

    protected enableEdit(row): boolean {
        return row && row.selected && !this.readOnly;
    }

    private clearWatchers() {
        //  Need too remove old before setting up new.
        //  Caused exponential increase of watchers in order view.
        (this.watchers || []).forEach(w => w());
        this.watchers = [];
    }

    private setupWatchers() {
        this.clearWatchers();
        if (this.useCompCategories) {
            this.watchers.push(this.$scope.$watch(() => this.compCategories, (newVal, oldVal) => {
                if (newVal !== oldVal)
                    super.gridDataLoaded(this.sortCompCategories());
            }));
        } else {
            this.watchers.push(this.$scope.$watch(() => this.selectedCategories, (newVal, oldVal) => {
                if (newVal && newVal !== oldVal)
                    super.gridDataLoaded(this.sortCategories());
            }));
        }

        this.watchers.push(this.$scope.$watchGroup([() => this.readOnly, () => this.showDateFrom, () => this.showDateTo], (oldVal, newVal) => {
            if (oldVal !== newVal)
                this.setupGridColumns();
        }));
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.categories.selected",
            "common.categories.category",
            "common.categories.standard",
            "common.categories.datefrom",
            "common.categories.dateto",
            "common.categories.isexecutive"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadAllCategories(): ng.IPromise<any> {
        return this.coreService.getCategories(this.categoryType, false, false, false, true).then(x => {
            this.allCategories = x;
        });
    }

    // EVENTS

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        if (this.useCompCategories) {
            let compCategory = _.find(this.compCategories, c => c.categoryId === row.categoryId);
            if (compCategory) {
                switch (colDef.field) {
                    case 'dateFrom':
                        compCategory.dateFrom = newValue;
                        this.setAsModified();
                        break;
                    case 'dateTo':
                        compCategory.dateTo = newValue;
                        this.setAsModified();
                        break;
                }
            }
        }
    }

    private defaultClicked(row) {
        if (this.useCompCategories && row.data) {
            let compCategory = _.find(this.compCategories, c => c.categoryId === row.data.categoryId);
            if (compCategory) {
                compCategory.default = row.data.default;
                this.setAsModified();
            }
        }
    }

    private isExecutiveClicked(row) {
        if (this.useCompCategories && row.data) {
            let compCategory = _.find(this.compCategories, c => c.categoryId === row.data.categoryId);
            if (compCategory) {
                compCategory.isExecutive = row.data.isExecutive;
                this.setAsModified();
            }
        }
    }

    private selectCategory(gridRow) {
        var row = gridRow.data;
        this.$scope.$applyAsync(() => {
            if (row.selected) {
                if (this.useCompCategories) {
                    var newCat: CompanyCategoryRecordDTO = new CompanyCategoryRecordDTO();
                    newCat.categoryId = row.categoryId;
                    newCat.dateFrom = row.dateFrom;
                    newCat.dateTo = row.dateTo;
                    newCat.isExecutive = row.isExecutive;
                    // Set default if first category selected
                    if (_.filter(this.compCategories, c => c.default).length === 0)
                        newCat.default = true;
                    this.compCategories.push(newCat);
                } else {
                    if (!this.selectedCategories)
                        this.selectedCategories = [];
                    if (!_.includes(this.selectedCategories, row.categoryId)) {
                        this.selectedCategories.push(row.categoryId);
                    }
                }
            } else {
                if (this.useCompCategories) {
                    let category = _.find(this.allCategories, c => c.categoryId === row.categoryId);
                    if (category) {
                        category.default = false;
                        category.dateFrom = null;
                        category.dateTo = null;
                        category.isExecutive = false;
                    }
                    _.pullAll(this.compCategories, _.filter(this.compCategories, t => t.categoryId === row.categoryId));
                } else {
                    if (!this.selectedCategories)
                        this.selectedCategories = [];
                    if (_.includes(this.selectedCategories, row.categoryId)) {
                        this.selectedCategories = _.filter(this.selectedCategories, v => v != row.categoryId);
                    }
                }
            }

            this.setAsModified();
            this.markSelected();
            this.soeGridOptions.refreshGrid();
        });
    }

    // HELP-METHODS

    private markSelected() {
        // Mark selected categories
        if (this.useCompCategories) {
            _.forEach(this.allCategories, cat => {
                let compCategory = _.find(this.compCategories, c => c.categoryId === cat.categoryId);
                if (compCategory) {
                    cat.selected = true;
                    cat.default = compCategory.default;
                    cat.dateFrom = compCategory.dateFrom;
                    cat.dateTo = compCategory.dateTo;
                    cat.isExecutive = compCategory.isExecutive;
                    cat['disabled'] = false;
                } else {
                    // Clear the fields
                    cat.selected = false;
                    cat.default = false;
                    cat.dateFrom = null;
                    cat.dateTo = null;
                    cat.isExecutive = false;
                    cat['disabled'] = true;
                }
            });
        }
    }

    private sortCompCategories() {
        this.markSelected();

        // Sort to get selected categories at the top
        return _.orderBy(this.allCategories, ['selected', 'name'], ['desc', 'asc'])
    }

    private sortCategories() {
        // Mark selected categories
        if (this.selectedCategories && !this.compCategories) {
            _.forEach(this.allCategories, (cat) => {
                cat.selected = (_.includes(this.selectedCategories, cat.categoryId));
            });
        }

        // Sort to get selected categories at the top
        return _.orderBy(this.allCategories, ['selected', 'name'], ['desc', 'asc'])
    }

    private setAsModified() {
        this.setModified(true, this.parentGuid);

        if (this.onChange)
            this.onChange();
    }
}