import '../Module';
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PayrollService } from '../PayrollService';
import { TimeService } from '../../Time/TimeService';
import { AccountingSettingsDirectiveFactory } from '../../../Common/Directives/AccountingSettings/AccountingSettingsDirective';

angular.module("Soe.Time.Payroll.PayrollImports.Module", ['Soe.Time.Payroll'])
    .service("payrollService", PayrollService)
    .service("timeService", TimeService)
    .directive("accountingSettings", AccountingSettingsDirectiveFactory.create)
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });
