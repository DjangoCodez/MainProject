import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IEmployeeService } from "../../EmployeeService";
import { Constants } from "../../../../Util/Constants";
import { SoeEntityType, Feature } from "../../../../Util/CommonEnumerations";


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

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Lookups
    constructor(
        $uibModal,
        coreService: ICoreService,
        private employeeService: IEmployeeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService) {
        super("Time.Employee.Csr.Import.Edit", Feature.Time_Employee_Csr_Import, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

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
        this.employeeService.getCsrResultFromDataStorage(this.dataStorageId).then((x) => {
            this.response = x;
            if (this.response.length === 0) {
                var keys: string[] = [
                    "time.employee.csr.noposts"
                ];
                this.translationService.translateMany(keys).then((terms) => {
                    this.response.push(terms["time.employee.csr.noposts"]);
                });
            }
        });
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