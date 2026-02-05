import { TimeAbsenceRuleRowDTO } from "../../../../../Common/Models/TimeAbsenceRuleHeadDTO";
import { PayrollProductDTO } from "../../../../../Common/Models/ProductDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup_TimeAbsenceRuleType, TermGroup_SysPayrollType, TermGroup_TimeAbsenceRuleRowScope } from "../../../../../Util/CommonEnumerations";

export class TimeAbsenceRuleRowDialogController {

    private timeAbsenceRuleRow: TimeAbsenceRuleRowDTO;
    private isNew: boolean;
    private payrollProductsForType: PayrollProductDTO[];

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        timeAbsenceRuleRow: TimeAbsenceRuleRowDTO,
        private type: number,
        private showScope: boolean,
        private showPayrollProduct: boolean,
        private types: ISmallGenericType[],
        private scopes: ISmallGenericType[],
        private defaultScope: TermGroup_TimeAbsenceRuleRowScope,
        private payrollProducts: PayrollProductDTO[]) {

        this.isNew = !timeAbsenceRuleRow;
        this.payrollProductsForType = this.getPayrollProductsForType();

        this.timeAbsenceRuleRow = new TimeAbsenceRuleRowDTO();

        //Set product on row if only one exists for type
        if (this.payrollProductsForType && this.payrollProductsForType.length === 1 && !this.timeAbsenceRuleRow.payrollProductId)
            this.timeAbsenceRuleRow.payrollProductId = this.payrollProductsForType[0].productId;
            
        angular.extend(this.timeAbsenceRuleRow, timeAbsenceRuleRow);

        if (this.timeAbsenceRuleRow.scope == undefined)
            this.timeAbsenceRuleRow.scope = this.defaultScope;
    }

    public getPayrollProductsForType(): PayrollProductDTO[] {
        var sysPayrollTypeLevel2 = TermGroup_SysPayrollType.SE_GrossSalary_Absence;
        var sysPayrollTypeLevel3 = TermGroup_SysPayrollType.None;
        var dontFilter = false;

        switch (this.type) {
            case TermGroup_TimeAbsenceRuleType.None:
                break;
            case TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_PAID: 
                //payrollproduct on intervall are not used
                break;
            case TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_UNPAID:
                //payrollproduct on intervall are not used
                break;
            case TermGroup_TimeAbsenceRuleType.SickDuringStandby_PAID:
                //payrollproduct on intervall are not used
                break;
            case TermGroup_TimeAbsenceRuleType.SickDuringStandby_UNPAID:
                //payrollproduct on intervall are not used
                break;
            case TermGroup_TimeAbsenceRuleType.Sick_PAID:
            case TermGroup_TimeAbsenceRuleType.Sick_UNPAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick;
                break;
            case TermGroup_TimeAbsenceRuleType.WorkInjury_PAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury;
                break;
            case TermGroup_TimeAbsenceRuleType.TemporaryParentalLeave_PAID:
            case TermGroup_TimeAbsenceRuleType.TemporaryParentalLeave_UNPAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_TemporaryParentalLeave;
                break;
            case TermGroup_TimeAbsenceRuleType.PregnancyMoney_PAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_PregnancyCompensation;
                break;
            case TermGroup_TimeAbsenceRuleType.ParentalLeave_PAID:
            case TermGroup_TimeAbsenceRuleType.ParentalLeave_UNPAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave;
                break;
            case TermGroup_TimeAbsenceRuleType.MilitaryService_PAID:
            case TermGroup_TimeAbsenceRuleType.MilitaryService_UNPAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService;
                break;
            case TermGroup_TimeAbsenceRuleType.SwedishForImmigrants_PAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_SwedishForImmigrants;
                break;
            case TermGroup_TimeAbsenceRuleType.RelativeCare_PAID:
            case TermGroup_TimeAbsenceRuleType.RelativeCare_UNPAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_RelativeCare;
                break;
            case TermGroup_TimeAbsenceRuleType.DiseaseCarrier_PAID:
            case TermGroup_TimeAbsenceRuleType.DiseaseCarrier_UNPAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_TransmissionOfInfection;
                break;
            case TermGroup_TimeAbsenceRuleType.UnionEducation_PAID:
            case TermGroup_TimeAbsenceRuleType.UnionEducation_UNPAID:
                sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_UnionEduction;
                break;
            case TermGroup_TimeAbsenceRuleType.PayedAbsence_PAID:
                //Should be able to connect any product
                dontFilter = true;
                break;
            case TermGroup_TimeAbsenceRuleType.LeaveOfAbsence_UNPAID:
                //Should be able to connect any product
                dontFilter = true;
                break;
            default:
                break;
        }

        if (dontFilter)
            return this.payrollProducts;
        else
            return _.filter(this.payrollProducts, s => s.sysPayrollTypeLevel2 === sysPayrollTypeLevel2 && s.sysPayrollTypeLevel3 == sysPayrollTypeLevel3);
    }

    private isValid(): boolean {
        if (!this.timeAbsenceRuleRow)
            return false;
        if (!this.timeAbsenceRuleRow.start)
            return false;
        if (!this.timeAbsenceRuleRow.stop)
            return false;
        if (!this.timeAbsenceRuleRow.type)
            return false;
        if (this.showPayrollProduct && !this.timeAbsenceRuleRow.payrollProductId)
            return false;
        return true;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ timeAbsenceRuleRow: this.timeAbsenceRuleRow });
    }
}
