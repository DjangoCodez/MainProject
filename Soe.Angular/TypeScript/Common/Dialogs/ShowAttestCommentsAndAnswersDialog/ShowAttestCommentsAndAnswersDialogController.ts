import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICommonCustomerService } from "../../Customer/CommonCustomerService";
import { ISmallGenericType, IAttestWorkFlowRowDTO } from "../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_AttestWorkFlowRowProcessType } from "../../../Util/CommonEnumerations";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class ShowAttestCommentsAndAnswersDialogController {
    private loading: boolean = true;
    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private commonCustomerService: ICommonCustomerService,
        private supplierService: ISupplierService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private invoiceId: number,
        private registeredTerm: string) {
        
    }

    private $onInit() {
        this.loadAttestFlowHead();
    }

    private loadAttestFlowHead() {
        this.supplierService.getAttestWorkFlowRowsFromInvoiceId(this.invoiceId).then((rows: IAttestWorkFlowRowDTO[]) => {
            var commentsAndAnswers: any[] = [];
            _.forEach(rows, row => {
                if (row.processType === TermGroup_AttestWorkFlowRowProcessType.Registered) {
                    var reg = { date: CalendarUtility.toFormattedDate(row.created), rows: [] };
                    reg.rows.push({ time: CalendarUtility.toFormattedTime(row.created, false), text: row.name + " " + this.registeredTerm, isComment: false });
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

                // Create log
                this.createHistoryLog(commentsAndAnswers);

        });
    }

    public createHistoryLog(commentsAndAnswers: any[]) {
        var container = document.getElementById("historyContainer");

        //Clear
        if (container.childNodes.length > 0) {
            while (container.firstChild) {
                container.removeChild(container.firstChild);
            }
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
                else
                    textRow.classList.add("history-bubble");
                textRow.innerText = row.text;

                historyRow.appendChild(timeRow);
                historyRow.appendChild(textRow);
                container.appendChild(historyRow);
            });
        });

        this.loading = false;
    }

    buttonOkClick() {
        this.$uibModalInstance.close('ok');
    }
}