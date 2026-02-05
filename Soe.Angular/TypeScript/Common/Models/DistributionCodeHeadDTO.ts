import { IDistributionCodeGridDTO, IDistributionCodeHeadDTO, IDistributionCodePeriodDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";


export class DistributionCodeHeadDTO implements IDistributionCodeHeadDTO {
    accountDimId: number;
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    distributionCodeHeadId: number;
    fromDate: Date;
    isInUse: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    noOfPeriods: number;
    openingHoursId: number;
    parentId: number;
    periods: DistributionCodePeriodDTO[];
    subType: number;
    type: number;
    typeId: number;

    // Extensions
    typeName: string;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }

    public setTypes() {
        if (this.periods) {
            this.periods = this.periods.map(p => {
                let pObj = new DistributionCodePeriodDTO(p.number, p.percent, p.periodSubTypeName);
                angular.extend(pObj, p);
                return pObj;
            });
        } else {
            this.periods = [];
        }
    }
}

export class DistributionCodeGridDTO implements IDistributionCodeGridDTO {
    accountDim: string;
    distributionCodeHeadId: number;
    fromDate: Date;
    name: string;
    noOfPeriods: number;
    openingHour: string;
    subLevel: string;
    type: string;
    typeId: number;
    typeOfPeriod: string;
    typeOfPeriodId: number;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class DistributionCodePeriodDTO implements IDistributionCodePeriodDTO {
    distributionCodePeriodId: number;
    number: number;
    isAdded: boolean;
    isModified: boolean;
    percent: number;
    comment: string;
    periodSubTypeName: string;
    parentToDistributionCodePeriodId: number;

    //Extensions
    parentToDistributionCodePeriodName: string;
    parentToDistributionCodePeriodChanged: boolean;

    constructor(number: number, percent: number, periodSubTypeName: string = "") {
        this.percent = percent;
        this.number = number;
        this.isAdded = true;
        this.comment = "";
        this.periodSubTypeName = periodSubTypeName;
    }
}
