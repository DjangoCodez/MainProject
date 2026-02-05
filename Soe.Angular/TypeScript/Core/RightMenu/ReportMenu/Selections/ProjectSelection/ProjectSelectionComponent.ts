import { IReportDataService } from "../../ReportDataService";
import { IProjectGridDTO, ISelectableTimePeriodDTO, } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO, IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { TimePeriodHeadDTO } from "../../../../../Common/Models/TimePeriodHeadDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

import { SelectionCollection } from "../../SelectionCollection";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";

interface TimePeriodSelectModel {
    id: number;
    label: string;
}

export class ProjectSelection {

    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ProjectSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/ProjectSelection/ProjectSelectionView.html",
            bindings: {
                onTimePeriodHeadSelected: "&",
                onSelected: "&",
                labelKey: "@",
                additionalLabelKey: "@",
                hideLabel: "<",
                hideAdditionalLabel: "<",
                showTimePeriodHeadSelector: "@",
                userSelectionInput: "=",
                onProjectFromSelected: "&",
                onProjectToSelected: "&",
                projectNrFrom: "<",
                projectNrTo: "<",
                projectsDict: "<",
            }
        };

        return options;
    }

    public static componentKey = "projectSelection";

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
    projectsDict: any[] = [];
    projectId: number;
    projectNrFrom: SmallGenericType;
    projectNrTo: SmallGenericType;
    private selections: SelectionCollection;
    private userSelection: ReportUserSelectionDTO;

    supplierNrFrom: SmallGenericType;
    supplierNrTo: SmallGenericType;

    private onProjectFromSelected: (_: { selection: SmallGenericType }) => void = angular.noop;
    private onProjectToSelected: (_: { selection: SmallGenericType }) => void = angular.noop;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private reportDataService: IReportDataService,) {
    }

    public $onInit() {
        this.loadProjects();
    }

    public loadProjectFrom(item: SmallGenericType) {
        this.onProjectFromSelected({ selection: item });
    }

    public loadProjectTo(item: SmallGenericType) {
        this.onProjectToSelected({ selection: item });
    }

    private loadProjects() {
        this.projectsDict = [];
        
        return this.reportDataService.getProjects(true, false, false, false, false, 0).then((projects:IProjectGridDTO[]) => {
            this.projectsDict.push({ id: null, name: "" });
            for (const element of projects) {
                let row = element;
                if ((!this.projectId || row.projectId !== this.projectId))
                    this.projectsDict.push({ id: row.projectId, name: row.number + ' ' + row.name, value: row.number });
            }
        });
    }

}