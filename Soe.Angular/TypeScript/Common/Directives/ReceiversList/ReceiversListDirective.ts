import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { Guid } from "../../../Util/StringUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { MessageGroupMemberDTO } from "../../Models/MessageDTOs";
import { XEMailRecipientType, SoeEntityType, SoeCategoryType, CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { EmployeeListDTO, EmployeeRightListDTO } from "../../Models/EmployeeListDTO";
import { EmployeeAvailabilitySortOrder, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { IAvailableEmployeesDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ShiftDTO } from "../../Models/TimeSchedulePlanningDTOs";
import { CategoryDTO } from "../../Models/Category";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";
import { AccountDTO } from "../../Models/AccountDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class ReceiversListDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('ReceiversList', 'ReceiversList.html'),
            scope: {
                recipients: '=',
                onlyEmployee: '=?',
                onlyMessageGroup: '=?',
                parentGuid: '=?',
                readOnly: '=?',
                showAvailability: '=?',
                showAvailableEmployees: '=?',
                allEmployees: '=?',
                shifts: '=?',
                preloadUsers: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: ReceiversListController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ReceiversListController {
    // Init parameters
    private recipients: MessageGroupMemberDTO[];
    private onlyEmployee: boolean;
    private onlyMessageGroup: boolean;
    private parentGuid: Guid;
    private readOnly: boolean;
    private showAvailableEmployees: boolean;
    private allEmployees: EmployeeListDTO[];
    private shifts: ShiftDTO[];
    private preloadUsers: boolean;
    private onChange: Function;

    private progress: IProgressHandler;

    // Terms
    private terms: any;

    // Permissions
    private rolePermission = false;
    private employeeGroupPermission = false;
    private editMessageGroupPermission = false;

    // Company settings
    private useAccountHierarchy: boolean = false;

    // Collections
    private availableRecipients: any[] = [];
    private filteredRecipients: any[] = [];
    private recipientType: XEMailRecipientType;
    private recipientTypes: any[] = [];
    private users: any[] = [];
    private employeeGroups: any[] = [];
    private roles: any[] = [];
    private categories: any[] = [];
    private employees: any[] = [];
    private messageGroups: any[] = [];
    private accounts: any[] = [];
    private userPermittedAccounts: AccountDTO[] = [];
    private accountDims: AccountDimSmallDTO[] = [];
    private selectedAccountDim: AccountDimSmallDTO;
    private shiftIds: number[];

    // Flags
    private loadingAccountDims: boolean = false;
    private loadingRecipients: boolean = false;

    // Employee list filter
    private employeeListFilterEmployeeIds: number[] = [];
    private filteringEmployees: boolean = false;
    private isEmployeeListFiltered: boolean = false;
    private employeesWantsExtraShifts: boolean = false;

    // Properties
    private get allItemsSelected(): boolean {
        var selected = true;
        _.forEach(this.availableRecipients, item => {
            if (!item.selected) {
                selected = false;
                return false;
            }
        });

        return selected;
    }

    private searchString: string;

    private availSortBy: string = 'name';
    private availSortByReverse: boolean = false;
    private recSortBy: string = 'name';
    private recSortByReverse: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();
    }

    // SETUP

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadReadOnlyPermissions(),
            this.loadCompanySettings()
        ]).then(() => {
            var queue = [];
            if (this.preloadUsers && !this.useAccountHierarchy)
                queue.push(this.loadUsers());

            if (this.onlyMessageGroup || this.showAvailableEmployees)
                queue.push(this.loadMessageGroups());

            this.$q.all(queue).then(() => {
                this.setupRecipientTypes();
                this.setDefaultRecipientType();
                this.setupWatchers();
            });
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.allEmployees, (newVal, oldVal) => {
            this.setupEmployees();
        });

        this.$scope.$watch(() => this.shifts, (newVal, oldVal) => {
            this.shiftIds = _.map(this.shifts, s => s.timeScheduleTemplateBlockId);
        });

        this.$scope.$watch(() => this.showAvailableEmployees, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.setupRecipientTypes();
                this.setDefaultRecipientType();
            }
        });

        this.$scope.$watchCollection(() => this.recipients, (newVal, oldVal) => {
            this.setRecipientNames();
        });
    }

    private setupEmployees() {
        _.forEach(this.allEmployees, emp => {
            this.employeeListFilterEmployeeIds.push(emp.employeeId);
        });
    }

    private setupRecipientTypes() {
        this.recipientTypes = [];

        if (this.onlyMessageGroup) {
            this.recipientTypes.push({ id: XEMailRecipientType.MessageGroup, name: this.terms["common.receiverslist.messagegroup"] });
            return;
        }

        if (this.showAvailableEmployees) {
            this.recipientTypes.push({ id: XEMailRecipientType.Employee, name: this.terms["common.receiverslist.employee"] });
        }

        if (!this.onlyEmployee) {
            this.recipientTypes.push({ id: XEMailRecipientType.User, name: this.terms["common.receiverslist.user"] });
            if (this.useAccountHierarchy) {
                this.recipientTypes.push({ id: XEMailRecipientType.Account, name: this.terms["common.receiverslist.account"] });
            } else {
                this.recipientTypes.push({ id: XEMailRecipientType.Category, name: this.terms["common.receiverslist.category"] });
            }
            if (this.rolePermission)
                this.recipientTypes.push({ id: XEMailRecipientType.Role, name: this.terms["common.receiverslist.role"] });
            if (this.employeeGroupPermission)
                this.recipientTypes.push({ id: XEMailRecipientType.Group, name: this.terms["common.receiverslist.employeegroup"] });
            this.recipientTypes.push({ id: XEMailRecipientType.MessageGroup, name: this.terms["common.receiverslist.messagegroup"] });
        }
    }

    private setDefaultRecipientType() {
        let type: XEMailRecipientType = XEMailRecipientType.User;
        if (this.onlyMessageGroup)
            type = XEMailRecipientType.MessageGroup;
        else if (this.showAvailableEmployees)
            type = XEMailRecipientType.Employee;
        else if (this.useAccountHierarchy)
            type = XEMailRecipientType.Account;

        this.recipientTypeChanged(type);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.xemail.from",
            "core.xemail.to",
            "common.users",
            "common.receiverslist.name",
            "common.receiverslist.username",
            "common.receiverslist.employee",
            "common.receiverslist.user",
            "common.receiverslist.employeegroup",
            "common.receiverslist.role",
            "common.receiverslist.category",
            "common.receiverslist.messagegroup",
            "common.receiverslist.account",
            "time.schedule.planning.available",
            "time.schedule.planning.unavailable"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadReadOnlyPermissions() {
        var featureIds: number[] = [];
        featureIds.push(Feature.Manage_Roles);
        featureIds.push(Feature.Time_Employee_Groups);
        featureIds.push(Feature.Manage_Preferences_Registry_EventReceiverGroups);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.rolePermission = x[Feature.Manage_Roles];
            this.employeeGroupPermission = x[Feature.Time_Employee_Groups];
            this.editMessageGroupPermission = x[Feature.Manage_Preferences_Registry_EventReceiverGroups];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadUsers = _.debounce(() => {
        return this.coreService.getUsers(false, true, true, true, false).then(x => {
            _.forEach(x, (usr: any) => {
                this.users.push({ type: this.terms["common.receiverslist.user"], name: usr.name, username: usr.loginName, recordId: usr.userId, entity: SoeEntityType.User, wantsExtraShifts: false });
            });
        });
    }, 200, { leading: true, trailing: false });

    private loadRoles(): ng.IPromise<any> {
        return this.coreService.getCompanyRolesDict(false, false).then(x => {
            _.forEach(x, (role: any) => {
                this.roles.push({ type: this.terms["common.receiverslist.role"], name: role.name, username: role.name, recordId: role.id, entity: SoeEntityType.Role, wantsExtraShifts: false });
            });
        });
    }

    private loadCategories(): ng.IPromise<any> {
        this.categories = [];
        return this.coreService.getCategories(SoeCategoryType.Employee, false, true, true, true).then(x => {
            _.forEach(x, (category: CategoryDTO) => {
                this.categories.push({ type: this.terms["common.receiverslist.category"], name: category.name, username: category.name, recordId: category.categoryId, entity: SoeEntityType.Category, wantsExtraShifts: false });
            });
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        this.employees = [];
        return this.coreService.getEmployeesByAccountSetting().then(x => {
            _.forEach(x, (employee: ISmallGenericType) => {
                this.employees.push({ type: this.terms["common.receiverslist.employee"], name: employee.name, username: employee.name, recordId: employee.id, entity: SoeEntityType.Employee, wantsExtraShifts: false });
            });
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(false).then(x => {
            _.forEach(x, (empGroup: ISmallGenericType) => {
                this.employeeGroups.push({ type: this.terms["common.receiverslist.employeegroup"], name: empGroup.name, username: empGroup.name, recordId: empGroup.id, entity: SoeEntityType.EmployeeGroup, wantsExtraShifts: false });
            });
        });
    }

    private loadMessageGroups = _.debounce(() => {
        this.messageGroups = [];
        return this.coreService.getMessageGroupsDict(false).then(x => {
            if (this.showAvailableEmployees)
                this.messageGroups.push({ type: this.terms["common.receiverslist.messagegroup"], name: '', recordId: 0, entity: SoeEntityType.MessageGroup });

            _.forEach(x, (msgGroup: ISmallGenericType) => {
                this.messageGroups.push({ type: this.terms["common.receiverslist.messagegroup"], name: msgGroup.name, username: msgGroup.name, recordId: msgGroup.id, entity: SoeEntityType.MessageGroup, wantsExtraShifts: false });
            });
        });
    }, 200, { leading: true, trailing: false });

    private loadAccountsByUserFromHierarchy(): ng.IPromise<any> {
        return this.coreService.getAccountsFromHierarchyByUser(CalendarUtility.getDateToday(), CalendarUtility.getDateToday(), true, false, false, true, true).then(x => {
            this.userPermittedAccounts = x;
        });
    }

    private loadAccountDims = _.debounce(() => {
        this.loadingAccountDims = true;
        this.accountDims = [];

        return this.coreService.getAccountDimsSmall(false, true, true, false, true, false, false).then(x => {
            // Only add account dims that the user is permitted to see
            let permittedAccountDimIds: number[] = _.uniq(_.map(this.userPermittedAccounts, a => a.accountDimId));
            _.forEach(x, dim => {
                if (dim.accounts && dim.accounts.length > 0)
                    dim.accounts = _.sortBy(dim.accounts, t => t.name);
                if (_.includes(permittedAccountDimIds, dim.accountDimId))
                    this.accountDims.push(dim);
            });
            this.loadingAccountDims = false;
            this.loadingRecipients = false;
        });
    }, 200, { leading: true, trailing: false });

    // EVENTS

    private recipientTypeChanged(item: XEMailRecipientType) {
        if (this.recipientType == item)
            return;

        this.availableRecipients = [];
        if (item !== XEMailRecipientType.Employee)
            this.loadingRecipients = true;

        this.recipientType = item;

        // Show selected recipient types in left list
        switch (this.recipientType) {
            case XEMailRecipientType.User:
                if (this.users.length === 0) {
                    this.loadUsers().then(() => {
                        this.availableRecipients = this.users;
                        this.setFilteredRecipients();
                    });
                } else {
                    this.availableRecipients = this.users;
                    this.setFilteredRecipients();
                }
                break;
            case XEMailRecipientType.Employee:
                // Need a timeout for the available employees directive to be created
                this.$timeout(() => {
                    this.$scope.$broadcast('getAvailableEmployees', null);
                }, 200);
                break;
            case XEMailRecipientType.Group:
                if (this.employeeGroups.length === 0) {
                    this.loadEmployeeGroups().then(() => {
                        this.availableRecipients = this.employeeGroups;
                        this.setFilteredRecipients();
                    });
                } else {
                    this.availableRecipients = this.employeeGroups;
                    this.setFilteredRecipients();
                }
                break;
            case XEMailRecipientType.Role:
                if (this.roles.length === 0) {
                    this.loadRoles().then(() => {
                        this.availableRecipients = this.roles;
                        this.setFilteredRecipients();
                    });
                } else {
                    this.availableRecipients = this.roles;
                    this.setFilteredRecipients();
                }
                break;
            case XEMailRecipientType.Category:
                if (this.categories.length === 0) {
                    this.loadCategories().then(() => {
                        this.availableRecipients = this.categories;
                        this.setFilteredRecipients();
                    });
                } else {
                    this.availableRecipients = this.categories;
                    this.setFilteredRecipients();
                }
                break;
            case XEMailRecipientType.Account:
                if (this.userPermittedAccounts.length === 0) {
                    this.loadAccountsByUserFromHierarchy().then(() => {
                        this.loadAccountDims();
                    });
                } else {
                    this.accountDimChanged();
                }
                //if (this.employees.length === 0) {
                //    this.loadEmployees().then(() => {
                //        this.availableRecipients = this.employees;
                //        this.setFilteredRecipients();
                //    });
                //} else {
                //    this.availableRecipients = this.employees;
                //    this.setFilteredRecipients();
                //}
                break;
            case XEMailRecipientType.MessageGroup:
                if (this.messageGroups.length === 0) {
                    this.loadMessageGroups().then(() => {
                        this.availableRecipients = this.messageGroups;
                        this.setFilteredRecipients();
                    });
                } else {
                    this.availableRecipients = this.messageGroups;
                    this.setFilteredRecipients();
                }
                break;
        }
    }

    private accountDimChanged() {
        this.$timeout(() => {
            this.accounts = [];
            if (this.selectedAccountDim) {
                _.forEach(this.selectedAccountDim.accounts, account => {
                    this.accounts.push({ type: this.selectedAccountDim.name, name: account.name, username: account.name, recordId: account.accountId, entity: SoeEntityType.Account, wantsExtraShifts: false });
                });
            }

            this.availableRecipients = this.accounts;
            this.setFilteredRecipients();
        });
    }

    private searchChanged() {
        this.$timeout(() => {
            this.setFilteredRecipients();
        });
    }

    private selectAllItems() {
        var selected: boolean = this.allItemsSelected;
        _.forEach(this.availableRecipients, recipient => {
            recipient.selected = !selected;
        });
    }

    private availSort(column: string) {
        this.availSortByReverse = !this.availSortByReverse && this.availSortBy === column;
        this.availSortBy = column;
    }

    private recSort(column: string) {
        this.recSortByReverse = !this.recSortByReverse && this.recSortBy === column;
        this.recSortBy = column;
    }

    private showUsersInGroup(recipient: any) {
        switch (this.recipientType) {
            case XEMailRecipientType.User:
                // Not implemented, no use since only one user
                break;
            case XEMailRecipientType.Employee:
                // Not implemented, no use since only one user
                break;
            case XEMailRecipientType.Group:
                this.progress.startLoadingProgress([() => {
                    return this.coreService.getMessageGroupUsersByEmployeeGroup(recipient.recordId).then(x => {
                        this.showUsersInDialog(x, recipient.name);
                    });
                }]);
                break;
            case XEMailRecipientType.Role:
                this.progress.startLoadingProgress([() => {
                    return this.coreService.getMessageGroupUsersByRole(recipient.recordId).then(x => {
                        this.showUsersInDialog(x, recipient.name);
                    });
                }]);
                break;
            case XEMailRecipientType.Category:
                this.progress.startLoadingProgress([() => {
                    return this.coreService.getMessageGroupUsersByCategory(recipient.recordId).then(x => {
                        this.showUsersInDialog(x, recipient.name);
                    });
                }]);
                break;
            case XEMailRecipientType.Account:
                this.progress.startLoadingProgress([() => {
                    return this.coreService.getMessageGroupUsersByAccount(recipient.recordId).then(x => {
                        this.showUsersInDialog(x, recipient.name);
                    });
                }]);
                break;
            case XEMailRecipientType.MessageGroup:
                // Not implemented, user need to look into that message group
                break;
        }
    }

    private showUsersInDialog(users: ISmallGenericType[], groupName: string) {
        let msg = users.map(u => u.name).join('\n');
        this.notificationService.showDialogEx('{0}, {1} {2}'.format(groupName, users.length.toString(), this.terms['common.users']), msg, SOEMessageBoxImage.Information);
    }

    private addRecipients() {
        // Add selected recipients to the recipient list
        var selectedRecipients = this.getSelectedRecipients();
        if (selectedRecipients.length === 0)
            return;

        _.forEach(selectedRecipients, recipient => {
            if (!this.recipients)
                this.recipients = [];

            // Don't add the same recipient twice
            if (!_.find(this.recipients, r => r.recordId === recipient.employeeId)) {
                var member = new MessageGroupMemberDTO;
                member.name = recipient.name;
                member.entity = recipient.entity;

                if (this.recipientType === XEMailRecipientType.Employee) {
                    member.recordId = recipient.employeeId;
                    member['type'] = this.terms["common.receiverslist.employee"];
                    member.username = recipient.employeeNr;
                } else {
                    member.recordId = recipient.recordId;
                    member['type'] = recipient.type;
                    member.username = recipient.username;
                }
                this.recipients.push(member);
            }

            recipient.selected = false;
        });

        _.filter(this.availableRecipients, r => _.includes(_.map(selectedRecipients, s => s.recordId), r.recordId)).forEach(r => r.added = true);

        if (this.onChange())
            this.onChange();
    }

    private removeAllRecipients() {
        this.recipients = [];
        this.availableRecipients.forEach(r => r.added = false);

        if (this.onChange())
            this.onChange();
    }

    private removeRecipient(recipient: any) {
        var index: number = this.recipients.indexOf(recipient);
        this.recipients.splice(index, 1);
        this.setRecipientAsAdded(recipient.recordId, false);

        if (this.onChange())
            this.onChange();
    }

    private filteringEmployeesDone(employees: IAvailableEmployeesDTO[]) {
        this.employeesWantsExtraShifts = false;
        this.copyAllEmployeeLists();

        if (!employees)
            employees = [];

        this.availableRecipients = _.filter(this.availableRecipients, r => _.includes(_.map(employees, e => e.employeeId), r.employeeId));

        // Set wantsExtraShifts flag
        _.forEach(_.filter(employees, e => e.wantsExtraShifts), employee => {
            var empList = _.find(this.availableRecipients, e => e.employeeId === employee.employeeId);
            if (empList)
                empList.wantsExtraShifts = true;
            if (!this.employeesWantsExtraShifts)
                this.employeesWantsExtraShifts = true;
        });

        // Set some common properties used by all types
        _.forEach(this.availableRecipients, recipient => {
            recipient.type = this.terms["common.receiverslist.employee"];
            recipient.username = recipient.employeeNr;
            recipient.recordId = recipient.employeeId;
            recipient.entity = SoeEntityType.Employee;
        });
        this.setAvailabilityOnEmployees();
        this.setFilteredRecipients();
    }

    private copyAllEmployeeLists() {
        this.availableRecipients = [];
        _.forEach(_.orderBy(this.allEmployees, 'name'), employee => {
            this.copyEmployeeList(employee);
        });
    }

    private copyEmployeeList(employee: EmployeeListDTO) {
        var emp = new EmployeeRightListDTO();
        emp.employeeId = employee.employeeId;
        emp.employeeNr = employee.employeeNr;
        emp.employeeNrSort = employee.employeeNrSort;
        emp.firstName = employee.firstName;
        emp.lastName = employee.lastName;
        emp.name = employee.numberAndName;
        emp.wantsExtraShifts = false;
        this.availableRecipients.push(emp);
    }

    private setAvailabilityOnEmployees() {
        if (!this.shifts)
            return;

        let availableRangeStart: Date = _.first(_.sortBy(this.shifts, s => s.actualStartTime)).actualStartTime;
        let availableRangeStop: Date = _.last(_.sortBy(this.shifts, s => s.actualStopTime)).actualStopTime;

        _.forEach(this.availableRecipients, employee => {
            let fullEmp = _.find(this.allEmployees, e => e.employeeId === employee.employeeId);
            if (fullEmp) {
                let availabilityToolTip: string = '';
                if (fullEmp.isFullyAvailableInRange(availableRangeStart, availableRangeStop)) {
                    employee.isFullyAvailable = true;
                    employee.availabilitySort = EmployeeAvailabilitySortOrder.FullyAvailable;
                    availabilityToolTip = this.terms["time.schedule.planning.available"];
                } else if (fullEmp.isFullyUnavailableInRange(availableRangeStart, availableRangeStop)) {
                    employee.isFullyUnavailable = true;
                    employee.availabilitySort = EmployeeAvailabilitySortOrder.FullyUnavailable;
                    availabilityToolTip = this.terms["time.schedule.planning.unavailable"];
                } else {
                    let partlyAvailable = fullEmp.isAvailableInRange(availableRangeStart, availableRangeStop);
                    let partlyUnavailable = fullEmp.isUnavailableInRange(availableRangeStart, availableRangeStop);
                    if (partlyAvailable && !partlyUnavailable) {
                        employee.isPartlyAvailable = true;
                        employee.availabilitySort = EmployeeAvailabilitySortOrder.PartlyAvailable;
                    } else if (partlyUnavailable && !partlyAvailable) {
                        employee.isPartlyUnavailable = true;
                        employee.availabilitySort = EmployeeAvailabilitySortOrder.PartlyUnavailable;
                    } else if (partlyAvailable && partlyUnavailable) {
                        employee.isMixedAvailable = true;
                        employee.availabilitySort = EmployeeAvailabilitySortOrder.MixedAvailable;
                    }
                    if (partlyAvailable) {
                        let availableDates = fullEmp.getAvailableInRange(availableRangeStart, availableRangeStop);
                        if (availableDates.length > 0) {
                            _.forEach(availableDates, availableDate => {
                                availabilityToolTip += "{0} {1}-{2}".format(this.terms["time.schedule.planning.available"], availableDate.start.toFormattedTime(), availableDate.stop.toFormattedTime());
                                if (availableDate.comment)
                                    availabilityToolTip += ", {0}".format(availableDate.comment);
                                availabilityToolTip += "\n";
                            });
                        }
                    }
                    if (partlyUnavailable) {
                        let unavailableDates = fullEmp.getUnavailableInRange(availableRangeStart, availableRangeStop);
                        if (unavailableDates.length > 0) {
                            _.forEach(unavailableDates, unavailableDate => {
                                availabilityToolTip += "{0} {1}-{2}".format(this.terms["time.schedule.planning.unavailable"], unavailableDate.start.toFormattedTime(), unavailableDate.stop.toFormattedTime());
                                if (unavailableDate.comment)
                                    availabilityToolTip += ", {0}".format(unavailableDate.comment);
                                availabilityToolTip += "\n";
                            });
                        }
                    }
                }

                if (availabilityToolTip.length > 0)
                    employee.toolTip = availabilityToolTip;
            }
        });
    }

    // HELP-METHODS

    private setRecipientNames() {
        if (!this.recipients)
            return;

        // First make sure all different entities are loaded
        let entities = _.uniq(this.recipients.map(r => r.entity));
        let queue = [];
        let secondQueue = [];
        _.forEach(entities, entity => {
            switch (entity) {
                case SoeEntityType.User:
                    if (this.users.length === 0)
                        queue.push(this.loadUsers());
                    break;
                case SoeEntityType.Role:
                    if (this.roles.length === 0)
                        queue.push(this.loadRoles());
                    break;
                case SoeEntityType.Category:
                    if (this.categories.length === 0)
                        queue.push(this.loadCategories());
                    break;
                case SoeEntityType.EmployeeGroup:
                    if (this.employeeGroups.length === 0)
                        queue.push(this.loadEmployeeGroups());
                    break;
                case SoeEntityType.Employee:
                    if (this.useAccountHierarchy && this.employees.length === 0)
                        queue.push(this.loadEmployees());
                    break;
                case SoeEntityType.MessageGroup:
                    if (this.messageGroups.length === 0)
                        queue.push(this.loadMessageGroups());
                    break;
                case SoeEntityType.Account:
                    if (this.useAccountHierarchy && this.userPermittedAccounts.length === 0) {
                        queue.push(this.loadAccountsByUserFromHierarchy());
                        secondQueue.push(this.loadAccountDims());
                    }
                    break;
            }
        });
        this.$q.all(queue).then(() => {
            this.$q.all(secondQueue).then(() => {
                _.forEach(this.recipients, r => {
                    switch (r.entity) {
                        case SoeEntityType.User:
                            r.type = this.terms["common.receiverslist.user"];
                            r.name = this.getUserName(r.recordId);
                            break;
                        case SoeEntityType.Role:
                            r.type = this.terms["common.receiverslist.role"];
                            r.name = this.getRoleName(r.recordId);
                            break;
                        case SoeEntityType.Category:
                            r.type = this.terms["common.receiverslist.category"];
                            r.name = this.getCategoryName(r.recordId);
                            break;
                        case SoeEntityType.EmployeeGroup:
                            r.type = this.terms["common.receiverslist.employeegroup"];
                            r.name = this.getEmployeeGroupName(r.recordId);
                            break;
                        case SoeEntityType.Employee:
                            r.type = this.terms["common.receiverslist.employee"];
                            if (this.useAccountHierarchy)
                                r.name = this.getEmployeeByAccountName(r.recordId);
                            else
                                r.name = this.getEmployeeName(r.recordId);
                            break;
                        case SoeEntityType.MessageGroup:
                            r.type = this.terms["common.receiverslist.messagegroup"];
                            r.name = this.getMessageGroupName(r.recordId);
                            break;
                        case SoeEntityType.Account:
                            this.setAccountNames(r);
                            break;
                    }
                });
                this.setFilteredRecipients();
            });
        });
    }

    private setRecipientAsAdded(recordId: number, added: boolean = true) {
        let rec = _.find(this.availableRecipients, r => r.recordId === recordId);
        if (rec)
            rec.added = added;
    }

    private getUserName(userId: number): string {
        this.setRecipientAsAdded(userId);
        var user = _.find(this.users, u => u.recordId === userId);
        return user ? user.name : '';
    }

    private getRoleName(roleId: number): string {
        this.setRecipientAsAdded(roleId);
        var role = _.find(this.roles, r => r.recordId === roleId);
        return role ? role.name : '';
    }

    private getCategoryName(categoryId: number): string {
        this.setRecipientAsAdded(categoryId);
        var category = _.find(this.categories, c => c.recordId === categoryId);
        return category ? category.name : '';
    }

    private getEmployeeGroupName(employeeGroupId: number): string {
        this.setRecipientAsAdded(employeeGroupId);
        var group = _.find(this.employeeGroups, e => e.recordId === employeeGroupId);
        return group ? group.name : '';
    }

    private getEmployeeName(employeeId: number): string {
        this.setRecipientAsAdded(employeeId);
        var emp = _.find(this.allEmployees, e => e.employeeId === employeeId);
        return emp ? emp.numberAndName : '';
    }

    private getEmployeeByAccountName(employeeId: number): string {
        this.setRecipientAsAdded(employeeId);
        var employee = _.find(this.employees, c => c.recordId === employeeId);
        return employee ? employee.name : '';
    }

    private getMessageGroupName(groupId: number): string {
        this.setRecipientAsAdded(groupId);
        var group = _.find(this.messageGroups, m => m.recordId === groupId);
        return group ? group.name : '';
    }

    private setAccountNames(recipient: any) {
        _.forEach(this.accountDims, dim => {
            if (dim.accounts && dim.accounts.length > 0) {
                let account = dim.accounts.find(a => a.accountId === recipient.recordId);
                if (account) {
                    recipient.type = dim.name;
                    recipient.name = account.name;
                    return false;
                }
            }
        });
    }

    private getSelectedRecipients(): any[] {
        return _.filter(this.availableRecipients, i => i.selected);
    }

    private setFilteredRecipients() {
        if (!this.searchString)
            this.filteredRecipients = _.filter(this.availableRecipients, r => !r.added);
        else
            this.filteredRecipients = _.filter(this.availableRecipients, r => !r.added && ((<string>r.username).contains(this.searchString) || (<string>r.name).contains(this.searchString)))

        this.loadingRecipients = false;
    }
}