import { ICoreService } from "../../../Core/Services/CoreService";
import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IAddInvoiceToAttestFlowService } from "./AddInvoiceToAttestFlowService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IAttestWorkFlowHeadDTO, IAttestWorkFlowTemplateHeadDTO, ISmallGenericType, IAttestWorkFlowTemplateRowDTO } from "../../../Scripts/TypeLite.Net4";
import { UserSelectorForTemplateHeadRowDirectiveController } from "./Directives/UserSelectorForTemplateHeadRowDirective";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { Feature, TermGroup, TermGroup_AttestWorkFlowRowProcessType, SoeEntityType, SoeEntityState, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../Util/Enumerations";
import { UserSmallDTO } from "../../Models/UserDTO";
import { NumberUtility } from "../../../Util/NumberUtility";

export class AddInvoiceToAttestFlowController extends GridControllerBase {

    private numberOfInvoicesText: string;
    private adminText: string = "";
    private sendMessage: boolean = true;
    private affectedSupplierInvoiceIds: any[] = [];

    // Amount & user setting
    private requiredUserId: number = 0;
    private totalAmountWhenUserRequired: number = 0;
    private requiredUser: UserSmallDTO;
    private userRequiredMessage: string;
    private userRequiredCss: string = "info";

    
    // Data
    attestWorkFlowHead: IAttestWorkFlowHeadDTO;

    // Lookups         
    attestGroups: any[] = [];
    templates: IAttestWorkFlowTemplateHeadDTO[];
    attestGroupTypes: ISmallGenericType[] = [];
    role_User: ISmallGenericType[] = [];     

    // Flags
    buttonOKClicked: boolean = false;

    private roleOrUser: number = 0;
    private templateRows: IAttestWorkFlowTemplateRowDTO[];
    private userSelectors: UserSelectorForTemplateHeadRowDirectiveController[];
    private existingAttestFlows: any[] = [];


    //@ngInject
    constructor(private $uibModalInstance,
        $http,
        $templateCache,
        private $window,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        private addInvoiceToAttestFlowService: IAddInvoiceToAttestFlowService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        //private supplierInvoiceId: number,
        //private invoiceAmount: number,            
        private selectedSupplierInvoiceIds: any[],
        private defaultAttestGroupId: number,
        private highestAmount: number,
        private $q: ng.IQService,
        private $scope: ng.IScope
    ) {
        super("Economy.Supplier.Invoices", "economy.supplier.invoices.addtoattestflow", Feature.Economy_Supplier_Invoice_AttestFlow, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        //Default values for variables
        this.numberOfInvoicesText = this.selectedSupplierInvoiceIds.length.toString();
        this.attestWorkFlowHead = <IAttestWorkFlowHeadDTO>{}; //we just fake it
        this.userSelectors = [];
        this.load();
    }

    public setupGrid() {

    }

    protected edit(row) {
        this.$uibModalInstance.close(row.actorSupplierId);
    }

    private load() {
        var promises = [];

        var groupPromise = this.supplierService.getAttestWorkFlowGroupsDict(true).then((data) => {
            this.attestGroups = data;
        });

        var templateHeadPromise = this.supplierService.getAttestWorkFlowTemplateHeadsForCurrentCompany().then((data) => {
            this.templates = data;
        });

        var groupTypePromise = this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowType, false, false).then((data) => {
            this.attestGroupTypes = data;
        });

        var roleUserPromise = this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowApproverType, false, false).then((data) => {
            this.role_User = data;
        });


        promises.push(groupPromise, templateHeadPromise, groupTypePromise, roleUserPromise, this.loadCompanySettings());

        this.$q.all(promises).then(() => this.lookupLoaded());
    }

    protected lookupLoaded() {

        if (this.defaultAttestGroupId) {
            this.groupChanged(this.defaultAttestGroupId);
        }

        this.stopProgress();
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let companySettingTypeIds = [
            CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired,
            CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired
        ]
        return this.coreService.getCompanySettings(companySettingTypeIds).then(x => {
            this.requiredUserId = x[CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired];
            this.totalAmountWhenUserRequired = x[CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired];
        }).then(() => {
            if (this.requiredUserId > 0 && this.totalAmountWhenUserRequired <= this.highestAmount) {
                this.setRequiredUserMessage();
            }
        })
    }

    private setRequiredUserMessage() {
        this.coreService.getUser(this.requiredUserId).then((user) => {
            this.requiredUser = user;
        }).then(() => {
            this.translationService.translate("economy.supplier.attestgroup.invoicerequiresspecificuser").then(term => {
                this.userRequiredMessage = term.format(NumberUtility.printDecimal(this.totalAmountWhenUserRequired), this.requiredUser.name)
            })
        })
    }

    //logic

    private groupChanged(attestWorkFlowHeadId: number) {
        this.userSelectors = [];
        this.supplierService.getAttestWorkFlowHead(attestWorkFlowHeadId, false, true).then((x) => {
            this.attestWorkFlowHead = x;
            this.attestWorkFlowHead.attestWorkFlowGroupId = x.attestWorkFlowHeadId;
            this.sendMessage = this.attestWorkFlowHead.sendMessage;
        }).then(() => this.loadCompanyTemplateRows(this.attestWorkFlowHead.attestWorkFlowTemplateHeadId));
    }

    private templateChanged(templateHeadId: number) {
        this.userSelectors = [];
        this.loadCompanyTemplateRows(templateHeadId);
    }

    private loadCompanyTemplateRows(templateHeadId: number) {
        this.supplierService.getAttestWorkFlowTemplateHeadRows(templateHeadId).then((data) => {
            this.templateRows = data;

            this.templateRows.forEach(row => {
                _.forEach(this.attestWorkFlowHead.rows, r => {
                    if (r.attestTransitionId == row.attestTransitionId) {
                        row.type = r.type;
                    }
                });

                if (row.type == null)
                    row.type = this.attestWorkFlowHead.type;
            });
        });
    }

    public registerUserSelector(control: UserSelectorForTemplateHeadRowDirectiveController) {
        this.userSelectors.push(control);
    }

    buttonOkClick() {        

        var keys: string[] = [
            "core.verifyquestion",
            "economy.supplier.invoice.existingattestflowmessage",   
        ];

        this.translationService.translateMany(keys).then((terms) => {

            var promises = [];
            _.forEach(this.selectedSupplierInvoiceIds, invoiceId => {
                var deferred = this.$q.defer();
                promises.push(deferred.promise);
                this.supplierService.getAttestWorkFlowHeadFromInvoiceId(invoiceId, false, false, false, false).then((attestWorkFlowHead) => {
                    if (attestWorkFlowHead) {
                        this.existingAttestFlows.push({ invoiceId: invoiceId, attestWorkFlowHeadId: attestWorkFlowHead.attestWorkFlowHeadId });
                    }
                    deferred.resolve();
                })
            });

            this.$q.all(promises).then(() => {
                
                if (this.existingAttestFlows.length > 0) {
                    var modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.invoice.existingattestflowmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                    modal.result.then(val => {
                        if (val != null && val === true) {
                            //prevent double attestflows (32389)        
                            if (!this.buttonOKClicked) {
                                this.buttonOKClicked = true;
                                this.saveAttestFlow();
                            }
                        }
                        else {
                            this.$uibModalInstance.dismiss('cancel');
                        }
                    });
                }
                else {
                    //prevent double attestflows (32389)        
                    if (!this.buttonOKClicked) {
                        this.buttonOKClicked = true;
                        this.saveAttestFlow();
                    }
                }                
            });

        });
    }
    

    private saveAttestFlow() {        
        if (this.attestWorkFlowHead.rows == null)
            this.attestWorkFlowHead.rows = [];

        //this.buttonOKClicked = false;
        this.attestWorkFlowHead.sendMessage = this.sendMessage;
        this.attestWorkFlowHead.adminInformation = this.adminText;
        (<any>this.attestWorkFlowHead).state = SoeEntityState.Active;
        this.attestWorkFlowHead.entity = SoeEntityType.SupplierInvoice;

        // Create rows from selected users
        var rows = [];
        var regRow = _.find(this.attestWorkFlowHead.rows, r => r.userId === CoreUtility.userId && (<any>r).processType === TermGroup_AttestWorkFlowRowProcessType.Registered && r.answer);

        if (regRow != null) {
            // Registration row exists
            rows.push(regRow);
        }
        else {
            // Add user that registered the flow, just for logging
            rows.push({
                attestTransitionId: this.userSelectors[0].getAttestTransitionId(),
                userId: CoreUtility.userId,
                processType: TermGroup_AttestWorkFlowRowProcessType.Registered,
                answer: true,
                type: this.attestWorkFlowHead.type
            });
        }
        var i = 0;
        var rowsValid: boolean = true;
        let requiredUserIsSelected = false;
        this.userSelectors.forEach(us => {
            i++;
            var urows = us.getRowsToSave();

            // Validate selector have selected rows
            if (!urows || urows.length === 0)
                rowsValid = false;

            urows.forEach(r => {
                requiredUserIsSelected = r.userId == this.requiredUserId ? true : requiredUserIsSelected;
                r.ProcessType = i === 1 ? TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess : TermGroup_AttestWorkFlowRowProcessType.LevelNotReached;
                rows.push(r);
            });
        });

        if (requiredUserIsSelected === false && this.requiredUserId > 0 && this.totalAmountWhenUserRequired <= this.highestAmount) {
            this.userRequiredCss = "warning";
            this.buttonOKClicked = false;
            return;
        }

        if (!rowsValid) {
            var keys: string[] = [
                "core.warning",
                "economy.supplier.invoice.attesthasinvalidrows",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.invoice.attesthasinvalidrows"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                this.buttonOKClicked = false;
            });
            return;
        }

        this.attestWorkFlowHead.rows = rows;

        if (this.selectedSupplierInvoiceIds.length > 0) {
            this.supplierService.saveAttestWorkFlowForMultipleInvoices(this.attestWorkFlowHead, this.selectedSupplierInvoiceIds).then((result) => {
                if (result.success) {
                    this.$uibModalInstance.close(result.integerValue);
                }
                else {
                    this.$uibModalInstance.close(0);
                }
            });

            /*var promises = [];
            _.forEach(this.selectedSupplierInvoiceIds, invoiceId => {
                var deferred = this.$q.defer();
                promises.push(deferred.promise);
                this.saveAttestWorkFlow(invoiceId).then(() => {                    
                    deferred.resolve();
                });
            });
            this.$q.all(promises).then(() => {
                this.$uibModalInstance.close(this.affectedSupplierInvoiceIds);
            });*/
        }
        else {
            this.$uibModalInstance.close(this.attestWorkFlowHead);
        }

        this.buttonOKClicked = false;
    }

    /*private saveAttestWorkFlow(invoiceId): ng.IPromise<any> {
        this.attestWorkFlowHead.recordId = invoiceId;
        return this.supplierService.saveAttestWorkFlow(this.attestWorkFlowHead).then((data) => {
            if (data.success) {
                
                var attestWorkFlow = _.find(this.existingAttestFlows, i => i.invoiceId = invoiceId);
                
                if (attestWorkFlow) {
                    this.supplierService.deleteAttestWorkFlow(attestWorkFlow.attestWorkFlowHeadId).then(() => { })
                }
                
                this.affectedSupplierInvoiceIds.push(invoiceId);
            }
                
        });
    }*/

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}