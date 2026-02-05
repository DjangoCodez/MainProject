import '../Module';

import { AccountingService as SharedAccountingService } from '../../../Shared/Economy/Accounting/AccountingService';
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { HierarchyAccountsDirectiveFactory } from './Directives/HierarchyAccounts/HierarchyAccountsDirective';
import { CategoriesDirectiveFactory } from '../../../Common/Directives/Categories/CategoriesDirective';
import { AccountingSettingsDirectiveFactory } from '../../../Common/Directives/AccountingSettings/AccountingSettingsDirective';
import { SkillsDirectiveFactory } from "../../../Common/Directives/Skills/SkillsDirective";
import { EmployeeStatisticDirectiveFactory } from "./Directives/EmployeeStatistic/EmployeeStatisticDirective";

angular.module("Soe.Time.Schedule.ShiftTypes.Module", ['Soe.Time.Schedule'])
    .service("sharedAccountingService", SharedAccountingService)
    .directive("hierarchyAccounts", HierarchyAccountsDirectiveFactory.create)
    .directive("categories", CategoriesDirectiveFactory.create)
    .directive("accountingSettings", AccountingSettingsDirectiveFactory.create)
    .directive("skills", SkillsDirectiveFactory.create)
    .directive("employeeStatistic", EmployeeStatisticDirectiveFactory.create); 
