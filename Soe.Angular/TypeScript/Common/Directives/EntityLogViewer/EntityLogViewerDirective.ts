import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { PersonalDataLogMessageDTO } from "../../Models/PersonalDataLogMessageDTO";
import { TrackChangesLogDTO } from "../../Models/TrackChangesDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreService } from "../../../Core/Services/CoreService";
import { SoeEntityType, TermGroup, TermGroup_PersonalDataType, TermGroup_PersonalDataInformationType, TermGroup_PersonalDataActionType } from "../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IActorSearchPersonDTO } from "../../../Scripts/TypeLite.Net4";

export class EntityLogViewerDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('EntityLogViewer', 'EntityLogViewer.html'),
            scope: {
                entity: '=?',
                recordId: '=?',
                hideLogs: '=?',
                onlyChangeLog: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: EntityLogViewerController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class EntityLogViewerController {

    // Init parameters
    private entity: SoeEntityType;
    private recordId: number;
    private hideLogs: boolean;
    private onlyChangeLog: boolean;

    // Selections
    private dateFrom: Date;
    private dateTo: Date;
    private loadDataLog: boolean = false;
    private loadChangeLog: boolean = false;

    private searchInEmployeeAndUser: boolean = false;
    private searchInCustomer: boolean = false;
    private searchInSupplier: boolean = false;
    private searchInContactPerson: boolean = false;
    private searchInRot: boolean = false;
    private searchLogsCausedByUser: boolean = false;
    private actionType: TermGroup_PersonalDataActionType;
    private searchString: string;

    // Flags
    private isChangeLogOpen: boolean = false;
    private isDataLogOpen: boolean = false;
    private searching: boolean = false;
    private noPersonsFound: boolean = false;
    private firstDataLoaded: boolean = false;

    private persons: IActorSearchPersonDTO[] = [];
    private actionTypes: SmallGenericType[] = [];
    private changes: TrackChangesLogDTO[] = [];
    private personalDataLogs: PersonalDataLogMessageDTO[] = [];

    // Properties
    private selectedPerson: any;

    //@ngInject
    constructor(
        private coreService: CoreService,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private $window: ng.IWindowService,
        private urlHelperService: IUrlHelperService) {
    }

    // SETUP

    public $onInit() {
        this.dateFrom = CalendarUtility.getDateToday();
        this.dateTo = CalendarUtility.getDateToday();

        if (this.onlyChangeLog)
            this.loadChangeLog = true;

        this.$q.all([this.loadPersonalDataActionTypes()]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.recordId, (newVal, oldVal) => {
            if (newVal != oldVal) {
                if ((this.loadChangeLog && this.firstDataLoaded) || this.loadDataLog)
                    this.search();
                else
                    this.clearLogs();
            }
        });
    }

    // SERVICE CALLS

    private loadPersonalDataActionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PersonalDataActionType, false, false).then(x => {
            this.actionTypes = x;
            this.actionType = TermGroup_PersonalDataActionType.Unspecified
        });
    }

    private loadPersonalDataLogForEmployee(): ng.IPromise<any> {
        return this.coreService.getPersonalDataLogsForEmployee(this.selectedPerson ? this.selectedPerson.recordId : this.recordId, TermGroup_PersonalDataInformationType.Unspecified, this.actionType, this.dateFrom, this.dateTo).then(x => {
            this.personalDataLogs = x;
            this.isDataLogOpen = true;
        });
    }

    private loadPersonalDataLogCausedByEmployee(): ng.IPromise<any> {
        return this.coreService.getPersonalDataLogsCausedByEmployee(this.selectedPerson ? this.selectedPerson.recordId : this.recordId, 0, TermGroup_PersonalDataInformationType.Unspecified, this.actionType, this.dateFrom, this.dateTo).then(x => {
            this.personalDataLogs = x;
            this.isDataLogOpen = true;
        });
    }

    private loadPersonalDataLogCausedByUser(): ng.IPromise<any> {
        return this.coreService.getPersonalDataLogsCausedByUser(this.selectedPerson ? this.selectedPerson.recordId : this.recordId, 0, TermGroup_PersonalDataInformationType.Unspecified, this.actionType, this.dateFrom, this.dateTo).then(x => {
            this.personalDataLogs = x;
            this.isDataLogOpen = true;
        });
    }

    private loadTrackChanges(): ng.IPromise<any> {
        return this.coreService.getTrackChangesLog(this.entity, this.recordId, this.dateFrom, this.dateTo).then(x => {
            this.changes = x;
            this.isChangeLogOpen = true;
        });
    }

    private searchPerson() {
        this.noPersonsFound = false;
        this.loadDataLog = true;
        var searchEntities = [];
        if (this.searchInEmployeeAndUser) {
            searchEntities.push(SoeEntityType.Employee);
            searchEntities.push(SoeEntityType.User);
        }
        if (this.searchInContactPerson)
            searchEntities.push(SoeEntityType.ContactPerson);
        if (this.searchInCustomer)
            searchEntities.push(SoeEntityType.Customer);
        if (this.searchInSupplier)
            searchEntities.push(SoeEntityType.Supplier);
        if (this.searchInRot)
            searchEntities.push(SoeEntityType.HouseholdTaxDeductionApplicant);

        this.coreService.searchPerson(this.searchString, searchEntities).then((x) => {
            this.persons = x;
            if (this.persons.length > 0) {
                this.selectedPerson = this.persons[0];
            }
            else {
                this.selectedPerson = undefined;
                this.noPersonsFound = true;
            }
        });
    }

    private search() {
        this.clearLogs();

        this.searching = true;

        let promises: ng.IPromise<any>[] = [];

        if (this.loadDataLog) {
            if (this.entity || !this.searchLogsCausedByUser) {
                promises.push(this.loadPersonalDataLogForEmployee());
            }
            else {
                var type: TermGroup_PersonalDataType = this.getPersonalDataType(this.selectedPerson.entityType);
                if (type == TermGroup_PersonalDataType.Employee)
                    promises.push(this.loadPersonalDataLogCausedByEmployee());
                else if (type == TermGroup_PersonalDataType.User)
                    promises.push(this.loadPersonalDataLogCausedByUser());
            }
        }

        if (this.loadChangeLog)
            promises.push(this.loadTrackChanges());

        this.$q.all(promises).then(() => {
            this.searching = false;
            this.firstDataLoaded = true;
        });
    }

    // EVENTS

    private openTab(person: any) {
        switch (person.entityType) {
            case SoeEntityType.Employee:
                HtmlUtility.openInNewTab(this.$window, "/soe/time/employee/employees/?employeeid=" + person.recordId + "&employeenr=" + person.number);
                break;
            case SoeEntityType.User:
                break;
            case SoeEntityType.Supplier:
                HtmlUtility.openInNewTab(this.$window, "/soe/economy/supplier/suppliers/?actorsupplierid=" + person.recordId + "&suppliernr=" + person.number);
                break;
            case SoeEntityType.Customer:
                HtmlUtility.openInNewTab(this.$window, "/soe/economy/customer/customers/?actorcustomerid=" + person.recordId + "&customernr=" + person.number);
                break;
        }
    }

    private disableSearchPerson() {
        return (!this.hasSelectedSearchRegistry() || !this.searchString || this.searchString.length === 0)
    }

    private disableSearch() {
        return (!this.dateFrom || !this.dateTo || (!this.loadDataLog && !this.loadChangeLog) || (!this.entity && !this.hasSelectedSearchRegistry) || (this.entity && !this.recordId));
    }

    private hasSelectedSearchRegistry() {
        return (this.searchInEmployeeAndUser || this.searchInCustomer || this.searchInSupplier || this.searchInContactPerson || this.searchInRot);
    }

    private showSearchOnSocialSec(): boolean {
        return (this.searchInEmployeeAndUser);
    }

    private showSearchOnEmployeeNr(): boolean {
        return (this.searchInEmployeeAndUser);
    }

    private showSearchOnLicensePlate(): boolean {
        return (this.searchInEmployeeAndUser);
    }

    private clearLogs() {
        this.personalDataLogs = [];
        this.changes = [];
    }

    // HELP-METHODS

    private getPersonalDataType(entityType: any): TermGroup_PersonalDataType {
        if (entityType == SoeEntityType.Employee)
            return TermGroup_PersonalDataType.Employee;
        else if (entityType == SoeEntityType.User)
            return TermGroup_PersonalDataType.User;
        else if (entityType == SoeEntityType.ContactPerson)
            return TermGroup_PersonalDataType.ContactPerson;
        else if (entityType == SoeEntityType.Customer)
            return TermGroup_PersonalDataType.Customer;
        else if (entityType == SoeEntityType.Supplier)
            return TermGroup_PersonalDataType.Supplier;
        else if (entityType == SoeEntityType.HouseholdTaxDeductionApplicant)
            return TermGroup_PersonalDataType.HouseholdApplicant;
        return TermGroup_PersonalDataType.Unknown;
    }
}