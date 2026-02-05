import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";

export class EmployeeGroupsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl("Common/Directives/EmployeeGroups/EmployeeGroups.html"),
            scope: {
                selectedEmployeeGroups: '=?',
                readOnly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeGroupsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class EmployeeGroupsController {

    // Init parameters
    private selectedEmployeeGroups: number[];
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private allEmployeeGroups: ISmallGenericType[] = [];

    // Flags
    private allGroupsSelected: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private coreService: ICoreService) {
    }

    public $onInit() {
        if (!this.selectedEmployeeGroups)
            this.selectedEmployeeGroups = [];

        this.setupWatchers();
        this.loadAllEmployeeGroups();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.selectedEmployeeGroups, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.setSelectedEmployeeGroups();
        });
    }

    // SERVICE CALLS

    private loadAllEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(false).then(x => {
            this.allEmployeeGroups = x;

            if (this.selectedEmployeeGroups && this.selectedEmployeeGroups.length > 0)
                this.setSelectedEmployeeGroups();
        });
    }

    // EVENTS

    private selectAllGroups() {
        if (this.readOnly)
            return;

        let select = !this.allGroupsSelected;

        _.forEach(this.allEmployeeGroups, group => {
            if (group['isSelected'] !== select) {
                this.toggleSelected(group, false);
            }
        });

        this.allGroupsSelected = this.isAllGroupsSelected;

        if (this.onChange)
            this.onChange();
    }

    private toggleSelected(group: ISmallGenericType, notify: boolean = true) {
        if (!this.readOnly) {
            if (!this.selectedEmployeeGroups)
                this.selectedEmployeeGroups = [];

            group['isSelected'] = !group['isSelected'];
            if (!group['isSelected'])
                _.pull(this.selectedEmployeeGroups, group.id);
            else
                this.selectedEmployeeGroups.push(group.id);

            if (notify) {
                this.allGroupsSelected = this.isAllGroupsSelected;
                if (this.onChange)
                    this.onChange();
            }
        }
    }

    private get isAllGroupsSelected() {
        let allSelected: boolean = true;

        _.forEach(this.allEmployeeGroups, group => {
            if (!group['isSelected']) {
                allSelected = false;
                return false;
            }
        });

        return allSelected;
    }

    // HELP-METHODS

    private setSelectedEmployeeGroups() {
        if (!this.selectedEmployeeGroups)
            this.selectedEmployeeGroups = [];

        _.forEach(this.selectedEmployeeGroups, groupId => {
            let group = _.find(this.allEmployeeGroups, g => g.id === groupId);
            if (group)
                group['isSelected'] = true;
        });

        this.allGroupsSelected = this.isAllGroupsSelected;
    }
}