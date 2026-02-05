import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { NotificationService } from "../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { TimeCalendarPeriodDTO, TimeCalendarSummaryDTO, TimeCalendarPeriodPayrollProductDTO } from "../../../Common/Models/TimeCalendarDTOs";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { IPayrollProductRowSelectionDTO, ISelectablePayrollTypeDTO } from "../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SelectionCollection } from "../../../Core/RightMenu/ReportMenu/SelectionCollection";
import { PayrollProductRowSelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";
import { DayOfWeek } from "../../../Util/Enumerations";
import { HolidaySmallDTO } from "../../../Common/Models/HolidayDTO";
import { IScheduleService } from "../../Schedule/ScheduleService";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { EmployeeSmallDTO } from "../../../Common/Models/EmployeeListDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private allSysPayrollTypes: ISelectablePayrollTypeDTO[];
    private employees: EmployeeSmallDTO[] = [];
    private periods: TimeCalendarPeriodDTO[] = [];
    private summary: TimeCalendarSummaryDTO[] = [];
    private holidays: HolidaySmallDTO[] = [];

    // Properties
    private loading: boolean = false;
    private nbrOfTransactions: number = 0;
    private delayRender: boolean = false;

    private employee: EmployeeSmallDTO;
    private payrollProductSelections: SelectionCollection;

    private _intervalType: number;
    private get intervalType(): number {
        return this._intervalType;
    }
    private set intervalType(type: number) {
        this._intervalType = type;

        if (type === 1) {
            if (!this.fromDate || !this.toDate || !this.fromDate.isSameDayAs(CalendarUtility.getFirstDayOfYear()) || !this.toDate.isSameDayAs(CalendarUtility.getLastDayOfYear())) {
                this.delayRender = true;
                this.fromDate = CalendarUtility.getFirstDayOfYear();
                this.toDate = CalendarUtility.getLastDayOfYear();
                this.delayRender = false;
                this.renderCalendar();
            }
        }
    }

    private _fromDate: Date;
    private set fromDate(date: Date) {
        this._fromDate = date;

        if (date && this.toDate) {
            let minToDate = date;
            let maxToDate = date.addYears(1).addDays(-1);

            if (this.toDate.isBeforeOnDay(minToDate))
                this.toDate = minToDate;
            else if (this.toDate.isAfterOnDay(maxToDate))
                this.toDate = maxToDate;
        }
        if (!this.delayRender)
            this.renderCalendar();
    }
    private get fromDate(): Date {
        return this._fromDate;
    }

    private _toDate: Date;
    private set toDate(date: Date) {
        this._toDate = date;

        if (date && this.fromDate) {
            let minFromDate = date.addYears(-1).addDays(1);
            let maxFromDate = date;

            if (this.fromDate.isBeforeOnDay(minFromDate))
                this.fromDate = minFromDate;
            else if (this.fromDate.isAfterOnDay(maxFromDate))
                this.fromDate = maxFromDate;
        }
        if (!this.delayRender)
            this.renderCalendar();
    }
    private get toDate(): Date {
        return this._toDate;
    }

    private calendarFromDate: Date;
    private calendarToDate: Date;
    private weeks: DateWeek[];
    private dayOfWeeks: number[] = [1, 2, 3, 4, 5, 6, 0];

    private hasHoliday(date: Date): boolean {
        return CalendarUtility.includesDate(_.map(this.holidays, h => h.date), date);
    }

    private getHolidayName(date: Date): string {
        var holiday = _.find(this.holidays, h => h.date.isSameDayAs(date));
        return holiday ? holiday.name : '';
    }

    private getHolidayDescription(date: Date): string {
        var holiday = _.find(this.holidays, h => h.date.isSameDayAs(date));
        return holiday ? holiday.description : '';
    }

    private isSaturday(date: Date): boolean {
        // Saturday
        if (date.getDay() === 6)
            return true;

        // Holiday not red day
        var holiday = _.find(this.holidays, h => h.date.isSameDayAs(date) && !h.isRedDay);
        if (holiday)
            return true;

        return false;
    }

    private isSunday(date: Date): boolean {
        // Sunday
        if (date.getDay() === 0)
            return true;

        // Holiday red day
        var holiday = _.find(this.holidays, h => h.date.isSameDayAs(date) && h.isRedDay);
        if (holiday)
            return true;

        return false;
    }

    private isOutOfRange(date: Date): boolean {
        return date.isBeforeOnDay(this.fromDate) || date.isAfterOnDay(this.toDate);
    }

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $window,
        private coreService: ICoreService,
        private reportDataService: IReportDataService,
        private sharedEmployeeService: SharedEmployeeService,
        private timeService: ITimeService,
        private scheduleService: IScheduleService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: NotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups());
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;

        this.intervalType = 1;
        this.summary.push(new TimeCalendarSummaryDTO());

        this.flowHandler.start([{ feature: Feature.Time_Time_TimeCalendar, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Time_TimeCalendar].readPermission;
        this.modifyPermission = response[Feature.Time_Time_TimeCalendar].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadSysPayrollTypes(),
            this.loadEmployees(),
        ]).then(() => {

        });
    }

    // LOOKUPS

    private loadSysPayrollTypes() {
        this.reportDataService.getPayrollTypes().then(payrollTypes => {
            this.allSysPayrollTypes = payrollTypes;
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.sharedEmployeeService.getEmployeesForGridSmall(true).then(x => {
            this.employees = x;
        });
    }

    // Use debounce
    // This will enable fast clicking on increase/decrease date buttons without loading after each click
    private loadHolidays = _.debounce(() => {
        this.scheduleService.getHolidaysSmall(this.calendarFromDate, this.calendarToDate).then(x => {
            this.holidays = x;
        });
    }, 500, { leading: false, trailing: true });

    // SERVICE CALLS

    private loadData() {
        this.loading = true;

        this.periods = [];
        this.summary = [];
        this.nbrOfTransactions = 0;
        this.clearPeriods();

        let sysPayrollTypeLevel1: number;
        let sysPayrollTypeLevel2: number;
        let sysPayrollTypeLevel3: number;
        let sysPayrollTypeLevel4: number;

        if (this.payrollProductSelections) {
            let rows: PayrollProductRowSelectionDTO[] = <PayrollProductRowSelectionDTO[]>this.payrollProductSelections.materialize();
            if (rows && rows.length > 0) {
                let row: PayrollProductRowSelectionDTO = rows[0];
                sysPayrollTypeLevel1 = row.sysPayrollTypeLevel1;
                sysPayrollTypeLevel2 = row.sysPayrollTypeLevel2;
                sysPayrollTypeLevel3 = row.sysPayrollTypeLevel3;
                sysPayrollTypeLevel4 = row.sysPayrollTypeLevel4;
            }
        }

        this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeCalendarPeriods(this.employee.employeeId, this.fromDate, this.toDate, sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4).then(x => {
                _.forEach(x, period => {
                    period.type1Amount = period.type2Amount = period.type3Amount = period.type4Amount = period.typesAmount = 0;
                    period.type1ToolTip = period.type2ToolTip = period.type3ToolTip = period.type4ToolTip = period.typesToolTip = '';

                    this.nbrOfTransactions += period.payrollProducts.length;

                    let payrollProductIds: number[] = _.uniq(_.map(_.sortBy(period.payrollProducts, p => p.name), p => p.payrollProductId));
                    let i: number = 1;
                    _.forEach(payrollProductIds, payrollProductId => {
                        let product: TimeCalendarPeriodPayrollProductDTO = _.find(period.payrollProducts, p => p.payrollProductId === payrollProductId);

                        let amount: number = _.sumBy(_.filter(period.payrollProducts, p => p.payrollProductId === payrollProductId), p => p.amount);

                        let toolTip: string = "{0} ({1})".format(product.name, CalendarUtility.minutesToTimeSpan(amount, true, false));

                        // If more than four different payroll products exists on the same day,
                        // only show one rectangle with all products in the same tooltip
                        if (payrollProductIds.length > 4) {
                            period.typesAmount++;
                            period.setTypeToolTip(5, toolTip);
                            period.setTypeColor(5, product);
                        } else if (i >= 1 || i <= 4) {
                            period[`type${i}Amount`] += amount;
                            period.setTypeToolTip(i, toolTip);
                            period.setTypeColor(i, product);
                        }

                        // Summary
                        let sumDTO: TimeCalendarSummaryDTO = _.find(this.summary, s => s.payrollProductId === payrollProductId);
                        if (!sumDTO) {
                            // Create new empty summary record for current product
                            sumDTO = new TimeCalendarSummaryDTO();
                            sumDTO.payrollProductId = payrollProductId;
                            sumDTO.sysPayrollTypeLevel1 = product.sysPayrollTypeLevel1;
                            sumDTO.sysPayrollTypeLevel2 = product.sysPayrollTypeLevel2;
                            sumDTO.sysPayrollTypeLevel3 = product.sysPayrollTypeLevel3;
                            sumDTO.sysPayrollTypeLevel4 = product.sysPayrollTypeLevel4;
                            sumDTO.number = product.number;
                            sumDTO.name = product.name;
                            sumDTO.amount = 0;
                            sumDTO.occations = 0;
                            sumDTO.days = 0;
                            this.summary.push(sumDTO);
                        }

                        sumDTO.amount += amount;
                        i++;
                    });

                    let dateDay: DateDay = this.getDateDay(period.date);
                    if (dateDay)
                        dateDay.period = period;

                    this.periods.push(period);
                });

                if (this.summary.length === 0)
                    this.summary.push(new TimeCalendarSummaryDTO());

                // Caclulate occations and days
                _.forEach(this.summary, sumDTO => {
                    let periods = _.filter(x, y => y.payrollProducts.some(p => p.payrollProductId === sumDTO.payrollProductId));
//                    sumDTO.days = _.filter(periods, p => p.type1Amount > 0 || p.type2Amount > 0 || p.type3Amount > 0 || p.type4Amount > 0).length;
                    sumDTO.days = periods.length;

                    let uniqueDates = _.sortBy(CalendarUtility.uniqueDates(periods.map(p => p.date)));
                    for (let i = 0; i < uniqueDates.length; i++) {
                        if (i === 0 || !uniqueDates[i].isSameDayAs(uniqueDates[i - 1].addDays(1)))
                            sumDTO.occations++;
                    }
                });

                // Create summary groups
                let groups: TimeCalendarSummaryDTO[] = [];
                let maxLevel = sysPayrollTypeLevel4 ? 4 : (sysPayrollTypeLevel3 ? 3 : (sysPayrollTypeLevel2 ? 2 : (sysPayrollTypeLevel1 ? 1 : 0)));
                _.forEach(this.summary, sumDTO => {
                    let level1: number = maxLevel >= 0 ? sumDTO.sysPayrollTypeLevel1 : 0;
                    let level2: number = maxLevel >= 1 ? sumDTO.sysPayrollTypeLevel2 : 0;
                    let level3: number = maxLevel >= 2 ? sumDTO.sysPayrollTypeLevel3 : 0;
                    let level4: number = maxLevel >= 3 ? sumDTO.sysPayrollTypeLevel4 : 0;

                    if (!_.find(groups, g => g.sysPayrollTypeLevel1 === level1 && g.sysPayrollTypeLevel2 === level2 && g.sysPayrollTypeLevel3 === level3 && g.sysPayrollTypeLevel4 === level4)) {
                        let groupDTO = new TimeCalendarSummaryDTO();
                        groupDTO.isGroup = true;
                        groupDTO.sysPayrollTypeLevel1 = level1;
                        groupDTO.sysPayrollTypeLevel2 = level2;
                        groupDTO.sysPayrollTypeLevel3 = level3;
                        groupDTO.sysPayrollTypeLevel4 = level4;
                        groupDTO.amount = 0;
                        groupDTO.occations = 0;
                        groupDTO.days = 0;

                        let typeLevel1 = this.allSysPayrollTypes.find(p => p.id === sumDTO.sysPayrollTypeLevel1);
                        let typeLevel2 = this.allSysPayrollTypes.find(p => p.id === sumDTO.sysPayrollTypeLevel2);
                        let typeLevel3 = this.allSysPayrollTypes.find(p => p.id === sumDTO.sysPayrollTypeLevel3);
                        let typeLevel4 = this.allSysPayrollTypes.find(p => p.id === sumDTO.sysPayrollTypeLevel4);

                        groupDTO.name = '';
                        if (maxLevel >= 0 && typeLevel1)
                            groupDTO.name = typeLevel1.name;
                        if (maxLevel >= 1 && typeLevel2)
                            groupDTO.name += ' - ' + typeLevel2.name;
                        if (maxLevel >= 2 && typeLevel3)
                            groupDTO.name += ' - ' + typeLevel3.name;
                        if (maxLevel >= 3 && typeLevel4)
                            groupDTO.name += ' - ' + typeLevel4.name;

                        groups.push(groupDTO);
                    }
                });

                _.forEach(groups, group => {
                    let sumForGroup = this.summary;
                    if (maxLevel >= 0)
                        sumForGroup = _.filter(sumForGroup, s => s.sysPayrollTypeLevel1 === group.sysPayrollTypeLevel1);
                    if (maxLevel >= 1)
                        sumForGroup = _.filter(sumForGroup, s => s.sysPayrollTypeLevel2 === group.sysPayrollTypeLevel2);
                    if (maxLevel >= 2)
                        sumForGroup = _.filter(sumForGroup, s => s.sysPayrollTypeLevel3 === group.sysPayrollTypeLevel3);
                    if (maxLevel >= 3)
                        sumForGroup = _.filter(sumForGroup, s => s.sysPayrollTypeLevel4 === group.sysPayrollTypeLevel4);
                    sumForGroup.forEach(s => group.amount += s.amount);

                    // Caclulate occations and days
                    let periods = x;
                    if (maxLevel >= 0)
                        periods = _.filter(periods, y => y.payrollProducts.some(p => p.sysPayrollTypeLevel1 === group.sysPayrollTypeLevel1));
                    if (maxLevel >= 1)
                        periods = _.filter(periods, y => y.payrollProducts.some(p => p.sysPayrollTypeLevel2 === group.sysPayrollTypeLevel2));
                    if (maxLevel >= 2)
                        periods = _.filter(periods, y => y.payrollProducts.some(p => p.sysPayrollTypeLevel3 === group.sysPayrollTypeLevel3));
                    if (maxLevel >= 3)
                        periods = _.filter(periods, y => y.payrollProducts.some(p => p.sysPayrollTypeLevel4 === group.sysPayrollTypeLevel4));
                    group.days = periods.length;

                    let uniqueDates = _.sortBy(CalendarUtility.uniqueDates(periods.map(p => p.date)));
                    for (let i = 0; i < uniqueDates.length; i++) {
                        if (i === 0 || !uniqueDates[i].isSameDayAs(uniqueDates[i - 1].addDays(1)))
                            group.occations++;
                    }
                });

                this.summary.push(...groups);
                this.summary = _.orderBy(this.summary, ['sysPayrollTypeLevel1', 'sysPayrollTypeLevel2', 'sysPayrollTypeLevel3', 'sysPayrollTypeLevel4', 'isGroup', 'number']);

                this.loading = false;
            });
        }]);
    }

    // EVENTS

    private addYears(years: number) {
        let year: number = this.fromDate.getFullYear() + years;
        this.fromDate = new Date(year, 0, 1);
        this.toDate = new Date(year, 11, 31);
        this.renderCalendar();
    }

    public onPayrollProductSelectionUpdated(selection: IPayrollProductRowSelectionDTO) {
        if (!this.payrollProductSelections)
            this.payrollProductSelections = new SelectionCollection();

        this.payrollProductSelections.upsert("payrollProducts", selection);
    }

    // HELP-METHODS    

    private renderCalendar() {
        if (!this.fromDate || !this.toDate)
            return;

        this.calendarFromDate = this.fromDate.beginningOfWeek();
        this.calendarToDate = this.toDate.endOfWeek();
        this.loadHolidays();
        this.setDateRange();
    }

    private setDateRange() {
        if (!this.calendarFromDate || !this.calendarToDate)
            return;

        let dateRange: Date[] = CalendarUtility.getDates(this.calendarFromDate, this.calendarToDate);

        this.weeks = [];
        let weekNbr: number = 1;
        let dayNbr: number = 0;
        _.forEach(dateRange, date => {
            dayNbr++;
            if (dayNbr > 7) {
                dayNbr = 1;
                weekNbr++;
            }

            // Create week objects (one per week in date range)
            let week: DateWeek = _.find(this.weeks, w => w.week === weekNbr);
            if (!week) {
                week = new DateWeek(weekNbr);
                this.weeks.push(week);
            }

            // Add placeholders for date info
            week.days.push(new DateDay(date));
        });
    }

    private getDateRangeText(date: Date): string {
        if (!this.fromDate || !this.toDate)
            return '';

        let text: string = this.fromDate.format('dddd D MMMM');
        if (this.fromDate.getFullYear() !== this.toDate.getFullYear())
            text += " {0}".format(this.fromDate.getFullYear().toString());

        text += " - {0}".format(this.toDate.format('dddd D MMMM'));
        if (this.fromDate.getFullYear() !== this.toDate.getFullYear())
            text += " {0}".format(this.toDate.getFullYear().toString());


        return text;
    }

    private dateHasPassed(date: Date): boolean {
        return date.isBeforeOnDay(new Date());
    }

    private getDayName(day: number): string {
        return CalendarUtility.getDayName(day);
    }

    private getDateDay(date: Date): DateDay {
        let dateDay: DateDay;

        _.forEach(this.weeks, week => {
            dateDay = _.find(week.days, d => d.date.isSameDayAs(date));
            if (dateDay)
                return false;
        });

        return dateDay;
    }

    private clearPeriods() {
        let dateRange: Date[] = CalendarUtility.getDates(this.calendarFromDate, this.calendarToDate);
        _.forEach(dateRange, date => {
            let dateDay = this.getDateDay(date);
            dateDay.period = undefined;
        });
    }
}

class DateWeek {
    constructor(week: number) {
        this.week = week;
        this.days = [];
    }

    public week: number;
    public days: DateDay[];
}

class DateDay {
    constructor(date: Date) {
        this.year = date.getFullYear();
        this.week = date.week();
        this.date = date;
    }

    public year: number;
    public week: number;
    public date: Date;
    public period: TimeCalendarPeriodDTO;

    public get weekday(): DayOfWeek {
        return this.date.dayOfWeek();
    }
}
