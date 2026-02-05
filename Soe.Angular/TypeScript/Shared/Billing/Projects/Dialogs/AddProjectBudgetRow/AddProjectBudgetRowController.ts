import { IProjectService } from "../../ProjectService";
import { SoeTimeCodeType, ProjectCentralBudgetRowType, SoeEntityState } from "../../../../../Util/CommonEnumerations";
import { BudgetRowDTO } from "../../../../../Common/Models/BudgetDTOs";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { BudgetRowEx } from "../../Directives/ProjectBudgetDirective";

export class AddProjectBudgetRowController {

    //Collections
    private timecodes: any[];
    private rows: BudgetRowEx[];
    private row: BudgetRowEx;
    private allowTimeCode: boolean;
    private allowQuantity: boolean = false;
    private isMinutes: boolean;
    private origTimeCodeId: number;


    //@ngInject
    constructor(private $uibModalInstance,
        private projectService: IProjectService,
        budgetRow: BudgetRowEx,
        budgetRows: BudgetRowEx[],
        allowTimeCode: boolean,
        isNewRow: boolean,
        private showIbAmount: boolean,
        private showIbQuantity: boolean,
    ) {
        if (isNewRow) {
            this.row = new BudgetRowEx();
            this.row.type = budgetRow.type;
            this.row.name = "";
        } else {
            this.row = budgetRow;
            this.origTimeCodeId = budgetRow.timeCodeId;
        }
        this.rows = budgetRows;
        this.allowTimeCode = allowTimeCode;
        switch (this.row.type) {
            case ProjectCentralBudgetRowType.BillableMinutesInvoiced:
                this.isMinutes = true;
                this.row.totalAmount = this.row.totalAmount / 60;
                break;
            case ProjectCentralBudgetRowType.CostMaterial:
                this.loadTimeCodes(SoeTimeCodeType.Material);
                break;
            case ProjectCentralBudgetRowType.CostPersonell:
                this.loadTimeCodes(SoeTimeCodeType.Work);
                this.allowQuantity = true;
                break;
            default:
                break;
        }
    }


    loadTimeCodes(type: SoeTimeCodeType): ng.IPromise<any> {
        return this.projectService.getTimeCodesByType(type, true, false).then((x) => {
            this.timecodes = x;
        });
    }

    buttonOkClick() {
        if (this.allowTimeCode && !this.row.timeCodeId)
            return

        this.row.isModified = true;

        if (this.row.ibType) {
            const ib = NumberUtility.parseDecimal(this.row.ib);
            const ibRow = this.rows.find(r => r.type === this.row.ibType);
            ibRow.isModified = true;
            ibRow.totalAmount = ib;
            if (this.isMinutes)
                ibRow.totalAmount *= 60;
        }
        if (this.row.ibQuantityType) {
            const ibRow = this.rows.find(r => r.type === this.row.ibQuantityType);
            if (this.showIbQuantity) {
                ibRow.totalAmount = this.row.ibHours * 60
                ibRow.isModified = true;
            }
        }

        this.row.totalAmount = NumberUtility.parseDecimal(this.row.totalAmount as any);
        if (this.isMinutes)
            this.row.totalAmount *= 60;

        const dupl = this.rows.find(r => r.type === this.row.type && r.timeCodeId === this.row.timeCodeId);
        if (!dupl) {
            this.rows.push(this.row);
        }

        //Prevent changing timecodes, not elegant but preventing several budget rows with same TimeCodeId.
        if (this.origTimeCodeId && this.origTimeCodeId != this.row.timeCodeId) {
            this.row.timeCodeId = this.origTimeCodeId;
        }

        if (this.row.timeCodeId > 0) {
            this.row.name = this.timecodes.find(t => t.timeCodeId === this.row.timeCodeId)?.name;
        }

        //If using overhead per hour, update the total overhead.
        if (this.row.type === ProjectCentralBudgetRowType.OverheadCostPerHour
            || this.row.type === ProjectCentralBudgetRowType.OverheadCost
            || this.row.type === ProjectCentralBudgetRowType.BillableMinutesInvoiced
            || this.row.type === ProjectCentralBudgetRowType.CostPersonell) {
            const overhead = this.rows.find(r => r.type === ProjectCentralBudgetRowType.OverheadCost);
            const overheadIb = this.rows.find(r => r.type === ProjectCentralBudgetRowType.OverheadCostIB);
            overhead.isModified = true;
            overheadIb.isModified = true;
        }

        if (this.row.type === ProjectCentralBudgetRowType.CostPersonell) {
            this.row.totalQuantity = this.row.hours * 60 || 0;
            if (!this.allowTimeCode) {
                let timeRow = this.rows.find(r => r.type === ProjectCentralBudgetRowType.BillableMinutesInvoiced);
                timeRow.totalAmount = this.row.totalQuantity;
                timeRow.isModified = true;
            }
        }

        if (this.allowTimeCode) {
            let row = this.rows.find(r => r.type == this.row.type);
            row.isModified = true;
        }

        this.$uibModalInstance.close(this.rows);
    }

    buttonDeleteClick() {
        this.row.isDeleted = true;
        this.buttonOkClick();
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonEnabled() {
        if (this.allowTimeCode && !this.row.timeCodeId) {
            return false;
        }
        return true;
    }
}