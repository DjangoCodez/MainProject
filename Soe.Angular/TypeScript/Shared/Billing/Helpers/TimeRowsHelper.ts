import { ProjectTimeBlockDTO } from "../../../Common/Models/ProjectDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IOrderService } from "../Orders/OrderService";
import { Feature } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SelectProductRowController } from "../Dialogs/SelectProductRow/SelectProductRowController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ShowTimeRowsController } from "../Dialogs/ShowTimeRows/ShowTimeRowsController";
import { ProductRowDTO } from "../../../Common/Models/InvoiceDTO";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Guid } from "../../../Util/StringUtility";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";

export class TimeRowsHelper {

    public timeProjectRendered = false;
    private loadingTimeProjectRows = false;
    public timeProjectPermission = false;

    public timeProjectFrom: Date;
    public timeProjectTo: Date;
    public projectTimeBlockRows: ProjectTimeBlockDTO[] = [];
    public reloadInvoiceAfterClose = false;
    private progress: IProgressHandler;
    //@ngInject
    constructor(
        private parentGuid: Guid, 
        private $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,        
        private orderService: IOrderService,
        private coreService: ICoreService,
        private invoiceId: number,
        private customerInvoiceRowId: number) {

        this.setupProgress();
        this.setupListeners();
    }

    private setupProgress() {
        const progressHandler = new ProgressHandlerFactory(this.$uibModal, this.translationService, this.$q, this.messagingService, this.urlHelperService, null);
        this.progress = progressHandler.create();
    }
    private setupListeners() {
        this.messagingService.subscribe(Constants.EVENT_RELOAD_INVOICE, (x) => {
            // Make sure event does not come from any other orders time project rows
            if (x.guid === this.parentGuid) {
                this.reloadInvoiceAfterClose = true;
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SEARCH_TIME_PROJECT_ROWS, (x) => {
            // Make sure event does not come from any other orders product rows
            if (x.guid === this.parentGuid)
                this.loadTimeProjectRows(x.GetIntervall);
        }, this.$scope);
    }

    public loadPermissions(): ng.IPromise<any> {
        const features: number[] = [];
        features.push(Feature.Time_Project_Invoice_Edit);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.timeProjectPermission = x[Feature.Time_Project_Invoice_Edit];
        });
    }
        
    public loadTimeProjectRows(getIntervall = true, customerInvoiceRowId: number = 0,) {
        if (!this.timeProjectRendered) {
            this.timeProjectRendered = true;
            const date = CalendarUtility.getDateToday();

            this.timeProjectFrom = date.beginningOfMonth();
            this.timeProjectTo = date.endOfMonth();
        }

        let tempProjectFrom: Date;
        let tempProjectTo: Date;
        if (customerInvoiceRowId) {
            this.customerInvoiceRowId = customerInvoiceRowId;
        }

        if (getIntervall) {
            tempProjectFrom = this.timeProjectFrom;
            tempProjectTo = this.timeProjectTo;
        }

        if (this.invoiceId && this.customerInvoiceRowId) {
            this.progress.startLoadingProgress([() => {
                return this.orderService.getProjectTimeBlocksForInvoiceRow(this.invoiceId, this.customerInvoiceRowId, tempProjectFrom, tempProjectTo).then((rows) => {
                    this.projectTimeBlockRows = rows.map(dto => {
                        const obj = new ProjectTimeBlockDTO();
                        angular.extend(obj, dto);
                        obj.date = CalendarUtility.convertToDate(obj.date);
                        if (obj.startTime)
                            obj.startTime = CalendarUtility.convertToDate(obj.startTime);
                        if (obj.stopTime)
                            obj.stopTime = CalendarUtility.convertToDate(obj.stopTime);
                        return obj;
                    });

                    if (!getIntervall) {
                        this.setMinMaxDates(this.projectTimeBlockRows);
                    }

                })
            }]);
        }
    }

    private setMinMaxDates(projectTimeBlockRows: ProjectTimeBlockDTO[]) {
        const dates = projectTimeBlockRows.map(d => d.date);
        if (dates.length > 0) {
            this.timeProjectTo = new Date(Math.max.apply(null, dates));
            this.timeProjectFrom = new Date(Math.min.apply(null, dates));
        }
    }

    public selectProductRow(productId:number,productRows: ProductRowDTO[]): ng.IPromise<number> {
        const deferral = this.$q.defer<number>();

        this.translationService.translate("billing.productrows.copyrows.choosetargetrow").then((titel) => {

            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/SelectProductRow/SelectProductRow.html"),
                controller: SelectProductRowController,
                controllerAs: "ctrl",
                size: 'lg',
                resolve: {
                    productRows: () => {
                        return productRows.filter(x => (x.isTimeBillingRow || x.isTimeProjectRow ) && x.productId === productId && x.customerInvoiceRowId !== this.customerInvoiceRowId)
                    },
                    toolbarTitle: () => { return titel }
                }
            }
            const modal = this.$uibModal.open(options);
            modal.result.then((result: any) => {
                deferral.resolve(result);
                // OK
            }, (result: any) => {
                deferral.resolve(0);
            });
        });
        return deferral.promise;
    }

    public showTimeRows(selectedRow: ProductRowDTO, productRows: ProductRowDTO[], isReadOnly:boolean): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ShowTimeRows/ShowTimeRows.html"),
            controller: ShowTimeRowsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                urlHelperService: () => { return this.urlHelperService },
                coreService: () => { return this.coreService },
                customerInvoiceRowId: () => { return this.customerInvoiceRowId },
                invoiceId: () => { return this.invoiceId },
                productId: () => { return selectedRow.productId },
                productRows: () => { return productRows },
                isReadOnly: () => { return isReadOnly },
            }
        }

        const modal = this.$uibModal.open(options);

        modal.result.then((result: any) => {
            if (result === true) {
                this.reloadInvoiceAfterClose = true;
            }
            deferral.resolve(true);
        }, function () {
            // Cancelled
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    public showTimeRowsAlternate(productId: number, isReadOnly:boolean, productRows: ProductRowDTO[]): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ShowTimeRows/ShowTimeRows.html"),
            controller: ShowTimeRowsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                urlHelperService: () => { return this.urlHelperService },
                coreService: () => { return this.coreService },
                customerInvoiceRowId: () => { return this.customerInvoiceRowId },
                invoiceId: () => { return this.invoiceId },
                productId: () => { return productId },
                productRows: () => { return productRows },
                isReadOnly: () => { return isReadOnly },
            }
        }

        const modal = this.$uibModal.open(options);

        modal.result.then((result: any) => {
            if (result === true) {
                this.reloadInvoiceAfterClose = true;
            }
            deferral.resolve(true);
        }, function () {
            // Cancelled
            deferral.resolve(false);
        });

        return deferral.promise;
    }
}