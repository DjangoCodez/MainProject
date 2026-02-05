import { ChecklistRowDTO } from "../../../../Common/Models/ChecklistRowDTO"
import { NotificationService } from "../../../../Core/Services/NotificationService";
import { TranslationService } from "../../../../Core/Services/TranslationService";
import { ICoreService, CoreService } from "../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { TermGroup, TermGroup_ChecklistRowType } from "../../../../Util/CommonEnumerations";
import { MultipleChoiceDialogController } from "../Dialogs/MultipleChoiceDialogController"

export class ChecklistRowDialogController {

    private checklistRow: ChecklistRowDTO;
    private isNew: boolean;
    private checklistRowTypes: any[];

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: TranslationService,
        private notificationService: NotificationService,
        private coreService: CoreService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $uibModal,
        checklistRow: ChecklistRowDTO,
        rowNr: number) {

        this.isNew = !checklistRow;      
        this.checklistRow = new ChecklistRowDTO();

        angular.extend(this.checklistRow, checklistRow);  

        if (this.isNew)
            this.checklistRow.rowNr = rowNr;
    }

    public $onInit() {
        this.$q.all([
            this.loadChecklistRowAnswerTypes(),
            ]);
    }

    private loadChecklistRowAnswerTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChecklistRowType, false, false).then(x => {
            this.checklistRowTypes = x;
        });
    }

    private rowTypeChanged(selectedType: any) {        
        if (selectedType == TermGroup_ChecklistRowType.MultipleChoice) 
            this.openMultipleChoiceDialog();                        
    }

    private openMultipleChoiceDialog() {
        var result: any;

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/Registry/Checklists/Dialogs/Views/MultipleChoice.html"),
            controller: MultipleChoiceDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                multipleChoiceAnswerHeadId: () => { return this.checklistRow.checkListMultipleChoiceAnswerHeadId },
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.multipleChoiceAnswerHeadId) {
                this.checklistRow.checkListMultipleChoiceAnswerHeadId = result.multipleChoiceAnswerHeadId;
            }
        });  
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {        
        this.$uibModalInstance.close({ checklistRow: this.checklistRow });
    }
    
}
