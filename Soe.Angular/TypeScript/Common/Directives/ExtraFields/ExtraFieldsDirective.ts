import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SoeEntityType, Feature, TermGroup_ExtraFieldType } from "../../../Util/CommonEnumerations";
import { Guid } from "../../../Util/StringUtility";
import { ExtraFieldRecordDTO } from "../../Models/ExtraFieldDTO";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class ExtraFieldRecordsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('ExtraFields', 'ExtraFields.html'),
            scope: {
                parentGuid: '=?',
                readOnly: '=?',
                recordId: '=',
                entity: '=',
                extraFieldRecords: '=',
                conncectedEntity: "=?",
                connectedRecordId: "=?"
            },
            restrict: 'E',
            replace: true,
            controller: ExtraFieldRecordsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class ExtraFieldRecordsController extends GridControllerBaseAg {

    // Init parameters
    private parentGuid: Guid;
    private recordId: number;
    private readOnly: boolean;
    private entity: SoeEntityType;
    private conncectedEntity: number;
    private connectedRecordId: number;

    // Collections

    // Lookups
    private extraFieldRecords: ExtraFieldRecordDTO[];
    private terms: { [index: string]: string; };
    private yesNoDict: ISmallGenericType[] = [];

    // Flags
    rowsRendered = false;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $compile: ng.ICompileService) {

        super("Common.Directives.Categories", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.$scope.$on('reloadExtraFields', (e, a) => {
            if (a.guid === this.parentGuid && a.recordId && a.recordId > 0) {
                this.rowsRendered = false;
                this.loadExtraFieldRecords().then(() => {
                    this.setExtraFieldRecordsData();
                    this.createRecordHtml();
                });
            }
        });
    }

    public $onInit() {
        this.setup();

        //need to fetch the form object from the parent.
        const parentForm = ((<any>this.$scope.$parent).ctrl || {}).edit;
        (<any>this.$scope).ctrl = {
            edit: parentForm
        };
    }

    // Lookups
    private setup() {
        this.progressBusy = true;

        this.$q.all([
            this.loadTerms(),
            this.loadExtraFieldRecords()
        ]).then(() => {
            this.$timeout(() => {
                this.setExtraFieldRecordsData();
                this.createRecordHtml();
                this.progressBusy = false;
            });
        });
    }

    private loadTerms(): ng.IPromise<any> {
        // Columns
        const keys: string[] = [
            "core.yes",
            "core.no",
            "core.warning",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.yesNoDict.push({ id: 0, name: "" });
            this.yesNoDict.push({ id: 1, name: this.terms["core.yes"] });
            this.yesNoDict.push({ id: 2, name: this.terms["core.no"] });
        });
    }

    private loadExtraFieldRecords(): ng.IPromise<any> {
        if (!this.extraFieldRecords)
            this.extraFieldRecords = [];

        return this.coreService.getExtraFieldWithRecords(this.recordId, this.entity, CoreUtility.languageId, this.conncectedEntity, this.connectedRecordId).then(fields => {
            fields.forEach(field => {
                if (this.extraFieldRecords.length > 0) {
                    // If extra field records are passed from outside (e.g. from parent entity), map existing values
                    const existingRecord = this.extraFieldRecords.find(ef => ef.extraFieldId === field.extraFieldId);
                    if (existingRecord) {
                        field.boolData = existingRecord.boolData;
                        field.dateData = existingRecord.dateData;
                        field.decimalData = existingRecord.decimalData;
                        field.intData = existingRecord.intData;
                        field.strData = existingRecord.strData;
                        field.comment = existingRecord.comment;
                    }
                }
            });
            this.extraFieldRecords = fields;
        });
    }

    private setExtraFieldRecordsData() {
        _.forEach(this.extraFieldRecords, (r) => {
            if (r.extraFieldType === TermGroup_ExtraFieldType.Date)
                r.dateData = CalendarUtility.convertToDate(r.dateData);
        })
    }

    private createRecordHtml() {
        this.$timeout(() => {
            let recordCounter = 0;
            _.forEach(this.extraFieldRecords, (r) => {
                const attachmentPoint = $(`.div-${r.extraFieldId}-${this.parentGuid}`);
                attachmentPoint.empty();

                const html = angular.element(this.createRowHtml(r, recordCounter));
                attachmentPoint.append(html);

                this.$compile(html)(this.$scope);

                recordCounter = recordCounter + 1;
            });
        }, 500).then(() => {
            this.rowsRendered = true;
        });
    }

    private createRowHtml(record: ExtraFieldRecordDTO, recordCounter: number) {

        // Add row
        const row = recordCounter > 0 ? $('<div class="row margin-large-top">') : $('<div class="row">')

        // Setup watchers
        this.$scope.$watch(() => record.strData, (newValue, oldValue) => this.generalWatch(newValue, oldValue, record), true);
        this.$scope.$watch(() => record.boolData, (newValue, oldValue) => this.generalWatch(newValue, oldValue, record), true);
        this.$scope.$watch(() => record.dateData, (newValue, oldValue) => this.generalWatch(newValue, oldValue, record), true);
        this.$scope.$watch(() => record.decimalData, (newValue, oldValue) => this.generalWatch(newValue, oldValue, record), true);
        this.$scope.$watch(() => record.intData, (newValue, oldValue) => this.generalWatch(newValue, oldValue, record), true);

        this.$scope.$watch(() => record.comment, (newValue, oldValue) => {
            if (newValue !== oldValue) {
                this.$timeout(() => {
                    this.$scope.$apply(() => {
                        record.isModified = true;
                        this.setAsDirty();
                    });
                });
            }
        }, true);


        // Add columns
        const value = $('<div class="col-sm-4">');
        const valueContent = this.getAnswerContent(record, recordCounter);
        value.append(valueContent);

        /*const comment = $('<div class="col-sm-6">');
        const commentContent = $('<soe-textbox>');
        commentContent.attr('hidelabel', 'true');
        commentContent.attr('is-readonly', 'directiveCtrl.readOnly');
        commentContent.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].comment');
        comment.append(commentContent);*/

        row.append(value);
        //row.append(comment);

        return row;
    }

    private generalWatch(newValue, oldValue, record) {
        if (newValue !== undefined && newValue !== oldValue && this.rowsRendered) {
            this.$timeout(() => {
                this.$scope.$apply(() => {
                    record.isModified = true;
                    this.setAsDirty();
                });
            });
        }
    }

    private getAnswerContent(row: ExtraFieldRecordDTO, recordCounter: number) {
        switch (row.extraFieldType) {
            case TermGroup_ExtraFieldType.FreeText:
                const textBoxContect = $('<soe-textbox>');
                textBoxContect.attr('label-value', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldText');
                textBoxContect.attr('is-readonly', 'directiveCtrl.readOnly');
                textBoxContect.attr('label-value-indiscreet', 'true');
                textBoxContect.attr('update-on', 'blur');
                textBoxContect.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].strData');
                return textBoxContect;
            case TermGroup_ExtraFieldType.Integer:
                const integerContect = $('<soe-textbox numeric="true" decimals="0">');
                integerContect.attr('label-value', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldText');
                integerContect.attr('is-readonly', 'directiveCtrl.readOnly');
                integerContect.attr('label-value-indiscreet', 'true');
                integerContect.attr('update-on', 'blur');
                integerContect.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].intData');
                return integerContect;
            case TermGroup_ExtraFieldType.Decimal:
                const decimalsContect = $('<soe-textbox numeric="true" decimals="4">');
                decimalsContect.attr('label-value', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldText');
                decimalsContect.attr('is-readonly', 'directiveCtrl.readOnly');
                decimalsContect.attr('label-value-indiscreet', 'true');
                decimalsContect.attr('update-on', 'blur');
                decimalsContect.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].decimalData');
                return decimalsContect;
            case TermGroup_ExtraFieldType.YesNo:
                //Set value
                if (!row.intData)
                    row.intData = 0;

                //Create content
                const yesNoContent = $('<soe-select>');
                yesNoContent.attr('label-value', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldText');
                yesNoContent.attr('label-value-indiscreet', 'true');
                yesNoContent.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].intData');
                yesNoContent.attr('is-readonly', 'directiveCtrl.readOnly');
                yesNoContent.attr('items', 'directiveCtrl.yesNoDict');
                yesNoContent.attr('options', 'item.id as item.name for item in items');
                return yesNoContent;
            case TermGroup_ExtraFieldType.Checkbox:
                const checkBoxContent = $('<soe-checkbox>');
                checkBoxContent.attr('label-value', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldText');
                checkBoxContent.attr('indiscreet', 'true');
                checkBoxContent.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].boolData');
                checkBoxContent.attr('is-readonly', 'directiveCtrl.readOnly');
                return checkBoxContent;
            case TermGroup_ExtraFieldType.Date:
                const dateContent = $('<soe-datepicker>');
                dateContent.attr('label-value', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldText');
                dateContent.attr('label-value-indiscreet', 'true');
                dateContent.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].dateData');
                dateContent.attr('is-readonly', 'directiveCtrl.readOnly');
                return dateContent;
            case TermGroup_ExtraFieldType.SingleChoice:
                const singleChoiceContent = $('<soe-select>');
                singleChoiceContent.attr('label-value', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldText');
                singleChoiceContent.attr('label-value-indiscreet', 'true');
                singleChoiceContent.attr('model', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].intData');
                singleChoiceContent.attr('is-readonly', 'directiveCtrl.readOnly');
                singleChoiceContent.attr('items', 'directiveCtrl.extraFieldRecords[' + recordCounter + '].extraFieldValues');
                singleChoiceContent.attr('options', 'item.extraFieldValueId as item.value for item in items');
                return singleChoiceContent;
            /*case TermGroup_ChecklistRowType.MultipleChoice:
                //Create content
                var content = $('<soe-select>');
                content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].intData');
                content.attr('is-readonly', '!directiveCtrl.hasAnswerChecklistsModifyPermission || directiveCtrl.readOnly');
                content.attr('hidelabel', 'true');
                var items: any[] = [];
                _.forEach(_.filter(this.multipleChoiceQuestions, { checkListMultipleChoiceAnswerHeadId: row.checkListMultipleChoiceAnswerHeadId }), (item) => {
                    items.push({ id: item.checkListMultipleChoiceAnswerRowId, name: item.question });
                });
                row['selectOption'] = items;
                content.attr('items', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].selectOption');
                content.attr('options', 'item.id as item.name for item in items');
                return content;*/
        }
    }

    private setAsDirty(dirty = true) {
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {
            guid: this.guid,
            dirty: dirty
        })
    }
}