import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Feature } from "../../../Util/CommonEnumerations";
import { Guid } from "../../../Util/StringUtility";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";

export class AccountsForDimDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AccountsForDim', 'AccountsForDim.html'),
            scope: {
                parentGuid: '=?',
                accountDimId: '=',
                selectedAccountIds: '=?',
                nbrOfRows: '@',
                hideHeader: '=?',
                showToolbar: '=?',
                readOnly: '=?',
                useFilters: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: AccountsForDimController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AccountsForDimController extends GridControllerBaseAg {

    // Setup
    private parentGuid: Guid;
    private accountDimId: number;
    private selectedAccountIds: number[];
    private nbrOfRows: number;
    private hideHeader: boolean;
    private showToolbar: boolean;
    private readOnly: boolean;
    private useFilters: boolean;
    private onChange: Function;

    // Converted init parameters
    private minRowsToShow: number = 8; // Default

    // Collections
    private terms: any;
    private accountDim: AccountDimSmallDTO;

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
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Common.Directives.AccountsForDim", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        if (!this.selectedAccountIds)
            this.selectedAccountIds = [];
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = this.useFilters;
        this.soeGridOptions.setMinRowsToShow(this.nbrOfRows ? this.nbrOfRows : this.minRowsToShow);
    }

    public setupGrid() {
        this.$q.all([
            this.loadTerms(),
            this.loadAccountDim()
        ]).then(() => this.setupGridColumns());
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnBool("selected", this.terms["common.selected"], 50, { enableEdit: !this.readOnly, onChanged: this.selectAccount.bind(this) });
        this.soeGridOptions.addColumnText("name", this.terms["common.name"], null);

        super.gridDataLoaded(this.sortAccounts());

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
        this.$scope.$watch(() => this.accountDimId, (oldVal, newVal) => {
            if (oldVal !== newVal) {
                this.selectedAccountIds = [];
                this.loadAccountDim();
            }
        });

        this.$scope.$watch(() => this.selectedAccountIds, (newVal, oldVal) => {
            if (newVal && newVal !== oldVal) {
                super.gridDataLoaded(this.sortAccounts());
                this.setAllSelected();
            }
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.selected",
            "common.name"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadAccountDim(): ng.IPromise<any> {
        return this.coreService.getAccountDim(this.accountDimId, true, false, false, true, true).then(x => {
            this.accountDim = x;
            super.gridDataLoaded(this.sortAccounts());
            this.setAllSelected();
        });
    }

    // EVENTS

    private selectAccount(gridRow) {
        var row = gridRow.data;
        this.$scope.$applyAsync(() => {
            if (row.selected) {
                if (!this.selectedAccountIds)
                    this.selectedAccountIds = [];
                if (!_.includes(this.selectedAccountIds, row.accountId))
                    this.selectedAccountIds.push(row.accountId);
            } else {
                if (!this.selectedAccountIds)
                    this.selectedAccountIds = [];
                if (_.includes(this.selectedAccountIds, row.accountId))
                    this.selectedAccountIds.splice(this.selectedAccountIds.indexOf(row.accountId), 1);
            }

            this.setAllSelected();
            this.setAsModified();
        });
    }

    private selectAllClicked() {
        this.$timeout(() => {
            if (!this.accountDim || this.accountDim.accounts.length === 0)
                return;

            this.selectedAccountIds = [];
            _.forEach(this.accountDim.accounts, acc => {
                acc['selected'] = this.allSelected;
                if (this.allSelected)
                    this.selectedAccountIds.push(acc.accountId);
            });

            this.setAsModified();
            super.gridDataLoaded(this.sortAccounts());
        });
    }

    // HELP-METHODS

    private setAllSelected() {
        this.allSelected = (this.selectedAccountIds && this.accountDim && this.accountDim.accounts && this.selectedAccountIds.length === this.accountDim.accounts.length)
    }

    private sortAccounts() {
        // Mark selected accounts
        if (this.accountDim && this.selectedAccountIds) {
            _.forEach(this.accountDim.accounts, account => {
                account['selected'] = (_.includes(this.selectedAccountIds, account.accountId));
            });
        }

        // Sort to get selected accounts at the top
        return _.orderBy(this.accountDim.accounts, ['selected', 'name'], ['desc', 'asc'])
    }

    private setAsModified() {
        this.setModified(true, this.parentGuid);

        if (this.onChange)
            this.onChange();
    }
}