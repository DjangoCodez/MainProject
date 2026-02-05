import { IProjectTimeMatrixDTO, IProjectTimeMatrixSaveDTO, IProjectTimeMatrixSaveRowDTO, ITimeDeviationCauseDTO, ITimeCodeDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class ProjectTimeMatrixSaveDTO implements IProjectTimeMatrixSaveDTO {
    customerInvoiceId: number;
    employeeId: number;
    projectId: number;
    projectInvoiceWeekId: number;
    timeCodeId: number;
    timeDeviationCauseId: number;
    weekDate: Date;
    isDeleted: boolean;
    rows: ProjectTimeMatrixSaveRowDTO[];

    // Extensions
    isModified: boolean;
}
export class ProjectTimeMatrixSaveRowDTO implements IProjectTimeMatrixSaveRowDTO {
    employeeChildId: number;
    externalNote: string;
    internalNote: string;
    invoiceQuantity: number;
    invoiceStateColor: string;
    isInvoiceEditable: boolean;
    isPayrollEditable: boolean;
    payrollQuantity: number;
    payrollStateColor: string;
    projectTimeBlockId: number;
    weekDay: number;

    // Extensions
    isModified: boolean;
}

export class ProjectTimeMatrixDTO implements IProjectTimeMatrixDTO {
    customerId: number;
    customerInvoiceId: number;
    customerName: string;
    employeeId: number;
    projectId: number;
    projectInvoiceWeekId: number;
    projectNr: string;
    projectName: string;
    invoiceNr: string;
    timeCodeId: number;
    timeCodeName: string;
    timeCodeReadOnly: boolean;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    isReadonly: boolean;
    rows: ProjectTimeMatrixSaveRowDTO[];

    //Extensions
    public timeDeviationCauseNeedChild = false;
    private isHeadInfoModified = false;
    public isGridRowModified = false;
    
    get timePayrollQuantityFormatted_1(): string {
        return this.getTimePayrollQuantityFormatted(1);
    }
    set timePayrollQuantityFormatted_1(time: string) {
        this.setTimePayrollQuantity(1, time);
    }
    get invoiceQuantityFormatted_1(): string {
        return this.getInvoiceQuantityFormatted(1);
    }
    set invoiceQuantityFormatted_1(time: string) {
        this.setInvoiceQuantity(1, time);
    }

    get timePayrollQuantityFormatted_2(): string {
        return this.getTimePayrollQuantityFormatted(2);
    }
    set timePayrollQuantityFormatted_2(time: string) {
        this.setTimePayrollQuantity(2, time);
    }
    get invoiceQuantityFormatted_2(): string {
        return this.getInvoiceQuantityFormatted(2);
    }
    set invoiceQuantityFormatted_2(time: string) {
        this.setInvoiceQuantity(2, time);
    }

    get timePayrollQuantityFormatted_3(): string {
        return this.getTimePayrollQuantityFormatted(3);
    }
    set timePayrollQuantityFormatted_3(time: string) {
        this.setTimePayrollQuantity(3, time);
    }
    get invoiceQuantityFormatted_3(): string {
        return this.getInvoiceQuantityFormatted(3);
    }
    set invoiceQuantityFormatted_3(time: string) {
        this.setInvoiceQuantity(3, time);
    }

    get timePayrollQuantityFormatted_4(): string {
        return this.getTimePayrollQuantityFormatted(4);
    }
    set timePayrollQuantityFormatted_4(time: string) {
        this.setTimePayrollQuantity(4, time);
    }
    get invoiceQuantityFormatted_4(): string {
        return this.getInvoiceQuantityFormatted(4);
    }
    set invoiceQuantityFormatted_4(time: string) {
        this.setInvoiceQuantity(4, time);
    }

    get timePayrollQuantityFormatted_5(): string {
        return this.getTimePayrollQuantityFormatted(5);
    }
    set timePayrollQuantityFormatted_5(time: string) {
        this.setTimePayrollQuantity(5, time);
    }
    get invoiceQuantityFormatted_5(): string {
        return this.getInvoiceQuantityFormatted(5);
    }
    set invoiceQuantityFormatted_5(time: string) {
        this.setInvoiceQuantity(5, time);
    }

    get timePayrollQuantityFormatted_6(): string {
        return this.getTimePayrollQuantityFormatted(6);
    }
    set timePayrollQuantityFormatted_6(time: string) {
        this.setTimePayrollQuantity(6, time);
    }
    get invoiceQuantityFormatted_6(): string {
        return this.getInvoiceQuantityFormatted(6);
    }
    set invoiceQuantityFormatted_6(time: string) {
        this.setInvoiceQuantity(6, time);
    }

    get timePayrollQuantityFormatted_0(): string {
        return this.getTimePayrollQuantityFormatted(7);
    }
    set timePayrollQuantityFormatted_0(time: string) {
        this.setTimePayrollQuantity(7, time);
    }
    get invoiceQuantityFormatted_0(): string {
        return this.getInvoiceQuantityFormatted(7);
    }
    set invoiceQuantityFormatted_0(time: string) {
        this.setInvoiceQuantity(7, time);
    }

    get timePayrollQuantityFormatted_Total(): string {
        return CalendarUtility.minutesToTimeSpan(this.getTimePayrollQuantity_Total());
    }

    get invoiceQuantityFormatted_Total(): string {
        return CalendarUtility.minutesToTimeSpan(this.getInvoiceQuantity_Total());
    }

    get noteIcon_1(): string {
        return this.getIcon(1);
    }
    get noteIcon_2(): string {
        return this.getIcon(2);
    }
    get noteIcon_3(): string {
        return this.getIcon(3);
    }
    get noteIcon_4(): string {
        return this.getIcon(4);
    }
    get noteIcon_5(): string {
        return this.getIcon(5);
    }
    get noteIcon_6(): string {
        return this.getIcon(6);
    }
    get noteIcon_0(): string {
        return this.getIcon(7);
    }
    private getIcon(weekDay: number): string {
        const row = this.getRow(weekDay, false);
        if (row && ((row.externalNote && row.externalNote.length > 0) || (row.internalNote && row.internalNote.length > 0)))
            return "fal fa-file-alt";
        else if (row && (row.payrollQuantity || row.invoiceQuantity) ) {
            return "fal fa-file";
        }
        else
            return "";
    }

    get childIcon(): string {
        if (this.timeDeviationCauseNeedChild) {
            return "fal fa-child";
        }
        else {
            return "";
        }
    }

    private clearAllInvoiceQuantities() {
        this.rows.forEach(r => {
            if (r.isInvoiceEditable) {
                r.invoiceQuantity = 0;
                this.isGridRowModified = r.isModified = true;
            }
        })
    }

    public getRow(weekDay: number, create: boolean): ProjectTimeMatrixSaveRowDTO {
        const foundRows = this.rows.filter(x => x.weekDay === weekDay);
        if (foundRows.length === 1) {
            return foundRows[0];
        }
        else if (foundRows.length === 0 && create) {
            const row = new ProjectTimeMatrixSaveRowDTO();
            row.weekDay = weekDay;
            row.isInvoiceEditable = true;
            row.isPayrollEditable = true;
            this.rows.push(row);
            return row;
        }

        return undefined;
    }

    public getTimePayrollQuantityFormatted(weekDay: number): string {
        const row = this.getRow(weekDay, false);
        const value = row ? row.payrollQuantity : 0;
        if (!value)
            return "";

        return CalendarUtility.minutesToTimeSpan(value);
    }

    public getInvoiceQuantityFormatted(weekDay: number): string {
        const row = this.getRow(weekDay, false);
        const value = row ? row.invoiceQuantity : 0;
        if (!value)
            return "";
        return CalendarUtility.minutesToTimeSpan(value);
    }

    public setTimePayrollQuantity(weekDay: number, time: string) {
        const span = CalendarUtility.parseTimeSpan(time, false, false, false);
        const value = CalendarUtility.timeSpanToMinutes(span);
        const row = this.getRow(weekDay, true);
        if (!row.isModified && (row.payrollQuantity || value) && (row.payrollQuantity !== value)) {
            this.isGridRowModified = row.isModified = true;
            if (!row.projectTimeBlockId && value && !row.invoiceQuantity && !this.timeCodeReadOnly) {
                row.invoiceQuantity = value;
            }
        }
        row.payrollQuantity = value;
    }
    
    public setInvoiceQuantity(weekDay: number, time: string) {
        const span = CalendarUtility.parseTimeSpan(time, false, false, false);
        const value = CalendarUtility.timeSpanToMinutes(span);
        const row = this.getRow(weekDay, true);
        if (!row.isModified && (row.invoiceQuantity || value) && (row.invoiceQuantity !== value)) {
            this.isGridRowModified = row.isModified = true;
        }
        row.invoiceQuantity = value;
    }

    public getTimePayrollQuantity_Total(): number {
        let total = 0;
        for (let i = 0; i < this.rows.length; i++) {
            total = total + this.rows[i].payrollQuantity;
        }
        return total;
    }

    public getInvoiceQuantity_Total(): number {
        let total = 0;
        for (let i = 0; i < this.rows.length; i++) {
            total = total + this.rows[i].invoiceQuantity;
        }
        return total;
    }

    public isEditableAll(): boolean {
        let result = true;
        this.rows.forEach(r => {
            if (!r.isInvoiceEditable || !r.isPayrollEditable) {
                result = false;
                return;
            }
        })

        return result;
    }

    public isEditable(field: string): boolean {
        const type = field.left(1);

        if (this.isReadonly) {
            return false;
        }

        if (type === "i" && this.timeCodeReadOnly) {
            return false;
        }

        const weekDay = field.right(1).toOnlyNumbers(true);
        const row = this.getRow(weekDay, false);

        if (!row)
            return true;

        return type === "i" ? row.isInvoiceEditable : row.isPayrollEditable;
    }

    public isTimeDevationCauseEditable() {
        let result = true;
        this.rows.forEach(r => {
            if (!r.isPayrollEditable) {
                result = false;
                return;
            }
        })

        return result;
    }

    public isOrderEditable() {
        let result = true;
        this.rows.forEach(r => {
            if (r.projectTimeBlockId) {
                result = false;
                return;
            }
        })

        return result;
    }
    public isTimeCodeEditable(): boolean {
        if (this.timeCodeReadOnly) {
            return false;
        }

        let result = true;
        this.rows.forEach(r => {
            if (!r.isInvoiceEditable) {
                result = false;
                return;
            }
        })

        return result;
    }
       
    public getCellStyle(field: string): any {
        const weekDay = field.right(1).toOnlyNumbers(true);
        const type = field.left(1);
        const row = this.getRow(weekDay, false);

        if (!row )
            return true;

        let color = "";
        if (type === "i" && row.invoiceQuantity) {
            color = row.invoiceStateColor;
        }
        else if (type === "t" && row.payrollQuantity) {
            color = row.payrollStateColor;
        }

        return (color) ? { 'border-right-color': color, 'border-right-width': 'thick' } : undefined;
    }

    public setExternalNote(weekDay: number, externalNote: string) {
        const row = this.getRow(weekDay, true);
        row.externalNote = externalNote;
        this.isGridRowModified = row.isModified = true;
    }

    public setInternalNote(weekDay: number, internalNote: string) {
        const row = this.getRow(weekDay, true);
        row.internalNote = internalNote;
        this.isGridRowModified = row.isModified = true;
    }

    public hasChanges(): boolean {
        if (this.isHeadInfoModified)
        {
            return true
        }
        else {
            return this.rows.filter(r => r.isModified).length > 0;
        }
    }

    public hasDbRows(): boolean {
        return this.rows.filter(r => r.projectTimeBlockId > 0).length > 0;
    }

    public timeDeviationCauseChanged(newTimeDeviationCause: ITimeDeviationCauseDTO) {
        if (newTimeDeviationCause) {
            this.isGridRowModified = this.timeDeviationCauseId !== newTimeDeviationCause.timeDeviationCauseId;

            this.timeCodeReadOnly = (!newTimeDeviationCause.isPresence || newTimeDeviationCause.notChargeable);
            this.timeDeviationCauseId = newTimeDeviationCause.timeDeviationCauseId;
            this.timeDeviationCauseName = newTimeDeviationCause.name;
            this.timeDeviationCauseNeedChild = newTimeDeviationCause.specifyChild;
            this.isReadonly = newTimeDeviationCause.mandatoryTime;
        }
        else {
            this.isReadonly = false;
            this.timeCodeReadOnly = false;
            this.timeDeviationCauseId = 0;
            this.timeDeviationCauseName = "";
            this.timeDeviationCauseNeedChild = false;
        }
        if (this.timeCodeReadOnly && this.timeCodeId) {
            this.timeCodeChanged(null);
        }
    }

    public timeCodeChanged(newTimeCode: ITimeCodeDTO) {
        if ( (newTimeCode) && (newTimeCode.timeCodeId !== this.timeCodeId) )
        {
            this.isHeadInfoModified = this.isGridRowModified = true;
        }
        if (newTimeCode) {
            this.timeCodeId = newTimeCode.timeCodeId;
            this.timeCodeName = newTimeCode.name;
        }
        else {
            this.timeCodeId = 0;
            this.timeCodeName = "";
            this.clearAllInvoiceQuantities();
        }
    }

    public projectChanged(newProjectId: number, newProjectName: string) {
        if (newProjectId && newProjectId !== this.projectId) {
            this.isHeadInfoModified = this.isGridRowModified = true;
        }
        this.projectId = newProjectId;
        this.projectName = newProjectName;
    }

    public orderChanged(customerInvoiceId: number, invoiceNr: string) {
        if (this.customerInvoiceId !== customerInvoiceId) {
            this.isHeadInfoModified = this.isGridRowModified = true;
        }
        this.customerInvoiceId = customerInvoiceId;
        this.invoiceNr = invoiceNr; 
    }

    public toSaveDTO(weekDate: Date): ProjectTimeMatrixSaveDTO {
        const dto = new ProjectTimeMatrixSaveDTO();
        dto.weekDate = weekDate;
        dto.customerInvoiceId = this.customerInvoiceId;
        dto.employeeId = this.employeeId;
        dto.projectId = this.projectId;
        dto.timeCodeId = this.timeCodeId;
        dto.timeDeviationCauseId = this.timeDeviationCauseId;
        dto.projectInvoiceWeekId = this.projectInvoiceWeekId;
        dto.rows = this.isHeadInfoModified ? this.rows : this.rows.filter(r => r.isModified);
        dto.rows = dto.rows.filter(r => (r.projectTimeBlockId || r.invoiceQuantity || r.payrollQuantity));
        return dto;
    }

    public toSaveDTOForDelete(weekDate: Date): ProjectTimeMatrixSaveDTO {
        const dto = new ProjectTimeMatrixSaveDTO();
        dto.isDeleted = true;
        dto.weekDate = weekDate;
        dto.customerInvoiceId = this.customerInvoiceId;
        dto.employeeId = this.employeeId;
        dto.projectId = this.projectId;
        dto.timeCodeId = this.timeCodeId;
        dto.timeDeviationCauseId = this.timeDeviationCauseId;
        dto.projectInvoiceWeekId = this.projectInvoiceWeekId;
        dto.rows = this.rows.filter(r => r.projectTimeBlockId);
        return dto;
    }

    public clearRowKeys() {
        this.projectInvoiceWeekId = 0;
        this.isHeadInfoModified = this.isGridRowModified= true;
        this.rows.forEach((r) => {
            r.isModified = true;
            r.isInvoiceEditable = true;
            r.isPayrollEditable = true;
            r.externalNote = undefined;
            r.internalNote = undefined;
            r.projectTimeBlockId = 0;
            r.invoiceStateColor = undefined;
            r.payrollStateColor = undefined;
            r.employeeChildId = 0
        });
    }
}