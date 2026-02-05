import '../../Core/Module';
import '../../Shared/Economy/Module';
import '../GoogleMaps/Module';
import '../../Shared/Time/Schedule/Absencerequests/Module'
import '../../Libs/Bundle';


import { TermPartsLoaderProvider } from '../../Core/Services/TermPartsLoader';
import { DashboardDirectiveFactory } from "./DashboadDirective";
import { WidgetDirectiveFactory } from "./WidgetDirective";

import { AttestFlowGaugeDirectiveFactory } from './Widgets/AttestFlow/AttestFlowGaugeDirective';
import { EmployeeRequestsGaugeDirectiveFactory } from './Widgets/EmployeeRequests/EmployeeRequestsGaugeDirective';
import { MapGaugeDirectiveFactory } from "./Widgets/Map/MapGaugeDirective";
import { MyScheduleGaugeDirectiveFactory } from "./Widgets/MySchedule/MyScheduleGaugeDirective";
import { ScheduleDirectiveFactory } from './Widgets/MySchedule/ScheduleDirective';
import { MyShiftsGaugeDirectiveFactory } from "./Widgets/MyShifts/MyShiftsGaugeDirective";
import { OpenShiftsGaugeDirectiveFactory } from "./Widgets/OpenShifts/OpenShiftsGaugeDirective";
import { PerformanceAnalyzerGaugeDirectiveFactory } from "./Widgets/PerformanceAnalyzer/PerformanceAnalyzerGaugeDirective";
import { ReportGaugeDirectiveFactory } from "./Widgets/Report/ReportGaugeDirective";
import { SoftOneStatusGaugeDirectiveFactory } from './Widgets/SoftOneStatus/SoftOneStatusGaugeDirective';
import { SysLogGaugeDirectiveFactory } from "./Widgets/SysLog/SysLogGaugeDirective";
import { SystemInfoGaugeDirectiveFactory } from "./Widgets/SystemInfo/SystemInfoGaugeDirective";
import { TaskWatchLogGaugeDirectiveFactory } from "./Widgets/TaskWatchLog/TaskWatchLogGaugeDirective";
import { TimeStampAttendanceGaugeDirectiveFactory } from "./Widgets/TimeStampAttendance/TimeStampAttendanceGaugeDirective";
import { TimeTerminalGaugeDirectiveFactory } from "./Widgets/TimeTerminal/TimeTerminalGaugeDirective";
import { WantedShiftsGaugeDirectiveFactory } from "./Widgets/WantedShifts/WantedShiftsGaugeDirective";
import { InsightsGaugeDirectiveFactory } from './Widgets/Insights/InsightsGaugeDirective';

import { ShiftQueueDirectiveFactory } from '../../Shared/Time/Directives/ShiftQueue/ShiftQueueDirective';
import { SkillMatcherDirectiveFactory } from '../../Shared/Time/Directives/SkillMatcher/SkillMatcherDirective';
import { ScheduleService as SharedScheduleService } from '../../Shared/Time/Schedule/ScheduleService';
import { TimeService as SharedTimeService } from '../../Shared/Time/Time/TimeService';
import { ReceiversListDirectiveFactory } from '../Directives/ReceiversList/ReceiversListDirective';
import { AvailableEmployeesDirectiveFactory } from '../Directives/AvailableEmployees/AvailableEmployeesDirective';
import { EmployeeService as SharedEmployeeService } from '../../Shared/Time/Employee/EmployeeService';
import { ReportDataService } from '../../Core/RightMenu/ReportMenu/ReportDataService';


import 'd3';
import 'nvd3';
import 'angular-nvd3';
import { EditEmployeeAvailabilityValidationDirectiveFactory } from '../Dialogs/EditEmployeeAvailability/EditEmployeeAvailabilityValidationDirective';

angular.module("Soe.Common.Dashboard.Module", ['Soe.Core', 'Soe.Shared.Economy', 'nvd3', 'ui.sortable', 'Soe.Shared.Time.Schedule.Absencerequests.Module', 'agGrid'])
    .service("sharedScheduleService", SharedScheduleService)
    .service("sharedTimeService", SharedTimeService)
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("reportDataService", ReportDataService)
    .directive("dashboard", DashboardDirectiveFactory.create)
    .directive("widget", WidgetDirectiveFactory.create)
    .directive("attestFlowGauge", AttestFlowGaugeDirectiveFactory.create)
    .directive("employeeRequestsGauge", EmployeeRequestsGaugeDirectiveFactory.create)
    .directive("mapGauge", MapGaugeDirectiveFactory.create)
    .directive("myScheduleGauge", MyScheduleGaugeDirectiveFactory.create)
    .directive("schedule", ScheduleDirectiveFactory.create)
    .directive("editEmployeeAvailabilityValidation", EditEmployeeAvailabilityValidationDirectiveFactory.create)
    .directive("myShiftsGauge", MyShiftsGaugeDirectiveFactory.create)
    .directive("openShiftsGauge", OpenShiftsGaugeDirectiveFactory.create)
    .directive("performanceAnalyzerGauge", PerformanceAnalyzerGaugeDirectiveFactory.create)
    .directive("reportGauge", ReportGaugeDirectiveFactory.create)
    .directive("softOneStatusGauge", SoftOneStatusGaugeDirectiveFactory.create)
    .directive("sysLogGauge", SysLogGaugeDirectiveFactory.create)
    .directive("systemInfoGauge", SystemInfoGaugeDirectiveFactory.create)
    .directive("taskWatchLogGauge", TaskWatchLogGaugeDirectiveFactory.create)
    .directive("timeStampAttendanceGauge", TimeStampAttendanceGaugeDirectiveFactory.create)
    .directive("timeTerminalGauge", TimeTerminalGaugeDirectiveFactory.create)
    .directive("wantedShiftsGauge", WantedShiftsGaugeDirectiveFactory.create)
    .directive("shiftQueue", ShiftQueueDirectiveFactory.create)
    .directive("skillMatcher", SkillMatcherDirectiveFactory.create)
    .directive("receiversList", ReceiversListDirectiveFactory.create)
    .directive("availableEmployees", AvailableEmployeesDirectiveFactory.create)
    .directive("insightsGauge", InsightsGaugeDirectiveFactory.create)
    .config(/*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('time');
    });
