import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SoeEntityType, TermGroup_AttestEntity, TermGroup_AttestWorkFlowRowProcessType } from "../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { AttestWorkFlowHeadDTO, AttestWorkFlowRowDTO, AttestWorkFlowTemplateHeadGridDTO, AttestWorkFlowTemplateRowDTO } from "../../Models/AttestWorkFlowDTOs";
import { UserSmallDTO } from "../../Models/UserDTO";
import { IAddDocumentToAttestFlowService } from "./AddDocumentToAttestFlowService";
import { AddDocumentToAttestFlowUserDirectiveController } from "./Directives/AddDocumentToAttestFlowUserDirective";

export class AddDocumentToAttestFlowController {

    // Data
    private attestWorkFlowHead: AttestWorkFlowHeadDTO;
    private adminText: string = "";
    private sendMessage: boolean = true;
    private signInitial: boolean = false;
    private canSignInitial: boolean = false;

    // Lookups
    private currentUser: UserSmallDTO;
    private endUser: UserSmallDTO;
    private templates: AttestWorkFlowTemplateHeadGridDTO[];
    private templateRows: AttestWorkFlowTemplateRowDTO[];
    private selectableRows: AttestWorkFlowTemplateRowDTO[];

    private userSelectors: AddDocumentToAttestFlowUserDirectiveController[];

    // Properties
    private get firstRow(): AttestWorkFlowTemplateRowDTO {
        return this.templateRows && this.templateRows.length > 0 ? this.templateRows[0] : null;
    }
    private get lastRow(): AttestWorkFlowTemplateRowDTO {
        return this.templateRows && this.templateRows.length > 0 ? _.last(this.templateRows) : null;
    }

    // Flags
    private okClicked = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $window: ng.IWindowService,
        private $uibModalInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private addDocumentToAttestFlowService: IAddDocumentToAttestFlowService,
        private recordId: number,
        private endUserId: number) {
    }

    public $onInit() {
        this.attestWorkFlowHead = new AttestWorkFlowHeadDTO();
        this.userSelectors = [];

        const queue = [];
        queue.push(this.loadCurrentUser());
        if (this.endUserId)
            queue.push(this.loadEndUser());
        queue.push(this.loadTemplates());
        this.$q.all(queue);

        this.messagingService.subscribe('canSignInitial', (x) => {
            this.canSignInitial = x;
            if (!this.canSignInitial)
                this.signInitial = false;
        });
    }

    private registerUserSelector(control: AddDocumentToAttestFlowUserDirectiveController) {
        this.userSelectors.push(control);
    }

    // SERVICE CALLS

    private loadCurrentUser(): ng.IPromise<any> {
        return this.coreService.getUser(CoreUtility.userId).then(x => {
            this.currentUser = x;
        });
    }

    private loadEndUser(): ng.IPromise<any> {
        return this.coreService.getUser(this.endUserId).then(x => {
            this.endUser = x;
        });
    }

    private loadTemplates(): ng.IPromise<any> {
        return this.addDocumentToAttestFlowService.getAttestWorkFlowTemplates(TermGroup_AttestEntity.SigningDocument).then(x => {
            this.templates = x;
            if (this.templates.length === 1) {
                const templateHeadId = this.templates[0].attestWorkFlowTemplateHeadId;
                this.attestWorkFlowHead.attestWorkFlowTemplateHeadId = templateHeadId;
                this.templateChanged(templateHeadId);
            }
        });
    }

    private loadTemplateRows(templateHeadId: number): ng.IPromise<any> {
        return this.addDocumentToAttestFlowService.getAttestWorkFlowTemplateHeadRows(templateHeadId).then(x => {
            this.templateRows = x;

            this.selectableRows = this.endUserId ? this.templateRows.filter(r => !r.closed) : this.templateRows;
        });
    }

    // EVENTS

    private templateChanged(templateHeadId: number) {
        this.userSelectors = [];
        this.loadTemplateRows(templateHeadId);
    }

    private ok() {
        if (this.okClicked)
            return;

        this.okClicked = true;

        this.attestWorkFlowHead.recordId = this.recordId;
        this.attestWorkFlowHead.entity = SoeEntityType.DataStorageRecord;
        this.attestWorkFlowHead.signInitial = this.signInitial;
        this.attestWorkFlowHead.adminInformation = this.adminText;
        this.attestWorkFlowHead.sendMessage = this.sendMessage;
        if (this.createRowsFromSelectedUsers()) {
            this.coreService.initiateDocumentSigning(this.attestWorkFlowHead).then(result => {
                if (result && result.success) {
                    this.$uibModalInstance.close({ success: true });
                    if (result.stringValue)
                        HtmlUtility.openInNewWindow(this.$window, result.stringValue);
                } else {
                    this.translationService.translate("error.default_error").then(term => {
                        this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                        this.okClicked = false;
                    });
                }
            });
        }
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // HELP-METHODS

    private createRowsFromSelectedUsers(): boolean {
        let rows: AttestWorkFlowRowDTO[] = [];

        let i = 0;
        let rowsValid: boolean = true;
        _.forEach(this.userSelectors, us => {
            i++;
            let urows = us.getRowsToSave();

            // At least one user must be selected in each transition
            if (!urows || urows.length === 0) {
                rowsValid = false;
                return false;
            }

            _.forEach(urows, r => {
                if (i === 1) {
                    // Current user (registering user)
                    r.processType = TermGroup_AttestWorkFlowRowProcessType.Registered;
                    r.answer = true;
                    r.answerDate = CalendarUtility.getDateNow();
                } else if (i === 2) {
                    // Next in turn
                    r.processType = TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess;
                } else {
                    r.processType = TermGroup_AttestWorkFlowRowProcessType.LevelNotReached;
                }
                rows.push(r);
            });
        });

        if (!rowsValid) {
            var keys: string[] = [
                "common.signdoc.invalidforinit",
                "common.signdoc.missingusers",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["common.signdoc.invalidforinit"], terms["common.signdoc.missingusers"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                this.okClicked = false;
            });

            return false;
        }

        this.attestWorkFlowHead.rows = rows;

        return true;
    }
}