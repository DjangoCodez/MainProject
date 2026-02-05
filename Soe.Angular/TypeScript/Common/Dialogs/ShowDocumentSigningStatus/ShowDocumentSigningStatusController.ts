import { ICoreService } from "../../../Core/Services/CoreService";
import { SigneeStatus, SoeEntityType, TermGroup_AttestWorkFlowRowProcessType, TermGroup_DataStorageRecordAttestStatus } from "../../../Util/CommonEnumerations";
import { AttestWorkFlowRowDTO } from "../../Models/AttestWorkFlowDTOs";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { CoreUtility } from "../../../Util/CoreUtility";

export class ShowDocumentSigningStatusController {
    // Data
    private attestWorkFlowHeadId: number;
    private attestWorkflowRows: AttestWorkFlowRowDTO[];
    private dateRows: DocumentSigningStatusDateRow[] = [];
    private comment: string;

    // Flags
    private loading: boolean = true;
    private answerClicked: boolean = false;
    private isDebug: boolean = false;

    private isCurrentUserCurrentSignee: boolean = false;
    private currentAttestWorkFlowRowId: number;

    // Properties
    private get isCancelled(): boolean {
        return this.attestStatus === TermGroup_DataStorageRecordAttestStatus.Cancelled;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private cancelPermission: boolean,
        private entity: SoeEntityType,
        private recordId: number,
        private attestStatus: TermGroup_DataStorageRecordAttestStatus,
        private registeredTerm: string,
        private openedTerm: string) {
    }

    private $onInit() {
        this.loading = true;
        this.loadAttestFlowRows().then(() => {
            this.createHistoryLog();
            this.loading = false;
        });
    }

    // SERVICE CALLS

    private loadAttestFlowRows(): ng.IPromise<any> {
        return this.coreService.getDocumentSigningStatus(this.entity, this.recordId).then(x => {
            this.attestWorkflowRows = x;
            this.dateRows = [];

            _.forEach(this.attestWorkflowRows, row => {
                if (!this.attestWorkFlowHeadId)
                    this.attestWorkFlowHeadId = row.attestWorkFlowHeadId;

                if (row.isCurrentUser && row.processType === TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess) {
                    this.isCurrentUserCurrentSignee = true;
                    this.currentAttestWorkFlowRowId = row.attestWorkFlowRowId;
                }

                if (row.processType === TermGroup_AttestWorkFlowRowProcessType.Registered) {
                    let reg = new DocumentSigningStatusDateRow(row.created);
                    let regRow = new DocumentSigningStatusRow(row.created);
                    regRow.isReg = true;
                    regRow.isAnswer = false;
                    regRow.name = row.name;
                    regRow.text = row.name + " " + this.registeredTerm.toLocaleLowerCase();
                    if (row.comment)
                        regRow.comment = row.comment;
                    reg.rows.push(regRow);
                    this.dateRows.push(reg);
                } else if (row.answerDate && row.answer === undefined) {
                    let answerRow = _.find(this.dateRows, r => r.date.isSameDayAs(row.answerDate));
                    if (!answerRow) {
                        answerRow = new DocumentSigningStatusDateRow(row.answerDate);
                        this.dateRows.push(answerRow);
                    }
                    let ansRow = new DocumentSigningStatusRow(row.answerDate);
                    ansRow.isOpened = true;
                    ansRow.isAnswer = false;
                    ansRow.isReg = false;
                    ansRow.name = row.name;
                    ansRow.text = row.name + " " + this.openedTerm.toLocaleLowerCase();
                    answerRow.rows.push(ansRow);
                } else if (row.answerDate) {
                    let answerRow = _.find(this.dateRows, r => r.date.isSameDayAs(row.answerDate));
                    if (!answerRow) {
                        answerRow = new DocumentSigningStatusDateRow(row.answerDate);
                        this.dateRows.push(answerRow);
                    }
                    let ansRow = new DocumentSigningStatusRow(row.answerDate);
                    ansRow.isAnswer = true;
                    ansRow.isReg = false;
                    ansRow.name = row.name;
                    ansRow.text = row.answerText;
                    if (row.comment)
                        ansRow.comment = row.comment;
                    ansRow.answer = row.answer;
                    answerRow.rows.push(ansRow);
                }
            });
        });
    }

    // HELP-METODS

    private createHistoryLog() {
        let container = document.getElementById("historyContainer");

        // Clear
        if (container.childNodes.length > 0) {
            while (container.firstChild) {
                container.removeChild(container.firstChild);
            }
        }

        _.forEach(this.dateRows, dateRow => {
            // Header
            let headerRow = document.createElement("div");
            headerRow.classList.add("row");

            let paragraph = document.createElement("p");
            paragraph.classList.add("history-paragraph");
            paragraph.classList.add("margin-large-left");
            paragraph.classList.add("margin-large-right");

            let span = document.createElement("span");
            span.classList.add("history-paragraph-span");
            span.innerText = dateRow.date.toFormattedDate();

            paragraph.appendChild(span);
            headerRow.appendChild(paragraph);
            container.appendChild(headerRow);

            _.forEach(_.orderBy(dateRow.rows, ['isReg', 'formattedTime', 'isAnswer'], ['desc', 'asc', 'desc']), row => {
                let historyRowElem = document.createElement("div");
                historyRowElem.classList.add("row");
                historyRowElem.classList.add("margin-large-bottom");

                let rowElem = this.createRow("history-bubble");
                let timeElem = document.createElement("span");
                timeElem.classList.add("margin-small-right");
                timeElem.innerText = row.formattedTime;
                rowElem.appendChild(timeElem);

                if (row.isReg) {
                    rowElem.appendChild(this.createRowIcon("fal", "fa-file-signature"));
                } else if (row.isOpened) {
                    rowElem.appendChild(this.createRowIcon("fal", "fa-eye"));
                } else if (row.isAnswer) {
                    rowElem.appendChild(this.createRowIcon("fas", row.answer ? "fa-thumbs-up" : "fa-thumbs-down", row.answer ? "okColor" : "errorColor"));
                }

                let textElem = document.createElement("span");
                textElem.innerText = row.text;
                rowElem.appendChild(textElem);
                historyRowElem.appendChild(rowElem);

                if (row.comment) {
                    let commentDiv = document.createElement("div");
                    commentDiv.classList.add("margin-large-left");
                    let commentRowElem = this.createRow("history-speech-bubble");
                    commentRowElem.appendChild(this.createRowIcon("fal", "fa-comment-dots"));
                    let commentElem = document.createElement("span");
                    commentElem.innerText = row.comment;
                    commentRowElem.appendChild(commentElem);
                    commentDiv.appendChild(commentRowElem);
                    historyRowElem.appendChild(commentDiv);
                }

                container.appendChild(historyRowElem);
            });
        });
    }

    private createRowIcon(thickness: string, iconName: string, colorClass?: string) {
        let icon = document.createElement("span");
        icon.classList.add("margin-small-right");
        icon.classList.add(thickness);
        icon.classList.add(iconName);
        if (colorClass)
            icon.classList.add(colorClass);

        return icon;
    }

    private createRow(bubbleClass: string) {
        let row = document.createElement("div");
        row.classList.add("margin-large-left");
        row.classList.add("margin-large-right");
        row.classList.add("padding-small-left");
        row.classList.add("padding-small-top");
        row.classList.add("padding-small-bottom");
        row.classList.add(bubbleClass);

        return row;
    }

    // EVENTS

    private initCancelAttestFlow() {
        let keys: string[] = [
            "common.signdoc.status.cancel",
            "common.signdoc.status.cancel.validate"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["common.signdoc.status.cancel"], terms["common.signdoc.status.cancel.validate"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                if (val)
                    this.validateCancelAttestFlow();
            }, (reason) => {
                // User cancelled
            });
        });
    }

    private validateCancelAttestFlow() {
        if (!this.comment) {
            let keys: string[] = [
                "common.signdoc.status.cancel",
                "common.signdoc.status.cancel.commentmandatory"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["common.signdoc.status.cancel"], terms["common.signdoc.status.cancel.commentmandatory"], SOEMessageBoxImage.Forbidden);
            });
        } else {
            this.cancelAttestFlow();
        }
    }

    private cancelAttestFlow() {
        this.coreService.cancelDocumentSigning(this.attestWorkFlowHeadId, this.comment).then(result => {
            if (result.success)
                this.$uibModalInstance.close({ cancelled: true });
            else {
                this.translationService.translate("error.default_error").then(term => {
                    this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                });
            }
        });
    }

    private initAnswer(answer: boolean) {
        if (this.answerClicked)
            return;

        this.answerClicked = true;

        if (answer) {
            this.answer(answer);
        } else {
            let keys: string[] = [
                "common.signdoc.status.reject",
                "common.signdoc.status.reject.validate"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["common.signdoc.reject.cancel"], terms["common.signdoc.status.reject.validate"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                    if (val)
                        this.validateAnswer(answer);
                }, (reason) => {
                    // User cancelled
                    this.answerClicked = false;
                });
            });
        }
    }

    private validateAnswer(answer: boolean) {
        if (!this.comment) {
            let keys: string[] = [
                "common.signdoc.status.reject",
                "common.signdoc.status.reject.commentmandatory"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["common.signdoc.status.reject"], terms["common.signdoc.status.reject.commentmandatory"], SOEMessageBoxImage.Forbidden);
                this.answerClicked = false;
            });
        } else {
            this.answer(answer);
        }
    }

    private answer(answer: boolean) {
        this.coreService.saveDocumentSigningAnswer(this.currentAttestWorkFlowRowId, answer ? SigneeStatus.Signed : SigneeStatus.Rejected, this.comment).then(result => {
            if (result.success)
                this.$uibModalInstance.close({ answered: true });
            else {
                this.translationService.translate("error.default_error").then(term => {
                    this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                    this.answerClicked = false;
                });
            }
        });
    }

    private close() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private debug() {
        if (CoreUtility.isSupportAdmin)
            this.isDebug = true;
    }
}

// HELP CLASSES

export class DocumentSigningStatusDateRow {
    constructor(date: Date) {
        this.date = date;
        this.rows = [];
    }

    date: Date;
    rows: DocumentSigningStatusRow[];
}

export class DocumentSigningStatusRow {
    constructor(time: Date) {
        this.time = time;
    }

    time: Date;
    name: string;
    text: string;
    comment: string;
    isReg: boolean;
    isOpened: boolean;
    isAnswer: boolean;
    answer: boolean;

    public get formattedTime(): string {
        return this.time ? this.time.toFormattedTime() : '';
    }
}