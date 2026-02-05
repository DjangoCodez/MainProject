import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { InventoryNoteDTO } from "../../../Common/Models/InventoryNoteDTO";
import { IInventoryService } from "../../../Shared/Economy/Inventory/InventoryService";
import { AccountDistributionEntryDTO } from "../../../Common/Models/AccountDistributionEntryDTO";

export class EditNoteController {
    
    currentIndex: number;

    // Terms
    private terms: any = [];

    private title: string = "";
    private description: string = "";
    private notes: string = "";
    private inventoryId: number;

    //@ngInject
    constructor(private $uibModalInstance,
        protected urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private notificationService: INotificationService,
        private rows: AccountDistributionEntryDTO[],
        private row: AccountDistributionEntryDTO,
        private inventoryService: IInventoryService
    ) {
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
            this.loadTerms()]);
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "error.default_error",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private isModified() { return this.row.inventoryNotes != this.notes || this.row.inventoryDescription != this.description };

    private setRow() {
        this.row = this.rows[this.currentIndex];
        this.description = this.row.inventoryDescription;
        this.notes = this.row.inventoryNotes;
        this.inventoryId = this.row.inventoryId;
        this.title = this.row.inventoryName;
    }

    private close() {
        this.$uibModalInstance.close({ rowIsModified: false });
    }

    private cancel() {
        this.close();
    }

    private ok() {
        if(this.isModified()) {
            this.save().then(() => {
                this.$uibModalInstance.close({ rowIsModified: true, inventoryId: this.inventoryId, notes: this.notes, description: this.description });
            });
        } else {
            this.$uibModalInstance.close({ rowIsModified: false })
        }
    }

    private isLeftButtonDisabled(): boolean {
        return this.currentIndex == 0;
    }

    private isRightButtonDisabled(): boolean {
        return this.currentIndex == this.rows.length - 1;
    }

    private save(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        const dto = new InventoryNoteDTO()
        {
            dto.inventoryId = this.inventoryId,
            dto.description = this.description,
            dto.notes = this.notes
        }

        this.inventoryService.saveNotesAndDescription(dto).then(saveResult => {
            if (saveResult.success) {
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
        if (this.isModified())
            this.save().then(() => { this.moveLeft(); });
        else
            this.moveLeft();
    }

    private rightButtonClick() {
        if (this.isModified())
            this.save().then(() => { this.moveRight(); });
        else
            this.moveRight();
    }

    private moveLeft() {
        if (this.currentIndex > 0) {
            this.currentIndex = this.currentIndex - 1;
        }
        this.setRow();
    }

    private moveRight() {
        if (this.currentIndex < this.rows.length) {
            this.currentIndex = this.currentIndex + 1;
        }
        this.setRow();
    }

    private setCurrentIndex() {
        this.currentIndex = this.rows.indexOf(this.row) ? this.rows.indexOf(this.row) : 0;
    }
}