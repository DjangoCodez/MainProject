import { RightMenuControllerBase, RightMenuType } from "../RightMenuControllerBase";
import { IUrlHelperService } from "../../Services/UrlHelperService";
import { InformationDTO, InformationFolder } from "../../../Common/Models/InformationDTOs";
import { IContextMenuHandler } from "../../Handlers/ContextMenuHandler";
import { IContextMenuHandlerFactory } from "../../Handlers/ContextMenuHandlerFactory";
import { IMessagingService } from "../../Services/MessagingService";
import { ITranslationService } from "../../Services/TranslationService";
import { INotificationService } from "../../Services/NotificationService";
import { ICoreService } from "../../Services/CoreService";
import { ILazyLoadService } from "../../Services/LazyLoadService";
import { Feature, XEMailAnswerType, SoeInformationType, SoeInformationSourceType } from "../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TinyMCEUtility } from "../../../Util/TinyMCEUtility";
import { Constants } from "../../../Util/Constants";
import { EditController } from "./EditController";
import { IStorageService } from "../../Services/StorageService";

declare var pdfjsLib;

export class InformationMenuDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('/Core/RightMenu/InformationMenu/InformationMenu.html'),
            scope: {
                positionIndex: "@",
            },
            restrict: 'E',
            replace: true,
            controller: InformationMenuController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class InformationMenuController extends RightMenuControllerBase {

    // Terms
    private terms: { [index: string]: string; };
    private toggleTooltip: string;

    // Permissions
    private modifyPermission: boolean = false;

    // Data
    private nbrOfUnreadInformations: number = 0;
    private hasSevereUnreadInformation: boolean = false;
    private folders: InformationFolder[] = [];
    private informations: InformationDTO[] = [];
    private selectedInformation: InformationDTO;

    // Properties
    private types: any[] = [];
    private selectedType: any;

    private get isUnreadType(): boolean {
        return this.selectedType && this.selectedType.id === InformationMenuTypes.Unread;
    }
    private get isCompanyType(): boolean {
        return this.selectedType && this.selectedType.id === InformationMenuTypes.Company;
    }
    private get isSoftOneType(): boolean {
        return this.selectedType && this.selectedType.id === InformationMenuTypes.Sys;
    }

    // Flags
    private loadingInformations: boolean = false;

    private contextMenuHandler: IContextMenuHandler;

    private htmlEditorLoaderPromise: Promise<any>;

    private tinyMceOptions: any;

    // Timer
    private unreadTimer;
    readonly UNREAD_TIMER_INTERVAL: number = (60 * 1000 * 10);  // 10 minutes

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        messagingService: IMessagingService,
        private $scope: ng.IScope,
        private $interval: ng.IIntervalService,
        private $q: ng.IQService,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private storageService: IStorageService,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory,
        private lazyLoadService: ILazyLoadService) {
        super($timeout, messagingService, RightMenuType.Information);

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.setupUnreadTimer();
        });
    }

    public $onInit() {
        this.setTopPosition();
        this.htmlEditorLoaderPromise = this.lazyLoadService.loadBundle("Soe.Common.HtmlEditor.Bundle");

        this.messagingService.subscribe(Constants.EVENT_TOGGLE_INFORMATION_MENU, (data: any) => {
            this.toggleShowMenu();
        });

        this.messagingService.subscribe(Constants.EVENT_SHOW_INFORMATION_MENU, (data: any) => {
            if (!this.showMenu)
                this.toggleShowMenu();
        });
    }

    // SETUP

    private setupUnreadTimer() {
        this.loadNbrOfUnreadInformations(true);

        this.unreadTimer = this.$interval(() => {
            this.loadNbrOfUnreadInformations(true);
        }, this.UNREAD_TIMER_INTERVAL);

        this.$scope.$on('$destroy', () => {
            this.$interval.cancel(this.unreadTimer);
        });
    }

    private init() {
        this.$q.all([
            this.loadModifyPermissions()
        ]).then(() => {
            this.setupTypes();
            this.setupContextMenu();
        });
    }

    private setupTypes() {
        this.types = [];
        this.types.push({ id: InformationMenuTypes.Unread, title: this.terms["core.informationmenu.type.unread"], selected: true });
        this.types.push({ id: InformationMenuTypes.Company, title: this.terms["core.informationmenu.type.company"], selected: false });
        this.types.push({ id: InformationMenuTypes.Sys, title: this.terms["core.informationmenu.type.softone"], selected: false });
        this.selectType(this.types[0], this.showMenu);
    }

    private setupContextMenu() {
        this.contextMenuHandler = this.contextMenuHandlerFactory.create();
    }

    private getContextMenuOptions(information: InformationDTO): any[] {
        this.createContextMenuOptions(information);
        return this.contextMenuHandler.getContextMenuOptions();
    }

    private createContextMenuOptions(information: InformationDTO) {
        this.contextMenuHandler.clearContextMenuItems();
        if (this.modifyPermission && information.isCompanyInformation) {
            this.contextMenuHandler.addContextMenuItem(this.terms["core.informationmenu.new"], 'fa-plus', ($itemScope, $event, modelValue) => { this.edit(null); }, () => { return true; });
            this.contextMenuHandler.addContextMenuItem(this.terms["core.informationmenu.edit"], 'fa-pencil iconEdit', ($itemScope, $event, modelValue) => { this.edit(information); }, () => { return true; });
            this.contextMenuHandler.addContextMenuSeparator();
        }
        this.contextMenuHandler.addContextMenuItem(this.terms["core.informationmenu.view"], 'fa-eye', ($itemScope, $event, modelValue) => { this.initViewInformation(information); }, () => { return true; });
        if (information.needsConfirmation)
            this.contextMenuHandler.addContextMenuItem(this.terms["common.messages.sendconfirmation"], 'fa-file-check okColor', ($itemScope, $event, modelValue) => { this.setInformationAsConfirmed(information.informationId); }, () => { return !!information.readDate && !information.answerDate; });
    }

    private setupTinyMCE() {
        this.tinyMceOptions = TinyMCEUtility.setupDefaultReadOnlyOptions();
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.informationmenu.type.unread",
            "core.informationmenu.type.company",
            "core.informationmenu.type.softone",
            "core.informationmenu.view",
            "core.informationmenu.new",
            "core.informationmenu.edit",
            "core.documentmenu.nofolder",
            "common.created",
            "common.messages.confirmed",
            "common.messages.notconfirmed",
            "common.messages.sendconfirmation"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Manage_Preferences_CompanyInformation);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.modifyPermission = x[Feature.Manage_Preferences_CompanyInformation];
        });
    }

    private hasNewInformations(useCache: boolean): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (useCache) {
            // Get last time checked from local storage
            var time = this.storageService.fetch('hasNewInformations');
            if (!time)
                time = CalendarUtility.DefaultDateTime().toDateTimeString();

            this.coreService.hasNewInformations(time).then(result => {
                // Update last time checked in local storage
                this.storageService.add('hasNewInformations', new Date().toDateTimeString());
                deferral.resolve(result);
            });
        } else {
            // Force check
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private loadNbrOfUnreadInformations(useCache: boolean) {
        this.hasNewInformations(useCache).then(hasNewInformation => {
            // If new information exists, do not use cache to get number of unread informations
            this.coreService.getNbrOfUnreadInformations(!hasNewInformation).then(x => {
                this.nbrOfUnreadInformations = x;

                let keys: string[] = ["core.informationmenu.title", "core.informationmenu.unread"];
                this.translationService.translateMany(keys).then(terms => {
                    this.toggleTooltip = terms["core.informationmenu.title"]
                    if (this.nbrOfUnreadInformations > 0) {
                        this.toggleTooltip += " ({0} {1})".format(this.nbrOfUnreadInformations.toString(), terms["core.informationmenu.unread"].toLocaleLowerCase());
                        this.checkSevereUnreadInformation(!hasNewInformation);
                    } else {
                        this.hasSevereUnreadInformation = false;
                    }
                });
            });
        });
    }

    private checkSevereUnreadInformation(useCache: boolean): ng.IPromise<any> {
        return this.coreService.hasSevereUnreadInformation(useCache).then(x => {
            this.hasSevereUnreadInformation = x;
            if (this.hasSevereUnreadInformation && !this.showMenu)
                this.toggleShowMenu();
        });
    }

    private loadInformations() {
        if (this.loadingInformations)
            return;

        this.loadingInformations = true;

        switch (this.selectedType.id) {
            case InformationMenuTypes.Unread:
                this.coreService.getUnreadInformations().then(x => {
                    this.informations = x;
                    this.informationsLoaded();
                });
                break;
            case InformationMenuTypes.Company:
                this.coreService.getCompanyInformations().then(x => {
                    this.informations = x;
                    this.informationsLoaded();
                });
                break;
            case InformationMenuTypes.Sys:
                this.coreService.getSysInformations().then(x => {
                    this.informations = x;
                    this.informationsLoaded();
                });
                break;
            default:
                this.loadingInformations = false;
        }
    }

    private setInformationAsRead(id: number) {
        let information = _.find(this.informations, i => i.informationId === id);
        if (!information || information.readDate)
            return;

        let informationId: number = (information.isCompanyInformation ? information.informationId : 0);
        let sysInformationId: number = (information.isSysInformation ? information.informationId : 0);

        this.coreService.setInformationAsRead(informationId, sysInformationId, false, false).then((result) => {
            if (result.success) {
                this.loadNbrOfUnreadInformations(false);

                information.readDate = CalendarUtility.convertToDate(result.dateTimeValue);
                let folder = _.find(this.folders, f => f.name === information.folder);
                if (folder && folder.nbrOfUnread > 0)
                    folder.nbrOfUnread--;
            }
        });
    }

    private setInformationAsConfirmed(id: number) {
        let information = _.find(this.informations, i => i.informationId === id);
        if (!information || !information.readDate || information.answerDate)
            return;

        let informationId: number = (information.isCompanyInformation ? information.informationId : 0);
        let sysInformationId: number = (information.isSysInformation ? information.informationId : 0);

        this.coreService.setInformationAsRead(informationId, sysInformationId, true, false).then((result) => {
            if (result.success) {
                information.answerDate = CalendarUtility.convertToDate(result.dateTimeValue);
                information.answerType = XEMailAnswerType.Yes;

                if (information.isSeverityEmergency)
                    this.loadNbrOfUnreadInformations(false);
            }
        });
    }

    // ACTIONS

    private initTimyMCE(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (!this.tinyMceOptions) {
            this.htmlEditorLoaderPromise.then(() => {
                this.setupTinyMCE();
                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private initViewInformation(information: InformationDTO) {
        if (information.hasText) {
            // First time, need to setup HTML reader
            this.initTimyMCE().then(() => {
                // Get whole information (including text)
                if (information.isCompanyInformation) {
                    this.coreService.getCompanyInformation(information.informationId).then(x => {
                        this.viewInformation(x);
                    });
                } else if (information.isSysInformation) {
                    this.coreService.getSysInformation(information.informationId).then(x => {
                        this.viewInformation(x);
                    });
                }
            });
        } else {
            this.viewInformation(information);
        }
    }

    private viewInformation(information: InformationDTO) {
        if (information.hasText) {
            this.selectedInformation = information;
            if (!this.fullscreen)
                this.toggleFullscreen();
        } else {
            information['expanded'] = true;
        }
        this.setInformationAsRead(information.informationId);
    }

    private edit(information: InformationDTO) {
        if (!this.modifyPermission)
            return;

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/InformationMenu/edit.html"),
            controller: EditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: information ? information.informationId : 0
            });
        });

        modal.result.then((result) => {
            if (result)
                this.loadInformations();
        });
    }

    // EVENTS

    protected toggleShowMenu() {
        super.toggleShowMenu();

        if (this.showMenu && this.types.length === 0)
            this.init();
    }

    private selectType(type, loadInformations: boolean = true) {
        if (this.selectedType == type || this.loadingInformations)
            return;

        _.forEach(this.types, p => {
            p['selected'] = false;
        });
        type['selected'] = true;

        this.selectedType = type;

        if (loadInformations)
            this.loadInformations();
    }

    private toggleFolder(folder: InformationFolder) {
        folder.expanded = !folder.expanded;
    }

    // HELP-METHODS

    private informationsLoaded() {
        // Create distinct collection of folder names from loaded documents
        _.filter(this.informations, d => !d.folder).forEach(d => d.folder = this.terms["core.documentmenu.nofolder"]);

        this.folders = [];
        let folderNames = _.orderBy(Array.from(new Set(_.map(this.informations, f => f.folder))));
        _.forEach(folderNames, folderName => {
            let folder = new InformationFolder(folderName);
            folder.expanded = this.informations.length < 11;
            folder.nbrOfUnread = this.nbrOfUnreadInformationsInFolder(folderName);
            this.folders.push(folder);
        });

        // Expand unread emergency information
        _.filter(this.informations, i => i.isSeverityEmergency && (!i.readDate || (i.needsConfirmation && !i.answerDate))).forEach(i => i['expanded'] = true);

        this.loadingInformations = false;
    }

    private informationsInFolder(folderName: string): InformationDTO[] {
        return _.filter(this.informations, f => f.folder === folderName);
    }

    private nbrOfUnreadInformationsInFolder(folderName: string): number {
        return _.filter(this.informationsInFolder(folderName), i => !i.readDate).length;
    }

    private getInformationToolTip(information: InformationDTO): string {
        if (information.created)
            return "{0} {1}".format(this.terms["common.created"], information.created.toFormattedDateTime());
        else
            return null;
    }

    private getConfirmedTooltip(information: InformationDTO): string {
        if (information.answerDate)
            return "{0} {1}".format(this.terms["common.messages.confirmed"], information.answerDate.toFormattedDateTime());
        else
            return this.terms["common.messages.notconfirmed"];
    }
}

export enum InformationMenuTypes {
    Unread = 0,
    Company = 1,
    Sys = 2
}