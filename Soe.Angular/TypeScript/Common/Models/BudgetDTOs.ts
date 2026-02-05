import { IBudgetHeadDTO, IBudgetHeadFlattenedDTO, IBudgetHeadSalesDTO, IBudgetPeriodDTO, IBudgetPeriodSalesDTO, IBudgetRowDTO, IBudgetRowFlattenedDTO, IBudgetRowSalesDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { Guid } from "../../Util/StringUtility";

export class BudgetHeadDTO implements IBudgetHeadDTO {
    accountYearId: number;
    accountYearText: string;
    actorCompanyId: number;
    budgetHeadId: number;
    created: Date;
    createdBy: string;
    createdDate: string;
    dim2Id: number;
    dim3Id: number;
    distributionCodeHeadId: number;
    fromDate: Date;
    modified: Date;
    modifiedBy: string;
    name: string;
    noOfPeriods: number;
    projectId: number;
    rows: IBudgetRowDTO[];
    status: number;
    statusName: string;
    toDate: Date;
    type: number;
    useDim2: boolean;
    useDim3: boolean;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }
}

export class BudgetRowDTO implements IBudgetRowDTO {
    accountId: number;
    budgetHead: IBudgetHeadDTO;
    budgetHeadId: number;
    budgetRowId: number;
    budgetRowNr: number;
    dim1Id: number;
    dim1Name: string;
    dim1Nr: string;
    dim2Id: number;
    dim2Name: string;
    dim2Nr: string;
    dim3Id: number;
    dim3Name: string;
    dim3Nr: string;
    dim4Id: number;
    dim4Name: string;
    dim4Nr: string;
    dim5Id: number;
    dim5Name: string;
    dim5Nr: string;
    dim6Id: number;
    dim6Name: string;
    dim6Nr: string;
    distributionCodeHeadId: number;
    distributionCodeHeadName: string;
    isAdded: boolean;
    isDeleted: boolean;
    isModified: boolean;
    modified: string;
    modifiedBy: string;
    modifiedUserId: number;
    name: string;
    periods: IBudgetPeriodDTO[];
    shiftTypeId: number;
    timeCodeId: number;
    totalAmount: number;
    totalQuantity: number;
    type: number;
}

export class BudgetHeadSalesDTO implements IBudgetHeadSalesDTO {
    actorCompanyId: number;
    budgetHeadId: number;
    created: Date;
    createdBy: string;
    distributionCodeHeadId: number;
    distributionCodeSubType: number;
    fromDate: Date;
    modified: Date;
    modifiedBy: string;
    name: string;
    noOfPeriods: number;
    rows: BudgetRowSalesDTO[];
    status: number;
    statusName: string;
    toDate: Date;
    type: number;

    constructor() {
        this.rows = [];
    }

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }

    public setTypes() {
        if (this.rows) {
            this.rows = this.rows.map(r => {
                let rObj = new BudgetRowSalesDTO();
                angular.extend(rObj, r);
                rObj.setTypes();
                return rObj;
            });
        } else {
            this.rows = [];
        }
    }
}

export class BudgetRowSalesDTO implements IBudgetRowSalesDTO {
    accountId: number;
    budgetHeadId: number;
    budgetRowId: number;
    budgetRowNr: number;
    dim1Id: number;
    dim1Name: string;
    dim1Nr: string;
    dim2Id: number;
    dim2Name: string;
    dim2Nr: string;
    dim3Id: number;
    dim3Name: string;
    dim3Nr: string;
    dim4Id: number;
    dim4Name: string;
    dim4Nr: string;
    dim5Id: number;
    dim5Name: string;
    dim5Nr: string;
    dim6Id: number;
    dim6Name: string;
    dim6Nr: string;
    distributionCodeHeadName: string;
    isDeleted: boolean;
    isModified: boolean;
    modified: string;
    modifiedBy: string;
    modifiedUserId: number;
    periods: BudgetPeriodSalesDTO[];
    totalAmount: number;
    totalQuantity: number;
    type: number;

    // Extensions
    firstLevel: boolean = true;

    constructor() {
        this.periods = [];
    }

    public setTypes() {
        if (this.periods) {
            this.periods = this.periods.map(p => {
                let pObj = new BudgetPeriodSalesDTO();
                angular.extend(pObj, p);
                pObj.fixDates();
                pObj.setTypes();
                return pObj;
            });
        } else {
            this.periods = [];
        }
    }

    public getValue(position: number): number {
        return this.periods[position].amount;
    }
}

export class BudgetPeriodSalesDTO implements IBudgetPeriodSalesDTO {
    amount: number;
    budgetRowId: number;
    budgetRowNr: number;
    budgetRowPeriodId: number;
    closingHour: number;
    distributionCodeHeadId: number;
    guid: Guid;
    isModified: boolean;
    parentGuid: Guid;
    percent: number;
    periodNr: number;
    periods: BudgetPeriodSalesDTO[];
    quantity: number;
    startDate: Date;
    startHour: number;
    type: number;

    constructor() {
        this.periods = [];
    }

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
    }

    public setTypes() {
        if (this.periods) {
            this.periods = this.periods.map(p => {
                let pObj = new BudgetPeriodSalesDTO();
                angular.extend(pObj, p);
                return pObj;
            });
        } else {
            this.periods = [];
        }
    }

    public getValue(position: number): number {
        return this.periods[position].amount;
    }
}

export class BudgetHeadFlattenedDTO implements IBudgetHeadFlattenedDTO {
    budgetHeadId: number;
    actorCompanyId: number;
    type: number;
    accountYearId: number;
    distributionCodeHeadId: number;
    distributionCodeSubType: number;
    noOfPeriods: number;
    status: number;
    projectId: number;
    accountYearText: string;
    statusName: string;
    name: string;
    createdDate: string;
    useDim2: boolean;
    useDim3: boolean;
    dim2Id: number;
    dim3Id: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    rows: BudgetRowFlattenedDTO[];

    isModified: boolean = false;
}

export class BudgetRowFlattenedDTO implements IBudgetRowFlattenedDTO {
    budgetRowId: number;
    budgetHeadId: number;
    accountId: number;
    distributionCodeHeadId: number;
    shiftTypeId: number;
    budgetRowNr: number;
    type: number;
    modifiedUserId: number;
    isModified: boolean;
    isDeleted: boolean;
    modified: string;
    modifiedBy: string;
    distributionCodeHeadName: string;
    totalAmount: number;
    totalQuantity: number;
    dim1Id: number;
    dim1Nr: string;
    dim1Name: string;
    dim2Id: number;
    dim2Nr: string;
    dim2Name: string;
    dim3Id: number;
    dim3Nr: string;
    dim3Name: string;
    dim4Id: number;
    dim4Nr: string;
    dim4Name: string;
    dim5Id: number;
    dim5Nr: string;
    dim5Name: string;
    dim6Id: number;
    dim6Nr: string;
    dim6Name: string;
    budgetRowPeriodId1: number;
    periodNr1: number;
    startDate1: Date;
    amount1: number;
    quantity1: number;
    budgetRowPeriodId2: number;
    periodNr2: number;
    startDate2: Date;
    amount2: number;
    quantity2: number;
    budgetRowPeriodId3: number;
    periodNr3: number;
    startDate3: Date;
    amount3: number;
    quantity3: number;
    budgetRowPeriodId4: number;
    periodNr4: number;
    startDate4: Date;
    amount4: number;
    quantity4: number;
    budgetRowPeriodId5: number;
    periodNr5: number;
    startDate5: Date;
    amount5: number;
    quantity5: number;
    budgetRowPeriodId6: number;
    periodNr6: number;
    startDate6: Date;
    amount6: number;
    quantity6: number;
    budgetRowPeriodId7: number;
    periodNr7: number;
    startDate7: Date;
    amount7: number;
    quantity7: number;
    budgetRowPeriodId8: number;
    periodNr8: number;
    startDate8: Date;
    amount8: number;
    quantity8: number;
    budgetRowPeriodId9: number;
    periodNr9: number;
    startDate9: Date;
    amount9: number;
    quantity9: number;
    budgetRowPeriodId10: number;
    periodNr10: number;
    startDate10: Date;
    amount10: number;
    quantity10: number;
    budgetRowPeriodId11: number;
    periodNr11: number;
    startDate11: Date;
    amount11: number;
    quantity11: number;
    budgetRowPeriodId12: number;
    periodNr12: number;
    startDate12: Date;
    amount12: number;
    quantity12: number;
    budgetRowPeriodId13: number;
    periodNr13: number;
    startDate13: Date;
    amount13: number;
    quantity13: number;
    budgetRowPeriodId14: number;
    periodNr14: number;
    startDate14: Date;
    amount14: number;
    quantity14: number;
    budgetRowPeriodId15: number;
    periodNr15: number;
    startDate15: Date;
    amount15: number;
    quantity15: number;
    budgetRowPeriodId16: number;
    periodNr16: number;
    startDate16: Date;
    amount16: number;
    quantity16: number;
    budgetRowPeriodId17: number;
    periodNr17: number;
    startDate17: Date;
    amount17: number;
    quantity17: number;
    budgetRowPeriodId18: number;
    periodNr18: number;
    startDate18: Date;
    amount18: number;
    quantity18: number;
    //Extension to get max 31 periods (days in month)
    budgetRowPeriodId19: number;
    periodNr19: number;
    startDate19: Date;
    amount19: number;
    quantity19: number;
    budgetRowPeriodId20: number;
    periodNr20: number;
    startDate20: Date;
    amount20: number;
    quantity20: number;
    budgetRowPeriodId21: number;
    periodNr21: number;
    startDate21: Date;
    amount21: number;
    quantity21: number;
    budgetRowPeriodId22: number;
    periodNr22: number;
    startDate22: Date;
    amount22: number;
    quantity22: number;
    budgetRowPeriodId23: number;
    periodNr23: number;
    startDate23: Date;
    amount23: number;
    quantity23: number;
    budgetRowPeriodId24: number;
    periodNr24: number;
    startDate24: Date;
    amount24: number;
    quantity24: number;
    budgetRowPeriodId25: number;
    periodNr25: number;
    startDate25: Date;
    amount25: number;
    quantity25: number;
    budgetRowPeriodId26: number;
    periodNr26: number;
    startDate26: Date;
    amount26: number;
    quantity26: number;
    budgetRowPeriodId27: number;
    periodNr27: number;
    startDate27: Date;
    amount27: number;
    quantity27: number;
    budgetRowPeriodId28: number;
    periodNr28: number;
    startDate28: Date;
    amount28: number;
    quantity28: number;
    budgetRowPeriodId29: number;
    periodNr29: number;
    startDate29: Date;
    amount29: number;
    quantity29: number;
    budgetRowPeriodId30: number;
    periodNr30: number;
    startDate30: Date;
    amount30: number;
    quantity30: number;
    budgetRowPeriodId31: number;
    periodNr31: number;
    startDate31: Date;
    amount31: number;
    quantity31: number;

    // Extensions
    showModalGetPreviousPeriodResult: boolean = true;
}
