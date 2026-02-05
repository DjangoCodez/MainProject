import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { UserDialogController } from "./UserDialogController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { TermGroup } from "../../../Util/CommonEnumerations";
import { EditUserFunctions } from "../../../Util/Enumerations";

export class UserDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Directives/User/Views/User.html'),
            scope: {
                currentUserId: '=',
                loginName: '=',
                langId: '=',
                blockedFromDate: '=',
                externalAuthId: '=',
                externalAuthIdModified: '=',
                lifetimeSeconds: '=',
                lifetimeSecondsModified: '=',
                userLinkConnectionKey: '=',
                isRequired: '=',
                readOnly: '=',
                hideConnectUser: '=',
                hideDisconnectUser: '=',
                hideDisconnectEmployee: '=',
                showSso: '=',
                showLifetime: '=',
                userHasChanges: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: UserController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class UserController {

    // Init parameters
    private currentUserId: number;
    private loginName: string;
    private langId: number;
    private blockedFromDate: Date;
    private externalAuthId: string;
    private externalAuthIdModified: boolean;
    private lifetimeSeconds: number;
    private lifetimeSecondsModified: boolean;
    private userLinkConnectionKey: string;
    private isRequired: boolean;
    private hideConnectUser: boolean;
    private hideDisconnectUser: boolean;
    private hideDisconnectEmployee: boolean;
    private showSso: boolean;
    private showLifetime: boolean;
    private userHasChanges: boolean;

    // Data
    private languages: SmallGenericType[] = [];
    private yesNo: SmallGenericType[] = [];

    // Properties
    private languageName: string;

    // Flags
    private readOnly: boolean;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService) {
    }

    public $onInit() {
        this.$q.all([
            this.loadLanguages(),
            this.loadYesNo()]).then(() => {
                this.setLanguageName();
                this.setupWatchers();
            });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.langId, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.setLanguageName();
        });

        this.$scope.$on('reloadUserInfo', (e) => {
            this.userHasChanges = false;
        });
    }

    // SERVICE CALLS

    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Language, false, false, true).then(x => {
            this.languages = x;
        });
    }

    private loadYesNo(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.YesNo, false, false).then(x => {
            this.yesNo = x;
        });
    }

    // ACTIONS

    private openUserDialog() {
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/User/Views/UserDialog.html"),
            controller: UserDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            scope: this.$scope,
            resolve: {
                currentUserId: () => { return this.currentUserId; },
                languages: () => { return this.languages; },
                hideConnectUser: () => { return this.hideConnectUser; },
                hideDisconnectUser: () => { return this.hideDisconnectUser; },
                hideDisconnectEmployee: () => { return this.hideDisconnectEmployee; },
                showSso: () => { return this.showSso; },
                showLifetime: () => { return this.showLifetime; },
                loginName: () => { return this.loginName; },
                langId: () => { return this.langId; },
                blockedFromDate: () => { return this.blockedFromDate; },
                externalAuthId: () => { return this.externalAuthId; },
                lifetimeSeconds: () => { return this.lifetimeSeconds; },
                userLinkConnectionKey: () => { return this.userLinkConnectionKey; }
            }
        });

        modal.result.then(result => {
            if (result) {
                if (result.function == EditUserFunctions.DisconnectUser) {
                    this.loginName = '';
                    this.langId = 0;
                    this.blockedFromDate = null;
                    this.currentUserId = 0;
                    this.userHasChanges = true;
                } else if (result.function != EditUserFunctions.DisconnectEmployee) {
                    if (result.function == EditUserFunctions.ConnectUser) {
                        this.currentUserId = result.connectUserId;
                        this.userHasChanges = true;
                    }

                    if (this.hasBeenModified(result))
                        this.userHasChanges = true;

                    this.loginName = result.loginName;
                    this.langId = result.langId;
                    this.blockedFromDate = result.blockedFromDate;
                    this.externalAuthId = result.externalAuthId;
                    this.externalAuthIdModified = result.externalAuthIdModified;
                    this.lifetimeSeconds = result.lifetimeSeconds;
                    this.lifetimeSecondsModified = result.lifetimeSecondsModified;
                    this.userLinkConnectionKey = result.userLinkConnectionKey;
                }

                if (this.userHasChanges && this.onChange)
                    this.onChange({ result: { function: result.function } });
            }
        });
    }

    // HELP-METHODS

    private hasBeenModified(result): boolean {
        if (result.externalAuthIdModified || result.lifetimeSecondsModified)
            return true;

        if (this.loginName !== result.loginName)
            return true;
        if (this.langId !== result.langId)
            return true;
        if (!this.blockedFromDate && result.blockedFromDate)
            return true;
        if (this.blockedFromDate && !result.blockedFromDate)
            return true;
        if (!this.blockedFromDate.isSameDayAs(result.blockedFromDate))
            return true;
    }

    private setLanguageName() {
        var lang = _.find(this.languages, l => l.id === this.langId);
        this.languageName = lang ? lang.name : '';
    }
}
