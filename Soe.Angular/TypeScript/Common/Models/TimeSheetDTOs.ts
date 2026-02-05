import { ITimeSheetRowDTO, IEmployeeProjectInvoiceDTO } from "../../Scripts/TypeLite.Net4";
import { ProjectInvoiceSmallDTO, ProjectSmallDTO } from "./ProjectDTO";
import { TermGroup_ProjectAllocationType, OrderInvoiceRegistrationType } from "../../Util/CommonEnumerations";

export class TimeSheetRowDTO implements ITimeSheetRowDTO {
    timeSheetWeekId: number;
    projectInvoiceWeekId: number;
    rowNr: number;
    projectId: number;
    projectNr: string;
    projectName: string;
    projectNumberName: string;
    allocationType: TermGroup_ProjectAllocationType;
    customerId: number;
    customerName: string;
    invoiceId: number;
    invoiceNr: string;
    registrationType: OrderInvoiceRegistrationType;
    timeCodeId: number;
    timeCodeName: string;
    attestStateId: number;
    attestStateName: string;
    attestStateColor: string;
    weekStartDate: Date;
    mondayQuantity: number;
    mondayInvoiceQuantity: number;
    tuesdayQuantity: number;
    tuesdayInvoiceQuantity: number;
    wednesdayQuantity: number;
    wednesdayInvoiceQuantity: number;
    thursdayQuantity: number;
    thursdayInvoiceQuantity: number;
    fridayQuantity: number;
    fridayInvoiceQuantity: number;
    saturdayQuantity: number;
    saturdayInvoiceQuantity: number;
    sundayQuantity: number;
    sundayInvoiceQuantity: number;
    weekSumQuantity: number;
    weekSumInvoiceQuantity: number;
    mondayNoteInternal: string;
    mondayNoteExternal: string;
    tuesdayNoteInternal: string;
    tuesdayNoteExternal: string;
    wednesdayNoteInternal: string;
    wednesdayNoteExternal: string;
    thursdayNoteInternal: string;
    thursdayNoteExternal: string;
    fridayNoteInternal: string;
    fridayNoteExternal: string;
    saturdayNoteInternal: string;
    saturdayNoteExternal: string;
    sundayNoteInternal: string;
    sundayNoteExternal: string;
    weekNoteInternal: string;
    weekNoteExternal: string;

    // Extensions
    mondayQuantityFormatted: string;
    mondayInvoiceQuantityFormatted: string;
    tuesdayQuantityFormatted: string;
    tuesdayInvoiceQuantityFormatted: string;
    wednesdayQuantityFormatted: string;
    wednesdayInvoiceQuantityFormatted: string;
    thursdayQuantityFormatted: string;
    thursdayInvoiceQuantityFormatted: string;
    fridayQuantityFormatted: string;
    fridayInvoiceQuantityFormatted: string;
    saturdayQuantityFormatted: string;
    saturdayInvoiceQuantityFormatted: string;
    sundayQuantityFormatted: string;
    sundayInvoiceQuantityFormatted: string;
    weekSumQuantityFormatted: string;
    weekSumInvoiceQuantityFormatted: string;

    get hasMondayNote(): boolean {
        return !!this.mondayNoteExternal || !!this.mondayNoteInternal;
    }

    get hasTuesdayNote(): boolean {
        return !!this.tuesdayNoteExternal || !!this.tuesdayNoteInternal;
    }

    get hasWednesdayNote(): boolean {
        return !!this.wednesdayNoteExternal || !!this.wednesdayNoteInternal;
    }

    get hasThursdayNote(): boolean {
        return !!this.thursdayNoteExternal || !!this.thursdayNoteInternal;
    }

    get hasFridayNote(): boolean {
        return !!this.fridayNoteExternal || !!this.fridayNoteInternal;
    }

    get hasSaturdayNote(): boolean {
        return !!this.saturdayNoteExternal || !!this.saturdayNoteInternal;
    }

    get hasSundayNote(): boolean {
        return !!this.sundayNoteExternal || !!this.sundayNoteInternal;
    }

    get hasWeekNote(): boolean {
        return !!this.weekNoteExternal || !!this.weekNoteInternal;
    }

    get hasWeekendTimes(): boolean {
        return this.saturdayQuantity !== 0 || this.saturdayInvoiceQuantity !== 0 || this.sundayQuantity !== 0 || this.sundayInvoiceQuantity !== 0;
    }

    constructor() {
        this.mondayQuantity = 0;
        this.mondayInvoiceQuantity = 0;
        this.tuesdayQuantity = 0;
        this.tuesdayInvoiceQuantity = 0;
        this.wednesdayQuantity = 0;
        this.wednesdayInvoiceQuantity = 0;
        this.thursdayQuantity = 0;
        this.thursdayInvoiceQuantity = 0;
        this.fridayQuantity = 0;
        this.fridayInvoiceQuantity = 0;
        this.saturdayQuantity = 0;
        this.saturdayInvoiceQuantity = 0;
        this.sundayQuantity = 0;
        this.sundayInvoiceQuantity = 0;
        this.weekSumQuantity = 0;
        this.weekSumInvoiceQuantity = 0;
    }
}

export class EmployeeProjectInvoiceDTO implements IEmployeeProjectInvoiceDTO {
    defaultTimeCodeId: number;
    employeeId: number;
    invoices: ProjectInvoiceSmallDTO[];
    projects: ProjectSmallDTO[];
}
