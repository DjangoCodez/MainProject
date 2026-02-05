import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";

export class updatePriceListsController  {
    private priceLists: any[] = [];    
    private title: string;
    private info: string;
    private upgrading: boolean = false;
   
    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private invoiceService: IInvoiceService,        
        private sysPriceListHeadId: number,
        private sysWholesellerId: number,
    ) {
       
        this.setupLabels();             
        this.loadPriceLists();        
    }

    private setupLabels() {
        var keys: string[] = [          
            "billing.invoice.wholesellerpricelist.upgrade.one.title",
            "billing.invoice.wholesellerpricelist.upgrade.many.title",
            "billing.invoice.wholesellerpricelist.upgrade.info",
            "billing.invoice.wholesellerpricelist.upgrade.many.info",
            "billing.invoice.wholesellerpricelist.upgrade.many.info2",
            "billing.invoice.wholesellerpricelist.upgrade.many.info3",            
        ];
        
        this.translationService.translateMany(keys).then((terms) => {
            if (this.sysPriceListHeadId > 0 && this.sysWholesellerId > 0) {
                this.title = terms["billing.invoice.wholesellerpricelist.upgrade.one.title"];
                this.info = terms["billing.invoice.wholesellerpricelist.upgrade.info"];
            }
            else {            
                this.title = terms["billing.invoice.wholesellerpricelist.upgrade.many.title"];
                
                this.info = terms["billing.invoice.wholesellerpricelist.upgrade.many.info"] + '\n' +
                    terms["billing.invoice.wholesellerpricelist.upgrade.many.info2"] + '\n' +
                    terms["billing.invoice.wholesellerpricelist.upgrade.info"] + '\n' +
                    terms["billing.invoice.wholesellerpricelist.upgrade.many.info3"];
            }
            
        });
    }

    private loadPriceLists(): ng.IPromise<any> {
        return this.invoiceService.getPriceListsToUpdate().then(x => {            
            _.forEach(x, (y) => {
                if (this.sysPriceListHeadId > 0 && this.sysWholesellerId > 0) {
                    if (y.sysWholesellerId == this.sysWholesellerId)
                        y.isSelected = true;
                    else
                        y.isSelected = false;
                }
                else {
                    y.isSelected = true;
                }
            })
            
            this.priceLists = x;            
        });
    }
    
   
    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonOkClick() {

        this.upgrading = true;

        var selectedPriceLists = _.filter(this.priceLists, (y) => y.isSelected == true);
        var wholesellerIds: any[] = [];        

        _.forEach(selectedPriceLists, (priceList) => {
            wholesellerIds.push(priceList.sysWholesellerId);        
        });

        this.invoiceService.upgradeCompanyWholesellerPriceLists(wholesellerIds).then((result) => {

            var keys: string[] = [
                "billing.invoice.wholesellerpricelist.upgrade.success",
                "billing.invoice.wholesellerpricelist.upgrade.notsuccess"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                var message: string;

                message = result.integerValue.toString() + " " + terms["billing.invoice.wholesellerpricelist.upgrade.success"];
                if (result.integerValue2 > 0)
                    message += "\n" + result.integerValue2.toString() + " " + terms["billing.invoice.wholesellerpricelist.upgrade.notsuccess"];
                
                this.notificationService.showDialog(terms["core.information"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                this.upgrading = false;
                this.$uibModalInstance.close({ result: true });
            });

        })        
    }
}