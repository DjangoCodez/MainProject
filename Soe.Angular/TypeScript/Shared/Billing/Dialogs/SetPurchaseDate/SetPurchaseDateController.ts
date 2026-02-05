import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { PurchaseRowDTO } from "../../../../Common/Models/PurchaseDTO";
import { ProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { SoeOriginStatus } from "../../../../Util/CommonEnumerations";
import { EmbeddedGridController } from "../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";

export class SetPurchaseDateController {
    private progressHandler: IProgressHandler;
    private propName: string;

    private _dateHead: Date;
    get dateHead() {
        return this._dateHead;
    }
    set dateHead(date) {
        if (!date)
            return;

        this.purchaseRowsChanges.forEach(r => {
            if ((!r[this.propName] || (this._dateHead && this._dateHead.isSameDayAs(r[this.propName])) || (this.purchaseDate && this.purchaseDate.isAfterOnDay(r[this.propName]))) && !r.isLocked) {
                r[this.propName] = new Date(date);
            }
        })
        this._dateHead = new Date(date);
        this.setData()
    }

    terms: { [index: string]: string; };
    dateLabel: string;
    purchaseRowsChanges = [];

    // Flags
    headDateValid = true;
    rowDateValid = true;

    // Grid
    private gridHandler: EmbeddedGridController;
    
    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private purchaseRows: PurchaseRowDTO[],
        private newStatus: SoeOriginStatus,
        private confirmedDate: Date,
        private useConfirmed: boolean,
        private $uibModal,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private gridHandlerFactory: IGridHandlerFactory,
        private purchaseDate?: Date,
    ) {
    }

    private $onInit() {

        this.progressHandler =
            new ProgressHandlerFactory(this.$uibModal, this.translationService, this.$q, this.messagingService, this.urlHelperService, null)
                .create();

        if (this.newStatus === SoeOriginStatus.PurchaseAccepted || this.useConfirmed) {
            if (this.useConfirmed && this.confirmedDate)
                this._dateHead = this.confirmedDate;

            this.propName = "accDeliveryDate";
        } else if (this.newStatus === SoeOriginStatus.Origin) {
            this.propName = "wantedDeliveryDate";
        }

        this.gridHandler = new EmbeddedGridController(this.gridHandlerFactory, "common.dialogs.searchcustomerinvoice");

        this.$timeout(() => {
            this.gridHandler.gridAg.options.setFilterFocus();
        });

        this.purchaseRowsChanges = this.purchaseRows.map(r => {
            const slimRow = {
                isLocked: r.isLocked,
                text: r.text,
                productNr: r.productNr,
                accDeliveryDate: r.accDeliveryDate,
                wantedDeliveryDate: r.wantedDeliveryDate
            }
            return slimRow;
        })

        this.startLoad();
    }

    private startLoad() {
        this.progressHandler.startLoadingProgress([() =>
            this.setup()
        ])
    }

    private setup(): ng.IPromise<any> {
        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.enableFiltering = true;
        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.options.ignoreResetFilterModel = true;
        this.gridHandler.gridAg.options.setMinRowsToShow(8);

        const keys: string[] = [
            "common.name",
            "core.saving",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "billing.productrows.productnr",
            "billing.purchaserows.accdeliverydate",
            "billing.purchaserows.wanteddeliverydate"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.gridHandler.gridAg.addColumnText("productNr", terms["billing.productrows.productnr"], null);
            this.gridHandler.gridAg.addColumnText("text", terms["common.name"], null);
            if (this.newStatus === SoeOriginStatus.PurchaseAccepted) {
                this.gridHandler.gridAg.addColumnDate("accDeliveryDate", terms["billing.purchaserows.accdeliverydate"], null, false, null, { editable: (data) => !data.isLocked });
                this.dateLabel = terms["billing.purchaserows.accdeliverydate"];
            } else if (this.newStatus === SoeOriginStatus.Origin) {
                this.gridHandler.gridAg.addColumnDate("wantedDeliveryDate", terms["billing.purchaserows.wanteddeliverydate"], null, false, null, { editable: (data) => !data.isLocked });
                this.dateLabel = terms["billing.purchaserows.wanteddeliverydate"];
            }

            this.gridHandler.gridAg.finalizeInitGrid("", true);

            this.setData();
        });
    }

    private setData() {
        this.gridHandler.gridAg.options.setData(this.purchaseRowsChanges);
    }


    private onCellEdit(entity: PurchaseRowDTO, colDef) {

    }

    private afterCellEdit(entity: PurchaseRowDTO, colDef) {
        switch (colDef.field) {
            case "quantity":
                break;
            case "purchasePrice":
                break;
            case "purchaseName":
                break;
        }
    }

    private isValid(): boolean {
        this.headDateValid = this.purchaseRowsChanges && this.purchaseRowsChanges.length > 0 ? !this.dateHead || this.dateHead.isSameOrAfterOnDay(this.purchaseDate) : true;
        this.rowDateValid = this.purchaseRowsChanges && this.purchaseRowsChanges.length > 0 ? (this.newStatus === SoeOriginStatus.PurchaseAccepted ? !_.find(this.purchaseRowsChanges, (r) => (r.accDeliveryDate && r.accDeliveryDate.isBeforeOnDay(this.purchaseDate))) : !_.find(this.purchaseRowsChanges, (r) => (r.wantedDeliveryDate && r.wantedDeliveryDate.isBeforeOnDay(this.purchaseDate)))) : true;
        return !this.purchaseDate || (this.headDateValid && this.rowDateValid);
    }

    buttonCancelClick() {
        this.close({ success: false });
    }

    buttonOkClick() {
        for (let i = 0; i < this.purchaseRows.length; i++) {
            this.purchaseRows[i].accDeliveryDate = this.purchaseRowsChanges[i].accDeliveryDate;
            this.purchaseRows[i].wantedDeliveryDate = this.purchaseRowsChanges[i].wantedDeliveryDate;
            this.purchaseRows[i].isModified = true;
        }
        this.close({ success: true, date: this.useConfirmed || this.newStatus === SoeOriginStatus.PurchaseAccepted ? this.dateHead : undefined });
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close(result);
        }
    }
}