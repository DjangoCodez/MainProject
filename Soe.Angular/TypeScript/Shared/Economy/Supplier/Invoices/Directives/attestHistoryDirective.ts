import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { ISupplierService } from "../../SupplierService";
import { TermGroup_AttestWorkFlowRowProcessType } from "../../../../../Util/CommonEnumerations";
import { IAttestWorkFlowRowDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { Guid } from "../../../../../Util/StringUtility";

//@ngInject
export function attestHistoryDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        templateUrl: urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/attestHistory.html"),
        replace: true,
        restrict: "E",
        controller: AttestHistoryController,
        controllerAs: "ctrl",
        bindToController: true,
        scope: {
            loadFromHead: "=",
            attestWorkflowHead: "=",
            attestWorkflowRows: "="
        },
        link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            scope.$watch(() => (ngModelController.attestWorkflowHead),
                (newValue) => {
                    if (newValue && ngModelController.loadFromHead) {
                        ngModelController.loadData();
                    }
                }, true);
            scope.$watch(() => (ngModelController.attestWorkflowRows),
                (newValue) => {
                    if (newValue) {
                        ngModelController.createComments(ngModelController.attestWorkflowRows);
                    }
                }, true);
        }
    }
}

export class AttestHistoryController {
    public attestWorkflowHead: any;
    public loadFromHead: boolean = false;
    private historyContainer: string;
    private terms: any;

    //@ngInject
    constructor(
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        $scope: ng.IScope) {
    }

    $onInit() {
        this.historyContainer = "continer" + Guid.newGuid();
        this.loadTerms();
        /*
        this.$scope.$watch(() => this.invoices, (newVal, oldVal) => {
            //this.updateData();
        });
        */
    }

    private loadTerms() {
        var keys: string[] = [
            "core.attestflowregistered",
            "core.yes",
            "core.no",
            "economy.supplier.attestflowoverview.deletedby",
            "economy.supplier.attestflowoverview.isdeleted"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadData() {
        if (this.attestWorkflowHead) {
            this.supplierService.getAttestWorkFlowTemplateHeadRowsUser(this.attestWorkflowHead.attestWorkFlowHeadId).then((rows: IAttestWorkFlowRowDTO[]) => {
                this.createComments(rows);
            });
        }
    } 

    private createComments(rows: IAttestWorkFlowRowDTO[]) {
        var commentsAndAnswers: any[] = [];
        _.forEach(rows, row => {
            row['answerStr'] = (row.answer === true ? this.terms["core.yes"] : this.terms["core.no"]);

            if (row.processType === TermGroup_AttestWorkFlowRowProcessType.Registered) {
                var reg = { date: CalendarUtility.toFormattedDate(row.created), rows: [] };
                reg.rows.push({ time: CalendarUtility.toFormattedTime(row.created, false), text: row.name + " " + this.terms["core.attestflowregistered"], isComment: false });
                commentsAndAnswers.push(reg);
            }

            // Find corresponding item for answerdate
            if (row.answerDate) {
                var caa1 = _.find(commentsAndAnswers, (item) => item.date === CalendarUtility.toFormattedDate(row.answerDate));
                if (caa1) {
                    caa1.rows.push({ time: CalendarUtility.toFormattedTime(row.answerDate, false), text: row.answerText, isComment: false });
                }
                else {
                    caa1 = { date: CalendarUtility.toFormattedDate(row.answerDate), rows: [] };
                    caa1.rows.push({ time: CalendarUtility.toFormattedTime(row.answerDate, false), text: row.answerText, isComment: false });
                    commentsAndAnswers.push(caa1);
                }
            }

            // Find corresponding item for commentdate
            if (row.commentDate) {
                var caa2 = _.find(commentsAndAnswers, (item) => item.date === CalendarUtility.toFormattedDate(row.commentDate));
                if (caa2) {
                    caa2.rows.push({ time: CalendarUtility.toFormattedTime(row.commentDate, false), text: row.comment, name: row.commentUser, isComment: true });
                }
                else {
                    caa2 = { date: CalendarUtility.toFormattedDate(row.commentDate), rows: [] };
                    caa2.rows.push({ time: CalendarUtility.toFormattedTime(row.commentDate, false), text: row.comment, name: row.commentUser, isComment: true });
                    commentsAndAnswers.push(caa2);
                }
            }
        });

        if (this.attestWorkflowHead.isDeleted && this.attestWorkflowHead.modified) {
            var caa1 = _.find(commentsAndAnswers, (item) => item.date === CalendarUtility.toFormattedDate(this.attestWorkflowHead.modified));
            if (caa1) {
                caa1.rows.push({ time: CalendarUtility.toFormattedTime(this.attestWorkflowHead.modified, false), text: this.terms["economy.supplier.attestflowoverview.deletedby"] + " " + this.attestWorkflowHead.modifiedBy, isComment: false, deleted: true });
            }
            else {
                caa1 = { date: CalendarUtility.toFormattedDate(this.attestWorkflowHead.modified), rows: [] };
                caa1.rows.push({ time: CalendarUtility.toFormattedTime(this.attestWorkflowHead.modified, false), text: this.terms["economy.supplier.attestflowoverview.deletedby"] + " " + this.attestWorkflowHead.modifiedBy, isComment: false, deleted: true });
                commentsAndAnswers.push(caa1);
            }
        }

        this.createHistoryLog(commentsAndAnswers);
    }

    private createHistoryLog(commentsAndAnswers: any[]) {
        var container = document.getElementById(this.historyContainer);

        //Clear
        if (container.childNodes.length > 0) {
            while (container.firstChild) {
                container.removeChild(container.firstChild);
            }
        }

        if (this.attestWorkflowHead.isDeleted) {
            //economy.supplier.attestflowoverview.isdeleted
            var deletedRow = document.createElement("div");
            deletedRow.classList.add("row");

            var labelDeleted = document.createElement("label");
            labelDeleted.classList.add("margin-large-left");
            labelDeleted.classList.add("errorColor");
            labelDeleted.innerText = "* " + this.terms["economy.supplier.attestflowoverview.isdeleted"];

            deletedRow.appendChild(labelDeleted);
            container.appendChild(deletedRow);
        }

        _.forEach(commentsAndAnswers, (dateItem) => {
            //Header
            var headerRow = document.createElement("div");
            headerRow.classList.add("row");

            var paragraph = document.createElement("p");
            paragraph.classList.add("history-paragraph");
            paragraph.classList.add("margin-large-left");
            paragraph.classList.add("margin-large-right");

            var span = document.createElement("span");
            span.classList.add("history-paragraph-span");
            span.innerText = dateItem.date;

            paragraph.appendChild(span);
            headerRow.appendChild(paragraph);
            container.appendChild(headerRow);

            _.forEach(_.orderBy(dateItem.rows, ['time', 'isComment']), (row) => {
                var historyRow = document.createElement("div");
                historyRow.classList.add("row");
                historyRow.classList.add("margin-large-bottom");

                var timeRow = document.createElement("div");
                timeRow.classList.add("margin-large-left");

                var labelTime = document.createElement("label");
                labelTime.classList.add("discreet");
                labelTime.innerText = row.time;
                timeRow.appendChild(labelTime);
                if (row.isComment) {
                    var labelName = document.createElement("label");
                    labelName.classList.add("margin-large-left");
                    labelName.innerText = row.name;
                    timeRow.appendChild(labelName);
                }

                var textRow = document.createElement("div");
                textRow.classList.add("margin-large-left");
                textRow.classList.add("margin-large-right");
                textRow.classList.add("padding-small-left");
                textRow.classList.add("padding-small-top");
                textRow.classList.add("padding-small-bottom");
                if (row.isComment)
                    textRow.classList.add("history-speech-bubble");
                else if (row.deleted)
                    textRow.classList.add("history-bubble-deleted");
                else
                    textRow.classList.add("history-bubble");
                textRow.innerText = row.text;

                historyRow.appendChild(timeRow);
                historyRow.appendChild(textRow);
                container.appendChild(historyRow);
            });
        });
    }
}