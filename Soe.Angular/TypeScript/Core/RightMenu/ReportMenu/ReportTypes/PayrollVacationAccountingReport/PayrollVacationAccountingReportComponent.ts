import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { EmployeeSelectionDTO, IdListSelectionDTO, IdSelectionDTO, BoolSelectionDTO, DateSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IEmployeeSelectionDTO, IIdListSelectionDTO, IIdSelectionDTO, IBoolSelectionDTO, IDateSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportDataService } from "../../ReportDataService";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";

export class PayrollVacationAccountingReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollVacationAccountingReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/PayrollVacationAccountingReport/PayrollVacationAccountingReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "payrollVacationAccountingReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private date: Date;
    private userSelectionInputCreateVoucher: BoolSelectionDTO;
    private noVoucherSeriesTypes: boolean;
    private hideVoucherSettings: boolean;
    private hasPermissionToCreateVoucher: boolean;

    //user selections
    private userSelectionInputDate: DateSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputVoucherSeriesType: IdSelectionDTO;
    private userSelectionInputVoucherRowMergeType: IdSelectionDTO;
    private voucherDate: Date;
    private userSkipQuantitySelection: BoolSelectionDTO;
    private userSelectionInputVoucherDate: DateSelectionDTO;

    //Data
    private voucherSeriesTypes: SmallGenericType[] = [];
    private voucherRowMergeTypes: SmallGenericType[] = [];

    //@ngInject
    constructor(        
        private coreService: ICoreService,
        private reportDataService: IReportDataService,
        private $scope: ng.IScope) {
        this.voucherDate = new Date();
        this.date = new Date();
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal) 
                return;

            console.log("userSelection", this.userSelection)
            console.log(this.userSelection.getDateSelectionFromKey(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERDATE));
            console.log(this.userSelectionInputVoucherSeriesType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERSERIESTYPE));
            console.log(this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERROWMERGETYPE));

            this.userSelectionInputDate = this.userSelection.getDateSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();
            this.userSelectionInputCreateVoucher = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_CREATEVOUCHER);
            this.userSkipQuantitySelection = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHER_SKIPQUANTITY);
            this.userSelectionInputVoucherSeriesType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERSERIESTYPE);
            this.userSelectionInputVoucherRowMergeType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERROWMERGETYPE);
            this.date = this.userSelectionInputDate ? this.userSelectionInputDate.date : new Date();
            this.userSelectionInputVoucherDate = this.userSelection.getDateSelectionFromKey(Constants.REPORTMENU_SELECTION_KEY_TIME_VOUCHERDATE);
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

    public onDateSelectionUpdated(selection: IDateSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE, selection);

        this.date = selection.date;
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
}
