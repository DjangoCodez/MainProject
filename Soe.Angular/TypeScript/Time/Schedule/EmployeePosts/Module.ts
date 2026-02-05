import '../Module';

import { SkillsDirectiveFactory } from "../../../Common/Directives/Skills/SkillsDirective";
import { EmployeeService } from '../../../Time/Employee/EmployeeService';

angular.module("Soe.Time.Schedule.EmployeePosts.Module", ['Soe.Time.Schedule'])
    .directive("skills", SkillsDirectiveFactory.create)
    .service("employeeService", EmployeeService);
