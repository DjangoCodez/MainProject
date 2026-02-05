import { ICompositionEditController } from "../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/ProgressHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../Core/Handlers/ValidationSummaryHandlerFactory";
import { EditControllerBase2 } from "../../Core/Controllers/EditControllerBase2";
import { IMessagingHandlerFactory } from "../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup, TermGroup_CustomerSpecificExports, TermGroup_ReportExportFileType } from "../../Util/CommonEnumerations";
import { ICoreService } from "../../Core/Services/CoreService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IAccountingService } from "../../Shared/Economy/Accounting/AccountingService";

import { saveAs } from 'file-saver';
import { HtmlUtility } from "../../Util/HtmlUtility";
import { ExportUtility } from "../../Util/ExportUtility";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { StringUtility } from "../../Util/StringUtility";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../Util/Enumerations";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Collections
    private exportTypes: any[];
    private accountYears: any = [];
    private accountYearsFromDict: any = [];
    private accountYearsToDict: any = [];
    private accountPeriodsFromDict: any = [];
    private accountPeriodsToDict: any = [];

    // Permissions
    private hasPiratPermission: boolean;
    private hasICAPermission: boolean;
    private hasSafiloPermission: boolean;

    // Selected values
    private selectedExport: any;
    private selectedCustomerNrFrom = "";
    private selectedCustomerNrTo = "";
    private selectedProductNrFrom: string;
    private selectedProductNrTo: string;
    private selectedAccountYearFrom: any;
    private selectedAccountYearTo: any;
    private selectedAccountPeriodFrom: any;
    private selectedAccountPeriodTo: any;

    // Flags
    private showProductNrSelection = false;
    private showCustomerNrSelection = false;
    private showAccountYearSelection = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        dirtyHandlerFactory: IDirtyHandlerFactory,
        private notificationService: INotificationService,
        private translationService: ITranslationService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups());
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;

        //this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Export_CustomerSpecific, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Export_CustomerSpecific_Pirat, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Export_ICABalanceExport, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Export_CustomerSpecific_Safilo, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Export_CustomerSpecific].readPermission;
        this.modifyPermission = response[Feature.Economy_Export_CustomerSpecific].modifyPermission;
        this.hasPiratPermission = response[Feature.Economy_Export_CustomerSpecific_Pirat].readPermission || response[Feature.Economy_Export_CustomerSpecific_Pirat].modifyPermission;
        this.hasICAPermission = response[Feature.Economy_Export_ICABalanceExport].readPermission || response[Feature.Economy_Export_ICABalanceExport].modifyPermission;
        this.hasSafiloPermission = response[Feature.Economy_Export_CustomerSpecific_Safilo].readPermission || response[Feature.Economy_Export_CustomerSpecific_Safilo].modifyPermission;
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadExportTypes(),
            this.loadAccountYears()]).then(() => {
                this.initialSetup();
            });
    }

    private loadExportTypes(): ng.IPromise<any> {
        this.exportTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.CustomerSpecificExports, false, false).then(x => {
            // Filter by permission
            _.forEach(x, (y) => {
                //if (y.id > 100 && y.id < 201 && this.hasPiratPermission) // Hidden for now for PIRAT
                if ((y.id === TermGroup_CustomerSpecificExports.PirateVouchers && this.hasPiratPermission) || (y.id > 200 && y.id < 301 && this.hasICAPermission) || (y.id > 300 && y.id < 3021 && this.hasSafiloPermission))
                    this.exportTypes.push(y);
            });
        });
    }

    private loadAccountYears(): ng.IPromise<any> {
        this.accountYears = [];
        return this.accountingService.getAccountYears(true, true).then((x) => {
            this.accountYears = x;
            /*this.accountYearsDict.push({ id: 0, name: " " });
            _.forEach(this.accountYears, (year) => {
                this.accountYearsDict.push({ id: year.accountYearId, name: year.yearFromTo });
            });*/
        });
    }

    private initialSetup() {
        this.selectedExport = this.exportTypes.length > 0 ? this.exportTypes[0].id : undefined;

        this.setupSelections();
    }

    private exportTypeChanged(item) {
        this.$timeout(() => {
            this.setupSelections();
        });
    }

    private setupSelections() {
        this.showProductNrSelection = false
        this.showAccountYearSelection = false;
        this.showCustomerNrSelection = false;

        this.selectedCustomerNrFrom = undefined;
        this.selectedCustomerNrTo = undefined;
        this.selectedProductNrFrom = undefined;
        this.selectedProductNrTo = undefined;
        this.selectedAccountYearFrom = undefined;
        this.selectedAccountYearTo = undefined;
        this.selectedAccountPeriodFrom = undefined;
        this.selectedAccountPeriodTo = undefined;

        switch (this.selectedExport) {
            case TermGroup_CustomerSpecificExports.PirateAveragePrice:
            case TermGroup_CustomerSpecificExports.PiratePrinted:
            case TermGroup_CustomerSpecificExports.PirateProducts:
            case TermGroup_CustomerSpecificExports.PirateRoyaltyRates:
                this.showProductNrSelection = true;
                break;
            case TermGroup_CustomerSpecificExports.PirateVouchers:
                this.showAccountYearSelection = true;
                if (soeConfig.accountYearId) {
                    const year = _.find(this.accountYears, (y) => y.accountYearId === soeConfig.accountYearId);
                    if (year)
                        this.selectedAccountYearFrom = this.selectedAccountYearTo = year;
                }
                break;
            case TermGroup_CustomerSpecificExports.ICACustomerBalance:
            case TermGroup_CustomerSpecificExports.ICACustomerBalanceMyStore:
            case TermGroup_CustomerSpecificExports.ICACustomersMyStore:
            case TermGroup_CustomerSpecificExports.SafiloCustomerRegister:
            case TermGroup_CustomerSpecificExports.SafiloOpenInvoices:
                this.showCustomerNrSelection = true;
                break;
        }
    }

    private createExportFile() {
        this.progress.startWorkProgress((completion) => {
            let fileName = "";
            let selection = {};
            switch (this.selectedExport) {
                /*case TermGroup_CustomerSpecificExports.PirateAveragePrice:
                case TermGroup_CustomerSpecificExports.PiratePrinted:
                case TermGroup_CustomerSpecificExports.PirateProducts:
                case TermGroup_CustomerSpecificExports.PirateRoyaltyRates:
                    this.showProductNrSelection = true;
                    break;*/
                case TermGroup_CustomerSpecificExports.PirateVouchers:
                    var dateFrom = this.selectedAccountPeriodFrom ? CalendarUtility.convertToDate(this.selectedAccountPeriodFrom.from) : CalendarUtility.convertToDate(this.selectedAccountYearFrom.from);
                    var dateTo = this.selectedAccountPeriodTo ? CalendarUtility.convertToDate(this.selectedAccountPeriodTo.to) : CalendarUtility.convertToDate(this.selectedAccountYearTo.to);
                    fileName = 'verifikat.csv';
                    selection['ExportFileType'] = TermGroup_ReportExportFileType.PIRATVoucher;
                    selection['DateFrom'] = dateFrom;
                    selection['DateTo'] = dateTo;
                    selection['HasDateInterval'] = true;
                    break;
                case TermGroup_CustomerSpecificExports.ICACustomerBalance:
                case TermGroup_CustomerSpecificExports.ICACustomerBalanceMyStore:
                    fileName = 'kund.dat';
                    selection['ExportFileType'] = this.selectedExport === TermGroup_CustomerSpecificExports.ICACustomerBalance ? TermGroup_ReportExportFileType.ICACustomerBalance : TermGroup_ReportExportFileType.ICACustomerBalanceMyStore;
                    selection['SL_ActorNrFrom'] = this.selectedCustomerNrFrom ? StringUtility.nullToEmpty(this.selectedCustomerNrFrom.toLowerCase()) : "";
                    selection['SL_ActorNrTo'] = this.selectedCustomerNrTo ? StringUtility.nullToEmpty(this.selectedCustomerNrTo.toLowerCase()) : "";
                    selection["SL_HasActorNrInterval"] = !StringUtility.isEmpty(selection['SL_ActorNrFrom']) || !StringUtility.isEmpty(selection['SL_ActorNrTo']);
                    break;
                case TermGroup_CustomerSpecificExports.ICACustomersMyStore:
                    selection['ExportFileType'] = TermGroup_ReportExportFileType.ICACustomerMyStore;
                    selection['SL_ActorNrFrom'] = this.selectedCustomerNrFrom ? StringUtility.nullToEmpty(this.selectedCustomerNrFrom.toLowerCase()) : "";
                    selection['SL_ActorNrTo'] = this.selectedCustomerNrTo ? StringUtility.nullToEmpty(this.selectedCustomerNrTo.toLowerCase()) : "";
                    selection["SL_HasActorNrInterval"] = !StringUtility.isEmpty(selection['SL_ActorNrFrom']) || !StringUtility.isEmpty(selection['SL_ActorNrTo']);
                    break;
                case TermGroup_CustomerSpecificExports.SafiloCustomerRegister:
                    selection['ExportFileType'] = TermGroup_ReportExportFileType.SafiloCustomers;
                    selection['SL_ActorNrFrom'] = this.selectedCustomerNrFrom ? StringUtility.nullToEmpty(this.selectedCustomerNrFrom.toLowerCase()) : "";
                    selection['SL_ActorNrTo'] = this.selectedCustomerNrTo ? StringUtility.nullToEmpty(this.selectedCustomerNrTo.toLowerCase()) : "";
                    selection["SL_HasActorNrInterval"] = !StringUtility.isEmpty(selection['SL_ActorNrFrom']) || !StringUtility.isEmpty(selection['SL_ActorNrTo']);
                    break;
                case TermGroup_CustomerSpecificExports.SafiloOpenInvoices:
                    selection['ExportFileType'] = TermGroup_ReportExportFileType.SafiloInvoices;
                    selection['SL_ActorNrFrom'] = this.selectedCustomerNrFrom ? StringUtility.nullToEmpty(this.selectedCustomerNrFrom.toLowerCase()) : "";
                    selection['SL_ActorNrTo'] = this.selectedCustomerNrTo ? StringUtility.nullToEmpty(this.selectedCustomerNrTo.toLowerCase()) : "";
                    selection["SL_HasActorNrInterval"] = !StringUtility.isEmpty(selection['SL_ActorNrFrom']) || !StringUtility.isEmpty(selection['SL_ActorNrTo']);
                    break;
            }

            return this.coreService.createCustomerSpecificExport(selection).then((result) => {
                if (result.success) {
                    if (!StringUtility.isEmpty(result.stringValue)) {
                        if (this.selectedExport === TermGroup_CustomerSpecificExports.ICACustomersMyStore || this.selectedExport === TermGroup_CustomerSpecificExports.SafiloCustomerRegister || this.selectedExport === TermGroup_CustomerSpecificExports.SafiloOpenInvoices)
                            HtmlUtility.openInSameTab(this.$window, result.stringValue);
                        else if (this.selectedExport === TermGroup_CustomerSpecificExports.ICACustomerBalance || this.selectedExport === TermGroup_CustomerSpecificExports.ICACustomerBalanceMyStore)
                            HtmlUtility.openInSameTab(this.$window, `/ajax/downloadTextFile.aspx?table=icafile&id=0&guid=${result.stringValue}&cid=${soeConfig.actorCompanyId}`);
                        else
                            ExportUtility.Export(result.stringValue, fileName);
                    }
                    else {
                        const keys: string[] = [
                            "core.info",
                            "core.noresultfromselection"
                        ];

                        this.translationService.translateMany(keys).then((terms) => {
                            this.notificationService.showDialog(terms["core.info"], terms["core.noresultfromselection"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                        });
                    }
                }
                completion.completed("", true)
            });
        });
    }
}