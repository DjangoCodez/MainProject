import '../../../Shared/Billing/Module';

import { ExpressionContainerDirectiveFactory } from '../../../Common/FormulaBuilder/ExpressionContainerDirective';
import { FormulaContainerDirectiveFactory } from '../../../Common/FormulaBuilder/FormulaContainerDirective';
import { WidgetDirectiveFactory } from '../../../Common/FormulaBuilder/WidgetDirective';
import { StartParanthesisWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/StartParanthesisWidgetDirective';
import { EndParanthesisWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/EndParanthesisWidgetDirective';
import { MultiplicationWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/MultiplicationWidgetDirective';
import { GnpWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/GnpWidgetDirective';
import { SupplierAgreementWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/SupplierAgreementWidgetDirective';
import { GainWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/GainWidgetDirective';
import { MarkupWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/MarkupWidgetDirective';
import { CustomerDiscountWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/CustomerDiscountWidgetDirective';
import { PriceBasedMarkupWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/PriceBasedMarkupWidgetDirective';
import { NetPriceWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/NetPriceWidgetDirective';
import { OrWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/OrWidgetDirective';

angular.module("Soe.Billing.Invoices.PriceRules.Module", ['Soe.Shared.Billing'])
    .directive("expressionContainer", ExpressionContainerDirectiveFactory.create)
    .directive("formulaContainer", FormulaContainerDirectiveFactory.create)
    .directive("widget", WidgetDirectiveFactory.create)
    .directive("startParanthesisWidget", StartParanthesisWidgetDirectiveFactory.create)
    .directive("endParanthesisWidget", EndParanthesisWidgetDirectiveFactory.create)
    .directive("multiplicationWidget", MultiplicationWidgetDirectiveFactory.create)
    .directive("orWidget", OrWidgetDirectiveFactory.create)
    .directive("gnpWidget", GnpWidgetDirectiveFactory.create)
    .directive("supplierAgreementWidget", SupplierAgreementWidgetDirectiveFactory.create)
    .directive("gainWidget", GainWidgetDirectiveFactory.create)
    .directive("markupWidget", MarkupWidgetDirectiveFactory.create)
    .directive("customerDiscountWidget", CustomerDiscountWidgetDirectiveFactory.create)
    .directive("priceBasedMarkupWidget", PriceBasedMarkupWidgetDirectiveFactory.create)
    .directive("netPriceWidget", NetPriceWidgetDirectiveFactory.create);
