import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Feature, TermGroup, SoeEntityState, CompanySettingType, SoeReportTemplateType, TermGroup_PayrollProductCentRoundingType, TermGroup_PayrollProductCentRoundingLevel, TermGroup_PayrollProductTaxCalculationType, TermGroup_PensionCompany, TermGroup_PayrollProductTimeUnit, TermGroup_PayrollProductQuantityRoundingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType, ISelectablePayrollTypeDTO } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { PayrollProductDTO, PayrollProductSettingDTO } from "../../../Common/Models/ProductDTOs";
import { IPayrollService } from "../PayrollService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ReportJobDefinitionFactory } from "../../../Core/Handlers/ReportJobDefinitionFactory";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";

interface ISelectablePayrollTypeViewModel {
    name: string,
    id: number,
    parentId: number
}

interface ISelectablePayrollTypeCollection {
    id: number;
    selected?: ISelectablePayrollTypeViewModel;
    available: ISelectablePayrollTypeViewModel[];
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private factorInfo: string;
    private resultTypeInfo: string;

    // Company settings
    private defaultReportId: number = 0;

    // Data
    private productId: number;
    private product: PayrollProductDTO;
    private resultTypes: ISmallGenericType[] = [];

    // SysPayrollType selector
    private allSysPayrollTypes: _.Dictionary<ISelectablePayrollTypeDTO[]>;
    private selectablePayrollTypes: ISelectablePayrollTypeCollection[] = [];
    private deselecter: ISelectablePayrollTypeViewModel = { id: -1, parentId: -1, name: "" };

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $window,
        private payrollService: IPayrollService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private reportDataService: IReportDataService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.productId = parameters.id;
        this.guid = parameters.guid;
        this.navigatorRecords = parameters.navigatorRecords;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Preferences_SalarySettings_PayrollProduct, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_SalarySettings_PayrollProduct].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_SalarySettings_PayrollProduct].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.productId);
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "core.print", IconLibrary.FontAwesome, "fa-print", () => {
            this.printPayrollProduct();
        }, () => {
            return !this.defaultReportId || !this.productId;
        })));

        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.productId, recordId => {
            if (recordId !== this.productId) {
                this.productId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadSysPayrollTypes(),
            this.loadResultTypes()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.productId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.warning",
            "common.all",
            "common.reportsettingmissing",
            "time.payroll.payrollproduct.factor.info",
            "time.payroll.payrollproduct.resulttype.info"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.factorInfo = this.terms["time.payroll.payrollproduct.factor.info"];
            this.resultTypeInfo = this.terms["time.payroll.payrollproduct.resulttype.info"];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.PayrollSettingsDefaultReport);
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.PayrollSettingsDefaultReport, 0);
        });
    }

    private loadSysPayrollTypes(): ng.IPromise<any> {
        return this.payrollService.getSysPayrollTypes().then(x => {
            this.allSysPayrollTypes = _.groupBy(x, v => v.parentSysTermId);
            this.buildSelectablePayrollTypesFromLevel(-1, 0);
        });
    }

    private loadResultTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollPriceFormulaResultType, false, true).then(x => {
            this.resultTypes = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.payrollService.getPayrollProduct(this.productId, false).then(x => {
            this.isNew = false;
            this.product = x;

            let noPayrollGroupSetting = _.find(this.product.settings, s => !s.payrollGroupId);
            if (noPayrollGroupSetting)
                noPayrollGroupSetting.payrollGroupName = this.terms["common.all"];

            if (this.product.sysPayrollTypeLevel1)
                this.selectPayrollTypeAtLevel(0, this.product.sysPayrollTypeLevel1, false);
            if (this.product.sysPayrollTypeLevel2)
                this.selectPayrollTypeAtLevel(1, this.product.sysPayrollTypeLevel2, false);
            if (this.product.sysPayrollTypeLevel3)
                this.selectPayrollTypeAtLevel(2, this.product.sysPayrollTypeLevel3, false);
            if (this.product.sysPayrollTypeLevel4)
                this.selectPayrollTypeAtLevel(3, this.product.sysPayrollTypeLevel4, false);

            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.product.name);
        });
    }

    private new() {
        this.isNew = true;
        this.productId = 0;
        this.product = new PayrollProductDTO();
        this.product.productId = 0;
        this.product.state = SoeEntityState.Active;
        if (this.resultTypes.length > 0)
            this.product.resultType = this.resultTypes[0].id;

        // TODO: Add more default values to this setting
        // Default "all" setting
        this.product.settings = [];
        let noPayrollGroupSetting = new PayrollProductSettingDTO();
        noPayrollGroupSetting.sort = 0;
        noPayrollGroupSetting.payrollGroupName = this.terms["common.all"];
        noPayrollGroupSetting.priceTypes = [];
        noPayrollGroupSetting.priceFormulas = [];
        noPayrollGroupSetting.accountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0";
        noPayrollGroupSetting.accountingSettings = [];
        this.setDefaultSettings(noPayrollGroupSetting);
        this.product.settings.push(noPayrollGroupSetting);
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.productId = this.product.productId = 0;
        this.product.number = undefined;
        this.product.shortName = undefined;
        this.product.name = undefined;
        this.product.externalNumber = undefined;
        this.product.state = SoeEntityState.Active;

        _.forEach(this.product.settings, setting => {
            setting.payrollProductSettingId = 0;

            _.forEach(setting.priceTypes, priceType => {
                priceType.payrollProductSettingId = 0;
                priceType.payrollProductPriceTypeId = 0;
                _.forEach(priceType.periods, period => {
                    period.payrollProductPriceTypePeriodId = 0;
                });
            });
            _.forEach(setting.priceFormulas, priceFormula => {
                priceFormula.payrollProductSettingId = 0;
                priceFormula.payrollProductPriceFormulaId = 0;
            });
        });

        this.focusService.focusByName("ctrl_product_number");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.payrollService.savePayrollProduct(this.product).then(result => {
                if (result.success) {
                    this.productId = result.integerValue;
                    this.product.productId = this.productId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.product);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.payrollService.deletePayrollProduct(this.product.productId).then(result => {
                if (result.success) {
                    completion.completed(this.product, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    // HELP-METHODS

    private setDefaultSettings(payrollProductSetting: PayrollProductSettingDTO) {

        //Rounding
        payrollProductSetting.centRoundingType = TermGroup_PayrollProductCentRoundingType.None;
        payrollProductSetting.centRoundingLevel = TermGroup_PayrollProductCentRoundingLevel.None

        //Tax
        payrollProductSetting.taxCalculationType = TermGroup_PayrollProductTaxCalculationType.TableTax;

        //Pension
        payrollProductSetting.pensionCompany = TermGroup_PensionCompany.NotSelected;

        //Timeunit
        payrollProductSetting.timeUnit = TermGroup_PayrollProductTimeUnit.Hours;
        payrollProductSetting.quantityRoundingType = TermGroup_PayrollProductQuantityRoundingType.None;
        payrollProductSetting.quantityRoundingMinutes = 0;

        //Salary specification
        payrollProductSetting.printOnSalarySpecification = true;
        payrollProductSetting.printDate = true;
    }

    private buildSelectablePayrollTypesFromLevel(levelIndex: number, parentId: number) {
        const nextLevelIndex = levelIndex + 1;
        this.selectablePayrollTypes.splice(nextLevelIndex);

        const payrollTypesForNextLevel = this.allSysPayrollTypes[parentId];
        if (!payrollTypesForNextLevel) {
            return;
        }

        const selectablePayrollTypesForLevel = payrollTypesForNextLevel.map(v => <ISelectablePayrollTypeViewModel>{ id: v.id, parentId: v.sysTermId, name: v.name });
        selectablePayrollTypesForLevel.unshift(this.deselecter);

        this.selectablePayrollTypes.push({
            id: nextLevelIndex,
            available: selectablePayrollTypesForLevel,
            selected: this.deselecter
        });
    }

    private selectPayrollTypeAtLevel(levelIndex: number, selectablePayrollTypeIndex: number, setOnProduct: boolean = true) {
        const payrollTypesAtLevel = this.selectablePayrollTypes[levelIndex];

        if (!payrollTypesAtLevel) {
            console.warn("Selectable payroll types was not found at specified level.", levelIndex, selectablePayrollTypeIndex);
            return;
        }

        const selectedFilter = payrollTypesAtLevel.available.find(s => s.id === selectablePayrollTypeIndex);
        payrollTypesAtLevel.selected = selectedFilter;

        if (selectedFilter)
            this.buildSelectablePayrollTypesFromLevel(levelIndex, selectedFilter.parentId);

        if (setOnProduct) {
            this.product.sysPayrollTypeLevel1 = null;
            this.product.sysPayrollTypeLevel2 = null;
            this.product.sysPayrollTypeLevel3 = null;
            this.product.sysPayrollTypeLevel4 = null;

            this.applyForSelectedPayrollTypeLevel((prop, selected) => {
                this.product[prop] = selected.id;
            });
        }
    }

    private applyForSelectedPayrollTypeLevel(callback: (typeLevelPropertyName: string, selectedPayrollType: ISelectablePayrollTypeViewModel) => void) {
        this.selectablePayrollTypes.forEach((f, i) => {
            if (f.selected && f.selected.id > 0) {
                const typeLevelProperty: string = "sysPayrollTypeLevel" + (i + 1);
                callback(typeLevelProperty, f.selected);
            }
        });
    }

    private printPayrollProduct() {
        if (this.defaultReportId && this.productId) {
            this.reportDataService.createReportJob(ReportJobDefinitionFactory.createPayrollProductReportDefinition(this.defaultReportId, SoeReportTemplateType.PayrollProductReport, [this.productId]), true);
        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.product) {
                if (!this.product.number)
                    mandatoryFieldKeys.push("common.number");

                if (!this.product.shortName)
                    mandatoryFieldKeys.push("common.shortname");

                if (!this.product.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}