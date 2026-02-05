import { ProjectTimeBlockDTO, ProjectTimeBlockSaveDTO } from "../../Models/ProjectDTO";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";

export class EditNoteController {

    currentIndex: number;

    // Terms
    private terms: any = [];

    private title: string = "";
    private rowIsModified = false;
    private rowsAreModified = false;

    //@ngInject
    constructor(private $uibModalInstance,
        protected urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private notificationService: INotificationService,
        private rows: ProjectTimeBlockDTO[],
        private row: ProjectTimeBlockDTO,
        private isReadonly: boolean,
        private saveDirect: boolean,
        private workTimePermission: boolean,
        private invoiceTimePermission: boolean,
        private projectService: IProjectService) {
        
        this.init();
    }


    // SETUP
    private init() {
        this.loadLookups();
        this.setCurrentIndex();
        this.setRow();
    }

    private loadLookups() {
        this.$q.all([
            this.loadTerms()]).then(() => {
                this.changeTitle();
            });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "error.default_error",
            "billing.project.timesheet.editnote.quantity",
            "billing.project.timesheet.editnote.invoicequantity"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private setRow() {
        this.row = this.rows[this.currentIndex];
        this.rowIsModified = false;
    }

    private changeTitle() {
        let name = this.row.employeeName;
        let date = CalendarUtility.toFormattedDate(this.row.date);

        this.title = `${name}, ${date}`
        if (this.workTimePermission) {
            let quantityText = this.terms["billing.project.timesheet.editnote.quantity"];
            let quantity = this.row.timePayrollQuantityFormatted;
            this.title += `, ${quantityText} (${quantity})`;
        }

        if (this.invoiceTimePermission) {
            let invoicequantityText = this.terms["billing.project.timesheet.editnote.invoicequantity"];
            let invoicequantity = this.row.invoiceQuantityFormatted;
            this.title += `, ${invoicequantityText} (${invoicequantity})`;
        }
    }

    private close() {
        if (this.rowIsModified && this.saveDirect) {
            this.save(this.row).then(() => {
                this.$uibModalInstance.close({ rowsAreModified: this.rowsAreModified });
            });
        }
        else
            this.$uibModalInstance.close({ rowsAreModified: this.rowsAreModified, rowIsModified: this.rowIsModified });
    }

    private cancel() {
        this.$uibModalInstance.close({ rowsAreModified: this.rowsAreModified });
    }

    private isLeftButtonDisabled(): boolean {
        return this.currentIndex == 0;
    }

    private isRightButtonDisabled(): boolean {
        return this.currentIndex == this.rows.length - 1;
    }

    private isReadOnly(): boolean {
        return this.isReadonly || !this.rows[this.currentIndex].isEditable;
    }

    private save(row: ProjectTimeBlockDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        let dto = new ProjectTimeBlockSaveDTO();
        dto.projectTimeBlockId = row.projectTimeBlockId;
        dto.date = row.date;
        dto.externalNote = row.externalNote;
        dto.internalNote = row.internalNote;
        dto.projectInvoiceWeekId = row.projectInvoiceWeekId;

        this.projectService.saveNotesForProjectTimeBlock(dto).then(saveResult => {
            if (saveResult.success) {
                this.rowsAreModified = true;
                deferral.resolve(true);
            } else {
                this.translationService.translate("error.unabletosave_title").then((term) => {
                    this.notificationService.showErrorDialog(term, saveResult.errorMessage, "");
                });
                deferral.reject();
            }
        });

        return deferral.promise;
    }

    private leftButtonClick() {
        if (this.rowIsModified) {

            this.save(this.row).then(() => { this.moveLeft(); });
        }
        else
            this.moveLeft();
    }

    private rightButtonClick() {
        if (this.rowIsModified) {
            this.save(this.row).then(() => { this.moveRight(); });
        }
        else
            this.moveRight();
    }

    private moveLeft() {
        if (this.currentIndex > 0) {
            this.currentIndex = this.currentIndex - 1;
        }
        this.setRow();
        this.changeTitle();
    }

    private moveRight() {
        if (this.currentIndex < this.rows.length) {
            this.currentIndex = this.currentIndex + 1;
        }
        this.setRow();
        this.changeTitle();
    }

    private setCurrentIndex() {
        this.currentIndex = this.rows.indexOf(this.row) ? this.rows.indexOf(this.row) : 0;
    }

    private onChange() {
        this.rowIsModified = true;
    }
}