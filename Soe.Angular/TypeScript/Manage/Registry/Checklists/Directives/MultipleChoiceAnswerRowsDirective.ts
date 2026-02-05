import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
//import { MultipleChoiceAnswerRowsDialogController } from "./MultipleChoiceAnswerRowsDialogController";
import { ICoreService } from "../..//../../Core/Services/CoreService";
import { IRegistryService } from "../../RegistryService";
import { CheckListMultipleChoiceAnswerRowDTO } from "../../../../Common/Models/CheckListMultipleChoiceAnswerRowDTO";
import { MultipleChoiceAnswerDialogController } from "../Dialogs/MultipleChoiceAnswerDialogController"


export class MultipleChoiceAnswerRowsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/Registry/Checklists/Directives/Views/MultipleChoiceAnswerRows.html'),
            scope: {
                answerHeadId: '=',                
            },
            restrict: 'E',
            replace: true,
            controller: MultipleChoiceAnswerRowsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class MultipleChoiceAnswerRowsController {
    // Data
    private answerHeadId: number;
    private answerRows: CheckListMultipleChoiceAnswerRowDTO[];

    // Flags
    

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private registryService: IRegistryService) {

        this.$q.all([
            this.loadMultipleChoiceAnswerRows(),
        ]).then(() => { });
    }
    

    // SERVICE CALLS    

    private loadMultipleChoiceAnswerRows(): ng.IPromise<any> {
        return this.registryService.getMultipleChoiceAnswerRows(this.answerHeadId).then(x => {
            this.answerRows = x;
        });
    }

    // EVENTS    

    private editAnswer(answer: CheckListMultipleChoiceAnswerRowDTO) {
        var result: any;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/Registry/Checklists/Dialogs/Views/MultipleChoiceAnswer.html"),
            controller: MultipleChoiceAnswerDialogController,
            controllerAs: "ctrl",
            size: 'sm',
            resolve: {                
                answer: () => { return answer },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.answer) {
                if (!answer) {
                    // Add new anwer to the original collection
                    answer = new CheckListMultipleChoiceAnswerRowDTO();
                    this.updateAnswer(answer, result.answer);
                    this.answerRows.push(answer);
                } else {
                    // Update original answer
                    var originalAnswer = _.find(this.answerRows, v => v.checkListMultipleChoiceAnswerRowId === answer.checkListMultipleChoiceAnswerRowId);
                    if (originalAnswer)
                        this.updateAnswer(originalAnswer, result.answer);
                }                                
            }
        });
    }

    private updateAnswer(answer: CheckListMultipleChoiceAnswerRowDTO, input: CheckListMultipleChoiceAnswerRowDTO) {
        answer.checkListMultipleChoiceAnswerHeadId = input.checkListMultipleChoiceAnswerHeadId;
        answer.question = input.question;        
    }

    private deleteAnswer(answer: CheckListMultipleChoiceAnswerRowDTO) {
        _.pull(this.answerRows, answer);               
    }

    // HELP-METHODS

    
   
}