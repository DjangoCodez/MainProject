import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { SelectUsersController } from "../../../Common/Dialogs/SelectUsers/SelectUsersController";
import { IOriginUserSmallDTO } from "../../../Scripts/TypeLite.Net4";
import { OriginUserDTO } from "../../../Common/Models/InvoiceDTO";
import { CoreUtility } from "../../../Util/CoreUtility";

export class OriginUserHelper {

    public originUsers: OriginUserDTO[] = [];

    //@ngInject
    constructor(private parent: EditControllerBase2,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private $uibModal: any) {
    }

    public setOriginUsers(originUsers: IOriginUserSmallDTO[]) {
        this.originUsers = [];
        if (originUsers) {
            originUsers.forEach((o) => {
                const user = new OriginUserDTO();
                user.originUserId = o.originUserId;
                user.name = o.name;
                user.userId = o.userId;
                user.main = o.main;
                user.state = 0;
                this.originUsers.push(user);
            });
        }
    }

    public getOriginUserDTOs(): OriginUserDTO[] {
        //OriginUser
        return this.originUsers;
    }

    public setDefaultUser() {
        this.originUsers = [];
        if (CoreUtility.userId > 0) {
            const user = new OriginUserDTO();
            user.name = CoreUtility.loginName;
            user.userId = CoreUtility.userId;
            user.main = true;
            this.originUsers.push(user);
        }
    }

    public clear() {
        this.originUsers = [];
    }

    //Dialogs...
    public selectUsersDialog(showParticipant: boolean, showSendMessage: boolean, showCategories: boolean): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();

        this.translationService.translate("common.customer.customer.selectusers").then(title => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectUsers", "SelectUsers.html"),
                controller: SelectUsersController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    translationService: () => { return this.translationService },
                    coreService: () => { return this.coreService },
                    title: () => { return title },
                    selectedUsers: () => { return this.originUsers },
                    showMain: () => { return true },
                    showParticipant: () => { return showParticipant },
                    showSendMessage: () => { return showSendMessage },
                    showCategories: () => { return showCategories }
                }
            });

            modal.result.then(x => {
                this.originUsers = x.selectedUsers;
                deferral.resolve(x);
            });
        });

        return deferral.promise;
    }
}