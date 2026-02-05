import { HtmlUtility } from "../../../../../Util/HtmlUtility";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ChecklistHeadRecordCompactDTO, ChecklistExtendedRowDTO } from "../../../../../Common/Models/checklistdto";
import { IImagesDTO, IChecklistHeadDTO, ICheckListMultipleChoiceAnswerRowDTO, IChecklistExtendedRowDTO, ISmallGenericType, IChecklistHeadRecordCompactDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ToolBarButtonGroup } from "../../../../../Util/ToolBarUtility";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IReportService } from "../../../../../Core/Services/ReportService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SelectReportController } from "../../../../../Common/Dialogs/SelectReport/SelectReportController";
import { TermGroup_ChecklistHeadType, SoeEntityType, SoeEntityState, Feature, SoeReportTemplateType, TermGroup_ChecklistRowType, SettingDataType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { Guid } from "../../../../../Util/StringUtility";
import { EditChecklistRowDialogController } from "../../../../../Shared/Billing/Orders/Directives/Checklists/EditChecklistRowDialogController";


export class ChecklistsDirective {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Orders/Directives/Checklists/Views/Checklists.html'),
            scope: {
                guid: '=?',
                readOnly: '=?',
                recordId: '=',
                records: '=?',
                checklistHeadType: '=',
                entity: '=',
                load: '=',
                copy: '=',
                loaded: '=',
                signatures: '=',
            },
            restrict: 'E',
            replace: true,
            controller: ChecklistsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class ChecklistsController {

    // Init parameters
    private recordId: number;
    private readOnly: boolean;
    private records: ChecklistHeadRecordCompactDTO[] = [];
    private checklistHeadType: TermGroup_ChecklistHeadType;
    private entity: SoeEntityType;
    private selectedChecklistHeadId = 0;
    private load: boolean;
    private copy: boolean;
    private loaded: boolean;
    private signatures: IImagesDTO[];

    // Permissions
    private hasAddChecklistsReadPermission = false;
    private hasAddChecklistsModifyPermission = false;
    private hasAnswerChecklistsReadPermission = false;
    private hasAnswerChecklistsModifyPermission = false;
    private hasReportPermission = false;

    // Company settings

    // Collection
    private reports: any[];
    private heads: IChecklistHeadDTO[];
    private multipleChoiceQuestions: ICheckListMultipleChoiceAnswerRowDTO[];
    private rowRecords: IChecklistExtendedRowDTO[];

    // User settings
    private showPrintIcon = false;

    // Lookups
    private terms: { [index: string]: string; };
    private checklistHeadsDict: ISmallGenericType[] = [];
    private multipleChoiceQuestionIds: number[];
    private yesNoDict: ISmallGenericType[] = [];

    // GUI
    private toolbarInclude: any;
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    // Modal
    private modalInstance: any;

    // Flags
    private progressBusy = false;
    private loadingChecklists = false;
    private loadingSignatures = false;

    private guid: Guid;

    //Watchers
    private activeWatchers = [];
    private scopes: ng.IScope[] = [];


    // Properties
    get activeRows(): ChecklistHeadRecordCompactDTO[] {
        return _.filter(this.records, r => r.state === SoeEntityState.Active);
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        protected coreService: ICoreService,
        private translationService: ITranslationService,
        private reportService: IReportService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $compile: ng.ICompileService,
        private $window: ng.IWindowService) {
    }

    $onInit() {
        this.toolbarInclude = this.urlHelperService.getGlobalUrl("Shared/Billing/Orders/Directives/Checklists/Views/gridHeader.html");

        this.modalInstance = this.$uibModal;

        this.setup();
        this.setupCustomToolBar();

        //need to fetch the form object from the parent.
        var parentForm = ((<any>this.$scope.$parent).ctrl || {}).edit;
        (<any>this.$scope).ctrl = {
            edit: parentForm
        };
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.recordId, (newValue, oldValue) => {
            this.loadingChecklists = true;
            this.loadingSignatures = true;
            this.resetWatchers();
            this.loadChecklistHeadRecords();
        });
        this.$scope.$watch(() => this.copy, (newValue, oldValue) => {
            if (!this.loadingChecklists)
                this.resetIds();
        });

        this.$scope.$on('reloadChecklists', (e, a) => {
            this.resetWatchers();
            this.loadChecklistHeadRecords();
        });
    }

    private resetWatchers = () => {
        //  A large checklist can amount is as much as 500-1000+ watchers.

        //  Since this component creates subcomponents on the fly, this method disposes them when context is switched.
        //  By creating subscopes and destroying them, we can prevent watchers stacking up.

        (this.activeWatchers || []).forEach(w => w());
        this.activeWatchers = [];

        (this.scopes || []).forEach(s => s.$destroy());
        this.scopes = [];
    }

    // Lookups
    private setup() {
        this.progressBusy = true;

        this.$q.all([this.loadTerms(),
        this.loadModifyPermissions(),
        this.loadReadOnlyPermissions(),
        this.loadChecklistHeads(),
        //this.loadChecklistSignatures(),
        this.loadReports()]).then(() => {
            this.$q.all([this.loadMultipleChoiceQuestions()]).then(() => {
                this.showPrintIcon = (this.hasReportPermission && this.reports && this.reports.length > 0);
                this.setupWatchers();
                if (this.recordId === 0)
                    this.addDefaultChecklists();
                this.progressBusy = false;
            });
        });
    }

    protected setupCustomToolBar() {
        if (!this.readOnly) {

        }
    }

    private loadTerms(): ng.IPromise<any> {
        // Columns
        const keys: string[] = [
            "core.yes",
            "core.no",
            "core.warning",
            "core.verifyquestion",
            "billing.invoices.checklists.deletechecklist"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.yesNoDict.push({ id: 0, name: "" });
            this.yesNoDict.push({ id: 1, name: this.terms["core.yes"] });
            this.yesNoDict.push({ id: 2, name: this.terms["core.no"] });
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const features: number[] = [
            Feature.Billing_Order_Checklists_AddChecklists,
            Feature.Billing_Order_Checklists_AnswerChecklists
        ];

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.hasAddChecklistsModifyPermission = x[Feature.Billing_Order_Checklists_AddChecklists];
            this.hasAnswerChecklistsModifyPermission = x[Feature.Billing_Order_Checklists_AnswerChecklists];
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        const features: number[] = [
            Feature.Billing_Order_Checklists_AddChecklists,
            Feature.Billing_Order_Checklists_AnswerChecklists,
            Feature.Billing_Distribution_Reports_Selection,
            Feature.Billing_Distribution_Reports_Selection_Download
        ];

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            this.hasAddChecklistsReadPermission = x[Feature.Billing_Order_Checklists_AddChecklists];
            this.hasAnswerChecklistsReadPermission = x[Feature.Billing_Order_Checklists_AnswerChecklists];
            this.hasReportPermission = x[Feature.Billing_Distribution_Reports_Selection] && x[Feature.Billing_Distribution_Reports_Selection_Download];
        });
    }

    private loadChecklistHeads(): ng.IPromise<any> {
        this.multipleChoiceQuestionIds = [];
        return this.coreService.getChecklistHeads(this.checklistHeadType, true).then(( x: IChecklistHeadDTO[]) => {
            this.heads = x;
            this.heads.forEach( (h) => {
                this.checklistHeadsDict.push({ id: h.checklistHeadId, name: h.name });

                _.forEach(h.checklistRows, (r) => {
                    if (r.checkListMultipleChoiceAnswerHeadId && !_.includes(this.multipleChoiceQuestionIds, r.checkListMultipleChoiceAnswerHeadId))
                        this.multipleChoiceQuestionIds.push(r.checkListMultipleChoiceAnswerHeadId);
                });
            });
        });
    }

    loadMultipleChoiceQuestions(): ng.IPromise<any> {
        if (this.multipleChoiceQuestionIds.length > 0) {
            return this.coreService.getChecklistMultipleChoiceQuestions(this.multipleChoiceQuestionIds).then(x => {
                this.multipleChoiceQuestions = x;
            });
        }
        else {
            return null;
        }
    }

    private loadChecklistHeadRecords(): ng.IPromise<any> {
        if (this.recordId && this.recordId > 0) {
            return this.coreService.getChecklistHeadRecords(this.entity, this.recordId).then(x => {
                this.records = x;
                let counter = 0;
                _.forEach(this.records, (r) => {
                    if (this.copy)
                        r.checklistHeadRecordId = 0;

                    r.rowNr = counter;
                    counter = counter + 1;
                    r.checklistRowRecords = _.sortBy(r.checklistRowRecords, 'rowNr');
                    _.forEach(r.checklistRowRecords, (rr) => {
                        if (this.copy) {
                            rr.headRecordId = 0;
                            rr.rowRecordId = 0;
                        }
                        if (rr.date)
                            rr.date = new Date(<any>rr.date);
                        else
                            rr.date = undefined;

                        if(rr.type === TermGroup_ChecklistRowType.Image)
                            rr.showEditIcon = !this.copy;
                    });

                    r.hideSignatureIcon = (!r.signatures || r.signatures.length === 0);
                });

                this.loaded = true;
                this.copy = false;
                this.loadingChecklists = false;
            });
        }
        else {
            this.loadingChecklists = false;
            this.loadingSignatures = false;
            return null;
        }
    }

    private resetIds() {
        _.forEach(this.records, (r) => {
            r.checklistHeadRecordId = 0;
            _.forEach(r.checklistRowRecords, (rr) => {
                rr.headRecordId = 0;
                rr.rowRecordId = 0;
            });
        });
        this.loaded = false;
    }

    private loadChecklistRowRecords(): ng.IPromise<any> {
        if (this.recordId) {
            return this.coreService.getChecklistRowRecords(this.entity, this.recordId).then(x => {
                this.rowRecords = x;
            });
        }
        else {
            return null;
        }
    }

    /*private loadChecklistSignatures(): ng.IPromise<any> {
        if (this.recordId) {
            return this.coreService.getChecklistSignatures(this.entity, this.recordId, true).then(x => {
                this.loadingSignatures = x.length > 0;
                this.signatures = x;
            });
        }
        else {
            return null;
        }
    }*/

    private loadReports(): ng.IPromise<any> {
        if (this.checklistHeadType === TermGroup_ChecklistHeadType.Order) {
            const reportTypes: number[] = [SoeReportTemplateType.OrderChecklistReport];

            return this.reportService.getReportsForType(reportTypes, true, false).then((x) => {
                this.reports = x;
            });
        }
        else {
            return null;
        }
    }

    // Events
    private addDefaultChecklists() {
        _.forEach(this.heads, (h) => {
            if (h.defaultInOrder)
                this.addChecklist(h);
        });
    }

    private addSelectedChecklist() {
        this.setAsDirty(true);
        this.addChecklist(_.find(this.heads, { checklistHeadId: this.selectedChecklistHeadId }));
    }

    private addChecklist(head: IChecklistHeadDTO) {
        //var head = _.find(this.heads, { checklistHeadId: this.selectedChecklistHeadId });

        if (head) {
            var nr = this.records.length;
            const newHead: ChecklistHeadRecordCompactDTO = new ChecklistHeadRecordCompactDTO();
            newHead.rowNr = nr;
            newHead.checklistHeadId = head.checklistHeadId;
            newHead.checklistHeadName = head.name;
            newHead.recordId = this.recordId;
            newHead.state = SoeEntityState.Active;
            newHead.addAttachementsToEInvoice = head.addAttachementsToEInvoice;

            newHead.checklistRowRecords = [];
            newHead.signatures = [];
            newHead.hideSignatureIcon = true;

            this.records.push(newHead);

            _.forEach(head.checklistRows, (r) => {
                const newRow: ChecklistExtendedRowDTO = new ChecklistExtendedRowDTO();
                newRow.name = r.checklistHead != null ? r.checklistHead.name : " ";
                newRow.rowId = r.checklistRowId;
                newRow.headId = r.checklistHeadId;
                newRow.rowNr = r.rowNr;
                newRow.text = r.text;
                newRow.type = r.type;
                newRow.mandatory = r.mandatory;
                newRow.rowRecordId = 0;
                newRow.comment = " ";
                newRow.date = null;
                newRow.boolData = null;
                newRow.checkListMultipleChoiceAnswerHeadId = r.checkListMultipleChoiceAnswerHeadId;

                switch (newRow.type) {
                    case TermGroup_ChecklistRowType.String:
                        newRow.dataTypeId = SettingDataType.String;
                        break;
                    case TermGroup_ChecklistRowType.YesNo:
                    case TermGroup_ChecklistRowType.Checkbox:
                        newRow.dataTypeId = SettingDataType.Boolean;
                        break;
                    case TermGroup_ChecklistRowType.MultipleChoice:
                        newRow.dataTypeId = SettingDataType.String;
                        break;
                    case TermGroup_ChecklistRowType.Image:
                        newRow.dataTypeId = SettingDataType.Image;
                        newRow.showEditIcon = false;
                        break;
                }

                newHead.checklistRowRecords.push(newRow);
            });

            // Fix for mixed up order of the rows
            newHead.checklistRowRecords = _.sortBy(newHead.checklistRowRecords, 'rowNr');

            this.$timeout(() => {
                let scope = this.$scope.$new();
                const attachmentPoint = $(`.accordion-${newHead.rowNr}-${this.guid}`);
                attachmentPoint.empty();
                const html = angular.element(this.createRowHtml(newHead, this.records.length - 1));
                attachmentPoint.append(html);
                this.$compile(html)(scope);
                this.scopes.push(scope);

                newHead.isRendered = true;
            });
        }
    }

    private deleteChecklist(head: IChecklistHeadRecordCompactDTO) {
        if (head) {
            const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], this.terms["billing.invoices.checklists.deletechecklist"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val != null && val === true) {
                    head.state = SoeEntityState.Deleted;
                    this.setAsDirty();
                };
            });
        }
    }

    private accordionOpen(head: ChecklistHeadRecordCompactDTO) {
        if (head && !head.isRendered) {
            let scope = this.$scope.$new();
            const attachmentPoint = $(`.accordion-${head.rowNr}-${this.guid}`);//'.accordion-' + head.rowNr);// + ' table');
            attachmentPoint.empty();
            const html = angular.element(this.createRowHtml(head, head.rowNr));
            attachmentPoint.append(html);
            this.$compile(html)(scope);
            this.scopes.push(scope);
            head.isRendered = true;
        }
    }

    private nbrOfFilesChanged(nbrOfFiles) {
        this.signatures = [];
        _.forEach(this.records, (r) => {
            this.signatures = this.signatures.concat(r.signatures);
        });

        if (this.loadingSignatures)
            this.loadingSignatures = false;
        else
            this.setAsDirty();
    }

    // Helper methods
    private createRecordHtml() {
        this.$timeout(() => {
            let recordCounter: number = 0;
            _.forEach(this.records, (r: ChecklistHeadRecordCompactDTO) => {
                let scope = this.$scope.$new()
                const attachmentPoint = $(`.accordion-${r.rowNr}-${this.guid}`);
                attachmentPoint.empty();

                const html = angular.element(this.createRowHtml(r, recordCounter));
                attachmentPoint.append(html);
                this.$compile(html)(scope);
                this.scopes.push(scope);

                recordCounter = recordCounter + 1;
            });
        });
    }

    private createRowHtml(record: ChecklistHeadRecordCompactDTO, recordCounter: number) {
        // SOE-panel
        var panel = $('<soe-panel condensed="true" button-icon="fa-print" button-label-key="core.print" button-hidden="!directiveCtrl.records[' + recordCounter + '].checklistHeadRecordId || !directiveCtrl.showPrintIcon" on-button-click="$event.stopPropagation();directiveCtrl.print(directiveCtrl.records[' + recordCounter + ']);">');
        var tableHolder = $('<div>')

        // Table
        var table = $('<table>');
        table.attr('class', 'table table-condensed table-hover horizontal-line-top noBorder')

        // Head
        var table_head = $('<thead>');

        // Header row
        var tr = $('<tr>');

        var thNumber = $('<th>');
        thNumber.attr('style', 'width:2%');
        thNumber.append('<soe-label label-key="core.nr">');

        var thQuestion = $('<th>');
        thQuestion.append('<soe-label label-key="core.question">');

        var thMandatory = $('<th>');
        thMandatory.attr('style', 'width:5%');
        thMandatory.append('<soe-label label-key="core.mandatory">');

        var thAnswer = $('<th>');
        thAnswer.attr('style', 'width:10%');
        thAnswer.append('<soe-label label-key="core.answer">');

        var thNote = $('<th>');
        thNote.attr('style', 'width:30%');
        thNote.append('<soe-label label-key="common.note">');

        var thDate = $('<th>');
        thDate.attr('style', 'width:126px');
        thDate.append('<soe-label label-key="common.date">');

        var thEdit = $('<th>');
        thEdit.attr('style', 'width:2%');

        tr.append(thNumber);
        tr.append(thQuestion);
        tr.append(thMandatory);
        tr.append(thAnswer);
        tr.append(thNote);
        tr.append(thDate);
        tr.append(thEdit);
        table_head.append(tr);
        table.append(table_head);

        let counter: number = 0;
        _.forEach(record.checklistRowRecords, (row) => {
            this.activeWatchers.push(this.$scope.$watch(() => row.boolData, (newValue, oldValue) => {
                if (newValue && newValue != oldValue) {
                    this.$timeout(() => {
                        this.$scope.$apply(() => {
                            row.date = CalendarUtility.getDateToday();
                            row.dateString = CalendarUtility.convertToDate(row.date).toLocaleDateString();
                            row.isModified = true;
                            this.setAsDirty();
                        });
                    });
                }
            }, true));

            this.activeWatchers.push(this.$scope.$watch(() => row.intData, (newValue, oldValue) => {
                if (newValue && newValue != oldValue) {
                    this.$timeout(() => {
                        this.$scope.$apply(() => {
                            if (row.type === TermGroup_ChecklistRowType.YesNo) {
                                switch (row.intData) {
                                    case 1:
                                        row.boolData = true;
                                        break;
                                    case 2:
                                        row.boolData = false;
                                        break;
                                    default:
                                        row.boolData = undefined;
                                        break;
                                }
                            }
                            row.date = CalendarUtility.getDateToday();
                            row.dateString = CalendarUtility.convertToDate(row.date).toLocaleDateString();
                            row.isModified = true;
                            this.setAsDirty();
                        });
                    });
                }
            }, true));

            this.activeWatchers.push(this.$scope.$watch(() => row.comment, (newValue, oldValue) => {
                if (newValue != oldValue) {
                    this.$timeout(() => {
                        this.$scope.$apply(() => {
                            row.isModified = true;
                            this.setAsDirty();
                        });
                    });
                }
            }, true));

            this.activeWatchers.push(this.$scope.$watch(() => row.date, (newValue, oldValue) => {
                if (newValue && newValue != oldValue) {
                    this.$timeout(() => {
                        this.$scope.$apply(() => {
                            row.dateString = CalendarUtility.convertToDate(row.date).toLocaleDateString();
                            row.isModified = true;
                            this.setAsDirty();
                        });
                    });
                }
            }, true));

            var body = $('<tbody>');

            var trRow = $('<tr>');

            var tdNumber = $('<td>');
            var content = $('<soe-label>');
            content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].rowNr');
            content.attr('discreet', 'true');
            tdNumber.append(content);

            var tdQuestion = $('<td>');
            var content = $('<soe-label>');
            content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].text');
            content.attr('discreet', 'true');
            tdQuestion.append(content);

            var tdMandatory = $('<td>');
            var content = $('<soe-label>');
            content.attr('model', '\'' + (row.mandatory ? this.terms["core.yes"] : this.terms["core.no"]) + '\'');
            content.attr('discreet', 'true');
            tdMandatory.append(content);

            var tdAnswer = $('<td>');
            var content = this.getAnswerContent(row, recordCounter, counter);
            tdAnswer.append(content);

            var tdNote = $('<td>');
            var content = $('<soe-textbox>');
            content.attr('hidelabel', 'true');
            content.attr('is-readonly', '!directiveCtrl.hasAnswerChecklistsModifyPermission || directiveCtrl.readOnly');
            content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].comment');
            tdNote.append(content);

            var tdDate = $('<td>');
            //tdDate.attr('class', 'padding');
            /*if (row.date) {
                row.dateString = CalendarUtility.convertToDate(row.date).toLocaleDateString();*/
            var content = $('<soe-datepicker>');
            content.attr('hidelabel', 'true');
            content.attr('is-disabled', '!directiveCtrl.hasAnswerChecklistsModifyPermission || directiveCtrl.readOnly');
            content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].date');
            tdDate.append(content);

            var tdEditModal = $('<td>');
            if (row.showEditIcon)
                tdEditModal.append('<span data-l10n-bind data-l10n-bind-title="core.edit"><i class="fal fa-pencil iconEdit link control-label" data-ng-click="$event.stopPropagation();directiveCtrl.editChecklistQuestion(directiveCtrl.records[' + recordCounter + '], ' + counter + ');"></i></span>');

            trRow.append(tdNumber);
            trRow.append(tdQuestion);
            trRow.append(tdMandatory);
            trRow.append(tdAnswer);
            trRow.append(tdNote);
            trRow.append(tdDate);
            trRow.append(tdEditModal);

            body.append(trRow);
            table.append(body);

            counter = counter + 1;
        });
        tableHolder.append(table);
        panel.append(tableHolder);
        return panel;
    }

    private getAnswerContent(row: IChecklistExtendedRowDTO, recordCounter: number, counter: number) {
        switch (row.type) {
            case TermGroup_ChecklistRowType.Checkbox:
                var content = $('<soe-checkbox>');
                content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].boolData');
                content.attr('is-readonly', '!directiveCtrl.hasAnswerChecklistsModifyPermission || directiveCtrl.readOnly');
                return content;
            case TermGroup_ChecklistRowType.Image:
                const icon = $('<span>');
                icon.append('<i class="{{directiveCtrl.getImageIcon(directiveCtrl.records[' + recordCounter + '], ' + counter + ')}}"></i>');
                return icon;
            case TermGroup_ChecklistRowType.YesNo:
                //Set value
                if (row.boolData !== undefined && row.boolData !== null)
                    row.intData = row.boolData === true ? 1 : 2;
                else
                    row.intData = 0;

                //Create content
                var content = $('<soe-select>');
                content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].intData');
                content.attr('is-readonly', '!directiveCtrl.hasAnswerChecklistsModifyPermission || directiveCtrl.readOnly');
                content.attr('hidelabel', 'true');
                content.attr('items', 'directiveCtrl.yesNoDict');
                content.attr('options', 'item.id as item.name for item in items');
                return content;
            case TermGroup_ChecklistRowType.MultipleChoice:
                //Create content
                var content = $('<soe-select>');
                content.attr('model', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].intData');
                content.attr('is-readonly', '!directiveCtrl.hasAnswerChecklistsModifyPermission || directiveCtrl.readOnly');
                content.attr('hidelabel', 'true');
                var items: any[] = [];
                _.forEach(_.filter(this.multipleChoiceQuestions, { checkListMultipleChoiceAnswerHeadId: row.checkListMultipleChoiceAnswerHeadId }), (item) => {
                    if(item.state === SoeEntityState.Active || (row.value && item.question === row.value))
                        items.push({ id: item.checkListMultipleChoiceAnswerRowId, name: item.question });
                });
                row['selectOption'] = items;
                content.attr('items', 'directiveCtrl.records[' + recordCounter + '].checklistRowRecords[' + counter + '].selectOption');
                content.attr('options', 'item.id as item.name for item in items');
                return content;
        }
    }

    private printPhase2(head: IChecklistHeadRecordCompactDTO, clHead: IChecklistHeadDTO) {
        if (clHead) {
            if (clHead.reportId) {
                this.reportService.getChecklistReportURL(this.recordId, head.checklistHeadRecordId, clHead.reportId).then((x) => {
                    var url = x;
                    HtmlUtility.openInSameTab(this.$window, url);
                });
            }
            else if (this.reports && this.reports.length === 1) {
                this.reportService.getChecklistReportURL(this.recordId, head.checklistHeadRecordId, this.reports[0].reportId).then((x) => {
                    var url = x;
                    HtmlUtility.openInSameTab(this.$window, url);
                });
            }
            else if (this.reports.length > 1) {
                const modal = this.modalInstance.open({
                    templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
                    controller: SelectReportController,
                    controllerAs: 'ctrl',
                    backdrop: 'static',
                    size: 'lg',
                    resolve: {
                        module: () => { return null },
                        reportTypes: () => { return null },
                        showCopy: () => { return false },
                        showEmail: () => { return false },
                        copyValue: () => { return false },
                        reports: () => { return this.reports },
                        defaultReportId: () => { return null },
                        langId: () => { return null },
                        showReminder: () => { return false },
                        showLangSelection: () => { return true },
                        showSavePrintout: () => { return false },
                        savePrintout: () => { return false }
                    }
                });

                modal.result.then((result: any) => {
                    //console.log(result);
                    if ((result) && (result.reportId)) {
                        this.reportService.getChecklistReportURL(this.recordId, head.checklistHeadRecordId, result.reportId).then((x) => {
                            var url = x;
                            HtmlUtility.openInSameTab(this.$window, url);
                        });
                    }
                });
            }
        }
    }
    private print(head: IChecklistHeadRecordCompactDTO) {
        if (head?.checklistHeadId) {
            const clHead = _.find(this.heads, { checklistHeadId: head.checklistHeadId });
            if (clHead) {
                this.printPhase2(head, clHead);
            }
            else {
                //disabled checklist?
                this.coreService.getChecklistHead(head.checklistHeadId, false).then((clHeadresponse) => {
                    if (head) {
                        this.printPhase2(head, clHeadresponse);
                    }
                    else { console.log("checklist head not found") }
                });
            }
        }
    }

    public getImageIcon(checklistHead: ChecklistHeadRecordCompactDTO, rowIdx: number) {
        const row = checklistHead.checklistRowRecords[rowIdx];

        if (row.boolData)
            return "fal fa-image";
        else
            return "fal fa-image-slash";
    }

    public editChecklistQuestion(checklistHead: ChecklistHeadRecordCompactDTO, rowIdx: number) {
        '!directiveCtrl.hasAnswerChecklistsModifyPermission || directiveCtrl.readOnly'
        const canEdit = !this.hasAnswerChecklistsModifyPermission || this.readOnly;
        const title: string = this.terms["core.question"];
        
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Orders/Directives/Checklists/Views/EditChecklistRowDialog.html"),
            controller: EditChecklistRowDialogController,
            controllerAs: "ctrl",
            size: 'md',
            windowClass: 'fullsize-modal',
            resolve: {
                index: () => { return rowIdx },
                row: () => { return checklistHead.checklistRowRecords[rowIdx] },
                rows: () => { return checklistHead.checklistRowRecords },
                rowsCtrl: () => { return this },
                questionsTerm: () => { return title },
                readOnly: () => { return canEdit }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.modified) {
                if(result.row)
                    checklistHead.checklistRowRecords[rowIdx] = result.row;
                this.setAsDirty();
            }
        });
    }

    private setAsDirty(dirty: boolean = true) {
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {
            guid: this.guid,
            dirty: dirty
        })
    }
}
