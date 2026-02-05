import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CustomerIODTO } from "../../../Common/Models/CustomerIODTO";
import { CustomerInvoiceIODTO } from "../../../Common/Models/CustomerInvoiceIODTO";
import { CustomerInvoiceRowIODTO } from "../../../Common/Models/CustomerInvoiceRowIODTO";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { IImportService } from "../ImportService";
import { Constants } from "../../../Util/Constants";
import { SoeEntityType, Feature } from "../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {
    showUpload: boolean = false;
    // FileName
    fileName: any;
    // File
    file: any;
    // Url
    fileUrl: string = Constants.WEBAPI_CORE_FILES_UPLOAD_INVOICE + SoeEntityType.None;
    // Datastorage
    dataStorageId: number = 0;
    // Response from import
    response: any[];
    //IOs
    customers: CustomerIODTO[] = [];
    invoices: CustomerInvoiceIODTO[] = [];
    invoiceRows: CustomerInvoiceRowIODTO[] = [];
    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Lookups
    //@ngInject
    constructor(
        $uibModal,
        coreService: ICoreService,
        private importService: IImportService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService) {
        super("Soe.Economy.Import.Automaster.Edit", Feature.Economy_Import_Invoices_Automaster, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.setupToolBar();
    }

    // SETUP

    protected setupLookups() {

    }

    // Toolbar
    private setupToolBar() {
        this.setupDefaultToolBar();
        this.stopProgress();
    }

    // LOOKUPS

    private load() {
        this.setupToolBar();
    }

    private startImportFile() {
        this.response = [];

    }

    protected delete() {

    }

    private new() {

    }

    // Help Methods


    // EVENTS
    public fileUploadedCallback(result) {
        this.file = result;

    }


    public fileUploaded(result) {
        this.dataStorageId = result.id;
        this.fileName = result.stringValue;
        this.showUpload = true;
    }

    protected lookupLoaded() {

    }

    // VALIDATION

    protected validate() {

    }
}