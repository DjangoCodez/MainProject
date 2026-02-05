import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { NotificationService } from "../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { Feature, TermGroup, TermGroup_EmployeeStatisticsType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IEmployeeService } from "../../Employee/EmployeeService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { ISmallGenericType, IEmployeeGridDTO } from "../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { DateRangeSelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";
import { EmployeeStatisticsChartData } from "../../../Common/Models/EmployeeStatistics";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private seeOtherEmployeesPermission: boolean = false;

    // Data
    private employees: SmallGenericType[] = [];
    private statisticTypes: SmallGenericType[] = [];
    private employeeDataArrival: EmployeeStatisticsChartData[] = [];
    private employeeDataGoHome: EmployeeStatisticsChartData[] = [];

    // Properties
    private currentEmployeeId: number;
    private employee: ISmallGenericType;
    private selectedType: SmallGenericType;
    private dateFrom: Date;
    private dateTo: Date;
    private autoLoad: boolean = true;

    private get hasData(): boolean {
        return this.employeeDataArrival.length > 0 || this.employeeDataGoHome.length > 0;
    }

    private get showArrival(): boolean {
        return this.selectedType && (this.selectedType.id == TermGroup_EmployeeStatisticsType.Arrival || this.selectedType.id == TermGroup_EmployeeStatisticsType.ArrivalAndGoHome);
    }

    private get showGoHome(): boolean {
        return this.selectedType && (this.selectedType.id == TermGroup_EmployeeStatisticsType.GoHome || this.selectedType.id == TermGroup_EmployeeStatisticsType.ArrivalAndGoHome);
    }

    // Chart
    private chartOptions: any;
    private chartData: any[];

    // Flags
    private loading: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $window,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private sharedEmployeeService: SharedEmployeeService,
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

        this.currentEmployeeId = soeConfig.employeeId;

        this.flowHandler.start([{ feature: Feature.Time_Employee_Statistics, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_Statistics].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Statistics].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()]).then(() => {
                let queue = [];
                if (this.seeOtherEmployeesPermission)
                    queue.push(this.loadEmployees());
                queue.push(this.loadStatisticTypes());
                this.$q.all(queue).then(() => {

                });
            })
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.chart.nodata",
            "common.chart.grouped",
            "common.chart.stacked",
            "time.employee.statistics.arrival",
            "time.employee.statistics.earlyarrival",
            "time.employee.statistics.latearrival",
            "time.employee.statistics.gohome",
            "time.employee.statistics.earlygohome",
            "time.employee.statistics.lategohome"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Employee_Statistics_OtherEmployees);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.seeOtherEmployeesPermission = x[Feature.Time_Employee_Statistics_OtherEmployees];
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.sharedEmployeeService.getEmployeesForGrid(false, false, false, false, false).then(x => {
            x = _.orderBy(x, 'name')
            _.forEach(x, (employee: IEmployeeGridDTO) => {
                this.employees.push(new SmallGenericType(employee.employeeId, "(" + employee.employeeNr + ") " + employee.name));
            });
        });
    }

    private loadStatisticTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeStatisticsType, false, true).then(x => {
            this.statisticTypes = x;

            // Remove old Taxi Kurir specific types
            let ids: number[] = [];
            ids.push(<number>TermGroup_EmployeeStatisticsType.AnsweredCalls);
            ids.push(<number>TermGroup_EmployeeStatisticsType.CallDuration);
            ids.push(<number>TermGroup_EmployeeStatisticsType.ConnectedTime);
            ids.push(<number>TermGroup_EmployeeStatisticsType.NotAnsweredCalls);
            _.pullAll(this.statisticTypes, _.filter(this.statisticTypes, t => _.includes(ids, t.id)));

            this.selectedType = this.statisticTypes[0];
        });
    }

    // SERVICE CALLS

    private loadEmployeeData(employeeId: number, type: TermGroup_EmployeeStatisticsType): ng.IPromise<any> {
        return this.employeeService.getEmployeeStatisticsEmployeeData(employeeId, this.dateFrom, this.dateTo, type).then(x => {
            if (type === TermGroup_EmployeeStatisticsType.Arrival)
                this.employeeDataArrival = x;
            else
                this.employeeDataGoHome = x;
        });
    }

    // EVENTS

    private onTimeIntervalSelected(selection: DateRangeSelectionDTO) {
        this.dateFrom = selection.from;
        this.dateTo = selection.to;
        if (!this.dateTo && selection.rangeType === 'date')
            this.dateTo = this.dateFrom;

        this.clearChartData();
    }

    private loadData() {
        if ((!this.employee && this.seeOtherEmployeesPermission) || !this.dateFrom || !this.dateTo || !this.selectedType)
            return;

        let employeeId = this.seeOtherEmployeesPermission ? this.employee.id : this.currentEmployeeId;

        this.loading = true;
        let queue = [];
        switch (<TermGroup_EmployeeStatisticsType>this.selectedType.id) {
            case TermGroup_EmployeeStatisticsType.Arrival:
                this.employeeDataArrival = [];
                queue.push(this.loadEmployeeData(employeeId, TermGroup_EmployeeStatisticsType.Arrival));
                break;
            case TermGroup_EmployeeStatisticsType.GoHome:
                this.employeeDataGoHome = [];
                queue.push(this.loadEmployeeData(employeeId, TermGroup_EmployeeStatisticsType.GoHome));
                break;
            case TermGroup_EmployeeStatisticsType.ArrivalAndGoHome:
                this.employeeDataArrival = [];
                this.employeeDataGoHome = [];
                queue.push(this.loadEmployeeData(employeeId, TermGroup_EmployeeStatisticsType.Arrival));
                queue.push(this.loadEmployeeData(employeeId, TermGroup_EmployeeStatisticsType.GoHome));
                break;
        }

        this.$q.all(queue).then(() => {
            this.createChartData();
            this.loading = false;
        });
    }

    // HELP-METHODS    

    private clearChartData() {
        this.$timeout(() => {
            this.employeeDataArrival = [];
            this.employeeDataGoHome = [];
            if (this.terms) {
                if (this.autoLoad)
                    this.loadData();
                else
                    this.createChartData();
            }
        });
    }

    public createChartData() {
        var zeroData = [];
        _.forEach(CalendarUtility.getDates(this.dateFrom, this.dateTo), date => {
            zeroData.push({ x: date, y: 0.5, color: 'black' });
        });

        var arrivalData = [];
        _.forEach(this.employeeDataArrival, data => {
            arrivalData.push({ x: data.date, y: data.value, color: data.value < 0 ? 'rgba(201, 48, 44)' : 'rgba(68, 157, 68)' });
        });

        var goHomeData = [];
        _.forEach(this.employeeDataGoHome, data => {
            goHomeData.push({ x: data.date, y: data.value, color: data.value < 0 ? 'rgba(236, 151, 31)' : 'rgba(49, 176, 213)' });
        });

        this.chartData = [];
        //this.chartData.push({ values: zeroData, key: 'zero', type: 'bar', yAxis: 1 });
        if (arrivalData.length > 0)
            this.chartData.push({ values: arrivalData, key: this.terms["time.employee.statistics.arrival"], type: 'bar', yAxis: 1 });
        if (goHomeData.length > 0)
            this.chartData.push({ values: goHomeData, key: this.terms["time.employee.statistics.gohome"], type: 'bar', yAxis: 1 });

        this.setChartOptions();
    }

    private setChartOptions() {
        // Chart options
        var chart: any = {};
        chart.type = 'multiBarChart';
        chart.duration = 300;
        chart.stacked = true;
        chart.showControls = this.showArrival && this.showGoHome;
        chart.controlLabels = { "grouped": this.terms["common.chart.grouped"], "stacked": this.terms["common.chart.stacked"] };
        chart.controls = {
            rightAlign: false, padding: 20, margin: { top: 2, left: 0 }
        };
        chart.height = 500;
        chart.margin = { top: 30, right: 0, bottom: 70, left: 40 };
        chart.noData = this.terms["common.chart.nodata"];
        chart.showLegend = false;
        chart.showValues = true;
        chart.clipEdge = false;
        chart.focusEnable = false;
        chart.groupSpacing = 0.1;
        chart.reduceXTicks = false;
        //chart.useInteractiveGuideline = true;
        // Using InteractiveGuideline leaves tootip open, see issue here:
        // https://github.com/krispo/angular-nvd3/pull/616
        chart.xAxis = {
            tickFormat: (d: number) => { return new Date(d).toFormattedDate(); },
            tickValues: d3.time.days(this.dateFrom, this.dateTo, 1),
            showMaxMin: false,
            rotateLabels: -45,
        };
        chart.yAxis = {
            tickFormat: function (d) { return CalendarUtility.minutesToTimeSpan(d); },
            showMaxMin: false,
        };

        // Set options to chart
        this.chartOptions = {};
        this.chartOptions.chart = chart;
    }
}
