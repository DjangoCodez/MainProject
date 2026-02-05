import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO, BoolSelectionDTO, IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IIdListSelectionDTO, IBoolSelectionDTO, ISmallGenericType, IIdSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { TermGroup } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";

export class KPADirektReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: KPADirektReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/KPADirektReport/KPADirektReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "kPADirektReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private timePeriodIds: number[];
    private kpaAgreementTypes: ISmallGenericType[] = [];

    //user selections
    private userSelectionInputPayrollPeriod: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputSetAsFinal: BoolSelectionDTO;
    private userSelectionInputOnlyChangedEmployments: BoolSelectionDTO;
    private userSelectionInputKPAAgreementType: IdSelectionDTO;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputPayrollPeriod = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputSetAsFinal = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_SETASFINAL);
            this.userSelectionInputOnlyChangedEmployments = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_ONLYCHANGEDEMPLOYMENTS);
            this.userSelectionInputKPAAgreementType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_KPAAGREEMENTTYPE);
            this.timePeriodIds = this.userSelectionInputPayrollPeriod ? this.userSelectionInputPayrollPeriod.ids : null;
        });
    }

    public $onInit() {
        this.loadKpaAgreementTypes();
    }

    private loadKpaAgreementTypes(): ng.IPromise<any> {
        this.kpaAgreementTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.KPAAgreementType, true, true).then(x => {
            this.kpaAgreementTypes = x;
        });
    }

    public onPeriodSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onBoolSelectionInputSetAsFinal(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_SETASFINAL, selection);
    }

    public onBoolSelectionInputOnlyChangedEmployments(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_ONLYCHANGEDEMPLOYMENTS, selection);
    }

    public onIdSelectionInputKPAAgreementType(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_KPAAGREEMENTTYPE, selection);
    }
}
