import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IRegistryService } from "../../RegistryService";
import { CheckListMultipleChoiceAnswerHeadDTO } from "../../../../Common/Models/CheckListMultipleChoiceAnswerHeadDTO";
import { CheckListMultipleChoiceAnswerRowDTO } from "../../../../Common/Models/CheckListMultipleChoiceAnswerRowDTO";
import { MultipleChoiceAnswerDialogController } from "./MultipleChoiceAnswerDialogController";

export class MultipleChoiceDialogController {

    private answerHead: CheckListMultipleChoiceAnswerHeadDTO;
    private answerHeads: CheckListMultipleChoiceAnswerHeadDTO[];    
    private answerRows: CheckListMultipleChoiceAnswerRowDTO[] = [];

    private newQuestion: string;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $uibModal,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private registryService: IRegistryService,
        private $q: ng.IQService,
        private multipleChoiceAnswerHeadId: number,
    ) {

        this.$q.all([
            this.loadMultipleChoiceAnswerHeads(),
        ]).then(() => {            
            if (this.answerHead.checkListMultipleChoiceAnswerHeadId)
                this.loadMultipleChoiceAnswerRows(this.answerHead.checkListMultipleChoiceAnswerHeadId);
        });
    }

    private loadMultipleChoiceAnswerHeads(): ng.IPromise<any> {
        return this.registryService.getMultipleChoiceAnswerHeads().then((x) => {
            this.answerHeads = x;

            this.answerHead = _.find(this.answerHeads, { checkListMultipleChoiceAnswerHeadId: this.multipleChoiceAnswerHeadId });

            if (!this.answerHead) {
                this.answerHead = new CheckListMultipleChoiceAnswerHeadDTO();
                this.answerHead.checkListMultipleChoiceAnswerHeadId = 0;
            }
            
        });
    }

    private loadMultipleChoiceAnswerRows(answerHeadId: number): ng.IPromise<any> {
        return this.registryService.getMultipleChoiceAnswerRows(answerHeadId).then((x) => {
            this.answerRows = x;            
        });
    }
    
    private answerHeadChanged(answerHeadId: any) {

        this.answerHead = _.find(this.answerHeads, { checkListMultipleChoiceAnswerHeadId: answerHeadId });
       // this.answerHead.title = this.answerHead.title;

        this.loadMultipleChoiceAnswerRows(answerHeadId);
    }

    private addAnswer() {

        var result: any;
        var answerRow = new CheckListMultipleChoiceAnswerRowDTO();

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/Registry/Checklists/Dialogs/Views/MultipleChoiceAnswer.html"),
            controller: MultipleChoiceAnswerDialogController,
            controllerAs: "ctrl",
            size: 'sm',
            resolve: {
                answerRow: () => { return answerRow }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            console.log(result);
            if (result) {                

                answerRow.checkListMultipleChoiceAnswerHeadId = this.answerHead ? this.answerHead.checkListMultipleChoiceAnswerHeadId : 0;
                answerRow.checkListMultipleChoiceAnswerRowId = 0;
                answerRow.question = result.answerRow.question;

                this.answerRows.push(answerRow);
            }
        });        
    }

    private deleteAnswer(answerRow: CheckListMultipleChoiceAnswerRowDTO) {
        _.pull(this.answerRows, answerRow);
        console.log(this.answerRows);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public save() {
        this.registryService.saveMultipleChoiceAnswerHead(this.answerHead, this.answerRows).then((result) => {
            if (result.success) {
                console.log("saveresult");
                console.log(result);
                //this.multipleChoiceAnswerHeadId = result.integerValue;
                this.$uibModalInstance.close({ multipleChoiceAnswerHeadId: result.integerValue });
            }
            else {

            }
        });

    }

    
}




