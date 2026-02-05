import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IRegistryService } from "../../RegistryService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { ChecklistRowDTO } from "../../../../Common/Models/ChecklistRowDTO"
import { TermGroup } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ChecklistRowDialogController } from "../Dialogs/ChecklistRowDialogController";


export class ChecklistRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        
        return {
            
            templateUrl: urlHelperService.getGlobalUrl('Manage/Registry/Checklists/Directives/Views/ChecklistRows.html'),
            scope: {                
                checklistRows: '=',                
                checklistHeadId: '=',
                parentGuid: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: ChecklistRowsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true            
        };
    }
}

export class ChecklistRowsDirectiveController {
    // Setup    
    private parentGuid: string;
    checklistRows: ChecklistRowDTO[] = [];            
    checklistRow: ChecklistRowDTO;
    checklistHeadId: number;
    private selectedRow: ChecklistRowDTO;    
    private yesNoDict: any[] = []; 
    private checklistRowTypes: any[];

    //@ngInject
    constructor($http,
        $templateCache,        
        private $uibModal,
        private $filter: ng.IFilterService,
        protected coreService: ICoreService,        
        private translationService: ITranslationService,        
        private registryService: IRegistryService,        
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private $window: ng.IWindowService,        
        private $q: ng.IQService,
        private $scope: ng.IScope) {                
    }
    
    public $onInit() {
        this.loadYesNoDict();
        this.loadChecklistRowAnswerTypes();
        
    }

    private loadYesNoDict(): ng.IPromise<any> {
        var keys: string[] = [
            "core.yes",
            "core.no",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.yesNoDict.push({ id: 1, name: terms["core.yes"] })
            this.yesNoDict.push({ id: 2, name: terms["core.no"] })
        });
    }

    private loadChecklistRowAnswerTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChecklistRowType, false, false).then(x => {
            this.checklistRowTypes = x;
        });
    }

    private editChecklistRow(checklistRow: ChecklistRowDTO, rowIndex: number) {        
       
        var result: any;        

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/Registry/Checklists/Dialogs/Views/ChecklistRow.html"),
            controller: ChecklistRowDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                checklistRow: () => { return checklistRow },
                rowNr: () => { return this.getNextRowNr() }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.checklistRow) {                
                if (!checklistRow)
                    this.addChecklistRow(result.checklistRow)
                else
                    this.updateChecklistRow(checklistRow, result.checklistRow, rowIndex);
                
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
            }
        });        
    }

    private addChecklistRow(input: ChecklistRowDTO) {

        var nextRowNr = this.getNextRowNr();
        var rowNrIncreased: boolean = input.rowNr < nextRowNr ? false : true;        

        var checklistRow = new ChecklistRowDTO();

        checklistRow.checklistHeadId = this.checklistHeadId;
        checklistRow.checklistRowId = 0;
        checklistRow.checkListMultipleChoiceAnswerHeadId = input.checkListMultipleChoiceAnswerHeadId;
        checklistRow.rowNr = input.rowNr;
        checklistRow.text = input.text;
        checklistRow.type = input.type;

        var type = _.find(this.checklistRowTypes, { id: input["type"] })
        checklistRow.typeName = type ? type.name : "";

        checklistRow.mandatory = input.mandatory;
        checklistRow.mandatoryName = input.mandatory ? this.yesNoDict[0].name : this.yesNoDict[1].name;

        checklistRow.isModified = true;
                
        this.checklistRows.push(checklistRow);

        this.reNumberRows(this.checklistRows.length-1, rowNrIncreased);
    }

    private updateChecklistRow(checklistRow: ChecklistRowDTO, input: ChecklistRowDTO, rowIndex: number) {
        
        var rowNrIncreased: boolean = input.rowNr > checklistRow.rowNr ? true : false;        

        checklistRow.checklistHeadId = this.checklistHeadId;
        checklistRow.checkListMultipleChoiceAnswerHeadId = input.checkListMultipleChoiceAnswerHeadId;
        checklistRow.rowNr = input.rowNr;
        checklistRow.text = input.text;
        checklistRow.type = input.type;

        var type = _.find(this.checklistRowTypes, { id: input["type"] })
        checklistRow.typeName = type.name;

        checklistRow.mandatory = input.mandatory;
        checklistRow.mandatoryName = input.mandatory ? this.yesNoDict[0].name : this.yesNoDict[1].name;

        checklistRow.mandatory = input.mandatory;
        checklistRow.isModified = checklistRow.checklistRowId ? true : false;
        
        this.reNumberRows(rowIndex, rowNrIncreased);        
    }

    private deleteChecklistRow(checklistRow: ChecklistRowDTO) {
        _.pull(this.checklistRows, checklistRow);
        this.reNumberRows();
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);        
    }

    private getNextRowNr(): number {
        var rowNr = rowNr = 0;

        if (this.checklistRows.length > 0)
            rowNr = this.checklistRows[this.checklistRows.length - 1].rowNr;
        
        return rowNr + 1;
    }

    private reNumberRows(rowIndex: number = 0, rowNrIncreased: boolean = false) {                     

        var index = 0;
        
        _.forEach(this.checklistRows, x => {
            
            x.rowNr *= 10;
            
            if (index == rowIndex) {
                if (rowNrIncreased)
                    x.rowNr += 1;
                else
                    x.rowNr -= 1;                       
            }            
            
            index++;
        });                      
        
        this.checklistRows = _.orderBy(this.checklistRows, 'rowNr');
        
        var rowNr: number = 1;

        for (var i = 0; i < this.checklistRows.length; i++) {
            this.checklistRows[i].rowNr = rowNr;
            rowNr++;
        }                

    }
    
}