import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IImportService } from "../ImportService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup, TermGroup_IOImportHeadType, TermGroup_SysImportDefinitionType, SoeEntityType, SoeEntityImageType } from "../../../Util/CommonEnumerations";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { Constants } from "../../../Util/Constants";
import { ImportRowsDirectiveController } from "../../../Common/Connect/Directives/ImportRowsDirective";
import { ImportDTO } from "../../../Common/Models/ImportDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    recordId: number;    
    importId: number;
    import: ImportDTO; 
    importHeadType: number;
    batchId: string;    
    terms: any;

    private module: number;       
    private showImportRows: boolean = false;

    // File upload
    entity: SoeEntityType = SoeEntityType.None;
    type: SoeEntityImageType = SoeEntityImageType.Unknown;
    public files: any[] = [];
    private nbrOfFiles: any;
    private filesLoaded: boolean = false;
    private loadingFiles: boolean = false;
    fileUrl: string = Constants.WEBAPI_CORE_FILES_UPLOAD_INVOICE + SoeEntityType.None;

    private uploadedFilesGridOptions: ISoeGridOptionsAg;    

    private importRowsDirective: ImportRowsDirectiveController;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,        
        urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private importService: IImportService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));        
        
    }

    public onInit(parameters: any) {
        
        this.batchId = parameters.batchId;
        this.importHeadType = parameters.importHeadType;
        this.importId = 0;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: parameters.feature, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([            
            () => this.loadTerms(),
        ]);        
    }


    private onLoadData(): ng.IPromise<any> {

        //this.importRowsDirective.batchId = this.batchId;

        //GetCustomerIOResult
        //GetCustomerInvoiceHeadIOResult
        //GetCustomerInvoiceRowIOResult
        //GetSupplierIOResult
        //GetSupplierInvoiceHeadIOResult
        //GetVoucherHeadIOResult
        //GetProjectIOResult
        return null;

        //return this.importService.getImportIOResult(this.importHeadType, this.batchId).then((x) => {                
        //        this.import = x;           
                
                                      
         //  })
                
    }    

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.fileupload.choosefiletoimport",            
            "core.error",
            "core.info",
            "common.connect.fileuploadnotsuccess",            
            "common.connect.importsuccess",
            "common.connect.importnotsuccess",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });

    }  
    

    //EVENTS             
    

    // HELP-METHODS
    

    // VALIDATION
    
    
}
