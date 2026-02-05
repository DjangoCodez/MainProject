import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeSmallDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ContactAddressDTO } from "../../../Common/Models/ContactAddressDTOs";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IReportService } from "../../../Core/Services/ReportService";
import { FlaggedEnum } from "../../../Util/EnumerationsUtility";
import { CustomerDTO } from "../../../Common/Models/CustomerDTO";
import { TextBlockDialogController } from "../../../Common/Dialogs/textblock/textblockdialogcontroller";
import { ProductRowsContainers, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { TermGroup_InvoiceVatType, SoeStatusIcon, TermGroup, TermGroup_SysContactAddressType, CompanySettingType, SoeEntityType, TextBlockType, SimpleTextEditorDialogMode, TermGroup_Languages, OrderInvoiceRegistrationType, TermGroup_SysContactAddressRowType } from "../../../Util/CommonEnumerations";
import { ShowContactAddressInfoController } from "../../../Common/Dialogs/ShowContactAddressInfo/ShowContactAddressInfoController";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { Guid } from "../../../Util/StringUtility";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";

export class InvoiceEditHandler {

    private terms: any;

    public vatTypes: ISmallGenericType[];
    public filteredVatTypes: ISmallGenericType[]; 
    public defaultVatType: TermGroup_InvoiceVatType = TermGroup_InvoiceVatType.Merchandise;

    public wholesellers: ISmallGenericType[];

    public deliveryTypes: ISmallGenericType[];
    public paymentConditions: any[];
    public defaultPaymentConditionId = 0;
    public defaultPaymentConditionDays = 0;
    public defaultPaymentConditionStartOfNextMonth = false;
    public deliveryConditions: ISmallGenericType[];
    public currencies: any[];
    public customerGLNs: ISmallGenericType[];
    public ediTransferModes: ISmallGenericType[];

    public deliveryAddresses: ContactAddressDTO[];
    public invoiceAddresses: ContactAddressDTO[];

    public defaultVoucherSeriesId = 0;

    //@ngInject
    constructor(private parentEdit: any,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private reportService: IReportService,
        private $uibModal,
        private progressHandler: IProgressHandler,
        private messagingService: IMessagingService,
        private parentGuid: Guid
    ) {
        this.loadTerms();
    }

    private loadTerms(): ng.IPromise<any> {

        const keys: string[] = [
            "core.comment",
            "billing.order.origindescription",
            "common.customer.customer.customerblocked",
            "billing.order.transfer.customerblocked"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public containsAttachments(statusIcon: SoeStatusIcon): boolean {
        const flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
        const statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(statusIcon);
        if (statusIcons.contains(SoeStatusIcon.Attachment) || statusIcons.contains(SoeStatusIcon.Image)) {
            return true;
        }
        else {
            return false;
        }
    }

    public loadEmployeeId(callback: (employeeId:number) => void): ng.IPromise<any> {
        return this.coreService.getEmployeeForUser().then((x: IEmployeeSmallDTO) => {
            callback( x ? x.employeeId : 0 );
        });
    }

    public loadWholesellers(): ng.IPromise<any> {
        return this.commonCustomerService.getSysWholesellersDict(true).then(x => {
            this.wholesellers = x;
        });
    }

    public loadPaymentConditions(): ng.IPromise<any> {
        return this.commonCustomerService.getPaymentConditions().then( conditions => {
            this.paymentConditions = conditions;

            // Get default number of days (or use 30 if not specified)
            const def = this.paymentConditions.find(x => x.paymentConditionId === this.defaultPaymentConditionId);
            this.defaultPaymentConditionDays = def ? def.days : 30;
            this.defaultPaymentConditionStartOfNextMonth = def && def.startOfNextMonth;
        });
    }

    public loadVatTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceVatType, false, false).then(x => {
            this.vatTypes = x;
            this.filteredVatTypes = _.filter(x, (y) => y.id !== 5 && y.id != 6);
            // Set default vat type (if not set in company setting, use Merchandise)
            if (this.defaultVatType === TermGroup_InvoiceVatType.None && this.vatTypes.length > 0) {
                if (_.find(this.vatTypes, v => v.id === TermGroup_InvoiceVatType.Merchandise))
                    this.defaultVatType = TermGroup_InvoiceVatType.Merchandise;
                else
                    this.defaultVatType = this.vatTypes[0].id;
            }
        });
    }

    public addMissingVatType(vatType: TermGroup_InvoiceVatType) {
        this.filteredVatTypes.push(_.find(this.vatTypes, { 'id': vatType }));
    }

    public loadDeliveryTypes(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryTypesDict(true).then(x => {
            this.deliveryTypes = x;
        });
    }

    public loadDeliveryConditions(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryConditionsDict(true).then(x => {
            this.deliveryConditions = x;
        });
    }

    public loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    public loadCustomerGLNs(customer: CustomerDTO) {
        if (!customer) {
            this.customerGLNs = null;
        } else {
            this.commonCustomerService.getCustomerGLNs(customer.actorCustomerId, true).then(x => {
                this.customerGLNs = x;
            });
        }
    }

    public loadDeliveryAddresses(customerId: number): ng.IPromise<any> {
        return this.commonCustomerService.getContactAddresses(customerId, TermGroup_SysContactAddressType.Delivery, true, true, true).then((x: ContactAddressDTO[]) => {
            this.deliveryAddresses = x;
        });
    }

    public loadInvoiceAddresses(customerId: number): ng.IPromise<any> {
        return this.commonCustomerService.getContactAddresses(customerId, TermGroup_SysContactAddressType.Billing, true, false, true).then((x: ContactAddressDTO[]) => {
            this.invoiceAddresses = x;
        });
    }

    public loadDefaultVoucherSeriesId(accountYearId: number) {
        return this.commonCustomerService.getDefaultVoucherSeriesId(accountYearId, CompanySettingType.CustomerInvoiceVoucherSeriesType).then((x) => {
            this.defaultVoucherSeriesId = x;
        });
    }

    public loadEdiTransferModes() {
        return this.coreService.getTermGroupContent(TermGroup.OrderEdiTransferMode, true, false).then(x => {
            this.ediTransferModes = x;
        });
    }

    //dialogs...
    public editOriginDescription(): any {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
            controller: TextBlockDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            backdrop: 'static',
            resolve: {
                text: () => { return this.parentEdit.invoice.originDescription },
                editPermission: () => { return this.parentEdit.modifyPermission },
                entity: () => { return SoeEntityType.CustomerInvoice },
                type: () => { return TextBlockType.TextBlockEntity },
                headline: () => { return this.terms["billing.order.origindescription"] },
                mode: () => { return SimpleTextEditorDialogMode.EditInvoiceDescription },
                container: () => { return ProductRowsContainers.Order },
                langId: () => { return TermGroup_Languages.Swedish },
                maxTextLength: () => { return null },
                textboxTitle: () => { return undefined },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.parentEdit.invoice.originDescription = result.text;
                this.parentEdit.setAsDirty();
            }
        });
    }

    public showContactInfo(contactPersonId:number): any {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowContactAddressInfo/ShowContactAddressInfo.html"),
            controller: ShowContactAddressInfoController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                contactPersonId: () => { return contactPersonId },
                commonCustomerService: () => { return this.commonCustomerService },
                coreService: () => { return this.coreService }
            }
        }
        this.$uibModal.open(options);
    }

    public showCustomerBlockNote(customer: CustomerDTO, registrationType: OrderInvoiceRegistrationType) {
        let msg = undefined;
        if (customer.blockOrder)
            msg = this.terms["common.customer.customer.customerblocked"] + "\n";
        else if (customer.blockInvoice && registrationType == OrderInvoiceRegistrationType.Invoice)
            msg = this.terms["billing.order.transfer.customerblocked"] + "\n";

        if (customer.blockNote && msg)
            msg += this.terms["core.comment"] + ": " + customer.blockNote + "\n";

        if (msg) {
            this.showBlockNote(msg);
        }
    }

    public showBlockNote(note: string) {
        this.translationService.translate("common.note").then((title) => {
            this.notificationService.showDialog(title, note, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });
    }

    public showCustomerNote(customer: any) {
        if (customer && customer.note) {
            this.translationService.translate("common.note").then((title) => {
                this.notificationService.showDialog(title, customer.note, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            });
        }
    }

    public sendReport(invoiceParams: any, registrationType: OrderInvoiceRegistrationType, email: boolean, reportId: number, languageId: number, copy: boolean, recipients: any[] = null, emailTemplate: number = 0, mergePdfs:boolean = false) {
        const keys: string[] = [
            "common.sent",
            "common.sending"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            this.progressHandler.startWorkProgress((completion) => {
                this.reportService.sendReport(invoiceParams.invoiceId, recipients, reportId, languageId, invoiceParams.invoiceNr,
                    invoiceParams.actorCustomerId, invoiceParams.printTimeReport, invoiceParams.includeOnlyInvoicedTime, registrationType, copy, emailTemplate, invoiceParams.addAttachmentsToEinvoice, invoiceParams.attachmentIds, invoiceParams.checklistIds, mergePdfs, invoiceParams.singleRecipient).then((x) => {
                        if (x.success) {
                            this.messagingService.publish(Constants.EVENT_RELOAD_ORDER_IMAGES, { guid: this.parentGuid });
                            completion.completed(null, false, terms["common.sent"]);
                        }
                        else {
                            completion.failed(x.errorMessage);
                        }
                    })
            })

        });
    }


    public formatDeliveryAddress(addressRows: any[], isFinInvoiceCustomer: boolean): string {

        var strAddress: string = "";
        var tmpName: string = "";
        var tmpCOAddress: string = "";
        var tmpStreetAddress: string = "";
        var tmpPostalCode: string = "";
        var tmpPostalAddress: string = "";
        var tmpCountry: string = "";

        _.forEach(addressRows, (row) => {
            switch (row.sysContactAddressRowTypeId) {
                case TermGroup_SysContactAddressRowType.Name:
                    tmpName += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.AddressCO:
                    tmpCOAddress += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.StreetAddress:
                    tmpStreetAddress += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.Address:
                    tmpStreetAddress += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.PostalCode:
                    tmpPostalCode += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.PostalAddress:
                    tmpPostalAddress += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.Country:
                    tmpCountry += row.text;
                    break;
            }
        });

        strAddress = tmpName;

        if (tmpCOAddress !== "" && tmpCOAddress !== " ")
            strAddress += '\r' + tmpCOAddress;

        if (strAddress == "" || strAddress == " ")
            strAddress = tmpStreetAddress;
        else
            strAddress += '\r' + tmpStreetAddress;

        strAddress += '\r' + tmpPostalCode;

        if (isFinInvoiceCustomer) //4 lines needed for finvoice
            strAddress += '\r' + tmpPostalAddress;
        else
            strAddress += ' ' + tmpPostalAddress;

        if (tmpCountry !== "") {
            strAddress += '\r' + tmpCountry;
        }

        return strAddress;
    }
}