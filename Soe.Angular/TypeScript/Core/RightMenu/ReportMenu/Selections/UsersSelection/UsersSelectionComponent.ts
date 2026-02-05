import { BoolSelectionDTO, IdListSelectionDTO, UserDataSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IReportDataService } from "../../ReportDataService";
import { UserGridDTO } from "../../../../../Common/Models/UserDTO";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class UsersSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: UsersSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/UsersSelection/UsersSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "=" ,
                selectedDate: "<"
            }
        };

        return options;
    }

    public static componentKey = "usersSelection";

    //binding properties
    private labelKey: string;    
    private onSelected: (_: { selection: UserDataSelectionDTO }) => void = angular.noop;
    private userSelectionInput: UserDataSelectionDTO;
    private selectedDate: Date;

    private includeInactive: boolean = false;

    private users: UserGridDTO[];
    private selectedUsers: UserGridDTO[] = [];
    
    private delaySetSavedUserSelection: boolean = false;
    private populating: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService, private reportDataService: IReportDataService) {
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        if (!this.selectedUsers)
            this.selectedUsers = [];
        if (!this.users)
            this.users = [];
        if (!this.selectedDate)
            this.selectedDate = CalendarUtility.getDateToday();

        this.loadUsers().then(() => {
            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
            this.populating = false;
        });

        this.propagateChange();
    }

    public $onChanges(objChanged) {
        if (!this.includeInactive)
            this.includeInactive = false;

        this.loadUsers().then(() => {
            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
            this.populating = false;
        });

        this.propagateChange();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.users.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedUsers = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedUsers = _.filter(this.users, u => _.includes(this.userSelectionInput.ids, u.userId));

        this.includeInactive = this.userSelectionInput.includeInactive;

        this.propagateChange();
    }

    private loadUsers(): ng.IPromise<any> {
        this.populating = true;
        return this.reportDataService.getUsersByCompanyDate(CoreUtility.actorCompanyId, false, this.includeInactive, false, this.selectedDate).then(x => {
            this.users = x;
        });
    }

    private onChange() {
        this.$timeout(() => {
            this.propagateChange();
        });
    }

    private showInactiveChanged() {
        this.$timeout(() => {
            this.loadUsers();
        });
    }

    private propagateChange() {
        let selectedIds = this.selectedUsers.map(u => u['userId']);
        const selection = new UserDataSelectionDTO(selectedIds, this.includeInactive);

        if (!selection)
            return;

        this.onSelected({ selection: selection });
    }
}