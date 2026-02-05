import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { InvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { Feature, PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../Util/Constants";
import { PriceRuleDTO } from "../../../Common/Models/PriceRuleDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { FormulaWidget, ExpressionWidget, OperatorWidget } from "../../../Common/Models/FormulaBuilderDTOs";
import { ICompanyWholesellerPriceListViewDTO } from "../../../Scripts/TypeLite.Net4";
import { CompanyWholesellerPriceListViewDTO } from "../../../Common/Models/CompanyWholeSellerPriceListViewDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    isLoading = true;

    // Data
    priceRuleId: number;
    priceRule: PriceRuleDTO;
    private expWidgets: ExpressionWidget[] = [];
    private opWidgets: OperatorWidget[] = [];
    private widgets: FormulaWidget[] = [];
    private formulaValid: string;
    private formulaError: string;
    private example: string;

    private compPricelists: any[];

    private selectedWholesellerPriceList: ICompanyWholesellerPriceListViewDTO;
    private wholesellerPriceLists: ICompanyWholesellerPriceListViewDTO[] = [];

    private currencyCode: string;

    terms: any;

    //@ngInject
    constructor(
        private $q,
        private coreService: ICoreService,
        private invoiceService: InvoiceService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        urlHelperService: IUrlHelperService,
        private $scope: ng.IScope,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookUps())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.priceRuleId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Billing_Preferences_InvoiceSettings_PriceRules_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);

        this.setupWidgets();
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_InvoiceSettings_PriceRules_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_InvoiceSettings_PriceRules_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    // SETUP

    private setupWidgets() {
        this.expWidgets.push(new ExpressionWidget("gnpWidget"));
        this.expWidgets.push(new ExpressionWidget("supplierAgreementWidget"));
        this.expWidgets.push(new ExpressionWidget("gainWidget"));
        this.expWidgets.push(new ExpressionWidget("markupWidget"));
        this.expWidgets.push(new ExpressionWidget("customerDiscountWidget"));
        this.expWidgets.push(new ExpressionWidget("priceBasedMarkupWidget"));
        //this.expWidgets.push(new ExpressionWidget("netPriceWidget"));

        this.opWidgets.push(new OperatorWidget("startParanthesisWidget"));
        this.opWidgets.push(new OperatorWidget("endParanthesisWidget"));
        this.opWidgets.push(new OperatorWidget("multiplicationWidget"));
        //this.opWidgets.push(new OperatorWidget("orWidget"));
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.widgets, (newval, oldval) => {
            this.calculateExample();
            this.evaluateRuleStructure(this.widgets, 0);
        }, true);
    }

    private onDoLookUps() {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompPricelists(),
            this.loadSysPriceLists(),
            this.loadCurrency()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    // LOOKUPS
    private loadTerms(): ng.IPromise<any> {

        // Columns
        const keys: string[] = [
            "billing.invoices.pricerules.pricelisttypename",
            "billing.invoices.pricerules.syswholesellername",
            "billing.invoices.pricerules.general",
            "billing.invoices.pricerules.pricerule",
            "core.error",
            "billing.invoices.pricerules.validateerror",
            "billing.invoices.pricerules.formulaerror.firstwidgetincorrect",
            "billing.invoices.pricerules.formulaerror.nowidgets",
            "billing.invoices.pricerules.formulaerror.operatorafterparentheses",
            "billing.invoices.pricerules.formulaerror.severaloperatorsinarow",
            "billing.invoices.pricerules.formulaerror.severalexpressionsinarow",
            "billing.invoices.pricerules.formulaerror.lastwidgetincorrect",
            "billing.invoices.pricerules.formulaerror.incorrectamountofparenthesis",
            "billing.invoices.pricerules.validatesuccess"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.formulaValid = this.terms["billing.invoices.pricerules.validatesuccess"];
        });
    }

    private loadCompPricelists(): ng.IPromise<any> {
        return this.invoiceService.getPriceListsDict(false).then((x) => {
            this.compPricelists = x;
        });
    }

    private loadSysPriceLists(): ng.IPromise<any> {
        return this.invoiceService.getCompanyWholesellerPriceLists(true).then((x: ICompanyWholesellerPriceListViewDTO[]) => {
            this.wholesellerPriceLists = _.orderBy(x, "sysWholesellerName");

            const general = new CompanyWholesellerPriceListViewDTO();
            general.priceListName = this.terms["billing.invoices.pricerules.general"];
            general.companyWholesellerPriceListId = 0;
            this.wholesellerPriceLists.unshift(general);

            this.wholesellerPriceLists.forEach(x => {
                x["displayName"] = x.priceListName + (x.date ? " " + CalendarUtility.toFormattedDate(x.date) : "");
            });
        });
    }

    private loadCurrency(): ng.IPromise<any> {
        return this.coreService.getCompanyBaseCurrency().then((x) => {
            this.currencyCode = x.code;
        });
    }

    private onLoadData(): ng.IPromise<any> {
        this.isLoading = true;

        if (this.priceRuleId > 0) {
            return this.invoiceService.getPriceRule(this.priceRuleId).then((x: PriceRuleDTO) => {
                this.isNew = false;
                this.priceRule = x;
                this.selectedWholesellerPriceList = null;
                if (!this.priceRule.companyWholesellerPriceListId) {
                    this.priceRule.companyWholesellerPriceListId = 0;
                }
                if (!this.priceRule.priceListImportedHeadId) {
                    this.priceRule.priceListImportedHeadId = 0;
                }
                this.extractRule();
                const pricelist = this.compPricelists.find(p => p.id === this.priceRule.priceListTypeId);
                if (this.priceRule.companyWholesellerPriceListId) {
                    this.selectedWholesellerPriceList = this.wholesellerPriceLists.find(p => p.companyWholesellerPriceListId === this.priceRule.companyWholesellerPriceListId);
                }
                else if (this.priceRule.priceListImportedHeadId) {
                    this.selectedWholesellerPriceList = this.wholesellerPriceLists.find(p => p.priceListImportedHeadId === this.priceRule.priceListImportedHeadId);
                }
                else {
                    this.selectedWholesellerPriceList = this.wholesellerPriceLists.find(p => p.companyWholesellerPriceListId === 0); //Generell
                }
                
                if (pricelist)
                    this.messagingHandler.publishSetTabLabel(this.guid, this.terms["billing.invoices.pricerules.pricerule"] + " " + pricelist.name);
                this.isLoading = false;
            });
        }
        else {
            this.isLoading = false;
            this.new();
        }
    }

    private validateRuleStructure(rule: PriceRuleDTO): boolean {
        if (rule == null)
            return false;

        let isValid = false;

        //Right side
        if (rule.rRule != null)
            isValid = this.validateRuleStructure(rule.rRule);
        else if (rule.rValueType != null && rule.rValue != null)
            isValid = true;
        if (!isValid)
            return isValid;

        //Left side
        if (rule.lRule != null)
            isValid = this.validateRuleStructure(rule.lRule);
        else if (rule.lValueType != null && rule.lValue != null)
            isValid = true;
        if (!isValid)
            return isValid;

        return isValid;
    }

    private save() {

        this.progress.startSaveProgress((completion) => {

            let rule: PriceRuleDTO = this.convertWidgetsToPriceRule();

            if (this.validateRuleStructure(rule)) {

                rule.priceListTypeId = this.priceRule.priceListTypeId;
                rule.companyWholesellerPriceListId = this.selectedWholesellerPriceList.companyWholesellerPriceListId;
                rule.priceListImportedHeadId = this.selectedWholesellerPriceList.priceListImportedHeadId;

                this.invoiceService.savePriceRule(rule).then((result) => {
                    if (result.success) {
                        if (result.integerValue && result.integerValue > 0)
                            this.priceRuleId = result.integerValue;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.priceRule);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }
            else {
                completion.failed(this.terms["billing.invoices.pricerules.validateerror"]);
            }

        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {
                console.log("save error", error);
            });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.invoiceService.deletePriceRule(this.priceRuleId).then((result) => {
                if (result.success) {
                    completion.completed(this.priceRule);
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

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {

            if (!this.selectedWholesellerPriceList) {
                mandatoryFieldKeys.push("billing.invoices.pricerules.syswholesellername");
            }
            
            if (!this.priceRule || !this.priceRule.priceListTypeId) {
                mandatoryFieldKeys.push("billing.invoices.pricerules.pricelisttypename");
            }
        });
    }

    protected copy() {
        this.isNew = true;
        this.priceRuleId = 0;
        this.priceRule.ruleId = 0;
        this.priceRule.companyWholesellerPriceListId = null;
        this.priceRule.priceListTypeId = 0;

        this.messagingService.publish(Constants.EVENT_EDIT_NEW, { guid: this.guid });
    }

    private new() {
        this.isNew = true;
        this.priceRuleId = 0;
        this.evaluateRuleStructure(this.widgets, 0);
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
        this.calculateExample();
    }

    // EVENTS

    private widgetDropped(widget: FormulaWidget, containerIndex: number) {
        if (widget) {
            let maxId: number = (this.widgets && this.widgets.length > 0) ? _.maxBy(this.widgets, w => w.internalId).internalId : 0;
            widget.internalId = maxId + 1;
            widget.sort = this.widgets.length + 1;
            this.widgets.push(widget);
        }
    }

    // VALIDATION

    private evaluateRuleStructure(widgets: FormulaWidget[], containerIndex: number) {
        if (this.isLoading)
            return;

        var countStartParenthesis = 0;
        var countEndParenthesis = 0;
        var previousWidget = null;

        this.formulaError = '';

        // No widgets
        if (widgets.length == 0) {
            this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.nowidgets"];
            return;
        }

        if (widgets.length == 1) {
            var widget = widgets[0];

            // First widget                
            if (widget.isOperator) {
                if (widget.priceRuleType !== PriceRuleItemType.StartParanthesis)
                    this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.firstwidgetincorrect"];
                else
                    this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.nowidgets"];
                return;
            }
        }

        _.forEach(_.sortBy(widgets, 'sort'), widget => {
            if (widget.priceRuleType === PriceRuleItemType.EndParanthesis) {
                // Start parenthesis
                countEndParenthesis++;
            }
            else if (widget.priceRuleType === PriceRuleItemType.StartParanthesis) {
                // End parenthesis
                countStartParenthesis++;
            }

            if (previousWidget != null) {
                // Operator following a start parenthesis
                if (widget.priceRuleType === PriceRuleItemType.Multiplication && previousWidget && previousWidget.priceRuleType === PriceRuleItemType.StartParanthesis) {
                    this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.operatorafterparentheses"];
                    return;
                }

                // Operator following another operator
                if (widget.priceRuleType === PriceRuleItemType.Multiplication && previousWidget && previousWidget.priceRuleType === PriceRuleItemType.Multiplication) {
                    this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.severaloperatorsinarow"];
                    return;
                }

                // Expression following another expression
                if ((widget.priceRuleType === PriceRuleItemType.GNP || widget.priceRuleType === PriceRuleItemType.SupplierAgreement ||
                    widget.priceRuleType === PriceRuleItemType.Gain || widget.priceRuleType === PriceRuleItemType.Markup || widget.priceRuleType === PriceRuleItemType.CustomerDiscount) &&
                    previousWidget &&
                    (previousWidget.priceRuleType === PriceRuleItemType.GNP || previousWidget.priceRuleType === PriceRuleItemType.SupplierAgreement ||
                        previousWidget.priceRuleType === PriceRuleItemType.Gain || previousWidget.priceRuleType === PriceRuleItemType.Markup || previousWidget.priceRuleType === PriceRuleItemType.CustomerDiscount)) {

                    this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.severalexpressionsinarow"];
                    return;
                }
            }

            previousWidget = widget;
        });

        // Check last item
        if (previousWidget.priceRuleType === PriceRuleItemType.Multiplication ||
            previousWidget.priceRuleType === PriceRuleItemType.StartParanthesis ||
            previousWidget.priceRuleType === PriceRuleItemType.Or) {
            this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.lastwidgetincorrect"];
            return;
        }

        // Check number of parenthesis
        if (countStartParenthesis != countEndParenthesis) {
            this.formulaError = this.terms["billing.invoices.pricerules.formulaerror.incorrectamountofparenthesis"];
            return;
        }

        this.calculateExample();
    }

    // HELP-METHODS

    private extractRule() {
        this.widgets = [];
        const items = this.getRulePresentationStructure(this.priceRule);
        let sort: number = 0;

        for (const item of items) {
            let name: string;
            let isExpression: boolean = false;
            let isOperator: boolean = false;
            let priceRuleValueType: PriceRuleValueType;
            let data: any;

            switch (item.type) {
                case PriceRuleItemType.GNP:
                    name = "gnpWidget";
                    isExpression = true;
                    priceRuleValueType = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleItemType.SupplierAgreement:
                    name = "supplierAgreementWidget";
                    isExpression = true;
                    priceRuleValueType = PriceRuleValueType.PositivePercent;
                    data = { percent: item.value, isExample: false, useNetPrice: item.useNetPrice };
                    break;
                case PriceRuleItemType.Gain:
                    name = "gainWidget";
                    isExpression = true;
                    priceRuleValueType = PriceRuleValueType.PositivePercent;
                    data = { percent: item.value, isExample: false };
                    break;
                case PriceRuleItemType.Markup:
                    name = "markupWidget";
                    isExpression = true;
                    priceRuleValueType = PriceRuleValueType.PositivePercent;
                    break;
                case PriceRuleItemType.CustomerDiscount:
                    name = "customerDiscountWidget";
                    isExpression = true;
                    priceRuleValueType = PriceRuleValueType.NegativePercent;
                    break;
                case PriceRuleItemType.StartParanthesis:
                    name = "startParanthesisWidget";
                    isOperator = true;
                    break;
                case PriceRuleItemType.EndParanthesis:
                    name = "endParanthesisWidget";
                    isOperator = true;
                    break;
                case PriceRuleItemType.Multiplication:
                    name = "multiplicationWidget";
                    isOperator = true;
                    break;
                case PriceRuleItemType.PriceBasedMarkup:
                    name = "priceBasedMarkupWidget";
                    isExpression = true;
                    priceRuleValueType = PriceRuleValueType.PositivePercent;
                    break;
                case PriceRuleItemType.NetPrice:
                    name = "netPriceWidget";
                    isExpression = true;
                    priceRuleValueType = PriceRuleValueType.Numeric;
                    break;
                case PriceRuleItemType.Or:
                    name = "orWidget";
                    isOperator = true;
                    break;
            }

            let widget: FormulaWidget = new FormulaWidget(!this.modifyPermission, name, isExpression ? 170 : 50);
            widget.priceRuleType = item.type;
            widget.priceRuleValueType = priceRuleValueType;
            widget.isExpression = isExpression;
            widget.isOperator = isOperator;
            widget.internalId = widget.sort = ++sort;
            widget.data = data;
            this.widgets.push(widget);
        };
    }

    private getRulePresentationStructure(rule: PriceRuleDTO): InterpretFormat[] {
        var result: InterpretFormat[] = [];

        if (rule.lRule && rule.rRule) {
            // Left rule
            result.push(new InterpretFormat(PriceRuleItemType.StartParanthesis));
            _.forEach(this.getRulePresentationStructure(rule.lRule), item => {
                result.push(item);
            });
            result.push(new InterpretFormat(PriceRuleItemType.EndParanthesis));

            // Operator
            result.push(new InterpretFormat(rule.operatorType));

            // Right rule
            result.push(new InterpretFormat(PriceRuleItemType.StartParanthesis));
            _.forEach(this.getRulePresentationStructure(rule.rRule), item => {
                result.push(item);
            });
            result.push(new InterpretFormat(PriceRuleItemType.EndParanthesis));
        } else if (rule.rRule) {
            result.push(new InterpretFormat(rule.lValueType, rule.lValue, rule.useNetPrice));
            result.push(new InterpretFormat(rule.operatorType));

            // Right rule
            result.push(new InterpretFormat(PriceRuleItemType.StartParanthesis));
            _.forEach(this.getRulePresentationStructure(rule.rRule), item => {
                result.push(item);
            });
            result.push(new InterpretFormat(PriceRuleItemType.EndParanthesis));
        } else if (rule.lRule) {
            // Left rule
            result.push(new InterpretFormat(PriceRuleItemType.StartParanthesis));
            _.forEach(this.getRulePresentationStructure(rule.lRule), item => {
                result.push(item);
            });
            result.push(new InterpretFormat(PriceRuleItemType.EndParanthesis));

            result.push(new InterpretFormat(rule.operatorType));
            result.push(new InterpretFormat(rule.rValueType, rule.rValue));
        } else {
            if (rule.lValueType && (rule.lValue || rule.lValue === 0))
                result.push(new InterpretFormat(rule.lValueType, rule.lValue, rule.useNetPrice));

            result.push(new InterpretFormat(rule.operatorType));

            if (rule.rValueType && (rule.rValue || rule.rValue === 0))
                result.push(new InterpretFormat(rule.rValueType, rule.rValue, rule.useNetPrice));
        }

        return result;
    }

    private calculateExample() {
        this.example = "";
        const exampleValue: any = this.applyRule(this.convertWidgetsToPriceRule());

        if (exampleValue && _.isFinite(exampleValue.value))
            this.example = exampleValue.value.toLocaleString(undefined,{ minimumFractionDigits: 2 }) + " " + this.currencyCode;
    }

    private applyRule(rule: PriceRuleDTO) {

        var result: RuleResult = null;
        var leftHand: RuleResult;
        var rightHand: RuleResult;

        if (rule.rRule != null && rule.lRule != null) {
            leftHand = this.applyRule(rule.lRule);
            rightHand = this.applyRule(rule.rRule);
        }
        else if (rule.rRule != null) {
            try {
                leftHand = new RuleResult(rule.lExampleType, rule.lValue);
            }
            catch (ex) {
                ex.ToString();
                return new RuleResult(PriceRuleValueType.Numeric, 0);
            }
            rightHand = this.applyRule(rule.rRule);
        }
        else if (rule.lRule != null) {
            leftHand = this.applyRule(rule.lRule);
            try {
                rightHand = new RuleResult(rule.rExampleType, rule.rValue);
            }
            catch (ex) {
                ex.ToString();
                return new RuleResult(PriceRuleValueType.Numeric, 0);
            }
        }
        else {
            try {
                leftHand = new RuleResult(rule.lExampleType, rule.lValue);
                rightHand = new RuleResult(rule.rExampleType, rule.rValue);
            }
            catch (ex) {
                ex.ToString();
                return new RuleResult(PriceRuleValueType.Numeric, 0);
            }
        }

        if (rule.operatorType == PriceRuleItemType.Multiplication) {
            result = this.calculateMultiplication(leftHand, rightHand);
        }

        return result;

    }

    private calculateMultiplication(left: RuleResult, right: RuleResult): RuleResult {

        var result: RuleResult = new RuleResult(0, 0);
        var leftValue: number = 0;
        var rightValue: number = 0;

        if (!left || !right)
            return;

        //LeftValue
        if (left.type == PriceRuleValueType.NegativePercent)
            leftValue = (1 - (left.value / 100));
        else if (left.type == PriceRuleValueType.PositivePercent)
            leftValue = 1 + (left.value / 100);
        else if (left.type == PriceRuleValueType.Percent)
            leftValue = (left.value);
        else
            leftValue = left.value;

        //RightValue
        if (right.type == PriceRuleValueType.NegativePercent)
            rightValue = (1 - (right.value / 100));
        else if (right.type == PriceRuleValueType.PositivePercent)
            rightValue = 1 + (right.value / 100);
        else if (right.type == PriceRuleValueType.Percent)
            rightValue = (right.value);
        else
            rightValue = right.value;

        switch (left.type) {
            case PriceRuleValueType.Numeric:
                result.type = PriceRuleValueType.Numeric;
                break;
            case PriceRuleValueType.NegativePercent:
                if (right.type == PriceRuleValueType.NegativePercent)
                    result.type = PriceRuleValueType.NegativePercent;
                else if (right.type == PriceRuleValueType.PositivePercent)
                    result.type = PriceRuleValueType.Percent;
                else
                    result.type = PriceRuleValueType.Numeric;
                break;
            case PriceRuleValueType.PositivePercent:
                if (right.type == PriceRuleValueType.NegativePercent)
                    result.type = PriceRuleValueType.Percent;
                else if (right.type == PriceRuleValueType.PositivePercent)
                    result.type = PriceRuleValueType.PositivePercent;
                else
                    result.type = PriceRuleValueType.Numeric;
                break;
        }

        // Special treatment for calculation on two 0% expressions
        if (left.type == PriceRuleValueType.PositivePercent && left.value == 0)
            result.value = right.value;
        else if (right.type == PriceRuleValueType.PositivePercent && right.value == 0)
            result.value = left.value;
        else
            result.value = leftValue * rightValue;

        return result;
    }

    private addWidgetToRule(left: boolean, currentRule: PriceRuleDTO, widget: FormulaWidget) {
        if (left) {
            currentRule.lValueType = widget.priceRuleType;
            currentRule.lExampleType = widget.priceRuleValueType;
            if (widget.data)
                currentRule.lValue = widget.data.percent;
            else if (widget.priceRuleType == PriceRuleItemType.GNP)
                currentRule.lValue = 100;
            else
                currentRule.lValue = 0;
        }
        else {
            currentRule.rValueType = widget.priceRuleType;
            currentRule.rExampleType = widget.priceRuleValueType;
            if (widget.data)
                currentRule.rValue = widget.data.percent;
            else if (widget.priceRuleType == PriceRuleItemType.GNP)
                currentRule.rValue = 100;
            else
                currentRule.rValue = 0;
        }

        if (widget.data?.useNetPrice != undefined) {
            currentRule.useNetPrice = widget.data?.useNetPrice;
        }
    }

    private convertWidgetsToPriceRule(): PriceRuleDTO {
        const outerRule: PriceRuleDTO = new PriceRuleDTO();
        let currentRule: PriceRuleDTO = outerRule;

        let previousRule: PriceRuleDTO = null;
        let previousRules: PriceRuleDTO[] = [];

        let leftSide: boolean = true;
        let restruct: boolean = false;
        let restructOperatorType: number = 0;
        
        // Get number of expressions to nest
        let expressions = _.filter(this.widgets, i => i.isExpression);
        let numberOfExpressions = expressions.length;

        let parenthesis = _.filter(this.widgets, i => i.priceRuleType == PriceRuleItemType.StartParanthesis || i.priceRuleType == PriceRuleItemType.EndParanthesis);
        let usesBlocks = parenthesis.length > 0;

        //_.forEach(_.sortBy(this.widgets, 'sort'), (widget) => {
        for (const widget of _.sortBy(this.widgets, 'sort'))
        {
            if (widget.priceRuleType == PriceRuleItemType.StartParanthesis) {
                if (numberOfExpressions <= 2 && !usesBlocks) {
                    leftSide = true;
                    previousRule = currentRule;
                    previousRules.push(previousRule);
                    restruct = false;
                }
            }
            else if (widget.priceRuleType == PriceRuleItemType.EndParanthesis) {

                if (numberOfExpressions <= 2 && !usesBlocks) {
                    previousRule = previousRules.length > 0 ? previousRules[previousRules.length - 1] : null;

                    if (previousRule.lValue == null)
                        leftSide = true;

                    if (leftSide == true)
                        previousRule.lRule = currentRule;
                    else
                        previousRule.rRule = currentRule;

                    if (previousRules.length > 0)
                        previousRules.splice(previousRules.length - 1);

                    currentRule = previousRule;
                    leftSide = true;
                    restruct = false;
                }
            }
            else if (widget.priceRuleType == PriceRuleItemType.Multiplication || widget.priceRuleType == PriceRuleItemType.Or) {

                if (restruct)
                    restructOperatorType = widget.priceRuleType;
                else
                    currentRule.operatorType = widget.priceRuleType;

                leftSide = false;
            }
            else {
                if (leftSide) {
                    restruct = false;
                    this.addWidgetToRule(true, currentRule, widget);
                }
                else {
                    //leftside == false
                    if (restruct) {

                        previousRule = currentRule;
                        previousRules.push(previousRule);
                        currentRule = new PriceRuleDTO();
                        var assignDynamicRuleToLeftSide: boolean = false;

                        if (!previousRule.lRule && previousRule.rRule)
                            assignDynamicRuleToLeftSide = true;

                        if (assignDynamicRuleToLeftSide) {
                            //Move values
                            currentRule.lValue = previousRule.lValue;
                            currentRule.lValueType = previousRule.lValueType;
                            currentRule.lExampleType = previousRule.lExampleType;
                            currentRule.useNetPrice = previousRule.useNetPrice;

                            //Reset parent rules value side
                            previousRule.lValue = null;
                            previousRule.lValueType = null;
                            previousRule.lExampleType = 0;
                            previousRule.useNetPrice = false;

                            //Set stored operator
                            currentRule.operatorType = restructOperatorType;

                            //Set third+ value that initiated the breakdown
                            this.addWidgetToRule(false, currentRule, widget);


                            //Assign new rule
                            previousRule.lRule = currentRule;
                        }
                        else {

                            //move values
                            currentRule.lValue = previousRule.rValue;
                            currentRule.lValueType = previousRule.rValueType;
                            currentRule.lExampleType = previousRule.rExampleType;
                            currentRule.useNetPrice = previousRule.useNetPrice;

                            //Reset parent rules value side
                            previousRule.rValue = null;
                            previousRule.rValueType = null;
                            previousRule.rExampleType = 0;
                            previousRule.useNetPrice = false;

                            //Set stored operator
                            currentRule.operatorType = restructOperatorType;

                            //Set third+ value that initiated the breakdown
                            this.addWidgetToRule(false, currentRule, widget);

                            //Assign new rule
                            previousRule.rRule = currentRule;
                        }

                        restruct = true;
                        currentRule = previousRule;
                        if (previousRules.length > 0)
                            previousRules.splice(previousRules.length - 1);

                    }
                    else {
                        // restruct == false
                        this.addWidgetToRule(false, currentRule, widget);

                        restruct = true;
                    }
                }
            }
        }

        return outerRule;
    }
}

export class InterpretFormat {
    public type: PriceRuleItemType;
    public value?: number;
    public useNetPrice?: boolean;

    constructor(type: PriceRuleItemType, value?: number, useNetPrice?: boolean) {
        this.type = type;
        this.value = value;
        this.useNetPrice = useNetPrice;
    }
}

export class RuleResult {
    public type: PriceRuleValueType;
    public value?: number;

    constructor(type: PriceRuleValueType, value?: number) {
        this.type = type;
        this.value = value;
    }
}
