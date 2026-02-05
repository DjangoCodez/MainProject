import { CompanyDTO, CompanyEditDTO, CopyFromTemplateCompanyInputDTO } from "../../../Common/Models/CompanyDTOs";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Validators } from "../../../Core/Validators/Validators";
import { IPaymentInformationDTO } from "../../../Scripts/TypeLite.Net4";
import { ContactAddressItemType, Feature, Permission, TermGroup_Currency, TermGroup_Languages, TermGroup_SysContactEComType } from "../../../Util/CommonEnumerations";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { ICompanyService } from "../CompanyService";
import { TemplateCopyController } from "./Dialogs/TemplateCopy/TemplateCopyController";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Parameters
    actorCompanyId: number;
    licenseId: number;
    licenseNr: number;
    licenseSupport: boolean;
    authorizedForEdit: boolean;
    isUserInCompany: boolean;

    // Collections
    countries: SmallGenericType[] = [];
    currencies: SmallGenericType[] = [];
    companiesInLicense: CompanyDTO[];

    // Permissions
    public ediModifyPermission: boolean;
    public ediReadOnlyPermission: boolean;
    public featureModifyPermission: boolean;
    public featureReadOnlyPermission: boolean;

    // Company
    company: CompanyEditDTO;

    // Flags
    licenseHasDemoComp = false;

    //@ngInject
    constructor(
        private $window,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private companyService: ICompanyService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData());
    }

    public onInit(parameters: any) {
        this.progress.setProgressBusy(true);
        this.guid = parameters.guid;
        this.actorCompanyId = soeConfig.selectedCompanyId;
        this.licenseId = soeConfig.selectedLicenseId;
        this.licenseNr = soeConfig.selectedLicenseNr;
        this.licenseSupport = soeConfig.selectedLicenseSupport && (<string>soeConfig.selectedLicenseSupport).toLowerCase() == 'true';
        this.authorizedForEdit = (soeConfig.isAuthorizedForEdit && (<string>soeConfig.isAuthorizedForEdit).toLowerCase() == 'true') || (soeConfig.isSupportAdmin || soeConfig.isSupportSuperAdmin) ;
        this.isUserInCompany = soeConfig.isUserInCompany && (<string>soeConfig.isUserInCompany).toLowerCase() == 'true';

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        
        this.flowHandler.start([
            { feature: Feature.Manage_Companies_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Import_XEEdi, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_Companies_Edit_Permission, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Companies_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Companies_Edit].modifyPermission;
        this.ediReadOnlyPermission = response[Feature.Billing_Import_XEEdi].readPermission;
        this.ediModifyPermission = response[Feature.Billing_Import_XEEdi].modifyPermission;
        this.featureReadOnlyPermission = response[Feature.Manage_Companies_Edit_Permission].readPermission;
        this.featureModifyPermission = response[Feature.Manage_Companies_Edit_Permission].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        if (this.featureReadOnlyPermission || soeConfig.isSupportAdmin || soeConfig.isSupportSuperAdmin) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.role.role.readpermission", "manage.role.role.readpermission", IconLibrary.FontAwesome, "fa-book-reader", () => {
                this.openPermissions(Permission.Readonly);
            }, () => {
                return this.isNew;
            })));
        }

        if (this.featureModifyPermission || soeConfig.isSupportAdmin || soeConfig.isSupportSuperAdmin) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.role.role.modifypermission", "manage.role.role.modifypermission", IconLibrary.FontAwesome, "fa-user-edit", () => {
                this.openPermissions(Permission.Modify);
            }, () => {
                return this.isNew;
            }))); 
        }

        if (this.modifyPermission || soeConfig.isSupportAdmin || soeConfig.isSupportSuperAdmin) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.company.editwhole", "manage.company.editwhole", IconLibrary.FontAwesome, "fa-box-check", () => {
                this.openWholesellers();
            }, () => {
                return this.isNew;
            }, () => {
                return !this.company.isEdiGOActivated;
            })));
        }

        if (this.modifyPermission || soeConfig.isSupportAdmin || soeConfig.isSupportSuperAdmin) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.company.templatecopy", "manage.company.templatecopy", IconLibrary.FontAwesome, "fa-copy", () => {
                this.openTemplateCopyDialog();
            }, () => {
                return this.isNew;
            })));
        }

        if (this.modifyPermission || soeConfig.isSupportAdmin || soeConfig.isSupportSuperAdmin) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.company.createnew", "manage.company.createnew", IconLibrary.FontAwesome, "fa-copy", () => {
                this.loadCompaniesByLicense().then(() => {
                    this.new();
                });
            }, () => {
                return this.isNew;
            })));
        }

        //Navigation
        /*this.toolbar.setupNavigationGroup(null, () => { return this.isNew }, (actorCompanyId) => {
            this.actorCompanyId = actorCompanyId;
            this.loadData(true);
        }, this.voucherIds, this.actorCompanyId);*/
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadCountries(),
            this.loadCurrencies(),
            this.loadCompaniesByLicense(),
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.actorCompanyId) {
            return this.loadData(false);
        } else {
            this.new();
        }
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(false, false).then(x => {
            this.countries = x;
        });
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getSysCurrenciesDict(true, true).then(x => {
            this.currencies = x;
        });
    }

    private loadCompaniesByLicense(): ng.IPromise<any> {
        return this.companyService.getCompaniesByLicense(this.licenseId).then(x => {
            this.companiesInLicense = x;
            _.forEach(this.companiesInLicense, (c) => {
                if (c.demo)
                    this.licenseHasDemoComp = true;
            });
        });
    }

    private loadData(updateTab: boolean): ng.IPromise<any> {
        return this.companyService.getCompany(this.actorCompanyId).then((comp) => {
            this.company = comp
            this.isNew = false;

            this.updateTabCaption();
            this.progress.setProgressBusy(false);

            // Backup clean
            this.dirtyHandler.clean();
        });
    }

    private new() {
        this.isNew = true;

        this.actorCompanyId = 0;

        this.company = new CompanyEditDTO();
        this.company.licenseId = this.licenseId;
        this.company.paymentInformation = <IPaymentInformationDTO>{};
        this.company.paymentInformation.rows = [];
        this.company.contactAddresses = [];
        this.company.baseSysCurrencyId = TermGroup_Currency.SEK;
        this.company.sysCountryId = TermGroup_Languages.Swedish;

        const comp = _.last(_.orderBy(_.filter(this.companiesInLicense, (c) => c.number), c => c.number));
        if (comp && comp.number)
            this.company.number = comp.number + 1;

        this.updateTabCaption();
        this.progress.setProgressBusy(false);
    }

    public demoChanged() {
        this.$timeout(() => {
            if (this.company.demo) {
                const keys: string[] = [
                    "manage.company.demo",
                    "manage.company.demoshort",
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.company.name = terms["manage.company.demo"];
                    this.company.shortName = terms["manage.company.demoshort"];
                    this.company.orgNr = "0000000000";
                });
            }
            else {
                this.company.name = "";
                this.company.shortName = "";
                this.company.orgNr = "";
            }
        });
    }

    public save() {
        this.$scope.$broadcast('stopEditing', {
            functionComplete: () => {
                this.startSave();
            }
        });
    }

    public startSave() {
        this.$timeout(() => {
            // Validate sysadminemail
            const missingSysEmail = (this.company.contactAddresses.length === 0 || !_.find(this.company.contactAddresses, { sysContactEComTypeId: TermGroup_SysContactEComType.CompanyAdminEmail }));
            const missingDistributionAddress = (this.company.contactAddresses.length === 0 || !_.find(this.company.contactAddresses, (a) => { return a.isAddress && a.contactAddressItemType === ContactAddressItemType.AddressDistribution }));

            if (missingSysEmail || missingDistributionAddress){
                const keys: string[] = [
                    "core.warning",
                    "manage.company.sysemail",
                    "manage.company.distraddress"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    let message = "";
                    if (missingSysEmail)
                        message += terms["manage.company.sysemail"];
                    if (missingDistributionAddress)
                        message += missingSysEmail ? "\n" + terms["manage.company.distraddress"] : terms["manage.company.distraddress"];

                    this.notificationService.showDialogEx(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                });
                return;
            }

            let hasInvalidBic = false;
            _.forEach(this.company.paymentInformation.rows, (r) => {
                if (!hasInvalidBic)
                    hasInvalidBic = !Validators.isValidBic(r.bic, r.paymentInformationRowId > 0);
            });

            if (hasInvalidBic) {
                const keys: string[] = [
                    "core.error",
                    "manage.company.bicerror",
                    "manage.company.distraddress"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["core.error"], terms["manage.company.bicerror"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                });
                return;
            }


            this.progress.startSaveProgress((completion) => {
                this.companyService.saveCompany(this.company).then((result) => {
                    if (result.success) {
                        this.actorCompanyId = result.integerValue;
                        completion.completed();
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, this.guid)
                .then(data => {
                    this.dirtyHandler.clean();
                    this.loadData(this.isNew);      
                }, error => {

            });
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.companyService.deleteCompany(this.company.actorCompanyId).then((result) => {
                if (result.success) {
                    completion.completed(this.company);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            }).then(data => {
                this.dirtyHandler.clean();
                this.loadCompaniesByLicense().then(() => {
                    this.new();
                });
            });
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;
            if (this.company) {
                if (!this.company.name)
                    mandatoryFieldKeys.push("common.companyname");
                if (!this.company.shortName)
                    mandatoryFieldKeys.push("common.shortname");
                if (!this.company.sysCountryId)
                    mandatoryFieldKeys.push("common.country");
                if (!this.company.orgNr)
                    mandatoryFieldKeys.push("common.orgnrshort");
            }
        });
    }

    public openPermissions(type: Permission) {
        if (this.actorCompanyId)
            HtmlUtility.openInNewTab(this.$window, "/soe/manage/companies/edit/permission/?license=" + this.licenseId + "&licenseNr=" + this.licenseNr + "&company=" + this.actorCompanyId + "&permission=" + type);
    }

    public openWholesellers() {
        if (this.actorCompanyId) 
            HtmlUtility.openInNewTab(this.$window, "/soe/billing/preferences/wholesellersettings?c=" + soeConfig.actorCompanyId + "&sc=" + this.actorCompanyId);
    }

    public openTemplateCopyDialog() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/Company/Company/Dialogs/TemplateCopy/TemplateCopy.html"),
            controller: TemplateCopyController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                coreService: () => { return this.coreService },
                companyService: () => { return this.companyService },
                licenseId: () => { return this.licenseId }
            }
        });

        modal.result.then((dto: CopyFromTemplateCompanyInputDTO) => {
            if (dto) {
                dto.actorCompanyId = this.actorCompanyId;
                dto.update = true;
                dto.userId = soeConfig.userId;
                this.translationService.translate("core.copying").then(term => {
                    this.progress.startWorkProgress((completion) => {
                        this.companyService.copyFromTemplateCompany(dto).then((result) => {
                            if (!result) {
                                var keys: string[] = [
                                    "core.warning",
                                    "manage.company.copyerror"
                                ];

                                this.translationService.translateMany(keys).then(terms => {
                                    this.notificationService.showDialogEx(terms["core.warning"], terms["manage.company.copyerror"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                                });
                            }
                            completion.completed(null);
                        }, error => {
                            completion.failed(error.message);
                        })
                    }, null, term);
                });
            } 
        });
    }

    private updateTabCaption() {
        this.translationService.translateMany(["manage.company.createnew", "common.company"]).then((terms) => {
            const shortNrName = this.company ? (this.company.number ? this.company.number + " - " + this.company.shortName : this.company.shortName) : "";
            const label = this.isNew ? terms["manage.company.createnew"] : terms["common.company"] + " " + shortNrName;
            this.messagingHandler.publishSetTabLabel(this.guid, label);
        });
    }
}