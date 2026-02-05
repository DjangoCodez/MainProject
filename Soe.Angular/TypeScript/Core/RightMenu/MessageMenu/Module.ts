import { ScheduleService as SharedScheduleService } from '../../../Shared/Time/Schedule/ScheduleService';
import { ReceiversListDirectiveFactory } from '../../../Common/Directives/ReceiversList/ReceiversListDirective';
import { AvailableEmployeesDirectiveFactory } from '../../../Common/Directives/AvailableEmployees/AvailableEmployeesDirective';
import { TermPartsLoaderProvider } from '../../Services/termpartsloader';
import { ITranslationService } from '../../Services/TranslationService';


var module = angular.module("Soe.Core.RightMenu.MessageMenu.Module", [])
    .service("sharedScheduleService", SharedScheduleService)
    .directive("receiversListForMessage", ReceiversListDirectiveFactory.create)
    .directive("availableEmployeesForMessage", AvailableEmployeesDirectiveFactory.create)
    .config(/*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('time');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });

export default module;
