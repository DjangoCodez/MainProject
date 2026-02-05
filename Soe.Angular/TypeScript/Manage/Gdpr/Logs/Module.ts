import '../../Module';
import { EntityLogViewerDirectiveFactory } from '../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';

angular.module("Soe.Manage.Gdpr.Logs.Module", ['Soe.Manage'])
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create);
