import { IScheduleService } from "../../../ScheduleService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IReportDataService } from "../../../../../Core/RightMenu/ReportMenu/ReportDataService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { ReportJobDefinitionFactory } from "../../../../../Core/Handlers/ReportJobDefinitionFactory";
import { SoeReportTemplateType } from "../../../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { AddDocumentToAttestFlowController } from "../../../../../Common/Dialogs/AddDocumentToAttestFlow/AddDocumentToAttestFlowController";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";

enum EnumSelectionType {

    Interval = 0,
    Dates = 1
}

export class PrintEmploymentCertificateController {

    private terms: any;
    private filteringEmployees: boolean;
    private executing: boolean = false;
    private dateIntervalText: string;
    private selectableDates = [];
    private selectedDates: Date[] = [];
    private oneReportPerEmployee: boolean = false;
    private oneReportPerEmployeeDisabled: boolean = false;
    private initSigning: boolean = false;
    private employeesWithSubstituteShifts: EmployeeListDTO[] = [];
    private searchString: string;
    private employeeTemplates: ISmallGenericType[] = [];

    public progress: IProgressHandler;

    private _selectedSelectionType: number;
    get selectedSelectionType(): number {
        return this._selectedSelectionType;
    }
    set selectedSelectionType(value: number) {
        this._selectedSelectionType = value;

        this.unSelectAllItems();
        this.employeesWithSubstituteShifts = [];
    }

    private _selectedEmployeeTemplateId: number = 0;
    get selectedEmployeeTemplateId(): number {
        return this._selectedEmployeeTemplateId;
    }
    set selectedEmployeeTemplateId(value: number) {
        this._selectedEmployeeTemplateId = value;

        this.savePrintout = value > 0;
        this.oneReportPerEmployee = value > 0;
        this.oneReportPerEmployeeDisabled = value > 0 || this.savePrintout;
    }

    private _savePrintout: boolean = false;
    get savePrintout(): boolean {
        return this._savePrintout;
    }
    set savePrintout(value: boolean) {
        this._savePrintout = value;

        if (value)
            this.oneReportPerEmployee = true;

        this.initSigning = value;
        this.oneReportPerEmployeeDisabled = value || this.selectedEmployeeTemplateId > 0;
    }

    get allItemsSelected(): boolean {
        let selected = true;
        _.forEach(this.employeesWithSubstituteShifts, item => {
            if (!item.isSelected) {
                selected = false;
                return false;
            }
        });
        return selected;
    }

    get filteredEmployeesWithSubstituteShifts() {
        if (!this.searchString)
            return this.employeesWithSubstituteShifts;
        else
            return _.filter(this.employeesWithSubstituteShifts, r => (<string>r.numberAndName).contains(this.searchString))
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModalInstance,
        private $uibModal,
        progressHandlerFactory: IProgressHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private reportDataService: IReportDataService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private title: string,
        private inputEmployees: EmployeeListDTO[],
        private reportId: number,
        private reportName: string,
        private dateFrom: Date,
        private dateTo: Date,
        private isSendMode: boolean,
        private hasEmployeeTemplates) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.selectedSelectionType = EnumSelectionType.Interval;
        this.dateIntervalText = "{0} - {1}".format(this.dateFrom.format('dddd D MMMM'), this.dateTo.format('dddd D MMMM'));
        const dates = CalendarUtility.getDates(this.dateFrom, this.dateTo);
        this.selectableDates.length = 0;
        _.forEach(dates, (date: Date) => {
            this.selectableDates.push({
                id: date,
                label: date.toFormattedDate()
            });
        });
        this.inputEmployees = _.filter(this.inputEmployees, e => !e.hidden);

        this.loadTerms();
        if (this.hasEmployeeTemplates)
            this.loadEmployeeTemplates();
    }

    // LOOKUPS

    private loadTerms() {
        const keys: string[] = [
            "core.printing",
            "error.default_error",
            "time.schedule.planning.printemploymentcertificate.issent",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadEmployeeTemplates(): ng.IPromise<any> {
        return this.scheduleService.getEmployeeTemplatesOfTypeSubstituteShifts().then(x => {
            this.employeeTemplates = x;
            if (this.reportId)
                this.employeeTemplates.splice(0, 0, new SmallGenericType(0, `${this.title} (${this.reportName})`));

            if (this.employeeTemplates.length === 1)
                this.selectedEmployeeTemplateId = this.employeeTemplates[0].id;
            else if (this.employeeTemplates.length > 1)
                this.selectedEmployeeTemplateId = this.employeeTemplates[1].id;
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private initPrint() {
        this.executing = true;

        let employeeIds: number[] = this.getSelectedEmployeeIds();

        if (this.selectedEmployeeTemplateId > 0)
            this.initPrintByTemplate(employeeIds);
        else {
            if (this.oneReportPerEmployee) {
                let counter: number = 0;
                _.forEach(employeeIds, employeeId => {
                    counter++;
                    this.print(this.reportId, [employeeId]).then(() => {
                        if (counter === employeeIds.length)
                            this.cancel();
                    });
                });
            } else {
                this.print(this.reportId, employeeIds).then(() => {
                    this.cancel();
                });
            }
        }
    }

    private print(reportId: number, employeeIds: number[]): ng.IPromise<any> {
        return this.reportDataService.createReportJob(ReportJobDefinitionFactory.createEmploymentContractFromPlanningReportDefinition(reportId, SoeReportTemplateType.TimeEmploymentContract, employeeIds, this.savePrintout, this.getDates()), true);
    }

    private initPrintByTemplate(employeeIds: number[]) {
        if (employeeIds.length === 0)
            return;

        const employeeId = employeeIds.shift();

        this.printByTemplate(employeeId).then(printOk => {
            if (printOk) {
                if (employeeIds.length > 0)
                    this.initPrintByTemplate(employeeIds);
                else {
                    this.executing = false;
                    this.cancel();
                }
            } else {
                this.executing = false;
            }
        });
    }

    private printByTemplate(employeeId: number): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        const employee = this.inputEmployees.find(e => e.employeeId === employeeId);

        this.progress.startWorkProgress((completion) => {
            this.scheduleService.printEmploymentContractFromTemplate(employeeId, this.selectedEmployeeTemplateId, this.getDates()).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                    if (this.initSigning) {
                        this.initSigningDocument(result.integerValue2, result.decimalValue).then(initSigningOk => {
                            deferral.resolve(initSigningOk);
                        });
                    } else {
                        deferral.resolve(true);
                    }
                } else {
                    completion.failed(result.errorMessage);
                    deferral.resolve(false);
                }
            });
        }, null, `${this.terms["core.printing"]} ${employee.name}...`).then(data => {

        }, error => { });

        return deferral.promise;
    }

    private initSigningDocument(userId: number, recordId: number): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AddDocumentToAttestFlow/Views/addDocumentToAttestFlow.html"),
            controller: AddDocumentToAttestFlowController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                recordId: () => { return recordId },
                endUserId: () => { return userId }
            }
        }).result.then(result => {
            deferral.resolve(true);
        }, () => {
            // Cancel
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    private send() {
        this.executing = true;
        this.scheduleService.sendTimeEmploymentContractShortSubstituteForConfirmation(this.getSelectedEmployeeIds(), this.getDates(), this.savePrintout).then((result) => {
            this.executing = false;
            if (result.success) {
                this.notificationService.showDialogEx("", this.terms["time.schedule.planning.printemploymentcertificate.issent"], SOEMessageBoxImage.Information);
                this.cancel();
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }

    private getEmployeesWithSubstituteShifts() {
        this.filteringEmployees = true;
        this.resetSelectedEmployees();
        this.scheduleService.getEmployeesWithSubstituteShifts(<number[]>(_.uniq(_.map(this.inputEmployees, 'employeeId'))), this.getDates()).then((x) => {
            this.filteringEmployees = false;
            this.employeesWithSubstituteShifts = _.filter(this.inputEmployees, e => _.includes(_.map(x, a => a['employeeId']), e.employeeId));
        });
    }

    //HELP METHODS

    private resetSelectedEmployees() {
        this.inputEmployees.forEach((e) => { e.isSelected = false; });
    }

    private getSelectedEmployeeIds(): number[] {
        return <number[]>(_.uniq(_.map(this.selectedEmployees(), 'employeeId')));
    }

    public selectedEmployees() {
        return _.orderBy(_.filter(this.employeesWithSubstituteShifts, e => e.isSelected), e => e.name);
    }

    private getDates(): Date[] {
        const dates: Date[] = [];

        if (this.selectedSelectionType === EnumSelectionType.Dates) {
            _.forEach(this.selectedDates, (item: any) => {
                dates.push(CalendarUtility.convertToDate(item.id));
            });
        }
        else {
            _.forEach(this.selectableDates, (item: any) => {
                dates.push(CalendarUtility.convertToDate(item.id));
            });
        }
        return dates;
    }

    private selectAllItems() {
        const selected: boolean = this.allItemsSelected;
        _.forEach(this.employeesWithSubstituteShifts, employee => {
            employee.isSelected = !selected;
        });
    }

    private unSelectAllItems() {
        _.forEach(this.employeesWithSubstituteShifts, employee => {
            employee.isSelected = false;
        });
    }
}
