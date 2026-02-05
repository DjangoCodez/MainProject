import { IEmployeeVehicleDeductionDTO, IEmployeeVehicleEquipmentDTO, IEmployeeVehicleTaxDTO, IEmployeeVehicleDTO, IEmployeeVehicleGridDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_SysVehicleFuelType, TermGroup_VehicleType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";


export class EmployeeVehicleDeductionDTO implements IEmployeeVehicleDeductionDTO {
    employeeVehicleDeductionId: number;
    employeeVehicleId: number;
    fromDate: Date;
    price: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class EmployeeVehicleEquipmentDTO implements IEmployeeVehicleEquipmentDTO {
    employeeVehicleEquipmentId: number;
    employeeVehicleId: number;
    description: string;
    fromDate: Date;
    toDate: Date;
    price: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }
}

export class EmployeeVehicleTaxDTO implements IEmployeeVehicleTaxDTO {
    amount: number;
    employeeVehicleId: number;
    employeeVehicleTaxId: number;
    fromDate: Date;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class EmployeeVehicleDTO implements IEmployeeVehicleDTO {
    actorCompanyId: number;
    benefitValueAdjustment: number;
    codeForComparableModel: string;
    comparablePrice: number;
    created: Date;
    createdBy: string;
    deduction: EmployeeVehicleDeductionDTO[];
    employeeId: number;
    employeeVehicleId: number;
    equipment: EmployeeVehicleEquipmentDTO[];
    fromDate: Date;
    fuelType: TermGroup_SysVehicleFuelType;
    hasExtensiveDriving: boolean;
    licensePlateNumber: string;
    modelCode: string;
    modified: Date;
    modifiedBy: string;
    price: number;
    priceAdjustment: number;
    registeredDate: Date;
    state: SoeEntityState;
    sysVehicleTypeId: number;
    tax: EmployeeVehicleTaxDTO[];
    taxableValue: number;
    toDate: Date;
    type: TermGroup_VehicleType;
    vehicleMake: string;
    vehicleModel: string;
    year: number;

    // Extensions
    manufacturingYear: number;
    hasEquipment: boolean = false;

    constructor() {
        this.year = new Date().getFullYear();
        this.fromDate = null;
        this.toDate = null;
        this.type = TermGroup_VehicleType.Car;
        this.fuelType = TermGroup_SysVehicleFuelType.Unknown;
        this.hasExtensiveDriving = false;
    }

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
        this.registeredDate = CalendarUtility.convertToDate(this.registeredDate);
    }
}

export class EmployeeVehicleGridDTO implements IEmployeeVehicleGridDTO {
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employeeVehicleId: number;
    equipmentSum: number;
    fromDate: Date;
    licensePlateNumber: string;
    netSalaryDeduction: number;
    price: number;
    taxableValue: number;
    toDate: Date;
    vehicleMakeAndModel: string;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }

    public get description(): string {
        return "{0} - ({1}) {2}".format(this.licensePlateNumber, this.employeeNr, this.employeeName);
    }
}