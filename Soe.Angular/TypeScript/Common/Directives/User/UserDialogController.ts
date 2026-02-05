import { ICoreService } from "../../../Core/Services/CoreService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { EditUserFunctions } from "../../../Util/Enumerations";
import { CoreUtility } from "../../../Util/CoreUtility";

export class UserDialogController {

    // Init parameters
    private loginName: string;
    private langId: number;
    private blockedFromDate: Date;
    private externalAuthId: string;
    private lifetimeSeconds: number;
    private userLinkConnectionKey: string;    

    // Data
    private usersWithoutEmployees: SmallGenericType[];

    // Properties
    private function: EditUserFunctions;
    private connectUserId: number = 0;

    private externalAuthIdModified: boolean = false;
    private lifetimeSecondsModified: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private coreService: ICoreService,
        private languages: SmallGenericType[],
        private currentUserId: number,
        private hideConnectUser: boolean,
        private hideDisconnectUser: boolean,
        private hideDisconnectEmployee: boolean,
        private showSso: boolean,
        private showLifetime: boolean,
        loginName: string,
        langId: number,
        blockedFromDate: Date,
        externalAuthId: string,
        lifetimeSeconds: number,
        userLinkConnectionKey: string,    ) {

        // Make copy to be able to cancel
        this.loginName = loginName;
        this.langId = langId;
        this.blockedFromDate = blockedFromDate;
        this.externalAuthId = externalAuthId;
        this.lifetimeSeconds = lifetimeSeconds;
        this.userLinkConnectionKey = userLinkConnectionKey;
    }

    public $onInit() {
        this.$q.all([this.loadUsersWithoutEmployees()]).then(() => {
            this.function = this.currentUserId ? EditUserFunctions.ModifyUser : EditUserFunctions.NewUser;

            if (!this.currentUserId && !this.loginName)
                this.newUser();
        });
    }

    // SERVICE CALLS

    private loadUsersWithoutEmployees(): ng.IPromise<any> {
        return this.coreService.getUsersWithoutEmployees(CoreUtility.actorCompanyId, this.currentUserId, true).then(x => {
            this.usersWithoutEmployees = x;
        });
    }

    private loadUser() {
        this.$timeout(() => {
            this.coreService.getUser(this.connectUserId).then(x => {
                if (x) {
                    this.loginName = x.loginName;
                    this.langId = x.langId;
                    this.blockedFromDate = x.blockedFromDate;
                }
            });
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({
            function: this.function,
            loginName: this.loginName,
            langId: this.langId,
            blockedFromDate: this.blockedFromDate,
            isMobileUser: true,
            externalAuthId: this.externalAuthId,
            externalAuthIdModified: this.externalAuthIdModified,
            connectUserId: this.connectUserId,
            lifetimeSeconds: this.lifetimeSeconds,
            lifetimeSecondsModified: this.lifetimeSecondsModified,
            userLinkConnectionKey: this.userLinkConnectionKey
        });
    }

    // HELP-METHODS

    private newUser() {
        // Set default values on new user
        this.loginName = '';
        if (this.languages.length > 0)
            this.langId = this.languages[0].id;
    }
}

