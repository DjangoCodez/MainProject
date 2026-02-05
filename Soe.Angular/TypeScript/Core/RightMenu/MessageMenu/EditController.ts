import { EditController as AbsenceRequestsEditController } from "../../../Shared/Time/Schedule/Absencerequests/EditController";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Guid } from "../../../Util/StringUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { MessageEditDTO, MessageRecipientDTO, MessageGroupMemberDTO, MessageAttachmentDTO } from "../../../Common/Models/MessageDTOs";
import { ShiftDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { TermGroup_MessageType, SoeEntityType, SoeEntityImageType, XEMailType, Feature, TermGroup_MessagePriority, TermGroup_MessageDeliveryType, TermGroup_MessageTextType, XEMailAnswerType, CompanySettingType, SettingMainType, ApplicationSettingType, XEMailRecipientType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUserSmallDTO } from "../../../Scripts/TypeLite.Net4";
import { EmployeeListDTO } from "../../../Common/Models/EmployeeListDTO";
import { ToolBarUtility, ToolBarButton, ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { AbsenceRequestGuiMode, AbsenceRequestParentMode, AbsenceRequestViewMode, IconLibrary } from "../../../Util/Enumerations";
import { TinyMCEUtility } from "../../../Util/TinyMCEUtility";
import { ILazyLoadService } from "../../Services/LazyLoadService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Modal
    private modal;

    // Terms
    private terms: any;
    private title: string;

    // Permissions
    private messageModifyPermission = false;
    private messageSendPermission = false;
    private messageDeletePermission = false;
    private sendSMSPermission = false;

    // Application settings
    private attachmentMaxSize = 0;

    // Company settings
    private useDefaultEmailAddress = false;
    private defaultCompanyEmailAddress = "";
    private hideRecipientsInShiftRequest = false;

    // Toolbar
    private toolbarInclude: any;

    // Message
    private messageId: number;
    private message: MessageEditDTO;
    private mailType: XEMailType = XEMailType.Outgoing;
    private messageType: TermGroup_MessageType;
    private isConfirmedByMe = false;
    private isConfirmedByMeDate: Date;
    private isRepliedNeedsConfirmation = false;
    private isReadOnly = false;
    private messageMinHeight = 500;

    // Collections
    private recipients: MessageGroupMemberDTO[];
    private recipientNames: string;

    // File upload
    entity: SoeEntityType = SoeEntityType.XEMail;
    type: SoeEntityImageType = SoeEntityImageType.XEMailAttachment;
    attachementUrl: string = Constants.WEBAPI_CORE_FILES_UPLOAD_INVOICE + SoeEntityType.XEMail;
    private loadingFiles = false;
    public files: MessageAttachmentDTO[] = [];
    private nbrOfFiles: any;
    private filesLoaded = false;

    private absenceRequestLoaderPromise: Promise<any>;

    // Properties
    private get isIncoming(): boolean {
        return this.mailType === XEMailType.Incoming;
    }

    private get isOutgoing(): boolean {
        return this.mailType === XEMailType.Outgoing;
    }

    private get isSent(): boolean {
        return this.mailType === XEMailType.Sent;
    }

    private get isDeleted(): boolean {
        return this.mailType === XEMailType.Deleted;
    }

    private get isUserInitiated(): boolean {
        return (this.messageType === TermGroup_MessageType.UserInitiated);
    }

    private get isPayrollSlip(): boolean {
        return (this.messageType === TermGroup_MessageType.PayrollSlip);
    }

    private get isShiftRequest(): boolean {
        return (this.messageType === TermGroup_MessageType.ShiftRequest);
    }

    private get isAbsenceRequest(): boolean {
        return (this.messageType === TermGroup_MessageType.AbsenceRequest);
    }

    private get isShiftRequestAnswer(): boolean {
        return (this.messageType === TermGroup_MessageType.ShiftRequestAnswer);
    }

    private get isSwapRequest(): boolean {
        return (this.messageType === TermGroup_MessageType.SwapRequest);
    }
    private get isTimeWorkAccount(): boolean {
        return (this.messageType === TermGroup_MessageType.TimeWorkAccountYearEmployeeOption);
    }

    private get isNeedsConfirmation(): boolean {
        return (this.messageType === TermGroup_MessageType.NeedsConfirmation);
    }

    private get isNeedsConfirmationAnswer(): boolean {
        return (this.messageType === TermGroup_MessageType.NeedsConfirmationAnswer);
    }

    private get needsConfirmation(): boolean {
        return this.messageType === TermGroup_MessageType.NeedsConfirmation
    }
    private set needsConfirmation(value: boolean) {
        this.messageType = value ? TermGroup_MessageType.NeedsConfirmation : TermGroup_MessageType.UserInitiated;
    }

    private tinyMceOptions: any;
    private tinyMceOptionsReadOnly: any;
    private user: IUserSmallDTO;
    private shift: ShiftDTO;
    private shifts: ShiftDTO[];
    private allEmployees: EmployeeListDTO[];

    // Flags
    private receiversInitiallyOpen: boolean = false;
    private messageInitiallyOpen: boolean = true;
    private showAvailableEmployees: boolean = false;
    private showAvailability: boolean = false;
    private disableCopyToEmail: boolean = false;
    private copyToEmail: boolean = false;

    //@ngInject
    constructor(
        private $timeout,
        private $q,
        private $uibModal,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private lazyLoadService: ILazyLoadService,
        private $scope) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.toolbarInclude = urlHelperService.getGlobalUrl("Core/RightMenu/MessageMenu/editHeader.html");

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            if (parameters.source) {
                if (parameters.source === 'MessageMenu' || parameters.source === 'Planning') {
                    parameters.guid = Guid.newGuid();
                    this.modal = parameters.modal;
                    this.onInit(parameters);

                    this.title = parameters.title;
                    this.shift = parameters.shift;
                    this.messageType = parameters.messageType;
                    this.showAvailableEmployees = parameters.showAvailableEmployees;
                    this.showAvailability = parameters.showAvailability;
                    this.allEmployees = parameters.allEmployees;
                    if (parameters.messageMinHeight)
                        this.messageMinHeight = parameters.messageMinHeight;
                } else if (parameters.source === 'DocumentMenu') {
                    parameters.guid = Guid.newGuid();
                    this.modal = parameters.modal;
                    this.onInit(parameters);

                    this.title = parameters.title;
                    //    this.messageId = parameters.id;
                    //    this.loadMessage();
                }
            }
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.messageId = parameters.id;
        this.guid = parameters.guid;
        this.mailType = parameters.type.id;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Communication_XEmail_Send, loadReadPermissions: true, loadModifyPermissions: true }]);

        this.absenceRequestLoaderPromise = this.lazyLoadService.loadBundle("Soe.Shared.Time.Schedule.Absencerequests.Bundle");
    }

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadTerms(),
            () => this.loadModifyPermissions(),
            () => this.loadApplicationSettings(),
            () => this.loadCompanySettings(),
            () => this.loadUser()
        ]).then(x => {
            this.setupCopyToEmail();

            if (this.messageId) {
                this.loadMessage().then(() => {
                    this.setupTinyMCE();
                });
            } else {
                this.newMessage();
                this.setupTinyMCE();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Communication_XEmail_Send].readPermission;
        this.modifyPermission = response[Feature.Communication_XEmail_Send].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        // Absence request 
        let absenceRequestGroup: ToolBarButtonGroup = ToolBarUtility.createGroup();
        absenceRequestGroup.buttons.push(new ToolBarButton("common.messages.showrequest", "common.messages.showrequest", IconLibrary.FontAwesome, "fa-file-contract", () => this.openAbsenceRequest(), null, () => { return !this.showOpenAbsenceRequest }));
        this.toolbar.addButtonGroup(absenceRequestGroup);
        // Cancel
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.cancel", "core.cancel", IconLibrary.FontAwesome, "fa-undo", () => { this.cancel(); }, null, () => { return !this.showCancel; })));
        // Send
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.send", "core.send", IconLibrary.FontAwesome, "fa-paper-plane", () => { this.send(); }, () => { return this.disableSend; }, () => { return !this.showSend; })));
        // Confirmation
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.messages.sendconfirmation", "common.messages.sendconfirmation", IconLibrary.FontAwesome, "fa-file-signature", () => { this.sendConfirmation(); }, null, () => { return !this.showSendConfirmation; }, { buttonClass: 'btn-success' })));
        // Shift request answer
        let shiftRequestGroup: ToolBarButtonGroup = ToolBarUtility.createGroup();
        shiftRequestGroup.buttons.push(new ToolBarButton("common.messages.sendyes", "common.messages.sendyes", IconLibrary.FontAwesome, "fa-thumbs-up", () => this.sendYes(), null, () => { return !this.showShiftRequestAnswer }, { buttonClass: 'btn-success' }));
        shiftRequestGroup.buttons.push(new ToolBarButton("common.messages.sendno", "common.messages.sendno", IconLibrary.FontAwesome, "fa-thumbs-down", () => this.sendNo(), null, () => { return !this.showShiftRequestAnswer }, { buttonClass: 'btn-danger' }));
        this.toolbar.addButtonGroup(shiftRequestGroup);
        // Delete
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.delete", "core.delete", IconLibrary.FontAwesome, "fa-times", () => { this.delete(); }, null, () => { return !this.showDelete; })));
        // Reply
        let replyGroup: ToolBarButtonGroup = ToolBarUtility.createGroup();
        replyGroup.buttons.push(new ToolBarButton("common.messages.reply", "common.messages.reply", IconLibrary.FontAwesome, "fa-reply", () => this.reply(), null, () => { return !this.showReply }));
        replyGroup.buttons.push(new ToolBarButton("common.messages.replyall", "common.messages.replyall", IconLibrary.FontAwesome, "fa-reply-all", () => this.replyAll(), null, () => { return !this.showReply }));
        this.toolbar.addButtonGroup(replyGroup);
        // Forward
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.messages.forward", "common.messages.forward", IconLibrary.FontAwesome, "fa-arrow-alt-right", () => { this.forward(); }, null, () => { return !this.showForward; })));

        this.toolbar.addInclude(this.toolbarInclude);
    }

    private setupTinyMCE() {
        if (this.isReadOnly) {
            if (!this.tinyMceOptionsReadOnly) {
                this.tinyMceOptionsReadOnly = {
                    baseURL: soeConfig.baseUrl,
                    toolbar: '',
                    menubar: '',
                    statusbar: false,
                    branding: false,
                    browser_spellcheck: false,
                    contextmenu: false,
                    editorReadOnly: true,
                    content_style: ".mce-content-body  { font-family:'Roboto Condensed', Verdana, Arial, Helvetica, Sans-serif; font-size: 12px; }",
                    setup: function (editor) {
                        editor.on('init', (event) => {
                            // Make links clickable in readonly editor
                            Array.from(editor.getDoc().querySelectorAll('a')).forEach(el => {
                                el['addEventListener']('click', () => {
                                    const href = el['getAttribute']('href');
                                    let target = el['getAttribute']('target');
                                    console.log(href, target);
                                    if (!target)
                                        target = '_blank';

                                    if (target !== '_blank') {
                                        document.location.href = href;
                                    } else {
                                        const link = document.createElement('a');
                                        link.href = href;
                                        link.target = target;
                                        link.rel = 'noopener';
                                        document.body.appendChild(link);
                                        link.click();
                                        document.body.removeChild(link);
                                    }
                                });
                            });
                        });
                        editor.on('focus', (event) => {
                            event.target.setMode('readonly');
                        });
                    },
                };
            }
        } else {
            if (!this.tinyMceOptions) {
                this.tinyMceOptions = {
                    ctrl: this,
                    baseURL: soeConfig.baseUrl,
                    language_url: TinyMCEUtility.getTinyMCELanguageUrl(),
                    language: TinyMCEUtility.getTinyMCELanguage(),
                    plugins: 'paste lists preview fullscreen',
                    toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | bullist numlist preview fullscreen',
                    paste_data_images: true,
                    valid_elements: '*[*]',
                    helpTitles: { title: " " },
                    menu: {
                        edit: { title: 'Edit', items: 'undo redo | cut copy paste pastetext | selectall' },
                        insert: { title: 'Insert', items: 'template hr' },
                        view: { title: 'View', items: 'visualaid' },
                        format: { title: 'Format', items: 'bold italic underline strikethrough superscript subscript | formats | removeformat' },
                        table: { title: 'Table', items: 'inserttable tableprops deletetable | cell row column' },
                        tools: { title: 'Tools', items: 'spellchecker code' }
                    },
                    content_style: ".help-toggle { background-color:#FFCCCF;} .help-toggle > a {font-size: 18px;} .help-toggle-content{padding-left: 10px; background-color: #CCFCFF; } .mce-content-body  { font-family:'Roboto Condensed', Verdana, Arial, Helvetica, Sans-serif; font-size: 12px; }",
                    verify_html: false,
                    apply_source_formatting: false,
                    setup: function (editor) {
                        editor.on('init', (event) => {

                            event.target.focus();
                        });
                        editor.on('PostProcess', (ed) => {
                            // replace <p></p> with <br />
                            ed.content = ed.content.replace(/(<p><\/p>)/gi, '<br />');
                        });
                        editor.on('KeyDown', function (e) {
                            // Escape close modal
                            if (e.keyCode == 27)
                                this.settings.ctrl.modal.close();
                        });
                    },
                    branding: false,
                    browser_spellcheck: true,
                    contextmenu: false,
                };
            }
        }
    }

    private setupCopyToEmail() {
        if (this.useDefaultEmailAddress && this.defaultCompanyEmailAddress) {
            this.disableCopyToEmail = false;
            this.copyToEmail = true;
            this.user.email = this.defaultCompanyEmailAddress;
        } else if (this.user.email) {
            this.disableCopyToEmail = false;
            this.copyToEmail = true;
        } else {
            this.disableCopyToEmail = true;
            this.copyToEmail = false;
            this.user.email = '';
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.warning",
            "common.messages.subject",
            "common.messages.sender",
            "common.messages.sentdate",
            "common.messages.reciever"
        ];

        if (this.isShiftRequest) {
            keys.push("core.xemail.shiftrequest");
            keys.push("core.xemail.assignmentrequest");
            keys.push("time.schedule.planning.available");
            keys.push("time.schedule.planning.unavailable");
        }

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        let featureIds: number[] = [];

        featureIds.push(Feature.Communication_XEmail);
        featureIds.push(Feature.Communication_XEmail_Send);
        featureIds.push(Feature.Communication_XEmail_Delete);
        featureIds.push(Feature.Communication_XEmail_Send_SMS);

        return this.coreService.hasModifyPermissions(featureIds).then(x => {
            this.messageModifyPermission = x[Feature.Communication_XEmail];
            this.messageSendPermission = x[Feature.Communication_XEmail_Send];
            this.messageDeletePermission = x[Feature.Communication_XEmail_Delete];
            this.sendSMSPermission = x[Feature.Communication_XEmail_Send_SMS];
        });
    }

    private loadApplicationSettings(): ng.IPromise<any> {
        return this.coreService.getIntSetting(SettingMainType.Application, ApplicationSettingType.AttachmentFileSize).then(x => {
            this.attachmentMaxSize = x;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.DefaultEmailAddress);
        settingTypes.push(CompanySettingType.UseDefaultEmailAddress);
        settingTypes.push(CompanySettingType.HideRecipientsInShiftRequest);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCompanyEmailAddress = SettingsUtility.getStringCompanySetting(x, CompanySettingType.DefaultEmailAddress);
            this.useDefaultEmailAddress = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseDefaultEmailAddress);
            // TODO: Implement setting (See SL)
            this.hideRecipientsInShiftRequest = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.HideRecipientsInShiftRequest);
        });
    }

    private loadUser(): ng.IPromise<any> {
        return this.coreService.getCurrentUser().then((x) => {
            this.user = x;
        });
    }

    private newShiftRequest() {
        this.message.entity = SoeEntityType.TimeScheduleTemplateBlock;

        this.loadLinkedShifts().then(() => {
            if (this.shifts.length > 0) {
                this.shifts = _.orderBy(this.shifts, s => s.actualStartTime, 'asc');
                this.message.subject = (this.shift.isOrder ? this.terms["core.xemail.assignmentrequest"] : this.terms["core.xemail.shiftrequest"]) + " " + CalendarUtility.toFormattedDate(this.shift.actualStartTime);
                this.message.text = '';
                _.forEach(this.shifts, (shift: ShiftDTO) => {
                    let msg = "{0}-{1} {2}".format(CalendarUtility.toFormattedTime(shift.actualStartTime), CalendarUtility.toFormattedTime(shift.actualStopTime), shift.shiftTypeName);
                    if (shift.accountName)
                        msg += " ({0})".format(shift.accountName);
                    if (this.shifts.length === 1)
                        this.message.subject += ", " + msg;
                    else
                        this.message.text += msg + "\n";
                });
                this.message.shortText = this.message.text;
            }
        });
    }

    private loadLinkedShifts(): ng.IPromise<any> {
        return this.coreService.getLinkedShifts(this.shift.timeScheduleTemplateBlockId).then(x => {
            this.shifts = x;
        });
    }

    // ACTIONS

    private newMessage() {
        this.isNew = true;
        this.receiversInitiallyOpen = true;
        this.message = new MessageEditDTO;
        this.message.recievers = [];
        this.messageId = 0;
        this.message.messageId = 0;
        this.message.parentId = 0;
        this.message.messageType = this.messageType ? this.messageType.valueOf() : TermGroup_MessageType.UserInitiated;

        if (this.shift)
            this.newShiftRequest();
        this.mailType = XEMailType.Outgoing;

        if (this.isReadOnly) {
            this.isReadOnly = false;
            this.setupTinyMCE();
        }
    }

    private loadMessage(): ng.IPromise<any> {
        return this.coreService.getXeMail(this.messageId).then(x => {
            this.isNew = false;
            this.message = x;
            this.messageType = this.message.messageType ? this.message.messageType : TermGroup_MessageType.UserInitiated;
            this.recipientNames = _.map(this.message.recievers, r => r.name).join('; ');
            this.files = this.message.attachments;

            if (this.message)
                this.isReadOnly = true;

            let mySelf = _.find(this.message.recievers, r => r.userId === CoreUtility.userId);
            if (mySelf && !mySelf.readDate) {
                this.coreService.setMessageAsRead(CalendarUtility.getDateNow(), this.message.messageId).then((result) => {
                    if (result && result.success)
                        this.publishEventReloadGrid(XEMailType.Incoming);
                })
            }

            this.isConfirmedByMe = this.needsConfirmation && mySelf && mySelf.answerType === XEMailAnswerType.Yes;
            this.isConfirmedByMeDate = this.isConfirmedByMe ? mySelf.answerDate : null;
            // Recipient of a needs confirmation message has replied to original sender.
            // Original sender should not be able to click confirmed.
            this.isRepliedNeedsConfirmation = (this.message.messageType === TermGroup_MessageType.NeedsConfirmation && !!this.message.parentId);
        });
    }

    public forward() {
        const parentId: number = this.messageId;
        const subject: string = "VB: " + this.message.subject;
        const body: string = this.createReplyHeader();

        this.newMessage();

        this.message.parentId = parentId;
        this.message.subject = subject;
        this.message.text = body;
        this.message.forwardDate = new Date();
        this.needsConfirmation = false;

        this.recipients = [];

        _.forEach(this.files, file => {
            file.messageAttachmentId = 0;
        });

        this.dirtyHandler.setDirty();
    }

    public reply() {
        const parentId: number = this.messageId;
        const senderId: number = this.message.senderUserId;
        const senderName: string = this.message.senderName;
        const subject: string = "SV: " + this.message.subject;
        const body: string = this.createReplyHeader();

        this.newMessage();
        this.receiversInitiallyOpen = false;

        this.message.parentId = parentId;
        this.message.subject = subject;
        this.message.text = body;
        this.message.replyDate = new Date();
        this.needsConfirmation = false;

        let rec: MessageGroupMemberDTO = new MessageGroupMemberDTO();
        rec.entity = SoeEntityType.User;
        rec.recordId = senderId;
        rec.username = senderName;
        this.recipients = [rec];

        // Remove attachments
        this.files = [];

        this.dirtyHandler.setDirty();
    }

    public replyAll() {
        const parentId: number = this.messageId;
        const senderId: number = this.message.senderUserId;
        const senderName: string = this.message.senderName;
        const subject: string = "SV: " + this.message.subject;
        const body: string = this.createReplyHeader();

        // Remember recipients before creating mew message
        let recievers = this.message.recievers;
        let newRecipients: MessageGroupMemberDTO[] = [];

        this.newMessage();
        this.receiversInitiallyOpen = false;

        this.message.parentId = parentId;
        this.message.subject = subject;
        this.message.text = body;
        this.message.replyDate = new Date();
        this.needsConfirmation = false;

        this.recipients = [];
        let rec: MessageGroupMemberDTO = new MessageGroupMemberDTO();
        rec.entity = SoeEntityType.User;
        rec.recordId = senderId;
        rec.username = senderName;
        newRecipients.push(rec);

        _.forEach(recievers, reciever => {
            if (reciever.userId !== CoreUtility.userId) {
                let newRec: MessageGroupMemberDTO = new MessageGroupMemberDTO();
                newRec.entity = SoeEntityType.User;
                newRec.recordId = reciever.userId;
                newRec.username = reciever.userName;
                newRecipients.push(newRec);
            }
        });
        this.recipients = newRecipients;

        // Remove attachments
        this.files = [];

        this.dirtyHandler.setDirty();
    }

    private createReplyHeader(): string {
        return "<br/><br/><div style='border-top:solid #000000 1.0pt;'><br/>" +
            "<p><b><span>" + this.terms["common.messages.sender"] + ": </span></b><span>" + this.message.senderName +
            "<br/><b>" + this.terms["common.messages.sentdate"] + ": </b>" + this.message.sentDate.toFormattedDateTime() +
            "<br/><b>" + this.terms["common.messages.reciever"] + ": </b> " + _.map(this.message.recievers, r => r.name).join('; ') +
            "<br/><b>" + this.terms["common.messages.subject"] + ": </b>" + this.message.subject +
            "</span></p></div><br/>" + (this.message.text ? this.message.text : '');
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteMessages([this.message.messageId], this.isIncoming).then(result => {
                if (result.success) {
                    completion.completed(this.message, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.messageId = 0;
            this.closeMe(true);
        });
    }

    public sendYes() {
        this.message.parentId = this.messageId;
        this.message.answerType = XEMailAnswerType.Yes;
        this.send();
    }

    public sendNo() {
        this.message.parentId = this.messageId;
        this.message.answerType = XEMailAnswerType.No;
        this.send();
    }

    public sendConfirmation() {
        this.message.parentId = this.messageId;
        this.message.answerType = XEMailAnswerType.Yes;
        this.send();
    }

    public openAbsenceRequest() {

        if (!this.showOpenAbsenceRequest)
            return;

        this.$q.all([this.absenceRequestLoaderPromise]).then(() => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"),
                controller: AbsenceRequestsEditController,
                controllerAs: 'ctrl',
                bindToController: true,
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                scope: this.$scope
            });

            modal.rendered.then(() => {
                this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                    modal: modal,
                    id: this.message.recordId,
                    employeeId: this.message.absenceRequestEmployeeId,
                    viewMode: this.message.absenceRequestEmployeeUserId == CoreUtility.userId ? AbsenceRequestViewMode.Employee : AbsenceRequestViewMode.Attest,
                    guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                    skipXEMailOnShiftChanges: false,
                    parentMode: AbsenceRequestParentMode.SchedulePlanning,
                    timeScheduleScenarioHeadId: null,
                });
            });
        });
    }

    public send() {
        this.createMessage();
        this.addRecipients();

        this.progress.startSaveProgress((completion) => {
            this.coreService.sendMessage(this.message).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.messageId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.message);
                } else {
                    completion.failed(result.errorMessage);
                    // Shift request could not be handled, maybe it was removed, close and reload
                    if (this.message.answerType === XEMailAnswerType.Yes) {
                        this.closeMe(true);
                    }
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.publishEventReloadGrid(XEMailType.Sent);
                this.closeMe(true);
            }, error => {
            });
    }

    // EVENTS

    private cancel() {
        this.modal.dismiss('cancel');
    }

    protected closeMe(reloadGrid: boolean = true) {
        if (reloadGrid)
            this.publishEventReloadGrid(-1);
        this.modal.close({ success: true });
    }

    public fileUploadedCallback(result) {
        let file: MessageAttachmentDTO = new MessageAttachmentDTO();
        file.messageAttachmentId = result.id || result.imageId;
        file.name = result.fileName || result.description;
        file.data = result.data || result.image;
        file.isUploadedAsImage = (result.fileType === "image");
        this.files.push(file);
        this.dirtyHandler.setDirty();
    }

    // HELP-METHODS

    private get showForward(): boolean {
        return this.messageModifyPermission && this.messageSendPermission && (this.isUserInitiated || this.isNeedsConfirmation || this.isNeedsConfirmationAnswer) && this.message && !!this.message.messageId;
    }

    private get showReply(): boolean {
        return this.messageModifyPermission && this.messageSendPermission && (this.isUserInitiated || this.isNeedsConfirmation || this.isNeedsConfirmationAnswer) && this.message && !!this.message.messageId;
    }

    private get showDelete(): boolean {
        return this.messageModifyPermission && this.messageDeletePermission && this.isIncoming && this.message && !!this.message.messageId;
    }

    private get showCancel(): boolean {
        return !this.isShiftRequest;
    }

    private get showShiftRequestAnswer(): boolean {
        return this.messageModifyPermission && this.messageSendPermission && this.isShiftRequest && !this.isOutgoing && !this.isDeleted;
    }

    private get showOpenAbsenceRequest(): boolean {
        return this.isAbsenceRequest && this.message && !!this.message.recordId && !!this.message.absenceRequestEmployeeId;
    }

    private get showSendConfirmation(): boolean {
        return this.messageModifyPermission && this.messageSendPermission && this.isNeedsConfirmation && !this.isOutgoing && !this.isConfirmedByMe && !this.isRepliedNeedsConfirmation;
    }

    private get showSend(): boolean {
        return this.messageModifyPermission && this.messageSendPermission && this.isOutgoing;
    }

    private get showSender(): boolean {
        return !this.isPayrollSlip;
    }     

    private get disableSend(): boolean {
        return !this.recipients || this.recipients.length === 0;
    }

    private createMessage() {
        this.message.licenseId = CoreUtility.licenseId;
        this.message.actorCompanyId = CoreUtility.actorCompanyId;
        this.message.roleId = CoreUtility.roleId;
        this.message.senderUserId = CoreUtility.userId;
        this.message.senderName = this.user.name;
        if (this.copyToEmail)
            this.message.senderEmail = this.user.email;

        this.message.messagePriority = TermGroup_MessagePriority.Normal.valueOf();
        this.message.messageDeliveryType = TermGroup_MessageDeliveryType.XEmail.valueOf();
        this.message.messageTextType = TermGroup_MessageTextType.Text.valueOf();
        this.message.messageType = this.messageType;
        this.message.markAsOutgoing = false;

        this.message.attachments = this.files.filter(f => (f.hasOwnProperty("isDeleted") && !f["isDeleted"] || !f.hasOwnProperty("isDeleted")));

        // Shift request
        if (this.isShiftRequest && this.shift)
            this.message.recordId = this.shift.timeScheduleTemplateBlockId;
    }

    private addRecipients() {
        // Recipients, convert from messageGroupMember
        this.message.recievers = [];
        _.forEach(this.recipients, (groupMember: MessageGroupMemberDTO) => {
            let receiverToAdd = new MessageRecipientDTO();
            receiverToAdd.userId = groupMember.recordId;
            receiverToAdd.userName = groupMember.username;
            switch (groupMember.entity) {
                case SoeEntityType.Employee:
                    receiverToAdd.type = XEMailRecipientType.Employee;
                    break;
                case SoeEntityType.User:
                    receiverToAdd.type = XEMailRecipientType.User;
                    break;
                case SoeEntityType.EmployeeGroup:
                    receiverToAdd.type = XEMailRecipientType.Group;
                    break;
                case SoeEntityType.Role:
                    receiverToAdd.type = XEMailRecipientType.Role;
                    break;
                case SoeEntityType.Category:
                    receiverToAdd.type = XEMailRecipientType.Category;
                    break;
                case SoeEntityType.Account:
                    receiverToAdd.type = XEMailRecipientType.Account;
                    break;
                case SoeEntityType.MessageGroup:
                    receiverToAdd.type = XEMailRecipientType.MessageGroup;
                    break;
            }

            this.message.recievers.push(receiverToAdd);
        });
    }

    private publishEventReloadGrid(type: XEMailType) {
        this.messagingHandler.publishEvent(Constants.EVENT_RELOAD_MESSAGES, { type: type, messageId: this.messageId });
    }
}