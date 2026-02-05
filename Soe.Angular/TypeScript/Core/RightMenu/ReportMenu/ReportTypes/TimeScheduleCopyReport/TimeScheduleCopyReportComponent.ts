import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { IdListSelectionDTO, IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IIdListSelectionDTO, IIdSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SoeModule, TermGroup_TimeScheduleCopyHeadType } from "../../../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";

export class TimeScheduleCopyReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeScheduleCopyReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeScheduleCopyReport/TimeScheduleCopyReportView.html",
            bindings: {
                module: "<",
                userSelection: "=",
                selections: "<",
                sysReportTemplateTypeId: "<",
                reportId: "<",
                isAnalysis: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeScheduleCopyReport";

    // Binding fields
    private module: SoeModule;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private isAnalysis: boolean;

    // User selections
    private userSelectionInputHead: IdSelectionDTO;
    private userSelectionInputEmployees: IdListSelectionDTO;

    // Data
    private heads: SmallGenericType[] = [];
    private employees: SmallGenericType[] = [];

    //@ngInject
    constructor(
        private sharedScheduleService: ScheduleService,
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputHead = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SCHEDULE_COPY_HEAD);
            this.userSelectionInputEmployees = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES);
        });
    }

    public $onInit() {
        this.loadTimeScheduleCopyHeads();
    }

    private loadTimeScheduleCopyHeads() {
        this.sharedScheduleService.getTimeScheduleCopyHeadsDict(TermGroup_TimeScheduleCopyHeadType.PrelToDef).then(x => {
            this.heads = x;
        });
    }

    private loadEmployees() {
        this.sharedScheduleService.getTimeScheduleCopyRowEmployeesDict(this.userSelectionInputHead.id).then(x => {
            this.employees = x;
        });
    }

    public onHeadSelectionUpdated(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SCHEDULE_COPY_HEAD, selection);

        if (!selection)
            return;

        if (!this.userSelectionInputHead)
            this.userSelectionInputHead = new IdSelectionDTO(0);

        this.userSelectionInputHead.id = selection.id;

        if (selection.id) {
            this.loadEmployees();
        } else {
            this.employees = [];
        }
    }

    public onEmployeesSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }
}
