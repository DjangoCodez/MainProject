import { IntKeyValue } from "../../../../../Common/Models/SmallGenericType";
import { TimeStampAdditionDTO } from "../../../../../Common/Models/TimeStampDTOs";
import { GridControllerBaseAg } from "../../../../../Core/Controllers/GridControllerBaseAg";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { Guid } from "../../../../../Util/StringUtility";
import { ITimeService } from "../../../TimeService";

export class TimeStampAdditionsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimeTerminals/Directives/TimeStampAdditions/TimeStampAdditions.html'),
            scope: {
                parentGuid: '=?',
                selectedAdditions: '=?',
                readOnly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: TimeStampAdditionsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class TimeStampAdditionsController extends GridControllerBaseAg {

    // Setup
    private parentGuid: Guid;
    private selectedAdditions: IntKeyValue[];
    private readOnly: boolean;
    private onChange: Function;

    // Collections
    private terms: any;
    private additions: TimeStampAdditionDTO[] = [];

    // Flags
    private allSelected: boolean = false;

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
        private timeService: ITimeService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Time.Time.TimeTerminals.Directives.TimeStampAdditions", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        if (!this.selectedAdditions)
            this.selectedAdditions = [];
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(8);
    }

    public setupGrid() {
        this.$q.all([
            this.loadTerms(),
            this.loadTimeStampAdditions()
        ]).then(() => this.setupGridColumns());
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnBool("selected", this.terms["common.selected"], 50, { enableEdit: !this.readOnly, onChanged: this.selectAddition.bind(this) });
        this.soeGridOptions.addColumnText("name", this.terms["common.name"], null);

        super.gridDataLoaded(this.sortAdditions());

        this.soeGridOptions.finalizeInitGrid();

        // For some reason, rowSelection is first displayed, then it is hidden
        // Columns are then dragged to the left leaving an empty space to the right.
        // Resizing the columns after a while solves that annoying problem.
        this.$timeout(() => {
            this.soeGridOptions.sizeColumnToFit();
        }, 500);

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.selectedAdditions, (newVal, oldVal) => {
            if (newVal && newVal !== oldVal) {
                super.gridDataLoaded(this.sortAdditions());
                this.setAllSelected();
            }
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.selected",
            "common.name"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadTimeStampAdditions(): ng.IPromise<any> {
        return this.timeService.getTimeStampAdditions(false).then(x => {
            this.additions = x;
            super.gridDataLoaded(this.sortAdditions());
            this.setAllSelected();
        });
    }

    // EVENTS

    private selectAddition(gridRow) {
        let addition: TimeStampAdditionDTO = gridRow.data;
        this.$scope.$applyAsync(() => {
            if (!this.selectedAdditions)
                this.selectedAdditions = [];

            let selectedAddition = this.getSelectedAddition(addition.type, addition.id);

            if (addition['selected']) {
                if (!selectedAddition)
                    this.selectedAdditions.push(new IntKeyValue(addition.type, addition.id));
            } else {
                if (selectedAddition)
                    this.selectedAdditions.splice(this.selectedAdditions.indexOf(selectedAddition), 1);
            }

            this.setAllSelected();
            this.setAsModified();
        });
    }

    private selectAllClicked() {
        this.$timeout(() => {
            if (this.additions.length === 0)
                return;

            this.selectedAdditions = [];
            this.additions.forEach(addition => {
                addition['selected'] = this.allSelected;
                if (this.allSelected)
                    this.selectedAdditions.push(new IntKeyValue(addition.type, addition.id));
            });

            this.setAsModified();
            super.gridDataLoaded(this.sortAdditions());
        });
    }

    // HELP-METHODS

    private getSelectedAddition(type: number, id: number): IntKeyValue {
        return this.selectedAdditions.find(a => a.key === type && a.value === id);
    }

    private setAllSelected() {
        this.allSelected = (this.selectedAdditions && this.selectedAdditions.length === this.additions.length)
    }

    private sortAdditions() {
        // Mark selected additions
        if (this.selectedAdditions) {
            this.additions.forEach(addition => {
                addition['selected'] = !!this.getSelectedAddition(addition.type, addition.id);
            });
        }

        // Sort to get selected additions at the top
        return _.orderBy(this.additions, ['selected', 'name'], ['desc', 'asc'])
    }

    private setAsModified() {
        this.setModified(true, this.parentGuid);

        if (this.onChange)
            this.onChange();
    }
}