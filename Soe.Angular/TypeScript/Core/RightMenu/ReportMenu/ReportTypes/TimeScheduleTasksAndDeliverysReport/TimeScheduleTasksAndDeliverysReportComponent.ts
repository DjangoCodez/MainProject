export class TimeScheduleTasksAndDeliverysReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeScheduleTasksAndDeliverysReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeScheduleTasksAndDeliverysReport/TimeScheduleTasksAndDeliverysReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeScheduleTasksAndDeliverysReport";

    //@ngInject
    constructor() { }
}
