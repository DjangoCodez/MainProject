import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { DateRangeSelectionDTO, EmployeeSelectionDTO, IdListSelectionDTO, IdSelectionDTO, BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateSelectionDTO, IDateRangeSelectionDTO, IEmployeeSelectionDTO, IIdListSelectionDTO, IBoolSelectionDTO, IIdSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { TermGroup_TimeSchedulePlanningDayViewSortBy, TermGroup_TimeSchedulePlanningDayViewGroupBy, CompanySettingType } from "../../../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../../Services/TranslationService";
import { ICoreService } from "../../../../Services/CoreService";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { IReportDataService } from "../../ReportDataService";
import { Constants } from "../../../../../Util/Constants";

export class TimeEmployeeLineScheduleReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeEmployeeLineScheduleReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeEmployeeLineScheduleReport/TimeEmployeeLineScheduleReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
                fromDate: "<",
                toDate: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeEmployeeLineScheduleReport";
    public static includeAbsence: string = "includeAbsence";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private fromDate: Date;
    private toDate: Date;

    //user selections
    private userSelectionInputDate: DateRangeSelectionDTO;
    private userSelectionInputIncludeAbsence: BoolSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputFilterOnAccounting: BoolSelectionDTO;
    private userSelectionInputShiftType: IdListSelectionDTO;
    private userSelectionInputGroupBy: IdSelectionDTO;
    private userSelectionInputSortBy: IdSelectionDTO;

    // Terms
    private terms: { [index: string]: string; };

    // Company settings
    private useAccountHierarchy: boolean = false;

    // Data
    private accountDims: AccountDimSmallDTO[];
    private groupByItems: SmallGenericType[] = [];
    private sortByItems: SmallGenericType[] = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private reportDataService: IReportDataService,
        private $scope: ng.IScope) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateRangeSelection();
            this.userSelectionInputIncludeAbsence = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_ABSENCE);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputFilterOnAccounting = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING);
            this.userSelectionInputShiftType = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_SHIFT_TYPES);
            this.userSelectionInputGroupBy = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_GROUP_BY);
            this.userSelectionInputSortBy = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_SORT_BY);

            this.fromDate = this.userSelectionInputDate ? this.userSelectionInputDate.from : null;
            this.toDate = this.userSelectionInputDate ? this.userSelectionInputDate.to : null;
        });
    }

    public $onInit() {
        this.fromDate = new Date();
        this.toDate = new Date();

        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings()
        ]).then(() => {
            (this.useAccountHierarchy ? this.loadAccountDims() : this.$q.resolve()).then(() => {
                this.setupGroupBy();
                this.setupSortBy();
            });
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.employee",
            "common.categories.category",
            "common.shifttype",
            "common.firstname",
            "common.lastname",
            "common.report.selection.employeenr",
            "common.report.selection.starttime",
            "common.shifttypefirstonday"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.reportDataService.getAccountDims().then(x => {
            this.accountDims = x;
            
            _.forEach(this.accountDims, accountDim => {
                accountDim.groupByIndex = accountDim.level + 10;               
            });
        });
    }

    private setupGroupBy() {
        this.groupByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee, this.terms["common.employee"]));
        if (this.useAccountHierarchy) {
            _.forEach(_.filter(this.accountDims, a => !a.linkedToShiftType), dim => {
                this.groupByItems.push(new SmallGenericType(dim.groupByIndex, dim.name));
            });
        } else {
            this.groupByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewGroupBy.Category, this.terms["common.categories.category"]));
        }
        this.groupByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType, this.terms["common.shifttype"]));
        this.groupByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftTypeFirstOnDay, this.terms["common.shifttypefirstonday"]));
    }

    private setupSortBy() {
        this.sortByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname, this.terms["common.firstname"]));
        this.sortByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewSortBy.Lastname, this.terms["common.lastname"]));
        this.sortByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr, this.terms["common.report.selection.employeenr"]));
        this.sortByItems.push(new SmallGenericType(TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime, this.terms["common.report.selection.starttime"]));
    }

    public onDateRangeSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);

        this.fromDate = selection.from;
        this.toDate = selection.to;
    }

    public onShiftTypeSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHIFT_TYPES, selection);
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onBoolSelectionFilterOnAccountingUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_FILTER_ON_ACCOUNTING, selection);
    }

    public onBoolSelectionIncludeAbsenceUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_ABSENCE, selection);
    }

    public onIdSelectionGroupByUpdated(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_GROUP_BY, selection);
    }

    public onIdSelectionSortByUpdated(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_SORT_BY, selection);
    }
}
