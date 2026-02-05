import '../../Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ContactPersonService } from "../../../Common/Directives/ContactPersons/ContactPersonService";
import { CategoriesDirectiveFactory } from "../../../Common/Directives/Categories/CategoriesDirective" 

angular.module("Soe.Manage.ContactPerson.ContactPersons.Module", ['Soe.Manage', 'Soe.Core'])
    .service("contactPersonService", ContactPersonService)
    .directive("categories", CategoriesDirectiveFactory.create)
