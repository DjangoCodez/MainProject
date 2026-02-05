import '../../Core/Module'

import { TranslationsDirectiveFactory } from '../Directives/Translations/TranslationsDirective';

angular.module("Soe.Common.ExtraFields.Module", ['Soe.Core'])
    .directive("compTerms", TranslationsDirectiveFactory.create)
