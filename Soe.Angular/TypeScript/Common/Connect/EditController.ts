import { EditControllerBase2 } from "../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { ICoreService } from "../../Core/Services/CoreService";
import { IConnectService } from "./ConnectService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { IAccountingService } from "../../Shared/Economy/Accounting/AccountingService";
import { ICompositionEditController } from "../../Core/ICompositionEditController";
import { CoreUtility } from "../../Util/CoreUtility";
import { IProgressHandlerFactory } from "../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { TermGroup, TermGroup_IOImportHeadType, SoeEntityType, SoeEntityImageType } from "../../Util/CommonEnumerations";
import { SOEMessageBoxButton, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../Util/Enumerations";
import { Constants } from "../../Util/Constants";
import { ImportDTO } from "../../Common/Models/ImportDTO";
import { ImportRowsDirectiveController } from "./Directives/ImportRowsDirective";
import { FilesLookupDTO } from "../Models/FilesLookupDTO";
import { ImportFileDTO } from '../Models/ImportFileDTO';
import { AccountSmallDTO } from "../Models/AccountDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    importId: number;
    import: ImportDTO;    
    importHeadType: number;
    batchId: string;    
    terms: any;

    private module: number;   
    private importDefinitions: any[];
    private definitionTypes: any[];
    private sysImportHeads: any[];
    private accountYears: any[];
    private accountStds: AccountSmallDTO[];
    private voucherSeries: any[];      
    private isCustomerInvoiceRow: boolean = false;
    private isCustomerInvoice: boolean = false;
    private isVoucher: boolean = false;
    private showAccountingYear: boolean = false;
    private fileUploadInitiallyOpen: boolean = true;
    private showImportRows: boolean = false;
    private $q: ng.IQService;

    // Properties
    get accountYearId() {
        return this.import ? this.import.accountYearId : 0;
    }
    set accountYearId(item: number) {
        if (this.import)
            this.import.accountYearId = item;
    }
    get voucherSeriesId() {
        return this.import ? this.import.voucherSeriesId : 0;
    }
    set voucherSeriesId(item: number) {
        if (this.import)
            this.import.voucherSeriesId = item;
    }

    private _account: AccountSmallDTO
    set account(item: AccountSmallDTO) {
        this._account = item;
        if (this._account?.accountId > 0)
            this.import.dim1AccountId = this._account.accountId;
        else
            this.import.dim1AccountId = undefined;
    }
    get account(): AccountSmallDTO {
        return this._account;
    }

    // File upload
    public files: any[] = [];
    private nbrOfFiles: any;
    private filesLoaded: boolean = false;
    private loadingFiles: boolean = false;
    private importRowsDirective: ImportRowsDirectiveController;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private accountingService: IAccountingService,
        urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private connectService: IConnectService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        $q: ng.IQService
      ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));       
        this.$q = $q;
    }

    public onInit(parameters: any) {
        this.importId = parameters.id;                
        this.guid = parameters.guid;
        this.module = parameters.module;
        this.files = parameters.files;
        
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: parameters.feature, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = true;
        this.modifyPermission = true;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadSysImportDefinitions(),
            () => this.loadDefinitionTypes(),         
            () => this.loadSysImportHeads(),
            () => this.loadAccountYears(),
            () => this.loadTerms(),
            () => this.loadAccountStd()
        ]);        
    }


    private onLoadData(): ng.IPromise<any> {
        if (this.importId > 0) {
            return this.connectService.getImport(this.importId).then((x) => {
                this.import = x;
                if (this.import.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice)
                    this.isCustomerInvoice = true;
                else if (this.import.importHeadType == TermGroup_IOImportHeadType.CustomerInvoiceRow)
                    this.isCustomerInvoiceRow = true;
                else if (this.import.importHeadType == TermGroup_IOImportHeadType.Voucher)
                    this.isVoucher = true;
                else if (this.import.importHeadType == TermGroup_IOImportHeadType.Budget && this.import.specialFunctionality === "ICABudget") {
                    this.showAccountingYear = true;
                    if (!this.import.accountYearId) 
                        this.import.accountYearId = soeConfig.accountYearId;
                }

                if (this.import.accountYearId && this.import.accountYearId > 0)
                    this.accountYearChanged(this.import.accountYearId);


                if (this.import.dim1AccountId && this.import.dim1AccountId > 0) {
                    this.account = this.accountStds.find(x => x.accountId == this.import.dim1AccountId);
                }

                this.isNew = false;
            })
        }
        else {
            this.new();
        }
    }

    private loadSysImportDefinitions(): ng.IPromise<any> {
        return this.connectService.getSysImportDefinitions(this.module).then((x) => {
            this.importDefinitions = x;              
        })
    }

    private loadSysImportHeads(): ng.IPromise<any> {
        return this.connectService.getSysImportHeads().then((x) => {
            this.sysImportHeads = x;
        })
    }

    private loadDefinitionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysImportDefinitionType, false, false).then(x => {
            this.definitionTypes = x;
        });
    }

    private loadAccountYears(): ng.IPromise<any> {
        return this.accountingService.getAccountYearDict(false).then((x) => {
            this.accountYears = x;            
        });
    }

    private loadAccountStd() {
        return this.accountingService.getAccountStdsNumberName(true).then((x) => {
            this.accountStds = x;
        })
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.connect.imports",
            "core.fileupload.choosefiletoimport",            
            "core.error",
            "core.info",
            "core.warning",
            "common.connect.fileuploadnotsuccess",            
            "common.connect.importsuccess",
            "common.connect.importnotsuccess",
            "common.connect.fileallreadyimported",
            "common.connect.filesExistWarning",
            "common.connect.fileExistsWarning",
            "common.connect.importrows"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });

    }

    private accountYearChanged(accountYearId: number) {
        return this.accountingService.getVoucherSeriesByYear(accountYearId, false, true).then((x) => {
            this.voucherSeries = [
                {
                    voucherSeriesId: undefined,
                    voucherSeriesTypeName: ""
                },
                ...x
            ];            
        });
    }   

    //EVENTS
    private voucherSeriesChanged() {
        //this.dirtyHandler.setDirty();
    }
    private accountDimsChanged() {
        //this.dirtyHandler.setDirty();
    }

    private importDefinitionChanged(item: any) {
                
        var importDefinition = _.find(this.importDefinitions, i => i.sysImportDefinitionId == item);
        var sysImportHead = _.find(this.sysImportHeads, i => i.sysImportHeadId == importDefinition.sysImportHeadId);

        if (sysImportHead.sysImportHeadTypeId == TermGroup_IOImportHeadType.CustomerInvoice)
            this.isCustomerInvoice = true;
        else
            this.isCustomerInvoice = false;
        if (sysImportHead.sysImportHeadTypeId == TermGroup_IOImportHeadType.CustomerInvoiceRow)
            this.isCustomerInvoiceRow = true;
        else
            this.isCustomerInvoiceRow = false;
        if (sysImportHead.sysImportHeadTypeId == TermGroup_IOImportHeadType.Voucher) {
            this.import.accountYearId = soeConfig.accountYearId;
            this.accountYearChanged(this.import.accountYearId);
            this.isVoucher = true;
        }
        else {
            if (this.showAccountingYear) 
                this.import.accountYearId = soeConfig.accountYearId;
            else
                this.import.accountYearId = undefined;

            this.import.voucherSeriesId = undefined;
            this.isVoucher = false;
        }

        var definitionType = _.find(this.definitionTypes, { id: importDefinition.type});
        
        this.import.typeText = definitionType.name;
        this.import.type = definitionType.id;
        this.import.importDefinitionId = importDefinition.sysImportDefinitionId;        

        //this.dirtyHandler.setDirty();
  }

    private uploadFiles() {
        const url = `${CoreUtility.apiPrefix}${Constants.WEBAPI_CORE_FILES_UPLOAD}${SoeEntityType.XEConnectImport}/0/${this.import.importId}?extractZip=true`;
        this.notificationService.showFileUpload(
            url,
            this.terms["core.fileupload.choosefiletoimport"], //title
            true, //show drop zone
            true, //show queue
            true, //allow multiple files
            true  //noMaxSize
        ).result.then(res => {
            this.files = res.result;
            return this.handleDuplicateFiles();
        }).then(_ => {
            if (this.files.length > 0)
                this.importFiles();
        })
        .catch(error => this.handleError(error));
    }

    // Checks for duplicate files and prompts user if duplicates are found
    private handleDuplicateFiles(): ng.IPromise<string[]> {
        if(this.files.length == 0)
            return this.$q.resolve([]);

        const obj = new FilesLookupDTO(SoeEntityType.XEConnectImport, this.files.map(file => new ImportFileDTO(file.integerValue2, file.stringValue)));
        return this.coreService.checkFilesDuplicate(obj).then(duplicateFiles => {
            if (duplicateFiles.length === 0)
                return this.$q.resolve([]);

            const lineBreak = "\n";
            const duplicateFilesString = duplicateFiles.join(lineBreak);
            const message = duplicateFiles.length == 1 ?
                this.terms["common.connect.fileExistsWarning"] + `${lineBreak}${duplicateFilesString}`:
                this.terms["common.connect.filesExistWarning"] + `${lineBreak}${duplicateFilesString}`;

            return this.notificationService.showDialog(
                this.terms["core.warning"],
                message,
                SOEMessageBoxImage.Warning,
                SOEMessageBoxButtons.YesNo
            ).result.then(removeDuplicates => {
                if (removeDuplicates)
                    this.files = this.files.filter(file => !duplicateFiles.includes(file.stringValue));
            }).catch(error => {
                return this.$q.reject(error);
            });
        }).catch(error => {
            return this.$q.reject(error);
        });
    }

    // Handles errors encountered during the file upload process
    private handleError(error: any): void {
        console.error("An error occurred during the upload process:", error);
        const errorMessage = `${this.terms["common.connect.fileuploadnotsuccess"]}\n${error.message || ''}`;
        this.notificationService.showDialog(
            this.terms["core.error"],
            errorMessage,
            SOEMessageBoxImage.Error,
            SOEMessageBoxButtons.OK
        );
    }

    private importFiles() {
        this.progress.startWorkProgress((completion) => {

            var dataStorageIds: number[] = [];

            _.forEach(this.files, (file) => {
                if (file.success) {
                    dataStorageIds.push(Number(file.integerValue2));
                }
            });

            this.connectService.importFiles(this.importId, dataStorageIds, this.accountYearId, this.voucherSeriesId, this.import.importDefinitionId).then((result) => {
                if (result.success) {
                    if (result.integerValue == TermGroup_IOImportHeadType.Supplier ||
                        result.integerValue == TermGroup_IOImportHeadType.SupplierInvoice ||
                        result.integerValue == TermGroup_IOImportHeadType.SupplierInvoiceAnsjo ||
                        result.integerValue == TermGroup_IOImportHeadType.Customer ||
                        result.integerValue == TermGroup_IOImportHeadType.CustomerInvoice ||
                        result.integerValue == TermGroup_IOImportHeadType.CustomerInvoiceRow ||
                        result.integerValue == TermGroup_IOImportHeadType.Voucher ||
                        result.integerValue == TermGroup_IOImportHeadType.Project
                    ) {
                        //load import data to dynamic grid                    
                        this.batchId = result.stringValue;
                        this.importHeadType = result.integerValue;
                        this.showImportRows = true;
                        completion.completed(this.files, true);
                    }
                    else {
                        this.showImportRows = false;                        
                        completion.completed(this.files, false, this.terms["common.connect.importsuccess"]);
                    }
                    
                }
                else {
                    this.showImportRows = false;                    
                    completion.failed(this.terms["common.connect.importnotsuccess"] + "\n" + result.errorMessage);
                }
            });

        });
        
    }

    public save() {
        this.progress.startSaveProgress((completion) => {         
            this.connectService.saveImport(this.import).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.importId = result.integerValue;                    
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.import);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {                                
                this.dirtyHandler.clean();

                this.onLoadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.connectService.deleteImport(this.importId).then((result) => {
                if (result.success) {
                    completion.completed(this.import);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.importId = 0;
        this.import = new ImportDTO();
        this.import.isStandard = true;
        this.import.isStandardText = null;
        this.import.module = this.module;
        this.import.updateExistingInvoice = false;
        this.import.useAccountDistribution = false;
        this.import.useAccountDimensions = false;
        this.fileUploadInitiallyOpen = false;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.import) {
                if (!this.import.importDefinitionId) {
                    mandatoryFieldKeys.push("common.connect.standardimportdefinition");
                }
                if (!this.import.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
    
}
