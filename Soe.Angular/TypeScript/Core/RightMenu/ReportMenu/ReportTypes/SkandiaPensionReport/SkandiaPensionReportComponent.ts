import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO, IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IIdListSelectionDTO, IIdSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { ICoreService } from "../../../../Services/CoreService";
import { TermGroup } from "../../../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class SkandiaPensionReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: SkandiaPensionReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/SkandiaPensionReport/SkandiaPensionReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "skandiaPensionReport";


    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];
    private skandaPensionReportTypes: SmallGenericType[];
  
    //user selections
    private userSelectionInputPayrollMonthYear: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputSkandiaPensionReportType: IdSelectionDTO;

    
    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollMonthYear = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.timePeriodIds = this.userSelectionInputPayrollMonthYear ? this.userSelectionInputPayrollMonthYear.ids : null;
            this.userSelectionInputSkandiaPensionReportType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_SKANDIAPENSIONREPORTTYPE);

        });
    }

    public $onInit() {
        this.loadSkandiaPensionReportTypes();
    }

    private loadSkandiaPensionReportTypes(): ng.IPromise<any> {
        this.skandaPensionReportTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.SkandiaPensionReportType, false, true).then(x => {
            this.skandaPensionReportTypes = x;
        });
    }

    public onPeriodSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onIdSelectionInputSkandiaReportType(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_SKANDIAPENSIONREPORTTYPE, selection);
    }

}