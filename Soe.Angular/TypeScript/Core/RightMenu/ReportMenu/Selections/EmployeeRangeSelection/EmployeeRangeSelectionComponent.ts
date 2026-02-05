import { IReportDataService } from "../../ReportDataService";
import { ISelectableTimePeriodDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO, IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { TimePeriodHeadDTO } from "../../../../../Common/Models/TimePeriodHeadDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ProjectDTO } from "../../../../../Common/Models/ProjectDTO";
import { SelectionCollection } from "../../SelectionCollection";

interface TimePeriodSelectModel {
    id: number;
    label: string;
}

export class EmployeeRangeSelection {

    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: EmployeeRangeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/EmployeeRangeSelection/EmployeeRangeSelectionView.html",
            bindings: {
                onTimePeriodHeadSelected: "&",
                onSelected: "&",
                labelKey: "@",
                additionalLabelKey: "@",
                hideLabel: "<",
                hideAdditionalLabel: "<",
                showTimePeriodHeadSelector: "@",
                userSelectionInput: "=",
                onEmployeeFromSelected: "&",
                onEmployeeToSelected: "&",
                employeeNrFrom: "<",
                employeeNrTo: "<",
                employeesDict: "<",

            }
        };

        return options;
    }

    public static componentKey = "employeeRangeSelection";

    private showTimePeriodHeadSelector: boolean;
    private onTimePeriodHeadSelected: (_: { selections: IdSelectionDTO }) => void = angular.noop;
    private onSelected: (_: { selections: IdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private timePeriodHeads: TimePeriodHeadDTO[] = [];
    private selectedTimePeriodHead: TimePeriodHeadDTO;
    private selectedTimePeriodId: number;

    private allTimePeriods: Map<number, ISelectableTimePeriodDTO> = new Map<number, ISelectableTimePeriodDTO>();
    private availableTimePeriods: TimePeriodSelectModel[] = [];
    private selectedTimePeriods: TimePeriodSelectModel[] = [];

    private delaySetSavedUserSelection: boolean = false;
    employeesDict: any[] = [];
    projects: any[] = [];
    projectId: number;
    employeeNrFrom: SmallGenericType;
    employeeNrTo: SmallGenericType;
    private project: ProjectDTO;
    private selections: SelectionCollection;

    private onEmployeeFromSelected: (_: { selection: SmallGenericType }) => void = angular.noop;
    private onEmployeeToSelected: (_: { selection: SmallGenericType }) => void = angular.noop;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private reportDataService: IReportDataService,) {

        this.project = new ProjectDTO();
    }

    public $onInit() {
        this.loadCustomerEmployees();
    }

    public loadEmployeeFrom(item: SmallGenericType) {
        this.onEmployeeFromSelected({ selection: item });

    }

    public loadEmployeeTo(item: SmallGenericType) {
        this.onEmployeeToSelected({ selection: item });

    }
    private loadCustomerEmployees(): ng.IPromise<any> {
        return this.reportDataService.getEmployeesDict(true, true, false, true).then((result) => {
            _.forEach(result, (employee: any) => {
                var substrings = employee.name.split(')');
                this.employeesDict.push({ id: employee.id, name: employee.name, value: substrings[0].substring(1) });
            });
        });
    }

}