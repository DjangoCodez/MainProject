import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { Guid } from "../../../Util/StringUtility";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";
import { AccountDTO } from "../../Models/AccountDTO";

export class EmployeeAccountsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('EmployeeAccounts', 'EmployeeAccounts.html'),
            scope: {
                parentGuid: '=?',
                accountDimId: '=?',
                selectedAccounts: '=?',
                nbrOfRows: '@',
                showAccountDims: '=?',
                readOnly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeAccountsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class EmployeeAccountsController extends GridControllerBaseAg {

    // Setup
    private parentGuid: Guid;
    private accountDimId: number;
    private selectedAccounts: number[];
    private nbrOfRows: number;
    private showAccountDims: boolean;
    private readOnly: boolean;
    private onChange: Function;

    private minRowsToShow: number = 8; // Default

    // Collections
    private terms: any;
    private userPermittedAccounts: AccountDTO[] = [];
    private allAccountDims: AccountDimSmallDTO[] = [];
    private accountDims: AccountDimSmallDTO[] = [];
    private allAccounts: AccountDTO[] = [];

    // Properties
    private _selectedAccountDim: AccountDimSmallDTO;
    private get selectedAccountDim(): AccountDimSmallDTO {
        return this._selectedAccountDim;
    }
    private set selectedAccountDim(accountDim: AccountDimSmallDTO) {
        this._selectedAccountDim = accountDim;
        this.allAccounts = accountDim ? accountDim.accounts : [];
        super.gridDataLoaded(this.sortAccounts());
    }

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

        super("Common.Directives.EmployeeAccounts", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        if (!this.selectedAccounts)
            this.selectedAccounts = [];
    }

    public setupGrid() {
        this.$q.all([
            this.loadTerms(),
            this.loadAccountsByUserFromHierarchy()
        ]).then(() => {
            this.$q.all([
                this.loadAccountDims()
            ]).then(() => {
                this.setupGridColumns();
            });
        });
    }

    private setupGridColumns() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.setMinRowsToShow(this.nbrOfRows ? this.nbrOfRows : this.minRowsToShow);

        this.soeGridOptions.resetColumnDefs();
        this.soeGridOptions.addColumnBool("selected", this.terms["common.selected"], 30, { suppressFilter: true, enableEdit: !this.readOnly, onChanged: this.selectAccount.bind(this) });
        this.soeGridOptions.addColumnText("name", this.terms["common.name"], null);

        this.soeGridOptions.finalizeInitGrid();
        this.restoreState();
        this.setupWatchers();

        if (this.accountDimId)
            this.selectedAccountDim = this.accountDims.find(a => a.accountDimId === this.accountDimId);
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.accountDimId, (newVal, oldVal) => {
            if (newVal && newVal !== oldVal) {
                this.accountDimChanged();
                this.selectedAccountDim = this.accountDims.find(a => a.accountDimId === this.accountDimId);
            }
        });
        this.$scope.$watch(() => this.selectedAccounts, (newVal, oldVal) => {
            if (newVal && newVal !== oldVal)
                super.gridDataLoaded(this.sortAccounts());
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.name",
            "common.selected"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadAccountsByUserFromHierarchy(): ng.IPromise<any> {
        return this.coreService.getAccountsFromHierarchyByUser(CalendarUtility.getDateToday(), CalendarUtility.getDateToday(), true).then(x => {
            this.userPermittedAccounts = x;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        this.accountDims = [];
        return this.coreService.getAccountDimsSmall(false, true, true, false, true, false, false).then(x => {
            this.allAccountDims = x;
            this.accountDimChanged();
        });
    }

    // EVENTS

    private accountDimChanged() {
        if (this.accountDimId) {
            let dim = this.allAccountDims.find(a => a.accountDimId === this.accountDimId);
            if (dim)
                this.accountDims.push(dim);
        } else {
            // Only add account dims that the user is permitted to see
            let permittedAccountDimIds: number[] = _.uniq(_.map(this.userPermittedAccounts, a => a.accountDimId));
            _.forEach(this.allAccountDims, dim => {
                if (dim.accounts && dim.accounts.length > 0)
                    dim.accounts = _.sortBy(dim.accounts, t => t.name);
                if (_.includes(permittedAccountDimIds, dim.accountDimId))
                    this.accountDims.push(dim);
            });
        }

    }

    private selectAccount(gridRow) {
        var row: AccountDTO = gridRow.data;
        this.$scope.$applyAsync(() => {
            if (row['selected']) {
                if (!this.selectedAccounts)
                    this.selectedAccounts = [];
                if (!_.includes(this.selectedAccounts, row.accountId)) {
                    this.selectedAccounts.push(row.accountId);
                }
            } else {
                if (!this.selectedAccounts)
                    this.selectedAccounts = [];
                if (_.includes(this.selectedAccounts, row.accountId)) {
                    this.selectedAccounts.splice(this.selectedAccounts.indexOf(row.accountId), 1);
                }
            }

            this.setAsModified();
            this.soeGridOptions.refreshGrid();
        });
    }

    // HELP-METHODS

    private sortAccounts() {
        // Mark selected
        if (this.selectedAccounts) {
            _.forEach(this.allAccounts, account => {
                account['selected'] = _.includes(this.selectedAccounts, account.accountId);
            });
        }

        // Sort to get selected at the top
        return _.orderBy(this.allAccounts, ['selected', 'name'], ['desc', 'asc'])
    }

    private setAsModified() {
        this.setModified(true, this.parentGuid);

        if (this.onChange)
            this.onChange();
    }
}