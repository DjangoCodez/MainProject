import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { EmployeePositionDTO, PositionDTO, SysPositionGridDTO } from "../../Models/EmployeePositionDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Feature } from "../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../Util/CoreUtility";

export class PositionsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('Positions', 'Positions.html'),
            scope: {
                employeeId: '=',
                selectedPositions: '=',
                nbrOfRows: '@',
                readOnly: '=',
                hideHeader: '@'
            },
            restrict: 'E',
            replace: true,
            controller: PositionsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class PositionsController extends GridControllerBase {

    // Init parameters
    private employeeId: number;
    private selectedPositions: EmployeePositionDTO[];
    private nbrOfRows: number;
    private readOnly: boolean;
    private hideHeader: boolean;

    // Collections
    private terms: any;
    private allPositions: PositionDTO[] = [];
    private sysPositions: SysPositionGridDTO[] = [];
    private gridPositions: EmployeePositionDTO[] = [];

    private selectAllPositions: boolean = false;
    private disableSelect: boolean = false;

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

        super("Common.Directives.Positions", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.enableDoubleClick = false;
        this.soeGridOptions.setMinRowsToShow(this.nbrOfRows ? this.nbrOfRows : 8);

        this.hideHeader = <any>this.hideHeader === 'true';
    }

    public setupGrid() {
        this.startLoad();
        this.$q.all([
            this.loadTerms(),
            this.loadAllPositions(),
            this.loadSysPositions(),
        ]).then(() => {
            this.setupGridColumns()
        });
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnBool("selected", this.terms["core.select"], "80", !this.readOnly, "selectEmployeePosition");
        this.soeGridOptions.addColumnText("employeePositionName", this.terms["common.name"], null);
        this.soeGridOptions.addColumnText("sysPositionCode", this.terms["time.employee.position.ssyk"], null);
        this.soeGridOptions.addColumnBool("default", this.terms["common.standard"], "100", !this.readOnly, "selectDefault");
        super.gridDataLoaded(this.sortEmployeePositions());

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.selectedPositions, () => {
            super.gridDataLoaded(this.sortEmployeePositions());
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.select",
            "common.name",
            "common.standard",
            "time.employee.position.ssyk"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadAllPositions(): ng.IPromise<any> {
        return this.coreService.getPositions(false, true).then(x => {
            this.allPositions = x;
        });
    }

    private loadSysPositions(): ng.IPromise<any> {
        return this.coreService.getSysPositions(CoreUtility.sysCountryId, CoreUtility.languageId, true).then(data => {
            this.sysPositions = data;
        });
    }

    // EVENTS

    private selectAll() {
        this.disableSelect = true;

        if (!this.selectedPositions)
            this.selectedPositions = [];

        this.$timeout(() => {
            _.forEach(this.gridPositions, position => {
                position.selected = this.selectAllPositions;
                var pos = _.find(this.selectedPositions, s => s.positionId === position.positionId);
                if (!pos && this.selectAllPositions === true)
                    this.selectedPositions.push(position);
                else {
                    if (this.selectAllPositions === false)
                        this.selectedPositions.splice(this.selectedPositions.indexOf(_.find(this.selectedPositions, s => s.positionId == position.positionId)), 1);
                }
            });
            this.disableSelect = false;
        });
    }

    private selectEmployeePosition(row) {
        this.$timeout(() => {
            var pos = _.find(this.selectedPositions, s => s.positionId === row.positionId);
            if (!pos && row.selected === true)
                this.selectedPositions.push(row);
            else {
                if (row.selected === false)
                    this.selectedPositions.splice(this.selectedPositions.indexOf(_.find(this.selectedPositions, s => s.positionId == pos.positionId)), 1);
            }

            this.positionChanged(row);
        });
    }

    private selectDefault(row) {
        this.$timeout(() => {
            _.forEach(this.gridPositions, position => {
                if (row.positionId === position.positionId)
                    position.default = row.default;
                else
                    position.default = false;
            });
            _.forEach(this.selectedPositions, position => {
                if (row.positionId === position.positionId)
                    position.default = row.default;
                else
                    position.default = false;
            });

            this.positionChanged(row);
        });
    }

    private positionChanged(row) {
        this.messagingService.publish('employeePositionChanged', { employeeId: this.employeeId, position: row });
    }

    // HELP-METHODS

    private sortEmployeePositions() {
        this.gridPositions = [];

        // Connect selectedEmployeePosistions and sysPositions
        if (this.selectedPositions) {
            _.forEach(this.selectedPositions, (item: EmployeePositionDTO) => {
                if (_.filter(this.gridPositions, s => s.positionId === item.positionId).length === 0)
                    this.gridPositions.push(item);
            });
        }
        if (this.gridPositions.length > 0) {
            _.forEach(this.gridPositions, (item: EmployeePositionDTO) => {
                item.selected = true;
            });
        }
        
        // Add rest of the positions
        _.forEach(this.allPositions, (item: PositionDTO) => {
            if (_.filter(this.gridPositions, s => s.positionId === item.positionId).length === 0) {

                if (item.name && item.name.length > 0) {
                    this.gridPositions.push({ positionId: item.positionId, employeePositionName: item.name, sysPositionCode: "", default: false, selected: false, employeeId: 0, employeePositionId: 0, sysPositionDescription: "", sysPositionName: "" });
                } else if (item.sysPositionId) {
                    var sysPosition = _.find(this.sysPositions, s => s.sysPositionId === item.sysPositionId);
                    this.gridPositions.push({ positionId: item.positionId, employeePositionName: sysPosition ? sysPosition.name : item.name, sysPositionCode: sysPosition ? sysPosition.code : '', default: false, selected: false, employeeId: 0, employeePositionId: 0, sysPositionDescription: "", sysPositionName: "" });
                }
            }
        });

        // Sort to get selected positons at the top
        return _.orderBy(this.gridPositions, ['selected', 'employeePositionName'], ['desc', 'asc'])
    }
}
