import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO, IdSelectionDTO, BoolSelectionDTO, DateSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IIdListSelectionDTO, IIdSelectionDTO, IBoolSelectionDTO, IDateSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportDataService } from "../../ReportDataService";
import { Feature, TermGroup_ReportExportFileType } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";

export class PayrollAccountingReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollAccountingReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/PayrollAccountingReport/PayrollAccountingReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
                exportFileType: "=",
             
            }
        };

        return options;
    }
    public static componentKey = "payrollAccountingReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private exportFileType: number;
    private timePeriodIds: number[];
    private userSelectionInputCreateVoucher: BoolSelectionDTO;
    private noVoucherSeriesTypes: boolean;
    private hideVoucherSettings: boolean;
    private hasPermissionToCreateVoucher: boolean;
    private hideNettedCheckbox: boolean;

    //user selections
    private userSelectionInputPayrollPeriod: IdListSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputVoucherSeriesType: IdSelectionDTO;
    private userSkipQuantitySelection: BoolSelectionDTO;
    private userSelectionInputVoucherRowMergeType: IdSelectionDTO;
    private voucherDate: Date;
    private userSelectionInputVoucherDate: DateSelectionDTO;
    private userSelectNettedSelection: BoolSelectionDTO;

    //Data
    private voucherSeriesTypes: SmallGenericType[] = [];
    private voucherRowMergeTypes: SmallGenericType[] = [];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private reportDataService: IReportDataService,
        private $scope: ng.IScope) {
        this.voucherDate = new Date();

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            this.hideNettedCheckbox = this.exportFileType != TermGroup_ReportExportFileType.Payroll_SIE_Accounting;
            if (!newVal)
                return;

            this.userSelectionInputPayrollPeriod = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS);
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputCreateVoucher = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_CREATEVOUCHER);
            this.userSelectionInputVoucherSeriesType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERSERIESTYPE);
            this.userSkipQuantitySelection = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHER_SKIPQUANTITY);
            this.userSelectionInputVoucherRowMergeType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERROWMERGETYPE);
            this.timePeriodIds = this.userSelectionInputPayrollPeriod ? this.userSelectionInputPayrollPeriod.ids : null;
            this.userSelectionInputVoucherDate = this.userSelection.getDateSelectionFromKey(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERDATE);
            this.userSelectNettedSelection = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHER_NETTED);
        });
    }

    public $onInit() {
        this.loadModifyPermissions();
        this.loadVoucherSeriesTypes();
        this.loadVoucherMergeTypes();
        this.noVoucherSeriesTypes = true;
        this.hideVoucherSettings = true;
    }

    private loadVoucherSeriesTypes() {
        this.reportDataService.getVoucherSeriesTypes().then(x => {
            _.forEach(x, (seriesType: any) => {
                this.voucherSeriesTypes.push(new SmallGenericType(seriesType.voucherSeriesTypeId, seriesType.name));
                this.noVoucherSeriesTypes = false;
            })
        });
    }

    private loadVoucherMergeTypes() {
        this.reportDataService.getVoucherRowMergeTypes().then(x => {
            _.forEach(x, (mergeType: any) => {
                this.voucherRowMergeTypes.push(new SmallGenericType(mergeType.id, mergeType.name));
            })
        });
    }

    public onBoolSelectionInputCreateVoucher(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_CREATEVOUCHER, selection);

        if (selection.value)
            this.hideVoucherSettings = false;
        else
            this.hideVoucherSettings = true;

        if (!this.hideVoucherSettings && !this.hasPermissionToCreateVoucher)
            this.hideVoucherSettings = true;
    }

    public onPeriodSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onVoucherSeriesTypeIdSelectionUpdated(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERSERIESTYPE, selection);
    }

    public onVoucherRowMergeTypeSelectionUpdated(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERROWMERGETYPE, selection);
    }

    public onSkipQuantitySelectionUpdated(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHER_SKIPQUANTITY, selection);
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Economy_Accounting_Vouchers_Edit);
        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.hasPermissionToCreateVoucher = x[Feature.Economy_Accounting_Vouchers_Edit];

            if (!this.hasPermissionToCreateVoucher)
                this.noVoucherSeriesTypes = true;
        });
    }

    public onVoucherDateSelectionUpdated(selection: IDateSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERDATE, selection);
        this.voucherDate = selection.date;
    }

    public onNettedSelectionUpdated(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHER_NETTED, selection);
    }

}
