import '../Module';

import { EmployeeService } from "../EmployeeService";
import { SkillsDirectiveFactory } from "../../../Common/Directives/Skills/SkillsDirective";

angular.module("Soe.Time.Employee.Positions.Module", ['Soe.Time.Employee'])
    .directive("skills", SkillsDirectiveFactory.create);
