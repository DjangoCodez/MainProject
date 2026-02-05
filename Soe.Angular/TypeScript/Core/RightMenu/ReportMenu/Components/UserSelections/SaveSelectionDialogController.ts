import { ReportUserSelectionAccessDTO, ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { IReportService } from "../../../../Services/reportservice";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { Feature, SoeModule, TermGroup, TermGroup_ReportUserSelectionAccessType } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";
import { INotificationService } from "../../../../Services/NotificationService";
import { ITranslationService } from "../../../../Services/TranslationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";

export class SaveSelectionDialogController {

    // Permissions
    private roleEditPermission: boolean = false;
    private messageGroupEditPermission: boolean = false;

    // Properties
    private isNew: boolean;
    private initiallyPrivate: boolean;
    private selection: ReportUserSelectionDTO;
    private accessType: TermGroup_ReportUserSelectionAccessType = TermGroup_ReportUserSelectionAccessType.Private;
    private includeColumnsSelection = false;

    private get accessTypeIsPrivate(): boolean {
        return this.accessType === TermGroup_ReportUserSelectionAccessType.Private;
    }
    private get accessTypeIsPublic(): boolean {
        return this.accessType === TermGroup_ReportUserSelectionAccessType.Public;
    }
    private get accessTypeIsRole(): boolean {
        return this.accessType === TermGroup_ReportUserSelectionAccessType.Role;
    }
    private get accessTypeIsMessageGroup(): boolean {
        return this.accessType === TermGroup_ReportUserSelectionAccessType.MessageGroup;
    }

    // Collections
    private accessTypes: ISmallGenericType[] = [];
    private scheduledJobHeads: SmallGenericType[] = [];
    private roles: ISmallGenericType[] = [];
    private messageGroups: ISmallGenericType[] = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private module: SoeModule,
        private showIncludeColumnsSelection: boolean,
        private savePublicPermission: boolean,
        selection: ReportUserSelectionDTO) {

        this.isNew = !selection || !selection.reportUserSelectionId;
        this.initiallyPrivate = !this.isNew && selection && !!selection.userId;

        this.selection = new ReportUserSelectionDTO();
        angular.extend(this.selection, selection);
    }

    // SETUP

    public $onInit() {
        this.loadModifyPermissions().then(() => {
            if (this.savePublicPermission)
                this.loadAccessTypes();
            else
                this.accessType = TermGroup_ReportUserSelectionAccessType.Private;

            this.loadScheduleJobHeads();

            this.setupSelection();
        });
    }

    private setupSelection() {
        if (this.isNew) {
            // New is always defaulted to private
            this.selection.userId = CoreUtility.userId;
        } else {
            // Existing, if user is specified, then it's private
            if (!this.selection.userId) {
                if (this.savePublicPermission) {
                    // Not private, check if any access restrictions is specified
                    // If multiple, they all have same type, so we can check the first one
                    if (this.selection.access && this.selection.access.length > 0) {
                        this.accessType = this.selection.access[0].type;
                        this.accessTypeChanged();
                    } else {
                        this.accessType = TermGroup_ReportUserSelectionAccessType.Public;
                    }
                } else {
                    this.selection.userId = CoreUtility.userId;
                    this.accessType = TermGroup_ReportUserSelectionAccessType.Private;
                }
            }
        }
    }

    // SERVICE CALLS

    private loadModifyPermissions(): ng.IPromise<any> {
        const features: number[] = [
            Feature.Manage_Roles_Edit_Permission,
            Feature.Manage_Preferences_Registry_EventReceiverGroups
        ];

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.roleEditPermission = x[Feature.Manage_Roles_Edit_Permission];
            this.messageGroupEditPermission = x[Feature.Manage_Preferences_Registry_EventReceiverGroups];
        });
    }

    private loadAccessTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ReportUserSelectionAccessType, false, false, true).then(types => {
            types.forEach(t => {
                if (t.id !== TermGroup_ReportUserSelectionAccessType.MessageGroup || this.messageGroupEditPermission)
                    this.accessTypes.push(t);
            });
        });
    }

    private loadScheduleJobHeads(): ng.IPromise<any> {
        return this.reportService.getScheduledJobHeads().then(x => {
            this.scheduledJobHeads = x;
        });
    }

    private loadRoles(): ng.IPromise<any> {
        if (this.roleEditPermission) {
            return this.coreService.getCompanyRolesDict(false, false).then(allRoles => {
                this.roles = allRoles;
                this.setExistingRoles();
            });
        } else {
            return this.coreService.getRolesByUserDict(CoreUtility.actorCompanyId).then(userRoles => {
                this.roles = userRoles;
                this.setExistingRoles();
            })
        }
    }

    private loadMessageGroups(): ng.IPromise<any> {
        return this.coreService.getMessageGroupsDict(true).then(x => {
            this.messageGroups = x;
            this.setExistingMessageGroups();
        });
    }

    // EVENTS

    private accessTypeChanged() {
        this.$timeout(() => {
            if (this.accessTypeIsRole && this.roles.length === 0)
                this.loadRoles();
            else if (this.accessTypeIsMessageGroup && this.messageGroups.length === 0)
                this.loadMessageGroups();
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private initSave() {
        if (!this.isNew && !this.initiallyPrivate) {
            const keys: string[] = [
                "core.reportmenu.selection.modifypublicwarning.title",
                "core.reportmenu.selection.modifypublicwarning.message"
            ]

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialogEx(terms["core.reportmenu.selection.modifypublicwarning.title"], terms["core.reportmenu.selection.modifypublicwarning.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.save();
                }, (reason) => {
                });
            });
        } else {
            this.save();
        }
    }

    private save() {
        this.selection.access = [];

        if (this.accessTypeIsRole) {
            this.roles.filter(r => r['selected']).map(r => r.id).forEach(id => {
                let acc = new ReportUserSelectionAccessDTO();
                acc.type = TermGroup_ReportUserSelectionAccessType.Role;
                acc.roleId = id;
                this.selection.access.push(acc);
            });
        } else if (this.accessTypeIsMessageGroup) {
            this.messageGroups.filter(r => r['selected']).map(r => r.id).forEach(id => {
                let acc = new ReportUserSelectionAccessDTO();
                acc.type = TermGroup_ReportUserSelectionAccessType.MessageGroup;
                acc.messageGroupId = id;
                this.selection.access.push(acc);
            });
        }

        this.$uibModalInstance.close({ selection: this.selection, includeColumnsSelection: this.includeColumnsSelection, accessType: this.accessType });
    }

    // HELP-METHODS

    private setExistingRoles() {
        if (this.selection.access && this.selection.access.length > 0) {
            let roleIds = this.selection.access.filter(a => a.type === TermGroup_ReportUserSelectionAccessType.Role && a.roleId).map(a => a.roleId);
            this.roles.filter(r => _.includes(roleIds, r.id)).forEach(r => r['selected'] = true);
        }
    }

    private setExistingMessageGroups() {
        if (this.selection.access && this.selection.access.length > 0) {
            let messageGroupIds = this.selection.access.filter(a => a.type === TermGroup_ReportUserSelectionAccessType.MessageGroup && a.messageGroupId).map(a => a.messageGroupId);
            this.messageGroups.filter(m => _.includes(messageGroupIds, m.id)).forEach(m => m['selected'] = true);
        }
    }
}